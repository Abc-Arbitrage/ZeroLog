using System;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Analyzers.IntegrationTest;

public static class Program
{
    private static readonly Log _log = LogManager.GetLogger(typeof(Program));

    private static void Main()
    {
        using var logs = LogManager.Initialize(new ZeroLogConfiguration
        {
            RootLogger = { Appenders = { new ConsoleAppender() } }
        });

        _log.Info("Hello, ZeroLog!", new Exception("lol"));

        _log.Info().Append("Hello, ZeroLog!").Log();
    }
}
