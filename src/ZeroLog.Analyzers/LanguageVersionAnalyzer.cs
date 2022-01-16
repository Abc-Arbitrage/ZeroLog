using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZeroLog.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LanguageVersionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UnsupportedLanguageVersionDiagnostic = new(
        DiagnosticIds.UnsupportedLanguageVersion,
        "Unsupported language version",
        "Language versions below C# 10 are not supported by ZeroLog. Unintended allocations could occur.",
        DiagnosticIds.Category,
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        UnsupportedLanguageVersionDiagnostic
    );

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        if (((CSharpCompilation)context.Compilation).LanguageVersion < LanguageVersion.CSharp10)
            context.ReportDiagnostic(Diagnostic.Create(UnsupportedLanguageVersionDiagnostic, null));
    }
}
