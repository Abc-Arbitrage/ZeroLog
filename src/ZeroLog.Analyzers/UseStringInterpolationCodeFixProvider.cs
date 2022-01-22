using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ZeroLog.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UseStringInterpolationCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticIds.UseStringInterpolation
    );

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var nodeToFix = root.FindNode(context.Span);

        const string title = "Use string interpolation";
        context.RegisterCodeFix(CodeAction.Create(title, ct => FixNode(context.Document, nodeToFix, root, ct), title), context.Diagnostics);
    }

    private static async Task<Document> FixNode(Document document,
                                                SyntaxNode logBuilderIdentifierNode,
                                                SyntaxNode rootNode,
                                                CancellationToken cancellationToken)
    {
        if (logBuilderIdentifierNode is not IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax logBuilderInvocation } })
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var parts = TryConvertToStringInterpolationParts(logBuilderInvocation, semanticModel, cancellationToken, out var logMethodInvocation);
        if (parts is null || logMethodInvocation is null)
            return document;

        parts = ConcatInterpolationStringTexts(FlattenNestedInterpolations(parts)).ToList();

        ExpressionSyntax resultExpression = parts.Count switch
        {
            0 => LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(string.Empty)
            ),
            1 when parts[0] is InterpolatedStringTextSyntax singleTextSyntax => LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal('"' + singleTextSyntax.TextToken.Text + '"', singleTextSyntax.TextToken.ValueText)
            ),
            _ => InterpolatedStringExpression(
                Token(SyntaxKind.InterpolatedStringStartToken),
                new SyntaxList<InterpolatedStringContentSyntax>(parts),
                Token(SyntaxKind.InterpolatedStringEndToken)
            )
        };

        rootNode = rootNode.ReplaceNode(
            logMethodInvocation,
            logBuilderInvocation.WithArgumentList(
                                    logBuilderInvocation.ArgumentList
                                                        .AddArguments(Argument(resultExpression))
                                )
                                .WithTrailingTrivia(logMethodInvocation.GetTrailingTrivia())
        );

        return document.WithSyntaxRoot(rootNode);
    }

    private static List<InterpolatedStringContentSyntax>? TryConvertToStringInterpolationParts(InvocationExpressionSyntax logBuilderInvocation,
                                                                                               SemanticModel semanticModel,
                                                                                               CancellationToken cancellationToken,
                                                                                               out InvocationExpressionSyntax? logMethodInvocation)
    {
        var parts = new List<InterpolatedStringContentSyntax>();
        logMethodInvocation = null;

        var currentNode = logBuilderInvocation.Parent;

        while (true)
        {
            if (currentNode is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax invocation } memberAccess)
                return null;

            if (memberAccess.Name.Identifier.Text == "Log")
            {
                logMethodInvocation = invocation;
                return parts;
            }

            if (memberAccess.Name.Identifier.Text is not ("Append" or "AppendEnum"))
                return null;

            if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation invocationOperation)
                return null;

            var valueOperation = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 0)?.Value;
            if (valueOperation is null)
                return null;

            var valueSyntaxNode = valueOperation.Syntax.WithoutTrivia();

            switch (valueOperation.Syntax.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                {
                    var literalSyntax = (LiteralExpressionSyntax)valueSyntaxNode;

                    // Verbatim strings have different escaping rules, treat them as separate expressions
                    if (literalSyntax.Token.IsVerbatimStringLiteral())
                        goto default;

                    parts.Add(
                        InterpolatedStringText(
                            Token(
                                SyntaxTriviaList.Empty,
                                SyntaxKind.InterpolatedStringTextToken,
                                EscapeBraces(GetNonVerbatimStringTokenInnerText(literalSyntax.Token.Text)),
                                EscapeBraces(literalSyntax.Token.ValueText),
                                SyntaxTriviaList.Empty
                            )
                        )
                    );

                    break;
                }

                default:
                {
                    var interpolation = Interpolation((ExpressionSyntax)valueSyntaxNode);

                    if (invocationOperation.Arguments.Length > 1)
                    {
                        var formatValueSyntax = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Name == "format")?.Value.Syntax.WithoutTrivia();

                        if (formatValueSyntax != null && formatValueSyntax.IsKind(SyntaxKind.StringLiteralExpression))
                        {
                            var formatExpression = (LiteralExpressionSyntax)formatValueSyntax;
                            var formatLiteralText = formatExpression.Token.Text;

                            if (formatExpression.Token.IsVerbatimStringLiteral())
                            {
                                // Try to convert a verbatim string into a standard string
                                formatLiteralText = SymbolDisplay.FormatLiteral(formatExpression.Token.ValueText, true);
                                if (formatLiteralText.StartsWith("@"))
                                    return null; // The format string contains a newline
                            }

                            interpolation = interpolation.WithFormatClause(
                                InterpolationFormatClause(
                                    Token(SyntaxKind.ColonToken),
                                    Token(
                                        SyntaxTriviaList.Empty,
                                        SyntaxKind.InterpolatedStringTextToken,
                                        GetNonVerbatimStringTokenInnerText(formatLiteralText),
                                        formatExpression.Token.ValueText,
                                        SyntaxTriviaList.Empty
                                    )
                                )
                            );
                        }
                    }

                    parts.Add(interpolation);
                    break;
                }
            }

            currentNode = invocation.Parent;
        }
    }

    private static IEnumerable<InterpolatedStringContentSyntax> FlattenNestedInterpolations(IEnumerable<InterpolatedStringContentSyntax> parts)
    {
        foreach (var part in parts)
        {
            if (part.IsKind(SyntaxKind.Interpolation))
            {
                var interpolationSyntax = (InterpolationSyntax)part;

                if (interpolationSyntax.Expression.IsKind(SyntaxKind.InterpolatedStringExpression))
                {
                    var innerInterpolation = (InterpolatedStringExpressionSyntax)interpolationSyntax.Expression;

                    foreach (var innerPart in FlattenNestedInterpolations(innerInterpolation.Contents))
                        yield return innerPart;
                }
                else
                {
                    yield return part;
                }
            }
            else
            {
                yield return part;
            }
        }
    }

    private static IEnumerable<InterpolatedStringContentSyntax> ConcatInterpolationStringTexts(IEnumerable<InterpolatedStringContentSyntax> parts)
    {
        InterpolatedStringTextSyntax? lastTextToken = null;

        foreach (var part in parts)
        {
            if (part.IsKind(SyntaxKind.InterpolatedStringText))
            {
                var interpolatedString = (InterpolatedStringTextSyntax)part;

                if (lastTextToken is null)
                {
                    lastTextToken = interpolatedString;
                }
                else
                {
                    var lastInterpolatedTextToken = lastTextToken.TextToken;

                    lastTextToken = lastTextToken.WithTextToken(
                        Token(
                            SyntaxTriviaList.Empty,
                            SyntaxKind.InterpolatedStringTextToken,
                            lastInterpolatedTextToken.Text + interpolatedString.TextToken.Text,
                            lastInterpolatedTextToken.ValueText + interpolatedString.TextToken.ValueText,
                            SyntaxTriviaList.Empty
                        )
                    );
                }
            }
            else
            {
                if (lastTextToken != null)
                {
                    yield return lastTextToken;
                    lastTextToken = null;
                }

                yield return part;
            }
        }

        if (lastTextToken != null)
            yield return lastTextToken;
    }

    private static string GetNonVerbatimStringTokenInnerText(string text)
        => text.Substring(1, text.Length - 2);

    private static string EscapeBraces(string text)
        => text.Replace("{", "{{").Replace("}", "}}");
}
