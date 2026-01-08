using System.Threading.Tasks;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class UseSimpleSyntaxAnalyzerTests
{
    [Test]
    public Task should_report_direct_log_opportunity()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => log.[|Info|]().Append("Foo").Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_interpolation_opportunity()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => log.[|Info|]().Append(42).AppendEnum(System.DayOfWeek.Friday).Append(true).Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_key_value_pairs_are_used()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                        => log.Info().AppendKeyValue("Key", "Value").Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_interpolation_opportunity_when_format_string_is_a_literal()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M1(ZeroLog.Log log)
                        => log.[|Info|]().Append(42, "X").Log();

                    void M2(ZeroLog.Log log)
                        => log.[|Info|]().Append(format: "X", value: 40 + 2).Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_format_string_is_not_a_literal()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    const string format = "X";

                    void M1(ZeroLog.Log log)
                        => log.Info().Append(42, format).Log();

                    void M2(ZeroLog.Log log)
                        => log.Info().Append(format: format, value: 42).Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_interpolation_opportunity_when_verbatim_inner_interpolations_are_used()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    const string format = "X";

                    void M(ZeroLog.Log log)
                        => log.Info().Append($@"").Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_report_direct_log_opportunity_with_exception()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                        => log.[|Info|]().Append("Foo").WithException(ex).Log();
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_not_report_direct_log_opportunity_with_multiple_exceptions()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                        => log.Info().Append("Foo").WithException(ex).WithException(ex).Log();
                }
                """
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<UseSimpleSyntaxAnalyzer>;
}
