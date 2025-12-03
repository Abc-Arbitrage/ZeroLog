using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ZeroLog.Formatting;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PatternAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor InvalidPatternDiagnostic = new(
        DiagnosticIds.InvalidPattern,
        "Invalid pattern",
        "Invalid pattern: {0}",
        DiagnosticIds.Category,
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        InvalidPatternDiagnostic
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

        if (compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.PatternWriter)?.InstanceConstructors is [{ } patternWriterConstructor])
        {
            compilationStartContext.RegisterOperationAction(
                operationContext =>
                {
                    if (operationContext.Operation is IObjectCreationOperation { Constructor: { } constructor, Arguments: [{ Value.ConstantValue: { HasValue: true, Value: var pattern } } argument] }
                        && SymbolEqualityComparer.Default.Equals(constructor, patternWriterConstructor)
                        && !PatternWriter.IsValidPattern(pattern as string))
                    {
                        operationContext.ReportDiagnostic(Diagnostic.Create(InvalidPatternDiagnostic, argument.Syntax.GetLocation(), pattern ?? "null"));
                    }
                },
                OperationKind.ObjectCreation
            );
        }

        if (compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.DefaultFormatter)?.GetMembers(ZeroLogFacts.PropertyNames.PrefixPattern) is [IPropertySymbol prefixPatternProperty])
        {
            compilationStartContext.RegisterOperationAction(
                operationContext =>
                {
                    if (operationContext.Operation is IAssignmentOperation { Value.ConstantValue: { HasValue: true, Value: var pattern }, Target: IPropertyReferenceOperation { Property: var assignedProperty } } assignmentOperation
                        && SymbolEqualityComparer.Default.Equals(assignedProperty, prefixPatternProperty)
                        && !PatternWriter.IsValidPattern(pattern as string))
                    {
                        operationContext.ReportDiagnostic(Diagnostic.Create(InvalidPatternDiagnostic, assignmentOperation.Value.Syntax.GetLocation(), pattern ?? "null"));
                    }
                },
                OperationKind.SimpleAssignment
            );
        }
    }
}
