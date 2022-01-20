using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class UseStringInterpolationAnalyzerTests
{
    [Test]
    public Task should_report_direct_log_opportunity()
    {
        var test = new Test
        {
            Source = @"
class C
{
    void M(ZeroLog.Log log)
        => log.{|#0:Info|}().Append(""Foo"").Log();
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult(UseStringInterpolationAnalyzer.UseStringInterpolationDiagnostic).WithLocation(0)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_interpolation_opportunity()
    {
        var test = new Test
        {
            Source = @"
class C
{
    void M(ZeroLog.Log log)
        => log.{|#0:Info|}().Append(42).AppendEnum(System.DayOfWeek.Friday).Append(true).Log();
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult(UseStringInterpolationAnalyzer.UseStringInterpolationDiagnostic).WithLocation(0)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_key_value_pairs_are_used()
    {
        var test = new Test
        {
            Source = @"
class C
{
    void M(ZeroLog.Log log)
        => log.Info().AppendKeyValue(""Key"", ""Value"").Log();
}
"
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<UseStringInterpolationAnalyzer>
    {
    }
}
