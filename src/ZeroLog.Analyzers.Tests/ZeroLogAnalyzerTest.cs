using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace ZeroLog.Analyzers.Tests;

internal static class ZeroLogAnalyzerTest
{
    public static readonly ReferenceAssemblies Net6ReferenceAssemblies = new(
        "net6.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
        Path.Combine("ref", "net6.0")
    );
}

internal class ZeroLogAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, NUnitVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public string Source
    {
        set => TestState.Sources.Add(value);
    }

    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.Default;

    protected ZeroLogAnalyzerTest()
    {
        ReferenceAssemblies = ZeroLogAnalyzerTest.Net6ReferenceAssemblies;
        TestState.AdditionalReferences.Add(typeof(LogManager).Assembly);
    }

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
}
