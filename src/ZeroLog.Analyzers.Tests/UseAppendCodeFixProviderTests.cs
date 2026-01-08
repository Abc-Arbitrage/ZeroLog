using System.Threading.Tasks;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class UseAppendCodeFixProviderTests
{
    [Test]
    public Task should_fix_message_only()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Debug|]("Foo"); // Bar
                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Debug().Append("Foo").Log(); // Bar
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_fix_interpolated_message()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, string message)
                    {
                        log.[|Info|]($"Foo {message}");
                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log, string message)
                    {
                        log.Info().Append($"Foo {message}").Log();
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_fix_message_and_exception()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.[|Error|]("Foo", ex);
                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.Error().Append("Foo").WithException(ex).Log();
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_fix_message_and_exception_reordered()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.[|Warn|](ex: ex, message: "Foo");
                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.Warn().Append("Foo").WithException(ex).Log();
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_normalize_whitespace()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {

                        log.[|Trace|](
                            "Foo",
                            ex
                        );

                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {

                        log.Trace().Append("Foo").WithException(ex).Log();

                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_fix_multi_line()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.[|Fatal|]("Foo", ex);
                    }
                }
                """,
            FixedCode = """
                class C
                {
                    void M(ZeroLog.Log log, System.Exception ex)
                    {
                        log.Fatal()
                           .Append("Foo")
                           .WithException(ex)
                           .Log();
                    }
                }
                """,
            CodeActionIndex = 1
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogCodeFixTest<UseAppendAnalyzer, UseAppendCodeFixProvider>;
}
