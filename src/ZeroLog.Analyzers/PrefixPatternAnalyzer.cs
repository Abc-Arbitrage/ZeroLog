using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ZeroLog.Formatting;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PrefixPatternAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor InvalidPrefixPatternDiagnostic = new(
        DiagnosticIds.InvalidPrefixPattern,
        "Invalid prefix pattern",
        "Invalid prefix pattern: {0}",
        DiagnosticIds.Category,
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        InvalidPrefixPatternDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext compilationStartContext)
    {
        var compilation = (CSharpCompilation)compilationStartContext.Compilation;

        var defaultFormatterType = compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.DefaultFormatter);
        var prefixPatternProperties = defaultFormatterType?.GetMembers(ZeroLogFacts.PropertyNames.PrefixPattern);
        if (prefixPatternProperties is not [IPropertySymbol prefixPatternProperty])
            return;

        compilationStartContext.RegisterOperationAction(
            operationContext =>
            {
                if (operationContext.Operation is IAssignmentOperation { Value.ConstantValue: { HasValue: true, Value: var pattern }, Target: IPropertyReferenceOperation { Property: var assignedProperty } } assignmentOperation
                    && SymbolEqualityComparer.Default.Equals(assignedProperty, prefixPatternProperty)
                    && !PrefixWriter.IsValidPattern(pattern as string))
                {
                    operationContext.ReportDiagnostic(Diagnostic.Create(InvalidPrefixPatternDiagnostic, assignmentOperation.Value.Syntax.GetLocation(), pattern ?? "null"));
                }
            },
            OperationKind.SimpleAssignment
        );
    }
}
