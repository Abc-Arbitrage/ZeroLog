using System;
using System.Collections.Generic;
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
        var loggerNames = context.SyntaxProvider
                                 .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                 .WhereNotNull()
                                 .Collect()
                                 .Combine(context.IsGeneratorEnabled("Initializer"))
                                 .EmptyIfFalse()
                                 .Combine(context.CompilationProvider.Select(SupportsGeneratedInitializer))
                                 .EmptyIfFalse()
                                 .Select(static (names, _) => LoggerNames.Normalize(names))
                                 .WithComparer(LoggerNames.Comparer);

        var hasModuleInitializerAttribute = context.CompilationProvider
                                                   .Select(static (i, _) => i.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") is not null);

        context.RegisterImplementationSourceOutput(
            loggerNames.Combine(hasModuleInitializerAttribute),
            static (context, pair) =>
            {
                var (loggerNames, hasModuleInitializerAttribute) = pair;

                if (loggerNames.Length == 0)
                    return;

                context.AddSource(
                    "Initializer.g.cs",
                    new Generator().Generate(loggerNames, hasModuleInitializerAttribute)
                );
            }
        );
    }

    private static bool SupportsGeneratedInitializer(Compilation compilation, CancellationToken cancellationToken)
    {
        // C# 11 is required for file-scoped types
        return compilation is CSharpCompilation { LanguageVersion: >= (LanguageVersion)1100 }
               && compilation.GetTypeByMetadataName(ZeroLogFacts.FullTypeNames.LogManager) is { } logManagerType
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

    private sealed class LoggerNames : IEqualityComparer<ImmutableArray<string>>
    {
        public static readonly LoggerNames Comparer = new();

        public static ImmutableArray<string> Normalize(ImmutableArray<string> names)
            => names.Distinct(StringComparer.Ordinal)
                    .OrderBy(static i => i, StringComparer.Ordinal)
                    .ToImmutableArray();

        public bool Equals(ImmutableArray<string> x, ImmutableArray<string> y)
            => x.SequenceEqual(y, StringComparer.Ordinal);

        public int GetHashCode(ImmutableArray<string> names)
        {
            var hash = 0;

            foreach (var name in names)
                hash = unchecked((hash * 397) ^ StringComparer.Ordinal.GetHashCode(name));

            return hash;
        }
    }

    private sealed class Generator
    {
        private const string _onInitializedMethodName = "OnInitialized";

        private readonly CodeWriter _writer = new();

        public string Generate(ImmutableArray<string> loggerNames, bool hasModuleInitializerAttribute)
        {
            _writer.Clear();

            AppendHeader();

            if (!hasModuleInitializerAttribute)
                AppendModuleInitializerAttribute();

            using (_writer.WriteBlock("file static class ZeroLogInitializer"))
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

        private void AppendModuleInitializerAttribute()
        {
            using (_writer.WriteBlock("namespace System.Runtime.CompilerServices"))
            {
                _writer.AppendLine(
                    """
                    [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
                    file sealed class ModuleInitializerAttribute : global::System.Attribute
                    { }
                    """
                );
            }

            _writer.AppendLine();
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
                        AttachEventHandler();
                    }
                    catch
                    {
                        // Ignore exceptions during module initialization, this is only an optimization.
                    }

                    return;

                    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    static void AttachEventHandler()
                        => global::{{ZeroLogFacts.FullTypeNames.LogManager}}.{{ZeroLogFacts.ShortTypeNames.InternalApi}}.{{ZeroLogFacts.EventNames.Initialized}} += {{_onInitializedMethodName}};
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
                    foreach (var loggerName in loggerNames)
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
                        => _ = global::{ZeroLogFacts.FullTypeNames.LogManager}.{ZeroLogFacts.MethodNames.GetLogger}(loggerName);
                    """
                );
            }
        }
    }
}
