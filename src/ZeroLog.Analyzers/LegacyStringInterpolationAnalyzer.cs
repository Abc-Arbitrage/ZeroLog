using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LegacyStringInterpolationAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor AllocatingStringInterpolationDiagnostic = new(
        DiagnosticIds.AllocatingStringInterpolation,
        "Allocating string interpolation",
        "This string interpolation will allocate. Set the language version to C# 10 or greater to fix this.",
        DiagnosticIds.Category,
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        AllocatingStringInterpolationDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        if (((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp10)
            return;

        var logType = context.Compilation.GetTypeByMetadataName("ZeroLog.Log");
        var logMessageType = context.Compilation.GetTypeByMetadataName("ZeroLog.LogMessage");

        if (logType is null || logMessageType is null)
            return;

        var stringParameters = logType.GetMembers()
                                      .Where(
                                          m => m.Kind == SymbolKind.Method
                                               && m.Name is "Trace" or "Debug" or "Info" or "Warn" or "Error" or "Fatal"
                                      )
                                      .Concat(
                                          logMessageType.GetMembers()
                                                        .Where(
                                                            m => m.Kind == SymbolKind.Method
                                                                 && m.Name is "Append"
                                                        )
                                      )
                                      .Cast<IMethodSymbol>()
                                      .Where(m => m.Parameters.Length > 0 && m.Parameters[0].Type.SpecialType == SpecialType.System_String)
                                      .Select(m => m.Parameters[0])
                                      .ToImmutableHashSet(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(
            operationContext =>
            {
                if (operationContext.Operation.Parent?.Kind != OperationKind.Argument)
                    return;

                var argumentOperation = (IArgumentOperation)operationContext.Operation.Parent;
                if (argumentOperation.Parameter is null)
                    return;

                if (stringParameters.Contains(argumentOperation.Parameter))
                    operationContext.ReportDiagnostic(Diagnostic.Create(AllocatingStringInterpolationDiagnostic, operationContext.Operation.Syntax.GetLocation()));
            },
            OperationKind.InterpolatedString
        );
    }
}
