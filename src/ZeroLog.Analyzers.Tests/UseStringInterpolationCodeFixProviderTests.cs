using System;
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

    private class Test : ZeroLogCodeFixTest<UseStringInterpolationAnalyzer, UseStringInterpolationCodeFixProvider>
    {
    }
}
