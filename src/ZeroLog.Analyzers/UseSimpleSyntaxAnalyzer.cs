using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseSimpleSyntaxAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UseSimpleSyntaxDiagnostic = new(
        DiagnosticIds.UseSimpleSyntax,
        "Use simple syntax",
        "String interpolation syntax can be used for this log message",
        DiagnosticIds.Category,
        DiagnosticSeverity.Info,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        UseSimpleSyntaxDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = (CSharpCompilation)context.Compilation;

        if (compilation.LanguageVersion < LanguageVersion.CSharp10)
            return;

        var logType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.Log);
        var logMessageType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.LogMessage);

        if (logType is null || logMessageType is null)
            return;

        var logMethod = logMessageType.GetMembers(ZeroLogFacts.MethodNames.Log)
                                      .Where(i => i.Kind == SymbolKind.Method)
                                      .OfType<IMethodSymbol>()
                                      .FirstOrDefault(i => i is
                                      {
                                          ReturnsVoid: true,
                                          IsStatic: false,
                                          DeclaredAccessibility: Accessibility.Public,
                                          Parameters.IsEmpty: true
                                      });

        if (logMethod is null)
            return;

        var analysis = new Analysis(logType, logMessageType, logMethod);

        context.RegisterOperationAction(
            analysis.AnalyzeOperation,
            OperationKind.Invocation
        );
    }

    private class Analysis(INamedTypeSymbol logTypeSymbol, INamedTypeSymbol logMessageTypeSymbol, IMethodSymbol logMethodSymbol)
    {
        public void AnalyzeOperation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.Name != ZeroLogFacts.MethodNames.Log)
                return;

            if (!SymbolEqualityComparer.Default.Equals(invocation.TargetMethod, logMethodSymbol))
                return;

            var rootMethodInvocation = FindSimplifiableLogBuilderInvocationOperation(invocation.Instance);

            if (rootMethodInvocation?.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax expressionSyntax })
                context.ReportDiagnostic(Diagnostic.Create(UseSimpleSyntaxDiagnostic, expressionSyntax.Name.GetLocation()));
        }

        private IInvocationOperation? FindSimplifiableLogBuilderInvocationOperation(IOperation? operation)
        {
            var hasWithExceptionCall = false;

            while (true)
            {
                if (operation?.Kind != OperationKind.Invocation)
                    return null;

                var invocationOperation = (IInvocationOperation)operation;

                if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, logMessageTypeSymbol))
                {
                    switch (invocationOperation.TargetMethod.Name)
                    {
                        case ZeroLogFacts.MethodNames.Append:
                        case ZeroLogFacts.MethodNames.AppendEnum:
                        {
                            var valueArgument = invocationOperation.Arguments.FirstOrDefault(i => i.Parameter?.Ordinal == 0);
                            if (valueArgument is null)
                                return null;

                            // Don't suggest a simplification if there is a verbatim string interpolation
                            if (valueArgument.Value is { Kind: OperationKind.InterpolatedString, Syntax: InterpolatedStringExpressionSyntax interpolatedStringSyntax }
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

                            break;
                        }

                        case ZeroLogFacts.MethodNames.WithException:
                        {
                            // Don't allow multiple WithException calls
                            if (hasWithExceptionCall)
                                return null;

                            if (invocationOperation.Arguments.Length != 1)
                                return null;

                            hasWithExceptionCall = true;
                            break;
                        }

                        default:
                        {
                            return null;
                        }
                    }

                    operation = invocationOperation.Instance;
                }
                else if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod.ContainingType, logTypeSymbol))
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
