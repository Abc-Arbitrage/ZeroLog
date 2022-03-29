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
        var compilation = (CSharpCompilation)compilationStartContext.Compilation;

        if (compilation.LanguageVersion < LanguageVersion.CSharp10)
            return;

        var logType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.Log);
        var logMessageType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.LogMessage);

        if (logType is null || logMessageType is null)
            return;

        var logMethod = logMessageType.GetMembers(ZeroLogFacts.MethodNames.Log)
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

            var rootMethodInvocation = FindSimplifiableLogBuilderInvocationOperation(invocation.Instance);

            if (rootMethodInvocation?.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax expressionSyntax })
                operationContext.ReportDiagnostic(Diagnostic.Create(UseStringInterpolationDiagnostic, expressionSyntax.Name.GetLocation()));
        }

        private IInvocationOperation? FindSimplifiableLogBuilderInvocationOperation(IOperation? operation)
        {
            while (true)
            {
                if (operation?.Kind != OperationKind.Invocation)
                    return null;

                var invocationOperation = (IInvocationOperation)operation;

                if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, _logMessageTypeSymbol))
                {
                    if (invocationOperation.TargetMethod.Name is not (ZeroLogFacts.MethodNames.Append or ZeroLogFacts.MethodNames.AppendEnum))
                        return null;

                    var valueArgument = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 0);
                    if (valueArgument is null)
                        return null;

                    // Don't suggest a simplification if there is a verbatim string interpolation
                    if (valueArgument.Value.Kind == OperationKind.InterpolatedString
                        && valueArgument.Value.Syntax is InterpolatedStringExpressionSyntax interpolatedStringSyntax
                        && interpolatedStringSyntax.StringStartToken.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken))
                    {
                        return null;
                    }

                    if (invocationOperation.Arguments.Length > 1)
                    {
                        // The format string needs to be a literal
                        var formatArgument = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Name is ZeroLogFacts.ParameterNames.FormatString);
                        if (formatArgument != null && formatArgument.Value.Kind != OperationKind.Literal)
                            return null;
                    }

                    operation = invocationOperation.Instance;
                }
                else if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, _logTypeSymbol))
                {
                    if (invocationOperation.TargetMethod.Parameters.IsEmpty && ZeroLogFacts.IsLogLevelName(invocationOperation.TargetMethod.Name))
                        return invocationOperation;

                    return null;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
