using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class DiscardedLogMessageAnalyzerTests
{
    [Test]
    public Task should_not_report_usual_usage()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => log.Info().Append(42).Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_missing_log()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => log.Info().{|#0:Append|}(42);
                }
                """,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(DiscardedLogMessageAnalyzer.DiscardedLogMessageDiagnostic).WithLocation(0)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_explicit_discard()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => _ = log.Info().Append(42);
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_discard_on_any_method_that_returns_log_message()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M()
                        => {|#0:N|}();

                    ZeroLog.LogMessage N()
                        => throw null;
                }
                """,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(DiscardedLogMessageAnalyzer.DiscardedLogMessageDiagnostic).WithLocation(0)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_returned_log_message()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    ZeroLog.LogMessage M(ZeroLog.Log log)
                        => log.Info().Append(42);
                }
                """
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<DiscardedLogMessageAnalyzer>
    {
    }
}
