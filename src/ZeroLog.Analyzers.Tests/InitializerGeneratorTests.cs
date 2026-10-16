using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class InitializerGeneratorTests
{
    [OneTimeSetUp]
    public async Task SetUp()
    {
        await Task.WhenAll(
            ReferenceAssemblies.NetStandard.NetStandard20.ResolveAsync(null, CancellationToken.None),
            ReferenceAssemblies.Net.Net100.ResolveAsync(null, CancellationToken.None)
        );
    }

    [Test]
    public async Task should_generate_initializer_for_all_overloads()
    {
        await Verify(
            """
            using System;
            using ZeroLog;

            namespace Test.Namespace;

            class Example
            {
                static void Initialize()
                {
                    _ = LogManager.GetLogger<DateTime>();
                    _ = LogManager.GetLogger(typeof(TimeSpan));
                    _ = LogManager.GetLogger(@"Logger""Test""Name");
                    _ = LogManager.GetLogger(typeof(Outer));
                    _ = LogManager.GetLogger(typeof(Outer.Nested));
                }
            }

            class Outer
            {
                public class Nested { }
            }
            """
        );
    }

    [Test]
    public async Task should_ignore_non_constant_and_unrelated_calls()
    {
        await Verify(
            """
            using ZeroLog;

            class Example
            {
                static void Initialize(string name)
                {
                    _ = LogManager.GetLogger("Constant");
                    _ = LogManager.GetLogger(name);
                    _ = OtherLogManager.GetLogger("Ignored");
                }
            }

            static class OtherLogManager
            {
                public static object GetLogger(string name)
                    => new();
            }
            """
        );
    }

    [Test]
    public async Task should_generate_module_initializer()
    {
        await Verify(
            """
            using ZeroLog;

            class Example
            {
                static Log _log = LogManager.GetLogger("Foo");
            }
            """,
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20
        );
    }

    private static async Task Verify([StringSyntax("csharp")] string source,
                                     ReferenceAssemblies? referenceAssemblies = null)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp11);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            [
                .. await (referenceAssemblies ?? ReferenceAssemblies.Net.Net100).ResolveAsync(null, CancellationToken.None),
                MetadataReference.CreateFromFile(typeof(LogManager).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        compilation.GetDiagnostics().ShouldBeEmpty();

        var driver = CSharpGeneratorDriver.Create(
            [new InitializerGenerator().AsSourceGenerator()],
            parseOptions: parseOptions
        );

        var result = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics)
                           .GetRunResult();

        var syntaxTree = result.GeneratedTrees.ShouldHaveSingleItem();
        var generated = (await syntaxTree.GetTextAsync()).ToString();

        generated.ShouldMatchApproved(
            cfg => cfg.WithFileExtension(".cs")
                      .LocateTestMethodUsingAttribute<TestAttribute>()
        );

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics().ShouldBeEmpty();
    }
}
