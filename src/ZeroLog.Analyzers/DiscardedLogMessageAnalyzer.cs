﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DiscardedLogMessageAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor DiscardedLogMessageDiagnostic = new(
        DiagnosticIds.DiscardedLogMessage,
        "Discarded LogMessage",
        "The returned LogMessage cannot be implicitly discarded. This is most often caused by a missing call to Log(). If needed, discard the return value explicitly.",
        DiagnosticIds.Category,
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiscardedLogMessageDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext compilationStartContext)
    {
        var logMessageType = compilationStartContext.Compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.LogMessage);
        if (logMessageType is null)
            return;

        compilationStartContext.RegisterOperationAction(
            operationContext =>
            {
                var operation = (IExpressionStatementOperation)operationContext.Operation;

                if (operation.Operation.Kind == OperationKind.Invocation
                    && SymbolEqualityComparer.Default.Equals(operation.Operation.Type, logMessageType))
                {
                    operationContext.ReportDiagnostic(Diagnostic.Create(DiscardedLogMessageDiagnostic, GetDiagnosticLocation(operation)));
                }
            },
            OperationKind.ExpressionStatement
        );
    }

    private static Location GetDiagnosticLocation(IExpressionStatementOperation operation)
    {
        return operation.Operation.Syntax switch
        {
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax expressionSyntax } => expressionSyntax.Name.GetLocation(),
            InvocationExpressionSyntax { Expression: IdentifierNameSyntax identifierNameSyntax }     => identifierNameSyntax.GetLocation(),
            _                                                                                        => operation.Syntax.GetLocation()
        };
    }
}
