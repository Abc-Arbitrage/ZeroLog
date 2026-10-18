#if ROSLYN5_0_OR_GREATER

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using ZeroLog.Analyzers.Support;

namespace ZeroLog.Analyzers;

[Generator(LanguageNames.CSharp)]
public class AppendChainsInterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var chains = context.SyntaxProvider
                            .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                            .Where(static i => i.LinkType is not AppendChain.LinkType.None)
                            .Collect();
        // .Combine(context.IsGeneratorEnabled("AppendChains"))
        // .EmptyIfFalse()
        // .Combine(context.CompilationProvider.Select(SupportsGeneratedAppendChains))
        // .EmptyIfFalse();

        context.RegisterImplementationSourceOutput(
            chains,
            static (context, chains) =>
            {
                // if (chains.Length == 0)
                //     return;

                context.AddSource(
                    "AppendChainsInterceptor.g.cs",
                    new Generator().Generate(chains)
                );
            }
        );
    }

    private static bool SupportsGeneratedAppendChains(Compilation compilation, CancellationToken cancellationToken)
    {
        // C# 11 is required for file-scoped types
        return compilation is CSharpCompilation { LanguageVersion: >= (LanguageVersion)1100 }
               && compilation.GetTypeByMetadataName(ZeroLogFacts.FullTypeNames.LogMessage) is { } logMessage
               && logMessage.GetTypeMembers(ZeroLogFacts.ShortTypeNames.InternalApi) is [{ } internalApiType];

        // TODO: Finish
    }

    private static bool SyntaxPredicate(SyntaxNode node, CancellationToken cancellationToken)
        => node.IsKind(SyntaxKind.InvocationExpression) && node is InvocationExpressionSyntax
        {
            InvokedMethodName: ZeroLogFacts.MethodNames.Append // TODO: Enums and key/values
        };

    private static AppendModel SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is InvocationExpressionSyntax invocationSyntax
            && context.SemanticModel.GetOperation(invocationSyntax, cancellationToken) is IInvocationOperation
            {
                TargetMethod:
                {
                    Name: ZeroLogFacts.MethodNames.Append,
                    ContainingType:
                    {
                        Name: ZeroLogFacts.ShortTypeNames.LogMessage,
                        ContainingType: null,
                        ContainingNamespace: { Name: ZeroLogFacts.Namespace, ContainingNamespace.IsGlobalNamespace: true }
                    }
                }
            } invocation
            && IsChainable(invocation, out var linkType)
            && context.SemanticModel.GetInterceptableLocation(invocationSyntax, cancellationToken) is { } interceptableLocation
           )
        {
            return new AppendModel(
                linkType,
                interceptableLocation
            );
        }

        return default;
    }

    private static bool IsChainable(IInvocationOperation invocation, out AppendChain.LinkType linkType)
    {
        if (!IsChainable(invocation.TargetMethod, out linkType))
            return false;

        return true;
    }

    private static bool IsChainable(IMethodSymbol method, out AppendChain.LinkType linkType)
    {
        linkType = default;

        if (method.Name != ZeroLogFacts.MethodNames.Append)
            return false;

        if (method.Parameters.Length != 1)
            return false;

        linkType = ToLinkType(method.Parameters[0].Type);
        return linkType is not AppendChain.LinkType.None;
    }

    private static AppendChain.LinkType ToLinkType(ITypeSymbol type)
    {
        var specialType = type.SpecialType switch
        {
            SpecialType.System_String  => AppendChain.LinkType.String,
            SpecialType.System_Char    => AppendChain.LinkType.Char,
            SpecialType.System_Boolean => AppendChain.LinkType.Boolean,
            SpecialType.System_SByte   => AppendChain.LinkType.SByte,
            SpecialType.System_Byte    => AppendChain.LinkType.Byte,
            SpecialType.System_Int16   => AppendChain.LinkType.Int16,
            SpecialType.System_UInt16  => AppendChain.LinkType.UInt16,
            SpecialType.System_Int32   => AppendChain.LinkType.Int32,
            SpecialType.System_UInt32  => AppendChain.LinkType.UInt32,
            SpecialType.System_Int64   => AppendChain.LinkType.Int64,
            SpecialType.System_UInt64  => AppendChain.LinkType.UInt64,
            SpecialType.System_Single  => AppendChain.LinkType.Single,
            SpecialType.System_Double  => AppendChain.LinkType.Double,
            SpecialType.System_Decimal => AppendChain.LinkType.Decimal,
            _                          => AppendChain.LinkType.None
        };

        if (specialType != AppendChain.LinkType.None)
            return specialType;

        // TODO: Handle other types

        return AppendChain.LinkType.None;
    }

    private static string? ToTypeName(AppendChain.LinkType linkType)
    {
        return linkType switch
        {
            AppendChain.LinkType.String  => "string?",
            AppendChain.LinkType.Char    => "char",
            AppendChain.LinkType.Boolean => "bool",
            AppendChain.LinkType.SByte   => "sbyte",
            AppendChain.LinkType.Byte    => "byte",
            AppendChain.LinkType.Int16   => "short",
            AppendChain.LinkType.UInt16  => "ushort",
            AppendChain.LinkType.Int32   => "int",
            AppendChain.LinkType.UInt32  => "uint",
            AppendChain.LinkType.Int64   => "long",
            AppendChain.LinkType.UInt64  => "ulong",
            AppendChain.LinkType.Single  => "float",
            AppendChain.LinkType.Double  => "double",
            AppendChain.LinkType.Decimal => "decimal",
            _                            => null
        };
    }

    private readonly record struct AppendModel(
        AppendChain.LinkType LinkType,
        InterceptableLocation InterceptableLocation
    );

    private class Generator
    {
        private readonly CodeWriter _writer = new();

        public string Generate(ImmutableArray<AppendModel> appends)
        {
            _writer.Clear();

            AppendHeader();

            using (_writer.WriteBlock($"namespace {ZeroLogFacts.Namespace}"))
            using (_writer.WriteBlock("file static class AppendChainsInterceptor"))
            {
                GenerateInterceptors(appends);
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

                namespace System.Runtime.CompilerServices
                {
                    [global::System.Diagnostics.Conditional("ZEROLOG_GENERATED")]
                    [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                    file sealed class InterceptsLocationAttribute : global::System.Attribute
                    {
                        public InterceptsLocationAttribute(int version, string data)
                        { }
                    }
                }

                """
            );
        }

        private void GenerateInterceptors(ImmutableArray<AppendModel> appends)
        {
            var counter = 0;

            foreach (var append in appends)
            {
                _writer.AppendLine(append.InterceptableLocation.GetInterceptsLocationAttributeSyntax());

                _writer.AppendLine(
                    $"""
                        public static global::{ZeroLogFacts.FullTypeNames.LogMessage} ReplaceAppend{++counter}(this global::{ZeroLogFacts.FullTypeNames.LogMessage} log, {ToTypeName(append.LinkType)} value)
                            => throw new global::System.NotImplementedException("TODO :)");

                        """
                );
            }
        }
    }
}

#endif
