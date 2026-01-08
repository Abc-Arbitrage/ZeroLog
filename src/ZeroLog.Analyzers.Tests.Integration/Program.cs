using System;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Analyzers.Tests.Integration;

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

        // -----

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

        Trivia();
    }

    private static void Trivia()
    {
        // @formatter:off

        /* 01 */ _logger.Info /* 02 */ (   /* 03 */             // 04
        /* 05 */     "Hello, World!"       /* 06 */ , /* 07 */  // 08
        /* 09 */     new Exception("Oops") /* 10 */             // 11
        /* 12 */ ) /* 13 */ ; /* 14 */                          // 15

        /* 01 */ _logger.Info /* 02 */ (   /* 03 */             // 04
        /* 05 */     "Hello, World!"       /* 06 */ , /* 07 */  // 08
        /* 09 */     new Exception("Oops") /* 10 */             // 11
        /* 12 */ ) /* 13 */ ; /* 14 */                          // 15

        // -----

        /* 01 */ _logger.Info /* 02 */ ( /* 03 */ ) /* 04 */                                // 05
        /* 06 */        .Append( /* 07 */ "Hello, World!" /* 08 */ ) /* 09 */               // 10
        /* 11 */        .WithException( /* 12 */ new Exception("Oops") /* 13 */ ) /* 14 */  // 15
        /* 16 */        .Log( /* 17 */ ) /* 18 */; /* 19 */                                 // 20

        /* 01 */ _logger.Info /* 02 */ ( /* 03 */ ) /* 04 */                                // 05
        /* 06 */        .Append( /* 07 */ "Hello, World!" /* 08 */ ) /* 09 */               // 10
        /* 11 */        .WithException( /* 12 */ new Exception("Oops") /* 13 */ ) /* 14 */  // 15
        /* 16 */        .Log( /* 17 */ ) /* 18 */; /* 19 */                                 // 20

        // @formatter:on
    }

    private static IDisposable InitLogger()
    {
        return LogManager.Initialize(new ZeroLogConfiguration
        {
            RootLogger = { Appenders = { new ConsoleAppender { ColorOutput = false } } }
        });
    }
}
