#if ROSLYN5_0_OR_GREATER

using System.Collections.Generic;
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
        => node.IsKind(SyntaxKind.InvocationExpression)
           && node is InvocationExpressionSyntax { InvokedMethodName: { } methodName }
           && IsPotentiallyChainableMethodName(methodName);

    private static ChainedAppendsModel SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is InvocationExpressionSyntax invocationSyntax
            && context.SemanticModel.GetOperation(invocationSyntax, cancellationToken) is IInvocationOperation invocation
            && IsPotentiallyChainableMethod(invocation.TargetMethod)
            && TryGetChainedAppends(invocation, context.SemanticModel, out var chainedAppends, cancellationToken))
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
                out var keyStringType,
                out var keyString,
                out var formatStringType,
                out var formatString
            ))
        {
            return false;
        }

        if (semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not { } appendCallLocation)
            return false;

        var constantString = default(string?);
        var valueParameterIndex = keyString is null ? 0 : 1;

        if (appendedType is AppendedType.String && invocation.GetArgumentByParameterIndex(valueParameterIndex)?.Value.ConstantValue is { HasValue: true } constantValue)
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
            keyStringType,
            keyString,
            formatStringType,
            formatString,
            appendCallLocation
        );

        return true;
    }

    private static bool IsPotentiallyChainableMethod(IMethodSymbol method)
        => IsPotentiallyChainableMethodName(method.Name)
           && IsLogMessageType(method.ContainingType);

    private static bool IsPotentiallyChainableMethodName(string methodName)
        => methodName is ZeroLogFacts.MethodNames.Append or ZeroLogFacts.MethodNames.AppendKeyValue;

    private static bool IsLogMessageType(ITypeSymbol type)
        => type is
        {
            Name: ZeroLogFacts.ShortTypeNames.LogMessage,
            ContainingType: null,
            ContainingNamespace: { Name: ZeroLogFacts.Namespace, ContainingNamespace.IsGlobalNamespace: true }
        };

    private static bool IsChainableInvocation(IInvocationOperation invocation,
                                              out AppendedType appendedType,
                                              [NotNullWhen(true)] out string? underlyingTypeName,
                                              out bool isNullableType,
                                              out ProvidedStringType keyStringType,
                                              out string? keyString,
                                              out ProvidedStringType formatStringType,
                                              out string? formatString)
    {
        appendedType = AppendedType.None;
        isNullableType = false;
        underlyingTypeName = null;
        keyStringType = ProvidedStringType.None;
        keyString = null;
        formatStringType = ProvidedStringType.None;
        formatString = null;

        var method = invocation.TargetMethod;
        if (!IsPotentiallyChainableMethod(method))
            return false;

        switch (method.Name)
        {
            case ZeroLogFacts.MethodNames.Append when method.Parameters.Length is 1 or 2:
            {
                if (method.Parameters[0].Type is not { } valueParameterType)
                    return false;

                underlyingTypeName = GetSupportedTypeName(valueParameterType, out appendedType, out isNullableType);
                if (underlyingTypeName is null)
                    return false;

                if (method.Parameters.Length == 2)
                {
                    if (method.Parameters[1].Type.SpecialType != SpecialType.System_String)
                        return false;

                    formatStringType = invocation.GetArgumentByParameterIndex(1)?.TryGetConstantString(out formatString) is true
                        ? ProvidedStringType.Constant
                        : ProvidedStringType.Variable;
                }

                return true;
            }

            case ZeroLogFacts.MethodNames.AppendKeyValue when method.Parameters.Length is 2:
            {
                if (method.Parameters[0].Type.SpecialType is not SpecialType.System_String)
                    return false;

                if (method.Parameters[1].Type is not { } valueParameterType)
                    return false;

                underlyingTypeName = GetSupportedTypeName(valueParameterType, out appendedType, out isNullableType);
                if (underlyingTypeName is null)
                    return false;

                keyStringType = invocation.GetArgumentByParameterIndex(0)?.TryGetConstantString(out keyString) is true
                    ? ProvidedStringType.Constant
                    : ProvidedStringType.Variable;

                return true;
            }
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

    private enum ProvidedStringType
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
        ProvidedStringType KeyStringType,
        string? ConstantKeyString,
        ProvidedStringType FormatStringType,
        string? ConstantFormatString,
        InterceptableLocation InterceptableLocation
    );

    private readonly record struct ChainedAppendsModel(
        AppendCallModel AppendCall,
        AppendCallModel FirstAppendCall,
        int IndexInChain
    );

    private readonly record struct ChainModel(ImmutableArray<AppendCallModel> Calls)
    {
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
                    var optimizedChain = ConcatConstants(chain);

                    foreach (var call in optimizedChain.Calls)
                    {
                        switch (call.KeyStringType)
                        {
                            case ProvidedStringType.Constant:
                                _writer.AppendLine($"context.SetKeyString({SymbolDisplay.FormatLiteral(call.ConstantKeyString ?? string.Empty, true)});");
                                break;

                            case ProvidedStringType.Variable:
                                _writer.AppendLine("context.SetKeyString();");
                                break;
                        }

                        switch (call.FormatStringType)
                        {
                            case ProvidedStringType.Constant:
                                _writer.AppendLine($"context.SetFormatString({SymbolDisplay.FormatLiteral(call.ConstantFormatString ?? string.Empty, true)});");
                                break;

                            case ProvidedStringType.Variable:
                                _writer.AppendLine("context.SetFormatString();");
                                break;
                        }

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
                                _writer.AppendLine($"context.OutputValueType<{call.UnderlyingTypeName}>({(call.IsNullableType ? "true" : "false")});");
                                break;
                        }
                    }
                }
            }

            return;

            static ChainModel ConcatConstants(ChainModel chain)
            {
                var calls = new List<AppendCallModel>();

                foreach (var call in chain.Calls)
                {
                    if (CanConcat(call) && calls.Count > 0)
                    {
                        var lastCall = calls[calls.Count - 1];

                        if (CanConcat(lastCall))
                        {
                            calls[calls.Count - 1] = lastCall with
                            {
                                ConstantString = lastCall.ConstantString + call.ConstantString
                            };

                            continue;
                        }
                    }

                    calls.Add(call);
                }

                return new ChainModel(calls.ToImmutableArray());
            }

            static bool CanConcat(AppendCallModel call)
                => call.Type is AppendedType.ConstantString
                   && call.KeyStringType is ProvidedStringType.None
                   && call.FormatStringType is ProvidedStringType.None;
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

                    _writer.Append(
                        $"""
                        {call.InterceptableLocation.GetInterceptsLocationAttributeSyntax()}
                        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                        public static global::{ZeroLogFacts.FullTypeNames.LogMessage} Append_{chainIndex}_{callIndex}
                        """
                    );

                    _writer.Append($"(this global::{ZeroLogFacts.FullTypeNames.LogMessage} log, ");

                    if (call.KeyStringType is not ProvidedStringType.None)
                        _writer.Append("string key, ");

                    _writer.Append(call.UnderlyingTypeName);

                    if (call.IsNullableType)
                        _writer.Append("?");

                    _writer.Append(" value");

                    if (call.FormatStringType is not ProvidedStringType.None)
                        _writer.Append(", string format");

                    _writer.AppendLine(")");

                    using (_writer.WriteBlock())
                    {
                        if (callIndex == 0)
                            _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.AppendChain}(log, Chain{chainIndex}.Instance);");

                        if (call.KeyStringType is ProvidedStringType.Variable)
                            _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.AppendChainItem}(log, key);");

                        if (call.FormatStringType is ProvidedStringType.Variable)
                            _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.LogMessage}.{ZeroLogFacts.ShortTypeNames.InternalApi}.{ZeroLogFacts.MethodNames.AppendChainItem}(log, format);");

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
