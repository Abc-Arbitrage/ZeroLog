using System;
using System.Linq;
using NUnit.Framework;
using PublicApiGenerator;
using Shouldly;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class SanityChecks
{
    [Test]
    public void should_export_expected_namespaces()
    {
        var expected = new[]
        {
            "ZeroLog",
            "ZeroLog.Appenders",
            "ZeroLog.Configuration",
            "ZeroLog.Formatting"
        };

        typeof(LogManager).Assembly
                          .ExportedTypes
                          .Select(i => i.Namespace)
                          .OrderBy(i => i)
                          .Distinct()
                          .ShouldBe(expected);
    }

    [Test]
    public void should_export_expected_types()
    {
        var expected = new[]
        {
            "ZeroLog.Appenders.Appender",
            "ZeroLog.Appenders.ConsoleAppender",
            "ZeroLog.Appenders.DateAndSizeRollingFileAppender",
            "ZeroLog.Appenders.NoopAppender",
            "ZeroLog.Appenders.StreamAppender",
            "ZeroLog.Appenders.TextWriterAppender",
            "ZeroLog.Configuration.AppenderConfiguration",
            "ZeroLog.Configuration.AppendingStrategy",
            "ZeroLog.Configuration.ILoggerConfigurationCollection",
            "ZeroLog.Configuration.LoggerConfiguration",
            "ZeroLog.Configuration.LogMessagePoolExhaustionStrategy",
            "ZeroLog.Configuration.RootLoggerConfiguration",
            "ZeroLog.Configuration.ThreadConfiguration",
            "ZeroLog.Configuration.ZeroLogConfiguration",
            "ZeroLog.Formatting.DefaultFormatter",
            "ZeroLog.Formatting.DefaultStyle",
            "ZeroLog.Formatting.DefaultStyle+Colored",
            "ZeroLog.Formatting.DefaultStyle+NoColor",
            "ZeroLog.Formatting.Formatter",
            "ZeroLog.Formatting.KeyValueList",
            "ZeroLog.Formatting.KeyValueList+Enumerator",
            "ZeroLog.Formatting.LoggedKeyValue",
            "ZeroLog.Formatting.LoggedMessage",
            "ZeroLog.Formatting.PatternWriter",
            "ZeroLog.Formatting.PatternWriter+LogLevelColorCodes",
            "ZeroLog.Formatting.PatternWriter+LogLevelNames",
            "ZeroLog.Formatting.PatternWriter+PatternAttribute",
            "ZeroLog.Log",
            "ZeroLog.Log+DebugInterpolatedStringHandler",
            "ZeroLog.Log+ErrorInterpolatedStringHandler",
            "ZeroLog.Log+FatalInterpolatedStringHandler",
            "ZeroLog.Log+InfoInterpolatedStringHandler",
            "ZeroLog.Log+TraceInterpolatedStringHandler",
            "ZeroLog.Log+WarnInterpolatedStringHandler",
            "ZeroLog.LogLevel",
            "ZeroLog.LogManager",
            "ZeroLog.LogManager+InternalApi",
            "ZeroLog.LogMessage",
            "ZeroLog.LogMessage+AppendInterpolatedStringHandler",
            "ZeroLog.LogMessage+AppendOperation`1",
            "ZeroLog.LogMessage+InternalApi",
            "ZeroLog.UnmanagedFormatterDelegate`1"
        };

        typeof(LogManager).Assembly
                          .ExportedTypes
                          .Select(i => i.FullName)
                          .OrderBy(i => i)
                          .Distinct()
                          .ShouldBe(expected);
    }

    [Test]
    public void should_reference_expected_assemblies()
    {
        typeof(LogManager).Assembly
                          .GetReferencedAssemblies()
                          .Select(i => i.FullName)
                          .OrderBy(i => i)
                          .Join(Environment.NewLine)
                          .ShouldMatchApproved(
                              cfg => cfg.LocateTestMethodUsingAttribute<TestAttribute>()
                                        .WithDiscriminator(typeof(SanityChecks).Assembly.GetMetadataValue("TargetFramework") ?? throw new InvalidOperationException("TargetFramework metadata not found"))
                          );
    }

    [Test]
    public void should_have_expected_public_api()
    {
        typeof(LogManager).Assembly
                          .GeneratePublicApi(new ApiGeneratorOptions
                          {
                              IncludeAssemblyAttributes = false,
                              ExcludeTypes =
                              [
                                  typeof(DefaultStyle)
                              ]
                          })
                          .ShouldMatchApproved(
                              cfg => cfg.WithFileExtension(".cs")
                                        .LocateTestMethodUsingAttribute<TestAttribute>()
                                        .WithDiscriminator(typeof(SanityChecks).Assembly.GetMetadataValue("TargetFramework") ?? throw new InvalidOperationException("TargetFramework metadata not found"))
                          );
    }

    [Test]
    public void should_have_generators_enabled_with_tfm_alias()
    {
        const string loggerName = "Test if using _gen TFM";

        var shouldBeAutoRegistered = typeof(SanityChecks).Assembly
                                                         .GetMetadataValue("TargetFramework")
                                                         .ShouldNotBeNull()
                                                         .Contains("_gen");

        LogManager.IsLoggerRegistered(loggerName)
                  .ShouldBe(shouldBeAutoRegistered);

        if (Guid.NewGuid() == Guid.Empty)
            LogManager.GetLogger(loggerName);
    }
}
