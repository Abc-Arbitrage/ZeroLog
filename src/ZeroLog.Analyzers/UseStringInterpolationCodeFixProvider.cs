using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    private static async Task<Document> FixNode(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        if (nodeToFix is not IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax rootInvocation } })
            return document;

        var parts = new List<InterpolatedStringContentSyntax>();

        var currentNode = rootInvocation.Parent;
        InvocationExpressionSyntax? logInvocation;

        string? currentString = null;
        string? currentValueString = null;

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

            var value = invocation.ArgumentList.Arguments[0].Expression.WithoutTrivia();

            switch (value.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                {
                    var literal = (LiteralExpressionSyntax)value;
                    currentString += GetInnerText(literal);
                    currentValueString += literal.Token.ValueText;
                    break;
                }

                default:
                {
                    AppendCurrentString();

                    var interpolation = Interpolation(value);

                    if (invocation.ArgumentList.Arguments.Count > 1)
                    {
                        var formatValue = invocation.ArgumentList.Arguments[1].Expression.WithoutTrivia();
                        if (!formatValue.IsKind(SyntaxKind.StringLiteralExpression))
                            return document;

                        var formatExpression = (LiteralExpressionSyntax)formatValue;

                        interpolation = interpolation.WithFormatClause(
                            InterpolationFormatClause(
                                Token(SyntaxKind.ColonToken),
                                Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, GetInnerText(formatExpression), formatExpression.Token.ValueText, SyntaxTriviaList.Empty)
                            )
                        );
                    }

                    parts.Add(interpolation);
                    break;
                }
            }

            currentNode = invocation.Parent;
        }

        AppendCurrentString();

        var interpolatedString = InterpolatedStringExpression(
            Token(SyntaxKind.InterpolatedStringStartToken),
            new SyntaxList<InterpolatedStringContentSyntax>(parts),
            Token(SyntaxKind.InterpolatedStringEndToken)
        );

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var newArgList = rootInvocation.ArgumentList.AddArguments(Argument(interpolatedString));

        root = root.ReplaceNode(logInvocation, rootInvocation.WithArgumentList(newArgList).WithoutTrailingTrivia());
        return document.WithSyntaxRoot(root);

        void AppendCurrentString()
        {
            if (currentString is null || currentValueString is null)
                return;

            parts.Add(InterpolatedStringText(Token(SyntaxTriviaList.Empty, SyntaxKind.InterpolatedStringTextToken, currentString, currentValueString, SyntaxTriviaList.Empty)));
            currentString = null;
            currentValueString = null;
        }

        static string GetInnerText(LiteralExpressionSyntax syntax)
        {
            var text = syntax.Token.Text;
            return text.Substring(1, text.Length - 2);
        }
    }
}
