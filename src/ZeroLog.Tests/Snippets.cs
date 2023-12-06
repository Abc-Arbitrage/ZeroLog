using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Tests;

[TestFixture]
public class Snippets
{
    #region GetLogger

    private static readonly Log _log = LogManager.GetLogger(typeof(YourClass));

    #endregion

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static void Init()
    {
        #region Initialize

        LogManager.Initialize(new ZeroLogConfiguration
        {
            RootLogger =
            {
                Appenders =
                {
                    new ConsoleAppender()
                }
            }
        });

        #endregion
    }

    [Test]
    public void StringInterpolationApi()
    {
        #region StringInterpolationApi

        var date = DateTime.Today.AddDays(1);
        _log.Info($"Tomorrow ({date:yyyy-MM-dd}) will be in {GetNumberOfSecondsUntilTomorrow():N0} seconds.");

        #endregion
    }

    [Test]
    public void StringBuilderApi()
    {
        #region StringBuilderApi

        _log.Info()
            .Append("Tomorrow (")
            .Append(DateTime.Today.AddDays(1), "yyyy-MM-dd")
            .Append(") will be in ")
            .Append(GetNumberOfSecondsUntilTomorrow(), "N0")
            .Append(" seconds.")
            .Log();

        #endregion
    }

    [Test]
    public void StructuredData()
    {
        #region StructuredData

        _log.Info()
            .Append("Tomorrow is another day.")
            .AppendKeyValue("NumSecondsUntilTomorrow", GetNumberOfSecondsUntilTomorrow())
            .Log();

        #endregion
    }

    private static int GetNumberOfSecondsUntilTomorrow()
        => 10;

    private class YourClass;
}
