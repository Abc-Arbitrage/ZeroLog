using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseStringInterpolationAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UseStringInterpolationDiagnostic = new(
        DiagnosticIds.UseStringInterpolation,
        "Use string interpolation",
        "String interpolation syntax can be used for this log message",
        DiagnosticIds.Category,
        DiagnosticSeverity.Info,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        UseStringInterpolationDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext compilationStartContext)
    {
        if (((CSharpCompilation)compilationStartContext.Compilation).LanguageVersion < LanguageVersion.CSharp10)
            return;

        var logType = compilationStartContext.Compilation.GetTypeByMetadataName("ZeroLog.Log");
        var logMessageType = compilationStartContext.Compilation.GetTypeByMetadataName("ZeroLog.LogMessage");
        if (logType is null || logMessageType is null)
            return;

        var logMethod = logMessageType.GetMembers("Log")
                                      .Where(i => i.Kind == SymbolKind.Method)
                                      .OfType<IMethodSymbol>()
                                      .FirstOrDefault(i => i.Parameters.IsEmpty && i.ReturnsVoid && !i.IsStatic);

        if (logMethod is null)
            return;

        var analysis = new Analysis(logType, logMessageType, logMethod);

        compilationStartContext.RegisterOperationAction(
            analysis.AnalyzeOperation,
            OperationKind.Invocation
        );
    }

    private class Analysis
    {
        private readonly INamedTypeSymbol _logTypeSymbol;
        private readonly INamedTypeSymbol _logMessageTypeSymbol;
        private readonly IMethodSymbol _logMethodSymbol;

        public Analysis(INamedTypeSymbol logTypeSymbol, INamedTypeSymbol logMessageTypeSymbol, IMethodSymbol logMethodSymbol)
        {
            _logTypeSymbol = logTypeSymbol;
            _logMessageTypeSymbol = logMessageTypeSymbol;
            _logMethodSymbol = logMethodSymbol;
        }

        public void AnalyzeOperation(OperationAnalysisContext operationContext)
        {
            var invocation = (IInvocationOperation)operationContext.Operation;

            if (!SymbolEqualityComparer.Default.Equals(invocation.TargetMethod, _logMethodSymbol))
                return;

            var rootMethodInvocation = FindRootBuilderInvocationOperation(invocation.Instance);
            if (rootMethodInvocation?.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax expressionSyntax })
            {
                operationContext.ReportDiagnostic(Diagnostic.Create(UseStringInterpolationDiagnostic, expressionSyntax.Name.GetLocation()));
            }
        }

        private IInvocationOperation? FindRootBuilderInvocationOperation(IOperation? operation)
        {
            while (operation is not null)
            {
                if (operation.Kind != OperationKind.Invocation)
                    break;

                var invocationOperation = (IInvocationOperation)operation;

                if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, _logMessageTypeSymbol))
                {
                    if (invocationOperation.TargetMethod.Name is not ("Append" or "AppendEnum"))
                        return null;

                    operation = invocationOperation.Instance;
                }
                else if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, _logTypeSymbol))
                {
                    if (invocationOperation.TargetMethod.Parameters.IsEmpty
                        && invocationOperation.TargetMethod.Name is "Trace" or "Debug" or "Info" or "Warn" or "Error" or "Fatal")
                    {
                        return invocationOperation;
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
    }
}
