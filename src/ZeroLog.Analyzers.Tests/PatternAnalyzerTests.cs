using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class PatternAnalyzerTests
{
    [Test]
    public Task should_report_invalid_pattern_in_PrefixPattern()
    {
        var test = new Test
        {
            TestCode = """
                using ZeroLog.Formatting;

                class C
                {
                    DefaultFormatter M()
                        => new DefaultFormatter { PrefixPattern = {|#0:"%{level:-20}"|} };
                }
                """,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(PatternAnalyzer.InvalidPatternDiagnostic).WithLocation(0).WithArguments("%{level:-20}")
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_valid_pattern_in_PrefixPattern()
    {
        var test = new Test
        {
            TestCode = """
                using ZeroLog.Formatting;

                class C
                {
                    DefaultFormatter M()
                        => new DefaultFormatter { PrefixPattern = "%{level:20}" };
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_assignment_to_different_symbol()
    {
        var test = new Test
        {
            TestCode = """
                using ZeroLog.Formatting;

                class C
                {
                    C M()
                        => new C { PrefixPattern = "%{level:-20}" };

                    string PrefixPattern { get; init; }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_invalid_pattern_in_PatternWriter_constructor()
    {
        var test = new Test
        {
            TestCode = """
                using ZeroLog.Formatting;

                class C
                {
                    void M()
                    {
                        _ = new PatternWriter({|#0:"%{level:-20}"|});
                    }
                }
                """,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(PatternAnalyzer.InvalidPatternDiagnostic).WithLocation(0).WithArguments("%{level:-20}")
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_valid_pattern_in_PatternWriter_constructor()
    {
        var test = new Test
        {
            TestCode = """
                using ZeroLog.Formatting;

                class C
                {
                    void M()
                    {
                        _ = new PatternWriter("%{level:20}");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<PatternAnalyzer>;
}
