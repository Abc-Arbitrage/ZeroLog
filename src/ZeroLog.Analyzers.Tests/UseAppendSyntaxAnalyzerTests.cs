using System.Threading.Tasks;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class UseAppendSyntaxAnalyzerTests
{
    [Test]
    public Task should_detect_message_only()
    {
        var test = new Test
        {
            TestCode = """
                class C
                {
                    void M(ZeroLog.Log log)
                    {
                        log.[|Debug|]("Foo");
                    }
                }
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_detect_interpolated_message()
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
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_detect_message_and_exception()
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
                """
        };

        return test.RunAsync();
    }

    [Test]
    public Task should_detect_message_and_exception_reordered()
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
                """
        };

        return test.RunAsync();
    }

    private class Test : ZeroLogAnalyzerTest<UseAppendSyntaxAnalyzer>;
}
