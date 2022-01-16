using System.IO;
using Microsoft.CodeAnalysis.Testing;

namespace ZeroLog.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    public static readonly ReferenceAssemblies Net6ReferenceAssemblies = new(
        "net6.0",
        new PackageIdentity("Microsoft.NETCore.App.Ref", "6.0.0"),
        Path.Combine("ref", "net6.0")
    );
}
