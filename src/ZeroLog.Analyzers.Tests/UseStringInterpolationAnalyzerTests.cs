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
            TestCode = @"
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
            TestCode = @"
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
            TestCode = @"
class C
{
    void M(ZeroLog.Log log)
        => log.Info().AppendKeyValue(""Key"", ""Value"").Log();
}
"
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_interpolation_opportunity_when_format_string_is_a_literal()
    {
        var test = new Test
        {
            TestCode = @"
class C
{
    void M1(ZeroLog.Log log)
        => log.{|#0:Info|}().Append(42, ""X"").Log();

    void M2(ZeroLog.Log log)
        => log.{|#1:Info|}().Append(format: ""X"", value: 40 + 2).Log();
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult(UseStringInterpolationAnalyzer.UseStringInterpolationDiagnostic).WithLocation(0),
                new DiagnosticResult(UseStringInterpolationAnalyzer.UseStringInterpolationDiagnostic).WithLocation(1)
            }
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_format_string_is_not_a_literal()
    {
        var test = new Test
        {
            TestCode = @"
class C
{
    const string format = ""X"";

    void M1(ZeroLog.Log log)
        => log.Info().Append(42, format).Log();

    void M2(ZeroLog.Log log)
        => log.Info().Append(format: format, value: 42).Log();
}
"
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_verbatim_inner_interpolations_are_used()
    {
        var test = new Test
        {
            TestCode = @"
class C
{
    const string format = ""X"";

    void M(ZeroLog.Log log)
        => log.Info().Append($@"""").Log();
}
"
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<UseStringInterpolationAnalyzer>
    {
    }
}
