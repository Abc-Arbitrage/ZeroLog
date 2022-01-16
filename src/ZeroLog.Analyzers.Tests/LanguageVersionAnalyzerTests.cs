using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
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
            TestState =
            {
                Sources = { string.Empty }
            },
            ExpectedDiagnostics =
            {
                new(LanguageVersionAnalyzer.UnsupportedLanguageVersionDiagnostic)
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
            TestState =
            {
                Sources = { string.Empty }
            }
        };

        return test.RunAsync();
    }

    private class Test : AnalyzerTest<LanguageVersionAnalyzer>
    {
    }
}
