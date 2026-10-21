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
                            .Where(static i => i.AppendCall.Type is not AppendedType.None)
                            .WithLambdaComparer(static (a, b) => a.AppendCall.Equals(b.AppendCall), static i => i.AppendCall.GetHashCode())
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
                    "AppendChains.g.cs",
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

    private static ChainedAppendsModel SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
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
            && TryGetChainedAppends(invocation, context.SemanticModel, out var chainedAppends, cancellationToken)
           )
        {
            return chainedAppends;
        }

        return default;
    }

    private static bool TryGetChainedAppends(IInvocationOperation invocation,
                                             SemanticModel semanticModel,
                                             out ChainedAppendsModel chainedAppends,
                                             CancellationToken cancellationToken)
    {
        chainedAppends = default;

        if (!TryGetAppendCall(invocation, semanticModel, out var call, cancellationToken))
            return false;

        var currentInvocation = invocation;
        var chainPosition = 0;

        var previousCall = default(AppendCallModel);

        while (TryGetAppendCall(currentInvocation, semanticModel, out var currentCall, cancellationToken))
        {
            previousCall = currentCall;
            ++chainPosition;
            currentInvocation = currentInvocation.Instance as IInvocationOperation;
        }

        chainedAppends = new ChainedAppendsModel(
            call,
            previousCall,
            chainPosition - 1
        );

        return true;
    }

    private static bool TryGetAppendCall([NotNullWhen(true)] IInvocationOperation? invocation,
                                         SemanticModel semanticModel,
                                         out AppendCallModel appendCall,
                                         CancellationToken cancellationToken)
    {
        appendCall = default;

        if (invocation is null)
            return false;

        if (!IsChainableInvocation(
                invocation,
                out var appendedType,
                out var underlyingTypeName,
                out var isNullableValueType,
                out var formatString
            ))
        {
            return false;
        }

        if (semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not { } appendCallLocation)
            return false;

        var constantString = default(string?);

        if (appendedType is AppendedType.String && invocation.GetArgumentByParameterIndex(0)?.Value.ConstantValue is { HasValue: true } constantValue)
        {
            switch (constantValue.Value)
            {
                case null:
                    appendedType = AppendedType.Null;
                    constantString = null;
                    break;

                case string value:
                    appendedType = AppendedType.ConstantString;
                    constantString = value;
                    break;
            }
        }

        // Consistency checks
        if (constantString is not null && formatString is not null)
            return false;

        appendCall = new AppendCallModel(
            appendedType,
            isNullableValueType,
            underlyingTypeName,
            constantString,
            FormatStringType.None,
            formatString,
            appendCallLocation
        );

        return true;
    }

    private static bool IsChainableInvocation(IInvocationOperation invocation,
                                              out AppendedType appendedType,
                                              [NotNullWhen(true)] out string? underlyingTypeName,
                                              out bool isNullableType,
                                              out string? formatString)
    {
        appendedType = AppendedType.None;
        isNullableType = false;
        underlyingTypeName = null;
        formatString = null;

        if (invocation.TargetMethod is
            {
                Name: ZeroLogFacts.MethodNames.Append,
                ContainingType:
                {
                    Name: ZeroLogFacts.ShortTypeNames.LogMessage,
                    ContainingType: null,
                    ContainingNamespace: { Name: ZeroLogFacts.Namespace, ContainingNamespace.IsGlobalNamespace: true }
                },
                Parameters.Length: 1
            } method)
        {
            if (method.Parameters[0].Type is not { } valueParameterType)
                return false;

            underlyingTypeName = GetSupportedTypeName(valueParameterType, out appendedType, out isNullableType);
            return underlyingTypeName is not null;
        }

        return false;
    }

    private static string? GetSupportedTypeName(ITypeSymbol type,
                                                out AppendedType appendedType,
                                                out bool isNullableType)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            appendedType = AppendedType.None;
            isNullableType = false;
            return null;
        }

        var isNullableValueType = namedType.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullableValueType ? namedType.TypeArguments[0] : namedType;
        isNullableType = isNullableValueType || namedType.IsReferenceType;

        if (underlyingType.SpecialType is SpecialType.System_String)
        {
            appendedType = AppendedType.String;
            return "string";
        }

        var fromSpecialType = underlyingType.SpecialType switch
        {
            SpecialType.System_Char     => "char",
            SpecialType.System_Boolean  => "bool",
            SpecialType.System_SByte    => "sbyte",
            SpecialType.System_Byte     => "byte",
            SpecialType.System_Int16    => "short",
            SpecialType.System_UInt16   => "ushort",
            SpecialType.System_Int32    => "int",
            SpecialType.System_UInt32   => "uint",
            SpecialType.System_Int64    => "long",
            SpecialType.System_UInt64   => "ulong",
            SpecialType.System_Single   => "float",
            SpecialType.System_Double   => "double",
            SpecialType.System_Decimal  => "decimal",
            SpecialType.System_DateTime => "global::System.DateTime",
            _                           => null
        };

        if (fromSpecialType is not null)
        {
            appendedType = AppendedType.ValueType;
            return fromSpecialType;
        }

        var fullNameType = underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        switch (fullNameType)
        {
            case "global::System.Guid":
            case "global::System.TimeSpan":
            case "global::System.DateTimeOffset":
            case "global::System.DateOnly":
            case "global::System.TimeOnly":
                appendedType = AppendedType.ValueType;
                return fullNameType;

            default:
                appendedType = AppendedType.None;
                return null;
        }
    }

    private static ImmutableArray<ChainModel> ToChains(ImmutableArray<ChainedAppendsModel> chainItems, CancellationToken cancellationToken)
    {
        return chainItems.GroupBy(static i => i.FirstAppendCall)
                         .Where(IsWorthIntercepting)
                         .Select(static i => new ChainModel(i.OrderBy(i => i.IndexInChain)
                                                             .Select(i => i.AppendCall)
                                                             .ToImmutableArray()))
                         .ToImmutableArray();

        static bool IsWorthIntercepting(IGrouping<AppendCallModel, ChainedAppendsModel> call)
            => call.Count() > 1 && call.Any(static i => i.AppendCall.Type is AppendedType.ConstantString);
    }

    private enum AppendedType
    {
        None,
        Null,
        String,
        ConstantString,
        ValueType
    }

    private enum FormatStringType
    {
        None,
        Constant,
        Variable
    }

    private readonly record struct AppendCallModel(
        AppendedType Type,
        bool IsNullableType,
        string UnderlyingTypeName,
        string? ConstantString,
        FormatStringType FormatStringType,
        string? ConstantFormatString,
        InterceptableLocation InterceptableLocation
    );

    private readonly record struct ChainedAppendsModel(
        AppendCallModel AppendCall,
        AppendCallModel FirstAppendCall,
        int IndexInChain
    );

    private readonly record struct ChainModel
    {
        public ImmutableArray<AppendCallModel> Calls { get; }

        public ChainModel(ImmutableArray<AppendCallModel> calls)
        {
            Calls = calls;
        }

        public bool Equals(ChainModel other)
            => ImmutableArrayEqualityComparer<AppendCallModel>.Default.Equals(Calls, other.Calls);

        public override int GetHashCode()
            => ImmutableArrayEqualityComparer<AppendCallModel>.Default.GetHashCode(Calls);
    }

    private class Generator
    {
        private readonly CodeWriter _writer = new();

        public string Generate(ImmutableArray<ChainModel> chains)
        {
            _writer.Clear();

            AppendHeader();

            using (_writer.WriteBlock($"namespace {ZeroLogFacts.Namespace}"))
            {
                for (var i = 0; i < chains.Length; ++i)
                {
                    GenerateChainClass(chains[i], i);
                    _writer.AppendLine();
                }

                using (_writer.WriteBlock("file static class ChainsInterceptor"))
                {
                    GenerateInterceptors(chains);
                }
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

        private void GenerateChainClass(ChainModel chain, int chainIndex)
        {
            _writer.AppendLine(
                $"""
                file sealed class Chain{chainIndex} : global::ZeroLog.LogMessage.InternalApi.Chain
                """
            );

            using (_writer.WriteBlock())
            {
                _writer.AppendLine($"public static readonly Chain{chainIndex} Instance = new();")
                       .AppendLine();

                using (_writer.WriteBlock("protected override void Execute(ref global::ZeroLog.LogMessage.InternalApi.ChainExecutionContext context)"))
                {
                    foreach (var call in chain.Calls)
                    {
                        switch (call.Type)
                        {
                            case AppendedType.Null:
                                _writer.AppendLine("context.OutputString(null);");
                                continue;

                            case AppendedType.String:
                                _writer.AppendLine("context.OutputString();");
                                continue;

                            case AppendedType.ConstantString:
                                _writer.AppendLine($"context.OutputString({SymbolDisplay.FormatLiteral(call.ConstantString ?? string.Empty, true)});");
                                continue;

                            case AppendedType.ValueType:
                                if (call.IsNullableType)
                                {
                                    // TODO
                                }

                                _writer.AppendLine($"context.OutputValueType<{call.UnderlyingTypeName}>();");
                                break;
                        }
                    }
                }
            }
        }

        private void GenerateInterceptors(ImmutableArray<ChainModel> chains)
        {
            var chainIndex = 0;

            foreach (var chain in chains)
            {
                _writer.AppendLine($"// Chain {chainIndex}").AppendLine();

                for (var callIndex = 0; callIndex < chain.Calls.Length; callIndex++)
                {
                    var call = chain.Calls[callIndex];

                    _writer.AppendLine(
                        $"""
                            {call.InterceptableLocation.GetInterceptsLocationAttributeSyntax()}
                            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                            public static global::{ZeroLogFacts.FullTypeNames.LogMessage} Append_{chainIndex}_{callIndex}(this global::{ZeroLogFacts.FullTypeNames.LogMessage} log, {call.UnderlyingTypeName}{(call.IsNullableType ? "?" : "")} value{(call.FormatStringType is not FormatStringType.None ? ", string format" : "")})
                            """
                    );

                    using (_writer.WriteBlock())
                    {
                        if (callIndex == 0)
                            _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.AppendChain}(log, Chain{chainIndex}.Instance);");

                        if (call.Type is AppendedType.String or AppendedType.ValueType)
                            _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.AppendChainItem}(log, value);");

                        _writer.AppendLine("return log;");
                    }

                    _writer.AppendLine();
                }

                ++chainIndex;
                _writer.AppendLine();
            }
        }
    }
}

#endif
