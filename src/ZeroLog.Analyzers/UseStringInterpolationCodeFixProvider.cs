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
        context.RegisterCodeFix(CodeAction.Create(title, ct => FixNode(context.Document, nodeToFix, ct), title), context.Diagnostics);
    }

    private static async Task<Document> FixNode(Document document, SyntaxNode logBuilderIdentifierNode, CancellationToken cancellationToken)
    {
        if (logBuilderIdentifierNode is not IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax rootInvocation } })
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var parts = new List<InterpolatedStringContentSyntax>();

        var currentNode = rootInvocation.Parent;
        InvocationExpressionSyntax? logInvocation;

        while (true)
        {
            if (currentNode is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax invocation } memberAccess)
                return document;

            if (memberAccess.Name.Identifier.Text == "Log")
            {
                logInvocation = invocation;
                break;
            }

            if (memberAccess.Name.Identifier.Text is not ("Append" or "AppendEnum"))
                return document;

            if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation invocationOperation)
                return document;

            var valueOperation = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Name == "value")?.Value;
            if (valueOperation is null)
                return document;

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
                                GetNonVerbatimStringTokenInnerText(literalSyntax.Token.Text),
                                literalSyntax.Token.ValueText,
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
                        if (!formatValueSyntax.IsKind(SyntaxKind.StringLiteralExpression))
                            return document;

                        var formatExpression = (LiteralExpressionSyntax)formatValueSyntax;

                        interpolation = interpolation.WithFormatClause(
                            InterpolationFormatClause(
                                Token(SyntaxKind.ColonToken),
                                Token(
                                    SyntaxTriviaList.Empty,
                                    SyntaxKind.InterpolatedStringTextToken,
                                    GetNonVerbatimStringTokenInnerText(formatExpression.Token.Text),
                                    formatExpression.Token.ValueText,
                                    SyntaxTriviaList.Empty
                                )
                            )
                        );
                    }

                    parts.Add(interpolation);
                    break;
                }
            }

            currentNode = invocation.Parent;
        }

        var interpolatedString = InterpolatedStringExpression(
            Token(SyntaxKind.InterpolatedStringStartToken),
            new SyntaxList<InterpolatedStringContentSyntax>(
                ConcatInterpolationStringTexts(FlattenNestedInterpolations(parts))
            ),
            Token(SyntaxKind.InterpolatedStringEndToken)
        );

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var newArgList = rootInvocation.ArgumentList.AddArguments(Argument(interpolatedString));

        root = root.ReplaceNode(logInvocation, rootInvocation.WithArgumentList(newArgList).WithTrailingTrivia(logInvocation.GetTrailingTrivia()));
        return document.WithSyntaxRoot(root);
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
}
