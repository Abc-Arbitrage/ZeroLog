using System.Threading;
using System.Threading.Tasks;
using DiffEngine;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[SetUpFixture]
public static class Global
{
    [OneTimeSetUp]
    public static async Task GlobalSetup()
    {
        DiffRunner.Disabled = true;

        await Task.WhenAll(
            ReferenceAssemblies.NetStandard.NetStandard20.ResolveAsync(null, CancellationToken.None),
            ReferenceAssemblies.Net.Net100.ResolveAsync(null, CancellationToken.None)
        );
    }
}
