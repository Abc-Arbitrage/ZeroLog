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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ZeroLog.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UseAppendSyntaxCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticIds.UseAppendSyntax
    );

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var nameNode = root.FindNode(context.Span);
        if (!nameNode.IsKind(SyntaxKind.IdentifierName))
            return;

        const string titleSingleLine = "Use Append syntax (single-line)";
        const string titleMultiLine = "Use Append syntax (multi-line)";

        context.RegisterCodeFix(
            CodeAction.Create(
                titleSingleLine,
                async cancellationToken => await Apply(document, nameNode, root, multiLine: false, cancellationToken).ConfigureAwait(false) ?? document,
                titleSingleLine
            ),
            context.Diagnostics
        );

        context.RegisterCodeFix(
            CodeAction.Create(
                titleMultiLine,
                async cancellationToken => await Apply(document, nameNode, root, multiLine: true, cancellationToken).ConfigureAwait(false) ?? document,
                titleMultiLine
            ),
            context.Diagnostics
        );
    }

    private static async Task<Document?> Apply(Document document,
                                               SyntaxNode initialIdentifierNode,
                                               SyntaxNode rootNode,
                                               bool multiLine,
                                               CancellationToken cancellationToken)
    {
        if (initialIdentifierNode is not IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax initialInvocation } initialMemberAccess })
            return null;

        if (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false) is not { } semanticModel)
            return null;

        if (semanticModel.GetOperation(initialInvocation, cancellationToken) is not IInvocationOperation invocationOperation)
            return null;

        if (invocationOperation.Arguments.Length is not (1 or 2))
            return null;

        if (invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 0) is not { } messageArgOp)
            return null;

        var exceptionArgOp = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 1);

        var invocationOptions = multiLine
            ? new InvocationOptions(
                new string(' ', initialMemberAccess.SyntaxTree.GetLineSpan(initialMemberAccess.OperatorToken.Span, cancellationToken).StartLinePosition.Character),
                (await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false)).GetOption(FormattingOptions.NewLine)
            )
            : new InvocationOptions(null, null);

        // Source: log.Info(message[, exception]);
        // Target: log.Info().Append(message)[.WithException(exception)].Log();

        // log.Info(...) -> log.Info()
        var chainExpression = initialInvocation.WithArgumentList(ArgumentList());

        // Add .Append(message)
        chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.Append, invocationOptions, messageArgOp.Value.Syntax);

        // Add .WithException(exception)
        if (exceptionArgOp is not null)
            chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.WithException, invocationOptions, exceptionArgOp.Value.Syntax);

        // Add .Log()
        chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.Log, invocationOptions);

        return document.WithSyntaxRoot(
            rootNode.ReplaceNode(
                initialInvocation,
                chainExpression.WithTrailingTrivia(initialInvocation.GetTrailingTrivia())
            )
        );
    }

    private static InvocationExpressionSyntax AddInvocation(ExpressionSyntax previousExpression,
                                                            string methodName,
                                                            InvocationOptions options,
                                                            params SyntaxNode[] arguments)
    {
        var memberAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            previousExpression,
            IdentifierName(methodName)
        );

        if (options.MultiLine)
        {
            memberAccess = memberAccess.WithOperatorToken(
                memberAccess.OperatorToken.WithLeadingTrivia(options.EndOfLine, options.Indent)
            );
        }

        return InvocationExpression(memberAccess)
            .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        arguments.Select(node => Argument((ExpressionSyntax)node.WithoutTrivia())).ToArray()
                    )
                )
            );
    }

    private class InvocationOptions(string? indent, string? eol)
    {
        public bool MultiLine { get; } = indent is not null;
        public SyntaxTrivia Indent { get; } = Whitespace(indent ?? string.Empty);
        public SyntaxTrivia EndOfLine { get; } = EndOfLine(eol ?? string.Empty);
    }
}
