#if ROSLYN5_0_OR_GREATER

using System;
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
public sealed class AppendChainsInterceptorGenerator : SourceGeneratorBase
{
    private const string _generatorName = "AppendChains";
    private const int _apiVersion = 1;

    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var chains = context.SyntaxProvider
                            .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                            .Where(static i => i.CallSite.Descriptor.AppendedType is not AppendedType.None)
                            .WithLambdaComparer(static (a, b) => a.CallSite.Equals(b.CallSite), static i => i.CallSite.GetHashCode())
                            .Collect()
                            .Combine(context.IsGeneratorEnabled(_generatorName))
                            .EmptyIfFalse()
                            .Combine(context.CompilationProvider.Select(SupportsGeneratedAppendChains))
                            .EmptyIfFalse()
                            .SelectMany(ToAppendChains)
                            .Collect();

        context.RegisterImplementationSourceOutput(
            chains,
            static (context, chains) =>
            {
                if (chains.Length == 0)
                    return;

                BuildDefinitions(chains, out var chainDefinitions, out var interceptorDefinitions);

                context.AddSource(
                    "AppendChains.g.cs",
                    new Generator().Generate(chainDefinitions, interceptorDefinitions)
                );
            }
        );
    }

    private static bool SupportsGeneratedAppendChains(Compilation compilation, CancellationToken cancellationToken)
    {
        // C# 11 is required for file-scoped types
        return compilation is CSharpCompilation { LanguageVersion: >= LanguageVersion.CSharp11 }
               && compilation.GetTypeByMetadataName($"{ZeroLogFacts.MetadataNames.LogMessageInternalApiChain}") is { } chainType
               && chainType.GetMembers(ZeroLogFacts.FieldNames.Version) is [IFieldSymbol { IsConst: true, ConstantValue: _apiVersion }];
    }

    private static bool SyntaxPredicate(SyntaxNode node, CancellationToken cancellationToken)
        => node.IsKind(SyntaxKind.InvocationExpression)
           && node is InvocationExpressionSyntax { InvokedMethodName: { } methodName }
           && IsPotentiallyChainableMethodName(methodName);

    private static AppendChainEntry SyntaxTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is InvocationExpressionSyntax invocationSyntax
            && context.SemanticModel.GetOperation(invocationSyntax, cancellationToken) is IInvocationOperation invocation
            && IsPotentiallyChainableMethod(invocation.TargetMethod)
            && TryGetChainEntry(invocation, context.SemanticModel, out var chainEntry, cancellationToken))
        {
            return chainEntry;
        }

        return default;
    }

    private static bool TryGetChainEntry(IInvocationOperation invocation,
                                         SemanticModel semanticModel,
                                         out AppendChainEntry chainEntry,
                                         CancellationToken cancellationToken)
    {
        chainEntry = default;

        if (!TryGetAppendCallSite(invocation, semanticModel, out var callSite, cancellationToken))
            return false;

        var currentInvocation = invocation;
        var chainPosition = 0;

        var previousCallSite = default(AppendCallSite);

        while (TryGetAppendCallSite(currentInvocation, semanticModel, out var currentCallSite, cancellationToken))
        {
            previousCallSite = currentCallSite;
            ++chainPosition;
            currentInvocation = currentInvocation.Instance as IInvocationOperation;
        }

        chainEntry = new AppendChainEntry(
            callSite,
            previousCallSite,
            chainPosition - 1
        );

        return true;
    }

    private static bool TryGetAppendCallSite([NotNullWhen(true)] IInvocationOperation? invocation,
                                             SemanticModel semanticModel,
                                             out AppendCallSite appendCallSite,
                                             CancellationToken cancellationToken)
    {
        appendCallSite = default;

        if (invocation is null)
            return false;

        if (!IsChainableInvocation(
                invocation,
                semanticModel,
                out var appendedType,
                out var underlyingTypeName,
                out var keyStringType,
                out var keyString,
                out var valueStringType,
                out var valueString,
                out var formatStringType,
                out var formatString
            ))
        {
            return false;
        }

        if (semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not { } appendCallLocation)
            return false;

        appendCallSite = new AppendCallSite(
            new AppendCallDescriptor(
                appendedType,
                keyStringType,
                valueStringType,
                formatStringType,
                underlyingTypeName,
                keyString,
                valueString,
                formatString
            ),
            appendCallLocation
        );

        return true;
    }

    private static bool IsPotentiallyChainableMethod(IMethodSymbol method)
        => IsPotentiallyChainableMethodName(method.Name)
           && IsLogMessageType(method.ContainingType);

    private static bool IsPotentiallyChainableMethodName(string methodName)
        => methodName is ZeroLogFacts.MethodNames.Append or ZeroLogFacts.MethodNames.AppendEnum or ZeroLogFacts.MethodNames.AppendKeyValue;

    private static bool IsLogMessageType(ITypeSymbol type)
        => type is
        {
            Name: ZeroLogFacts.ShortTypeNames.LogMessage,
            ContainingType: null,
            ContainingNamespace: { Name: ZeroLogFacts.Namespaces.ZeroLog, ContainingNamespace.IsGlobalNamespace: true }
        };

    private static bool IsChainableInvocation(IInvocationOperation invocation,
                                              SemanticModel semanticModel,
                                              out AppendedType appendedType,
                                              [NotNullWhen(true)] out string? underlyingTypeName,
                                              out ProvidedStringType keyStringType,
                                              out string? keyString,
                                              out ProvidedStringType valueStringType,
                                              out string? valueString,
                                              out ProvidedStringType formatStringType,
                                              out string? formatString)
    {
        appendedType = AppendedType.None;
        underlyingTypeName = null;
        keyStringType = ProvidedStringType.None;
        keyString = null;
        valueStringType = ProvidedStringType.None;
        valueString = null;
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

                underlyingTypeName = GetSupportedTypeName(
                    valueParameterType,
                    semanticModel.Compilation,
                    out appendedType
                );

                if (underlyingTypeName is null)
                    return false;

                if (appendedType is AppendedType.String)
                {
                    valueStringType = invocation.GetArgumentByParameterIndex(0)?.TryGetConstantString(out valueString) is true
                        ? ProvidedStringType.Constant
                        : ProvidedStringType.Variable;
                }

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

            case ZeroLogFacts.MethodNames.AppendEnum when method.Parameters.Length is 1:
            {
                if (method.Parameters[0].Type is not { } valueParameterType)
                    return false;

                underlyingTypeName = GetSupportedTypeName(
                    valueParameterType,
                    semanticModel.Compilation,
                    out appendedType
                );

                return underlyingTypeName is not null;
            }

            case ZeroLogFacts.MethodNames.AppendKeyValue when method.Parameters.Length is 2:
            {
                if (method.Parameters[0].Type.SpecialType is not SpecialType.System_String)
                    return false;

                if (method.Parameters[1].Type is not { } valueParameterType)
                    return false;

                underlyingTypeName = GetSupportedTypeName(
                    valueParameterType,
                    semanticModel.Compilation,
                    out appendedType
                );

                if (underlyingTypeName is null)
                    return false;

                keyStringType = invocation.GetArgumentByParameterIndex(0)?.TryGetConstantString(out keyString) is true
                    ? ProvidedStringType.Constant
                    : ProvidedStringType.Variable;

                if (appendedType is AppendedType.String)
                {
                    valueStringType = invocation.GetArgumentByParameterIndex(1)?.TryGetConstantString(out valueString) is true
                        ? ProvidedStringType.Constant
                        : ProvidedStringType.Variable;
                }

                return true;
            }
        }

        return false;
    }

    private static string? GetSupportedTypeName(ITypeSymbol type,
                                                Compilation compilation,
                                                out AppendedType appendedType)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            appendedType = AppendedType.None;
            return null;
        }

        var isNullableValueType = namedType.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullableValueType ? namedType.TypeArguments[0] : namedType;

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
            SpecialType.System_IntPtr   => "nint",
            SpecialType.System_UIntPtr  => "nuint",
            SpecialType.System_Single   => "float",
            SpecialType.System_Double   => "double",
            SpecialType.System_Decimal  => "decimal",
            SpecialType.System_DateTime => "global::System.DateTime",
            _                           => null
        };

        if (fromSpecialType is not null)
        {
            appendedType = isNullableValueType ? AppendedType.NullableValueType : AppendedType.ValueType;
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
                appendedType = isNullableValueType ? AppendedType.NullableValueType : AppendedType.ValueType;
                return fullNameType;
        }

        if (underlyingType is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && enumType.HasAssemblyAccessibility(compilation))
        {
            appendedType = isNullableValueType ? AppendedType.NullableEnum : AppendedType.Enum;
            return fullNameType;
        }

        appendedType = AppendedType.None;
        return null;
    }

    private static ImmutableArray<AppendChain> ToAppendChains(ImmutableArray<AppendChainEntry> chainEntries,
                                                              CancellationToken cancellationToken)
    {
        return chainEntries.GroupBy(static item => item.FirstCallSite)
                           .Where(ShouldIntercept)
                           .Select(
                               static item => new AppendChain(
                                   item.OrderBy(i => i.IndexInChain)
                                       .Select(i => i.CallSite)
                                       .ToImmutableArray())
                           )
                           .ToImmutableArray();

        static bool ShouldIntercept(IGrouping<AppendCallSite, AppendChainEntry> call)
            => call.Count() > 1 && call.Any(static i => IncludesConstantString(i.CallSite.Descriptor));

        static bool IncludesConstantString(AppendCallDescriptor call)
            => call.KeyStringType is ProvidedStringType.Constant
               || call.ValueStringType is ProvidedStringType.Constant
               || call.FormatStringType is ProvidedStringType.Constant;
    }

    private static void BuildDefinitions(ImmutableArray<AppendChain> appendChains,
                                         out ImmutableArray<ChainDefinition> chainDefinitions,
                                         out ImmutableArray<InterceptorDefinition> interceptorDefinitions)
    {
        var chainIndexByBody = new Dictionary<ChainBody, int>(appendChains.Length);
        var interceptLocations = new Dictionary<InterceptorDescriptor, List<InterceptableLocation>>(appendChains.Length);

        foreach (var chain in appendChains)
        {
            var chainBody = chain.GetChainBody();

            if (!chainIndexByBody.TryGetValue(chainBody, out var chainIndex))
            {
                chainIndex = chainIndexByBody.Count;
                chainIndexByBody.Add(chainBody, chainIndex);
            }

            for (var callIndex = 0; callIndex < chain.CallSites.Length; ++callIndex)
            {
                var (callDescriptor, location) = chain.CallSites[callIndex];

                var interceptorDescriptor = new InterceptorDescriptor(
                    callIndex == 0 ? new ChainIdentifier(chainIndex) : null,
                    callDescriptor.AppendedType,
                    callDescriptor.KeyStringType,
                    callDescriptor.ValueStringType,
                    callDescriptor.FormatStringType,
                    callDescriptor.UnderlyingTypeName
                );

                if (interceptLocations.TryGetValue(interceptorDescriptor, out var callLocations))
                    callLocations.Add(location);
                else
                    interceptLocations.Add(interceptorDescriptor, [location]);
            }
        }

        chainDefinitions = chainIndexByBody.Select(i => new ChainDefinition(new ChainIdentifier(i.Value), i.Key))
                                           .Select(ConcatConstants)
                                           .OrderBy(i => i.ChainId.Index)
                                           .ToImmutableArray();

        interceptorDefinitions = interceptLocations.OrderBy(i => i.Key)
                                                   .Select(kvp => new InterceptorDefinition(
                                                               kvp.Key,
                                                               kvp.Value
                                                                  .OrderBy(i => i.Data, StringComparer.Ordinal)
                                                                  .ToImmutableArray()
                                                           )
                                                   )
                                                   .ToImmutableArray();

        return;

        static ChainDefinition ConcatConstants(ChainDefinition chain)
        {
            var calls = ImmutableArray.CreateBuilder<AppendCallDescriptor>(chain.Body.Calls.Length);

            foreach (var call in chain.Body.Calls)
            {
                if (CanConcat(call) && calls.Count > 0)
                {
                    var lastCall = calls[calls.Count - 1];

                    if (CanConcat(lastCall))
                    {
                        calls[calls.Count - 1] = lastCall with
                        {
                            ConstantValueString = lastCall.ConstantValueString + call.ConstantValueString
                        };

                        continue;
                    }
                }

                calls.Add(call);
            }

            return chain with { Body = new ChainBody(calls.ToImmutable()) };
        }

        static bool CanConcat(AppendCallDescriptor call)
            => call.ValueStringType is ProvidedStringType.Constant
               && call.ConstantValueString is not null
               && call.KeyStringType is ProvidedStringType.None
               && call.FormatStringType is ProvidedStringType.None;
    }

    private enum AppendedType : byte
    {
        None,

        String,

        ValueType,
        NullableValueType,

        Enum,
        NullableEnum
    }

    private enum ProvidedStringType : byte
    {
        None,
        Constant,
        Variable
    }

    private readonly record struct AppendCallDescriptor(
        AppendedType AppendedType,
        ProvidedStringType KeyStringType,
        ProvidedStringType ValueStringType,
        ProvidedStringType FormatStringType,
        string UnderlyingTypeName,
        string? ConstantKeyString,
        string? ConstantValueString,
        string? ConstantFormatString
    );

    private readonly record struct AppendCallSite(
        AppendCallDescriptor Descriptor,
        InterceptableLocation Location
    );

    private readonly record struct AppendChainEntry(
        AppendCallSite CallSite,
        AppendCallSite FirstCallSite,
        int IndexInChain
    );

    private readonly record struct AppendChain(ImmutableArray<AppendCallSite> CallSites)
    {
        public bool Equals(AppendChain other)
            => ImmutableArrayEqualityComparer<AppendCallSite>.Default.Equals(CallSites, other.CallSites);

        public override int GetHashCode()
            => ImmutableArrayEqualityComparer<AppendCallSite>.Default.GetHashCode(CallSites);

        public ChainBody GetChainBody()
            => new(CallSites.Select(static i => i.Descriptor).ToImmutableArray());
    }

    private readonly record struct ChainIdentifier(int Index)
    {
        public override string ToString()
            => $"Chain{Index}";
    }

    private readonly record struct ChainDefinition(
        ChainIdentifier ChainId,
        ChainBody Body
    );

    private readonly record struct ChainBody(ImmutableArray<AppendCallDescriptor> Calls)
    {
        public bool Equals(ChainBody other)
            => ImmutableArrayEqualityComparer<AppendCallDescriptor>.Default.Equals(Calls, other.Calls);

        public override int GetHashCode()
            => ImmutableArrayEqualityComparer<AppendCallDescriptor>.Default.GetHashCode(Calls);
    }

    private readonly record struct InterceptorDescriptor(
        ChainIdentifier? ChainId,
        AppendedType AppendedType,
        ProvidedStringType KeyStringType,
        ProvidedStringType ValueStringType,
        ProvidedStringType FormatStringType,
        string UnderlyingTypeName
    ) : IComparable<InterceptorDescriptor>
    {
        public int CompareTo(InterceptorDescriptor other)
        {
            var a = (ChainId?.Index ?? int.MaxValue, AppendedType, KeyStringType, ValueStringType, FormatStringType);
            var b = (other.ChainId?.Index ?? int.MaxValue, other.AppendedType, other.KeyStringType, other.ValueStringType, other.FormatStringType);

            var tupleComparison = a.CompareTo(b);
            return tupleComparison != 0 ? tupleComparison : StringComparer.Ordinal.Compare(UnderlyingTypeName, other.UnderlyingTypeName);
        }
    }

    private readonly record struct InterceptorDefinition(
        InterceptorDescriptor Descriptor,
        ImmutableArray<InterceptableLocation> Locations
    );

    private sealed class Generator : GeneratorBase
    {
        public string Generate(ImmutableArray<ChainDefinition> chainDefinitions,
                               ImmutableArray<InterceptorDefinition> interceptorDefinitions)
        {
            _writer.Clear();

            AppendHeader();

            using (_writer.WriteBlock($"namespace {ZeroLogFacts.Namespaces.Generated}"))
            {
                foreach (var chainDefinition in chainDefinitions)
                {
                    GenerateChainClass(chainDefinition);
                    _writer.AppendLine();
                }

                using (_writer.WriteBlock("file static class AppendChainInterceptors"))
                {
                    GenerateInterceptors(interceptorDefinitions);
                }
            }

            return _writer.ToString();
        }

        private void AppendHeader()
        {
            AppendCommonHeader(_generatorName);

            _writer.AppendLine(
                """
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

        private void GenerateChainClass(ChainDefinition chainDefinition)
        {
            var (chainId, body) = chainDefinition;

            _writer.AppendLine($"file sealed class {chainId} : global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}");

            using (_writer.WriteBlock())
            {
                _writer.AppendLine($"public static readonly {chainId} Instance = new();")
                       .AppendLine();

                using (_writer.WriteBlock($"protected override void Execute(ref global::{ZeroLogFacts.FullTypeNames.InternalApi.ChainExecutionContext} context)"))
                {
                    foreach (var call in body.Calls)
                    {
                        switch (call.KeyStringType)
                        {
                            case ProvidedStringType.Constant:
                                _writer.AppendLine($"context.SetKeyString({FormatStringLiteral(call.ConstantKeyString)});");
                                break;

                            case ProvidedStringType.Variable:
                                _writer.AppendLine("context.SetKeyString();");
                                break;
                        }

                        switch (call.FormatStringType)
                        {
                            case ProvidedStringType.Constant:
                                _writer.AppendLine($"context.SetFormatString({FormatStringLiteral(call.ConstantFormatString)});");
                                break;

                            case ProvidedStringType.Variable:
                                _writer.AppendLine("context.SetFormatString();");
                                break;
                        }

                        switch (call.AppendedType)
                        {
                            case AppendedType.String when call.ValueStringType is ProvidedStringType.Constant:
                                _writer.AppendLine($"context.OutputString({FormatStringLiteral(call.ConstantValueString)});");
                                break;

                            case AppendedType.String when call.ValueStringType is ProvidedStringType.Variable:
                                _writer.AppendLine("context.OutputString();");
                                break;

                            case AppendedType.ValueType:
                                _writer.AppendLine($"context.OutputValueType<{call.UnderlyingTypeName}>();");
                                break;

                            case AppendedType.NullableValueType:
                                _writer.AppendLine($"context.OutputNullableValueType<{call.UnderlyingTypeName}>();");
                                break;

                            case AppendedType.Enum:
                                _writer.AppendLine("context.OutputEnum();");
                                break;

                            case AppendedType.NullableEnum:
                                _writer.AppendLine("context.OutputNullableEnum();");
                                break;
                        }
                    }
                }
            }

            return;

            static string FormatStringLiteral(string? value)
                => value is null ? "null" : SymbolDisplay.FormatLiteral(value, true);
        }

        private void GenerateInterceptors(ImmutableArray<InterceptorDefinition> interceptorDefinitions)
        {
            var interceptorIndex = 0;

            foreach (var (call, locations) in interceptorDefinitions)
            {
                foreach (var location in locations)
                    _writer.AppendLine(location.GetInterceptsLocationAttributeSyntax());

                _writer.Append(
                    $"""
                    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                    public static global::{ZeroLogFacts.FullTypeNames.LogMessage} Interceptor{interceptorIndex}
                    """
                );

                _writer.Append($"(this global::{ZeroLogFacts.FullTypeNames.LogMessage} log, ");

                if (call.KeyStringType is not ProvidedStringType.None)
                    _writer.Append("string key, ");

                _writer.Append(call.UnderlyingTypeName);

                if (call.AppendedType is AppendedType.NullableValueType or AppendedType.NullableEnum or AppendedType.String)
                    _writer.Append("?");

                _writer.Append(" value");

                if (call.FormatStringType is not ProvidedStringType.None)
                    _writer.Append(", string format");

                _writer.AppendLine(")");

                using (_writer.WriteBlock())
                {
                    if (call.ChainId is { } chainId)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendChain}(log, {chainId}.Instance);");

                    if (call.KeyStringType is ProvidedStringType.Variable)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendValue}(log, key);");

                    if (call.ValueStringType is ProvidedStringType.Variable)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendValue}(log, value);");

                    if (call.FormatStringType is ProvidedStringType.Variable)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendValue}(log, format);");

                    var methodName = call.AppendedType switch
                    {
                        AppendedType.ValueType         => ZeroLogFacts.MethodNames.InternalApi.AppendValue,
                        AppendedType.NullableValueType => ZeroLogFacts.MethodNames.InternalApi.AppendValue,
                        AppendedType.Enum              => ZeroLogFacts.MethodNames.InternalApi.AppendEnumValue,
                        AppendedType.NullableEnum      => ZeroLogFacts.MethodNames.InternalApi.AppendEnumValue,
                        _                              => null
                    };

                    if (methodName is not null)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{methodName}(log, value);");

                    _writer.AppendLine("return log;");
                }

                ++interceptorIndex;
                _writer.AppendLine();
            }

            _writer.AppendLine();
        }
    }
}

#endif
