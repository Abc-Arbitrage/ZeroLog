using System;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Analyzers.IntegrationTest;

public static class Program
{
    private static readonly Log _logger = LogManager.GetLogger(typeof(Program));

    private static void Main()
    {
        using var logs = InitLogger();

        // Example log statements to verify that the analyzer code fixes work as expected in the IDE.

        _logger.Info("Hello, World!");

        _logger.Info(
            "Hello, World!",
            new Exception("Oops")
        );

        _logger.Info()
               .Append("Hello,")
               .Append(' ')
               .Append("World!")
               .Log();

        _logger.Info()
               .Append("Hello,")
               .Append(' ')
               .Append("World!")
               .WithException(new Exception("Oops"))
               .Log();
    }

    private static IDisposable InitLogger()
    {
        return LogManager.Initialize(new ZeroLogConfiguration
        {
            RootLogger = { Appenders = { new ConsoleAppender { ColorOutput = false } } }
        });
    }
}
