using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace ZeroLog.Analyzers.Tests;

internal static class ZeroLogAnalyzerTest
{
    private static readonly ReferenceAssemblies _net6ReferenceAssemblies = new(
        "net6.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
        Path.Combine("ref", "net6.0")
    );

    public static void ConfigureTest(AnalyzerTest<NUnitVerifier> test)
    {
        test.ReferenceAssemblies = _net6ReferenceAssemblies;
        test.TestState.AdditionalReferences.Add(typeof(LogManager).Assembly);
    }
}

internal abstract class ZeroLogAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.Default;

    protected ZeroLogAnalyzerTest()
    {
        ZeroLogAnalyzerTest.ConfigureTest(this);
    }

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
}

internal abstract class ZeroLogCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    protected ZeroLogCodeFixTest()
    {
        ZeroLogAnalyzerTest.ConfigureTest(this);
    }
}
