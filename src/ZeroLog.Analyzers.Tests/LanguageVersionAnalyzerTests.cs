using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class LanguageVersionAnalyzerTests
{
    [Test]
    public Task should_warn_below_csharp10()
    {
        var test = new Test
        {
            LanguageVersion = LanguageVersion.CSharp9,
            Source = string.Empty,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(LanguageVersionAnalyzer.UnsupportedLanguageVersionDiagnostic)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_warn_above_csharp10()
    {
        var test = new Test
        {
            LanguageVersion = LanguageVersion.CSharp10,
            Source = string.Empty,
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<LanguageVersionAnalyzer>
    {
    }
}
