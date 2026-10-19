#if ROSLYN5_0_OR_GREATER

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                            .Where(static i => i.Link.LinkType is not AppendChain.LinkType.None)
                            .WithLambdaComparer(static (a, b) => a.Link.Equals(b.Link), static i => i.Link.GetHashCode())
                            .Collect()
                            .Combine(context.IsGeneratorEnabled("AppendChains"))
                            .EmptyIfFalse()
                            .Combine(context.CompilationProvider.Select(SupportsGeneratedAppendChains))
                            .EmptyIfFalse()
                            .SelectMany(ToChains)
                            .Collect();

        context.RegisterImplementationSourceOutput(
            chains,
            static (context, chains) =>
            {
                if (chains.Length == 0)
                    return;

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

    private static ChainLinkModel SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
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
            && TryGetChainLink(invocation, context.SemanticModel, out var chainLink, cancellationToken)
           )
        {
            return chainLink;
        }

        return default;
    }

    private static bool TryGetChainLink(IInvocationOperation invocation,
                                        SemanticModel semanticModel,
                                        out ChainLinkModel chainLink,
                                        CancellationToken cancellationToken)
    {
        chainLink = default;

        if (!TryGetLink(invocation, semanticModel, out var link, cancellationToken))
            return false;

        var currentInvocation = invocation;
        var chainPosition = 0;

        var parentLink = default(LinkModel);

        while (TryGetLink(currentInvocation, semanticModel, out var currentLink, cancellationToken))
        {
            parentLink = currentLink;
            ++chainPosition;
            currentInvocation = currentInvocation.Instance as IInvocationOperation;
        }

        chainLink = new ChainLinkModel(
            link,
            parentLink,
            chainPosition - 1
        );

        return true;
    }

    private static bool TryGetLink([NotNullWhen(true)] IInvocationOperation? invocation,
                                   SemanticModel semanticModel,
                                   out LinkModel link,
                                   CancellationToken cancellationToken)
    {
        link = default;

        if (invocation is null)
            return false;

        if (!IsChainable(invocation.TargetMethod, out var linkType))
            return false;

        if (semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not { } linkLocation)
            return false;

        string? constantString;

        if (linkType is AppendChain.LinkType.String && invocation.GetArgumentByParameterIndex(0)?.Value.ConstantValue is { HasValue: true } constantValue)
        {
            switch (constantValue.Value)
            {
                case null:
                    linkType = AppendChain.LinkType.Null;
                    constantString = null;
                    break;

                case string value:
                    linkType = AppendChain.LinkType.ConstantString;
                    constantString = value;
                    break;

                default:
                    constantString = null;
                    break;
            }
        }
        else
        {
            constantString = null!;
        }

        link = new LinkModel(
            linkType,
            constantString,
            linkLocation
        );

        return true;
    }

    private static bool IsChainable(IMethodSymbol method, out AppendChain.LinkType linkType)
    {
        linkType = default;

        if (method is
            {
                Name: ZeroLogFacts.MethodNames.Append,
                ContainingType:
                {
                    Name: ZeroLogFacts.ShortTypeNames.LogMessage,
                    ContainingType: null,
                    ContainingNamespace: { Name: ZeroLogFacts.Namespace, ContainingNamespace.IsGlobalNamespace: true }
                },
                Parameters.Length: 1
            })
        {
            linkType = ToLinkType(method.Parameters[0].Type);
            return linkType is not AppendChain.LinkType.None;
        }

        return false;
    }

    private static ImmutableArray<ChainModel> ToChains(ImmutableArray<ChainLinkModel> chainLinks, CancellationToken cancellationToken)
    {
        return chainLinks.GroupBy(static i => i.FirstChainLink)
                         .Where(IsWorthIntercepting)
                         .Select(static i => new ChainModel(i.OrderBy(i => i.IndexInChain)
                                                             .Select(i => i.Link)
                                                             .ToImmutableArray()))
                         .ToImmutableArray();

        static bool IsWorthIntercepting(IGrouping<LinkModel, ChainLinkModel> links)
            => links.Count() > 1 && links.Any(static i => i.Link.LinkType is AppendChain.LinkType.ConstantString or AppendChain.LinkType.ConstantKeyString);
    }

    private static string SerializeChain(ImmutableArray<LinkModel> links)
    {
        var serializer = new AppendChain.Serializer();

        foreach (var link in links)
        {
            switch (link.LinkType)
            {
                case AppendChain.LinkType.ConstantString:
                    serializer.AddConstantString(link.ConstantString);
                    break;

                case AppendChain.LinkType.ConstantKeyString:
                    serializer.AddConstantKeyString(link.ConstantString);
                    break;

                default:
                    serializer.AddFixedSizeLink(link.LinkType);
                    break;
            }
        }

        return serializer.ToSerializedResult();
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
            AppendChain.LinkType.String            => "string?",
            AppendChain.LinkType.ConstantString    => "string?",
            AppendChain.LinkType.ConstantKeyString => "string",
            AppendChain.LinkType.VariableKeyString => "string",
            AppendChain.LinkType.Char              => "char",
            AppendChain.LinkType.Boolean           => "bool",
            AppendChain.LinkType.SByte             => "sbyte",
            AppendChain.LinkType.Byte              => "byte",
            AppendChain.LinkType.Int16             => "short",
            AppendChain.LinkType.UInt16            => "ushort",
            AppendChain.LinkType.Int32             => "int",
            AppendChain.LinkType.UInt32            => "uint",
            AppendChain.LinkType.Int64             => "long",
            AppendChain.LinkType.UInt64            => "ulong",
            AppendChain.LinkType.Single            => "float",
            AppendChain.LinkType.Double            => "double",
            AppendChain.LinkType.Decimal           => "decimal",
            _                                      => null
        };
    }

    private readonly record struct LinkModel(
        AppendChain.LinkType LinkType,
        string? ConstantString,
        InterceptableLocation InterceptableLocation
    );

    private readonly record struct ChainLinkModel(
        LinkModel Link,
        LinkModel FirstChainLink,
        int IndexInChain
    );

    private readonly record struct ChainModel
    {
        public ImmutableArray<LinkModel> Links { get; init; }
        public string SerializedChain { get; init; }

        public ChainModel(ImmutableArray<LinkModel> links)
        {
            Links = links;
            SerializedChain = SerializeChain(links);
        }

        public bool Equals(ChainModel other)
            => ImmutableArrayEqualityComparer<LinkModel>.Default.Equals(Links, other.Links);

        public override int GetHashCode()
            => ImmutableArrayEqualityComparer<LinkModel>.Default.GetHashCode(Links);
    }

    private class Generator
    {
        private readonly CodeWriter _writer = new();

        public string Generate(ImmutableArray<ChainModel> chains)
        {
            _writer.Clear();

            AppendHeader();

            using (_writer.WriteBlock($"namespace {ZeroLogFacts.Namespace}"))
            using (_writer.WriteBlock("file static class AppendChainsInterceptor"))
            {
                GenerateInterceptors(chains);
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

        private void GenerateInterceptors(ImmutableArray<ChainModel> chains)
        {
            var chainCounter = 0; // TODO: Find something better

            foreach (var chain in chains)
            {
                _writer.AppendLine($"// Chain {chainCounter}").AppendLine();

                for (var linkIndex = 0; linkIndex < chain.Links.Length; linkIndex++)
                {
                    var link = chain.Links[linkIndex];

                    _writer.AppendLine(
                        $"""
                        {link.InterceptableLocation.GetInterceptsLocationAttributeSyntax()}
                        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                        public static global::{ZeroLogFacts.FullTypeNames.LogMessage} Append_{chainCounter}_{linkIndex}(this global::{ZeroLogFacts.FullTypeNames.LogMessage} log, {ToTypeName(link.LinkType)} value)
                        """
                    );

                    if (linkIndex == 0)
                    {
                        using (_writer.WriteBlock())
                        {
                            _writer.AppendLine(
                                $"""
                                global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.StartChain}(log, {SymbolDisplay.FormatLiteral(chain.SerializedChain, true)});
                                return log.Append(value);
                                """
                            );
                        }
                    }
                    else
                    {
                        _writer.AppendLine("    => log.Append(value);");
                    }

                    _writer.AppendLine();
                }

                ++chainCounter;
            }
        }
    }
}

#endif
