using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using ZeroLog.Analyzers.Support;

namespace ZeroLog.Analyzers;

[Generator(LanguageNames.CSharp)]
public class InitializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var isEnabled = context.CompilationProvider
                               .Select(CompilationPredicate);

        var invocations = context.SyntaxProvider
                                 .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                 .WhereNotNull();

        var pipeline = invocations.Combine(isEnabled)
                                  .SelectMany(static (pair, _) =>
                                  {
                                      var (invocations, isEnabled) = pair;
                                      return isEnabled ? ImmutableArray.Create(invocations) : ImmutableArray<string>.Empty;
                                  })
                                  .Collect();

        context.RegisterImplementationSourceOutput(
            pipeline,
            static (context, loggerNames) =>
            {
                if (loggerNames.Length == 0)
                    return;

                context.AddSource(
                    "Initialization.g.cs",
                    new Generator().Generate(loggerNames)
                );
            }
        );
    }

    private static bool CompilationPredicate(Compilation compilation, CancellationToken cancellationToken)
    {
        // C# 11 is required for file-scoped types
        return compilation is CSharpCompilation { LanguageVersion: >= (LanguageVersion)1100 }
               && compilation.GetTypeByMetadataName(ZeroLogFacts.TypeNames.LogManager) is { } logManagerType
               && logManagerType.GetTypeMembers(ZeroLogFacts.ShortTypeNames.InternalApi) is [{ } internalApiType]
               && internalApiType.GetMembers(ZeroLogFacts.EventNames.Initialized) is
               [
                   IEventSymbol
                   {
                       Type: INamedTypeSymbol
                       {
                           Name: nameof(Action),
                           IsGenericType: false,
                           ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true }
                       }
                   }
               ];
    }

    private static bool SyntaxPredicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax i => i.Name.Identifier.ValueText == ZeroLogFacts.MethodNames.GetLogger,
            SimpleNameSyntax i             => i.Identifier.ValueText == ZeroLogFacts.MethodNames.GetLogger,
            _                              => false
        };
    }

    private static string? SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetOperation(context.Node, cancellationToken) is not IInvocationOperation invocation)
            return null;

        if (invocation.TargetMethod is not
            {
                IsStatic: true,
                Name: ZeroLogFacts.MethodNames.GetLogger,
                ContainingType:
                {
                    Name: ZeroLogFacts.ShortTypeNames.LogManager,
                    ContainingNamespace: { Name: ZeroLogFacts.Namespace, ContainingNamespace.IsGlobalNamespace: true }
                }
            } method)
        {
            return null;
        }

        switch (method.Parameters.Length)
        {
            case 0:
                if (method is { IsGenericMethod: true, TypeArguments.Length: 1 })
                    return GetFullName(method.TypeArguments[0]);

                break;

            case 1:
                var parameter = method.Parameters[0];
                var argument = invocation.Arguments.FirstOrDefault();

                if (parameter.Type.SpecialType == SpecialType.System_String)
                    return argument?.Value.ConstantValue is { HasValue: true, Value: string name } ? name : null;

                if (argument?.Value is ITypeOfOperation typeOf)
                    return GetFullName(typeOf.TypeOperand);

                break;
        }

        return null;
    }

    private static string? GetFullName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol type)
        {
            if (type.ContainingType is { } containingType)
            {
                return GetFullName(containingType) is { } containingTypeName
                    ? $"{containingTypeName}+{type.MetadataName}"
                    : null;
            }

            var ns = type.ContainingNamespace;

            return ns.IsGlobalNamespace
                ? type.MetadataName
                : $"{ns.ToDisplayString()}.{type.MetadataName}";
        }

        return null;
    }

    private class Generator
    {
        private const string _onInitializedMethodName = "OnInitialized";

        private readonly CodeWriter _writer = new();

        public string Generate(ImmutableArray<string> loggerNames)
        {
            _writer.Clear();

            AppendHeader();
            AppendAttributes();

            using (_writer.WriteBlock($"namespace {ZeroLogFacts.Namespace}"))
            using (_writer.WriteBlock("file static class ZeroLogGenerated"))
            {
                GenerateModuleInitializer();
                GenerateOnInitialized(loggerNames);
            }

            return _writer.ToString();
        }

        private void AppendHeader()
        {
            _writer.AppendLine(
                """
                // <auto-generated>
                //   This file was generated by ZeroLog to provide additional optimizations.
                // </auto-generated>

                #nullable enable

                """
            );
        }

        private void AppendAttributes()
        {
            _writer.AppendUnindentedLine("#if !NET5_0_OR_GREATER")
                   .AppendLine();

            using (_writer.WriteBlock("namespace System.Runtime.CompilerServices"))
            {
                _writer.AppendLine(
                    """
                    [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
                    file sealed class ModuleInitializerAttribute : global::System.Attribute;
                    """
                );
            }

            _writer.AppendLine()
                   .AppendUnindentedLine("#endif")
                   .AppendLine();
        }

        private void GenerateModuleInitializer()
        {
            _writer.AppendLine(
                $$"""
                [global::System.Runtime.CompilerServices.ModuleInitializer]
                public static void ModuleInitializer()
                {
                    try
                    {
                        global::{{ZeroLogFacts.TypeNames.LogManager}}.{{ZeroLogFacts.ShortTypeNames.InternalApi}}.{{ZeroLogFacts.EventNames.Initialized}} += {{_onInitializedMethodName}};
                    }
                    catch
                    {
                        // Ignore exceptions during module initialization, this is only an optimization.
                    }
                }

                """
            );
        }

        private void GenerateOnInitialized(ImmutableArray<string> loggerNames)
        {
            using (_writer.WriteBlock($"public static void {_onInitializedMethodName}()"))
            {
                using (_writer.WriteBlock("try"))
                {
                    foreach (var loggerName in loggerNames.Distinct().OrderBy(i => i, StringComparer.Ordinal))
                        _writer.AppendLine($"Prepare({SymbolDisplay.FormatLiteral(loggerName, true)});");
                }

                using (_writer.WriteBlock("catch"))
                {
                    _writer.AppendLine("// Ignore exceptions during initialization, this is only an optimization.");
                }

                _writer.AppendLine(
                    $"""

                    return;

                    static void Prepare(string loggerName)
                        => _ = global::{ZeroLogFacts.TypeNames.LogManager}.{ZeroLogFacts.MethodNames.GetLogger}(loggerName);
                    """
                );
            }
        }
    }
}
