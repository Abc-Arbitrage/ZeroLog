using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PublicApiGenerator;
using VerifyNUnit;

namespace ZeroLog.Tests.NetStandard;

[TestFixture]
public class SanityChecks
{
    [Test]
    public Task should_export_expected_namespaces()
    {
        return Verifier.Verify(
            typeof(LogManager).Assembly
                              .ExportedTypes
                              .Select(i => i.Namespace)
                              .OrderBy(i => i)
                              .Distinct()
        );
    }

    [Test]
    public Task should_export_expected_types()
    {
        return Verifier.Verify(
            typeof(LogManager).Assembly
                              .ExportedTypes
                              .Select(i => i.FullName)
                              .OrderBy(i => i)
                              .Distinct()
        );
    }

    [Test]
    public Task should_have_expected_public_api()
    {
        return Verifier.Verify(
            typeof(LogManager).Assembly
                              .GeneratePublicApi(new ApiGeneratorOptions
                              {
                                  IncludeAssemblyAttributes = false,
                                  ExcludeAttributes = new[]
                                  {
                                      typeof(ObsoleteAttribute).FullName
                                  }
                              })
        );
    }
}
