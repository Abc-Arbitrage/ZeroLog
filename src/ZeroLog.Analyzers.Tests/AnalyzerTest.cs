using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace ZeroLog.Analyzers.Tests;

internal class AnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.CSharp10;

    protected AnalyzerTest()
    {
        ReferenceAssemblies = AnalyzerTestHelper.Net6ReferenceAssemblies;
        TestState.AdditionalReferences.Add(typeof(LogManager).Assembly);
    }

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
}
