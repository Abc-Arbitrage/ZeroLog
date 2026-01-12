using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ZeroLog.Analyzers.Support;
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

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var compilation = (CSharpCompilation)context.Compilation;

        var patternAttributeType = compilation.GetTypeByMetadataName(typeof(PatternWriter.PatternAttribute).FullName!);
        if (patternAttributeType is null)
            return;

        context.RegisterOperationAction(
            i => Analyze(ref i, patternAttributeType),
            OperationKind.Argument, OperationKind.SimpleAssignment
        );
    }

    private static void Analyze(ref OperationAnalysisContext context, INamedTypeSymbol patternAttributeType)
    {
        ILiteralOperation? literalOperation = null;

        switch (context.Operation.Kind)
        {
            case OperationKind.Argument:
            {
                if (context.Operation is IArgumentOperation
                    {
                        Value.Kind: OperationKind.Literal,
                        Value: ILiteralOperation literal,
                        Parameter: { Type.SpecialType: SpecialType.System_String } parameter
                    }
                    && parameter.HasAttribute(patternAttributeType))
                {
                    literalOperation = literal;
                }

                break;
            }

            case OperationKind.SimpleAssignment:
            {
                if (context.Operation is ISimpleAssignmentOperation
                    {
                        Value.Kind: OperationKind.Literal,
                        Value: ILiteralOperation literal,
                        Target.Kind: OperationKind.PropertyReference,
                        Target: IPropertyReferenceOperation
                        {
                            Property: { Type.SpecialType: SpecialType.System_String } property
                        }
                    }
                    && property.HasAttribute(patternAttributeType))
                {
                    literalOperation = literal;
                }

                break;
            }

            default:
            {
                return;
            }
        }

        if (literalOperation is { ConstantValue: { HasValue: true, Value: string pattern } }
            && !PatternWriter.IsValidPattern(pattern))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidPatternDiagnostic, literalOperation.Syntax.GetLocation(), pattern));
        }
    }
}
