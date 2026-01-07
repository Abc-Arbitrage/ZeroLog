using System.Threading.Tasks;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class UseStringInterpolationCodeFixProviderTests
{
    [Test]
    public Task should_fix()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]()
                           .Append("Foo")
                           .Append("Bar:\n")
                           .Append(' ')
                           .Append(42)
                           .Append(Guid.NewGuid(), "B")
                           .AppendEnum(DayOfWeek.Friday)
                           .Append(" Baz")
                           .Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"FooBar:\n {42}{Guid.NewGuid():B}{DayOfWeek.Friday} Baz");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_keep_external_trivia()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        /* start trivia */ log.[|Info|]().Append("Foo ").Append( /* foo */ 42 /* bar */ ).Log() /* end trivia */ ;
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        /* start trivia */ log.Info($"Foo {42}") /* end trivia */ ;
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_transform_out_of_order_format_argument()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append(format: "X", value: 42).Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"{42:X}");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_transform_braces()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append("Foo{Bar}Baz").Append(42).Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"Foo{{Bar}}Baz{42}");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_handle_verbatim_format_string()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append(42, @"Foo""\").Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"{42:Foo\"\\}");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_flatten_interpolated_appends()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append("Foo").Append($"Bar {42} Baz").Append("!").Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"FooBar {42} Baz!");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_add_parentheses_for_conditional_expressions()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M1(ZeroLog.Log log, bool condition)
                    {
                        log.[|Info|]().Append(condition ? "Foo" : "Bar").Log();
                    }

                    void M2(ZeroLog.Log log, bool condition)
                    {
                        string str = null;
                        log.[|Info|]().Append(str ??= condition ? "Foo" : "Bar").Log();
                    }

                    void M3(ZeroLog.Log log, bool condition)
                    {
                        string str = null;
                        log.[|Info|]().Append(str ??= (condition ? "Foo" : "Bar")).Log();
                    }

                    void M4(ZeroLog.Log log, bool condition)
                    {
                        log.[|Info|]().Append(GetValue(condition ? "Foo" : "Bar")).Log();
                        string GetValue(string value) => value;
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M1(ZeroLog.Log log, bool condition)
                    {
                        log.Info($"{(condition ? "Foo" : "Bar")}");
                    }

                    void M2(ZeroLog.Log log, bool condition)
                    {
                        string str = null;
                        log.Info($"{(str ??= condition ? "Foo" : "Bar")}");
                    }

                    void M3(ZeroLog.Log log, bool condition)
                    {
                        string str = null;
                        log.Info($"{str ??= (condition ? "Foo" : "Bar")}");
                    }

                    void M4(ZeroLog.Log log, bool condition)
                    {
                        log.Info($"{GetValue(condition ? "Foo" : "Bar")}");
                        string GetValue(string value) => value;
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_treat_verbatim_strings_as_expressions()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append("Foo").Append(@"Bar").Append("Baz").Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info($"Foo{@"Bar"}Baz");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_output_string_literals_when_possible()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Append("Foo").Append("Bar").Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info("FooBar");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_handle_empty_log_message()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Info|]().Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.Info("");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_handle_exceptions()
    {
        var test = new Test
        {
            TestCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log, Exception ex)
                    {
                        log.[|Info|]().Append("Foo").WithException(ex).Log();
                    }
                }
                """,
            FixedCode = """
                using System;

                class C
                {
                    void M(ZeroLog.Log log, Exception ex)
                    {
                        log.Info("Foo", ex);
                    }
                }
                """
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogCodeFixTest<UseStringInterpolationAnalyzer, UseStringInterpolationCodeFixProvider>;
}
