using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ZeroLog.Analyzers.Tests;

internal static class ZeroLogAnalyzerTest
{
    private static readonly ReferenceAssemblies _netReferenceAssemblies = new(
        "net8.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.0"),
        Path.Combine("ref", "net8.0")
    );

    public static void ConfigureTest(AnalyzerTest<DefaultVerifier> test)
    {
        test.ReferenceAssemblies = _netReferenceAssemblies;
        test.TestState.AdditionalReferences.Add(typeof(LogManager).Assembly);
    }
}

internal abstract class ZeroLogAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    [StringSyntax("csharp")]
    public new string TestCode
    {
        set => base.TestCode = value;
    }

    public LanguageVersion LanguageVersion { get; init; } = LanguageVersion.Default;

    protected ZeroLogAnalyzerTest()
    {
        ZeroLogAnalyzerTest.ConfigureTest(this);
    }

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
}

internal abstract class ZeroLogCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    [StringSyntax("csharp")]
    public new string TestCode
    {
        set => base.TestCode = value;
    }

    [StringSyntax("csharp")]
    public new string FixedCode
    {
        set => base.FixedCode = value;
    }

    protected ZeroLogCodeFixTest()
    {
        ZeroLogAnalyzerTest.ConfigureTest(this);
    }
}
