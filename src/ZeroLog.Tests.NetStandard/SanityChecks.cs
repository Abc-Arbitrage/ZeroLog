using System;
using System.Linq;
using NUnit.Framework;
using PublicApiGenerator;
using Shouldly;

namespace ZeroLog.Tests.NetStandard;

[TestFixture]
public class SanityChecks
{
    [Test]
    public void should_export_expected_namespaces()
    {
        typeof(LogManager).Assembly
                          .ExportedTypes
                          .Select(i => i.Namespace)
                          .Distinct()
                          .ShouldBe([
                              "ZeroLog"
                          ]);
    }

    [Test]
    public void should_export_expected_types()
    {
        var expected = new[]
        {
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
            "ZeroLog.LogMessage+InternalApi+Chain",
            "ZeroLog.LogMessage+InternalApi+ChainExecutionContext"
        };

        typeof(LogManager).Assembly
                          .ExportedTypes
                          .Select(i => i.FullName)
                          .Distinct()
                          .ShouldBe(expected, ignoreOrder: true);
    }

    [Test]
    public void should_have_expected_public_api()
    {
        typeof(LogManager).Assembly
                          .GeneratePublicApi(new ApiGeneratorOptions
                          {
                              IncludeAssemblyAttributes = false,
                              ExcludeAttributes =
                              [
                                  typeof(ObsoleteAttribute).FullName
                              ]
                          })
                          .ShouldMatchApproved(
                              cfg => cfg.WithFileExtension(".cs")
                                        .LocateTestMethodUsingAttribute<TestAttribute>()
                          );
    }
}
