using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
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
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}()
           .Append(""Foo"")
           .Append(""Bar:\n"")
           .Append(' ')
           .Append(42)
           .Append(Guid.NewGuid(), ""B"")
           .AppendEnum(DayOfWeek.Friday)
           .Append("" Baz"")
           .Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""FooBar:\n {42}{Guid.NewGuid():B}{DayOfWeek.Friday} Baz"");
    }
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
    public Task should_keep_external_trivia()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        /* start trivia */ log.{|#0:Info|}().Append(""Foo "").Append( /* foo */ 42 /* bar */ ).Log() /* end trivia */ ;
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        /* start trivia */ log.Info($""Foo {42}"") /* end trivia */ ;
    }
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
    public Task should_transform_out_of_order_format_argument()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(format: ""X"", value: 42).Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""{42:X}"");
    }
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
    public Task should_transform_braces()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(""Foo{Bar}Baz"").Append(42).Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""Foo{{Bar}}Baz{42}"");
    }
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
    public Task should_handle_verbatim_format_string()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(42, @""Foo""""\"").Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""{42:Foo\""\\}"");
    }
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
    public Task should_flatten_interpolated_appends()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(""Foo"").Append($""Bar {42} Baz"").Append(""!"").Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""FooBar {42} Baz!"");
    }
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
    public Task should_treat_verbatim_strings_as_expressions()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(""Foo"").Append(@""Bar"").Append(""Baz"").Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info($""Foo{@""Bar""}Baz"");
    }
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
    public Task should_output_string_literals_when_possible()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Append(""Foo"").Append(""Bar"").Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info(""FooBar"");
    }
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
    public Task should_handle_empty_log_message()
    {
        var test = new Test
        {
            TestCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.{|#0:Info|}().Log();
    }
}
",
            FixedCode = @"
using System;

class C
{
    void M(ZeroLog.Log log)
    {
        log.Info("""");
    }
}
",
            ExpectedDiagnostics =
            {
                new DiagnosticResult(UseStringInterpolationAnalyzer.UseStringInterpolationDiagnostic).WithLocation(0)
            }
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogCodeFixTest<UseStringInterpolationAnalyzer, UseStringInterpolationCodeFixProvider>
    {
    }
}
