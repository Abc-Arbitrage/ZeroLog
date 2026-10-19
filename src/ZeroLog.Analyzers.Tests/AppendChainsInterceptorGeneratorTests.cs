#if ROSLYN5_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Shouldly;
using ZeroLog.Analyzers.Tests.Support;

namespace ZeroLog.Analyzers.Tests;

[TestFixture]
public class AppendChainsInterceptorGeneratorTests
{
    [Test]
    public async Task should_generate_append_chains_interceptor()
    {
        await Verify(
            """
            using ZeroLog;

            namespace Test.Namespace;

            class Example
            {
                LogMessage ChainWithInt(LogMessage log)
                    => log.Append("The answer is ").Append(42).Append(", as expected.");

                LogMessage ChainWithString(LogMessage log, string value)
                    => log.Append("The answer is ").Append(value).Append(", as expected.");

                LogMessage SingleAppend(LogMessage log)
                    => log.Append("Hello!");

                LogMessage Mergeable(LogMessage log)
                    => log.Append("Hello, ").Append("World!");

                LogMessage Null(LogMessage log)
                    => log.Append("Hello, ").Append((string)null);

                LogMessage Nullables(LogMessage log, int a, int? b)
                    => log.Append("Hello, ").Append(a).Append(", ").Append(b);

                LogMessage StartsWithInt(LogMessage log, int a, int? b)
                    => log.Append(a).Append("Hello, ").Append(a).Append(", ").Append(b);
            }
            """
        );
    }

    private static async Task Verify([StringSyntax("csharp")] string source)
    {
        var parseOptions = CSharpParseOptions.Default
                                             .WithLanguageVersion(LanguageVersion.CSharp11)
                                             .WithFeatures([new("InterceptorsNamespaces", "ZeroLog")]);

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            [
                .. await ReferenceAssemblies.Net.Net100.ResolveAsync(null, CancellationToken.None),
                MetadataReference.CreateFromFile(typeof(LogManager).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        compilation.GetDiagnostics().ShouldBeEmpty();

        var driver = CSharpGeneratorDriver.Create(
            [new AppendChainsInterceptorGenerator().AsSourceGenerator()],
            parseOptions: parseOptions,
            optionsProvider: new AnalyzerConfigOptionsProviderMock
            {
                { "build_property.ZeroLogEnabledGenerators", "AppendChains" }
            }
        );

        var result = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics)
                           .GetRunResult();

        var syntaxTree = result.GeneratedTrees.ShouldHaveSingleItem();
        var generated = (await syntaxTree.GetTextAsync()).ToString();

        Console.WriteLine(generated);

        // TODO: Uncomment when stable
        // generated.ShouldMatchApproved(
        //     cfg => cfg.WithFileExtension(".cs")
        //               .LocateTestMethodUsingAttribute<TestAttribute>()
        // );

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics().ShouldBeEmpty();
    }
}

#endif
