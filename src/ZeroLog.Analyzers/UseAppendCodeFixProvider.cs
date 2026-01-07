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
public class UseAppendCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticIds.UseAppend
    );

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var nodeToFix = root.FindNode(context.Span);

        const string titleSingleLine = "Use Append syntax (single line)";
        const string titleMultiLine = "Use Append syntax (multi line)";

        context.RegisterCodeFix(
            CodeAction.Create(
                titleSingleLine,
                async ct => await FixNode(context.Document, nodeToFix, root, multiLine: false, ct).ConfigureAwait(false) ?? context.Document,
                titleSingleLine),
            context.Diagnostics
        );

        context.RegisterCodeFix(
            CodeAction.Create(
                titleMultiLine,
                async ct => await FixNode(context.Document, nodeToFix, root, multiLine: true, ct).ConfigureAwait(false) ?? context.Document,
                titleMultiLine),
            context.Diagnostics
        );
    }

    private static async Task<Document?> FixNode(Document document,
                                                 SyntaxNode identifierNodeToFix,
                                                 SyntaxNode rootNode,
                                                 bool multiLine,
                                                 CancellationToken cancellationToken)
    {
        if (identifierNodeToFix is not IdentifierNameSyntax { Parent: MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax invocationToFix } })
            return null;

        if (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false) is not { } semanticModel)
            return null;

        if (semanticModel.GetOperation(invocationToFix, cancellationToken) is not IInvocationOperation invocationOperation)
            return null;

        if (invocationOperation.Arguments.Length is not (1 or 2))
            return null;

        if (invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 0) is not { } messageArgOp)
            return null;

        var exceptionArgOp = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 1);

        // Source: log.Info(message[, exception]);
        // Target: log.Info().Append(message)[.WithException(exception)].Log();

        // log.Info(...) -> log.Info()
        var chainExpression = InvocationExpression(invocationToFix.Expression);

        // Add .Append(message)
        chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.Append, multiLine, messageArgOp.Value.Syntax);

        // Add .WithException(exception)
        if (exceptionArgOp is not null)
            chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.WithException, multiLine, exceptionArgOp.Value.Syntax);

        // Add .Log()
        chainExpression = AddInvocation(chainExpression, ZeroLogFacts.MethodNames.Log, multiLine);

        var resultNode = multiLine
            ? Formatter.Format(chainExpression, document.Project.Solution.Workspace, cancellationToken: cancellationToken)
            : chainExpression;

        return document.WithSyntaxRoot(
            rootNode.ReplaceNode(
                invocationToFix,
                resultNode
            )
        );
    }

    private static InvocationExpressionSyntax AddInvocation(ExpressionSyntax baseExpression,
                                                            string methodName,
                                                            bool multiLine,
                                                            params SyntaxNode[] arguments)
    {
        if (multiLine)
        {
            var lastToken = baseExpression.GetLastToken();
            baseExpression = baseExpression.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(CarriageReturnLineFeed));
        }

        var memberAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            baseExpression,
            IdentifierName(methodName)
        );

        if (multiLine)
        {
            memberAccess = memberAccess.WithOperatorToken(
                memberAccess.OperatorToken.WithLeadingTrivia(Whitespace("    "))
            ).WithAdditionalAnnotations(Formatter.Annotation);
        }

        return InvocationExpression(memberAccess)
            .WithArgumentList(
                ArgumentList(
                    SeparatedList(
                        arguments.Select(node => Argument((ExpressionSyntax)node.NormalizeWhitespace())).ToArray()
                    )
                )
            );
    }
}
