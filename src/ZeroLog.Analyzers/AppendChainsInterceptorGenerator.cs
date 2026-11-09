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
                            .Where(static i => i.CallSite.IsDefined())
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
        if (!TryGetAppendCallSite(invocation, semanticModel, out var callSite, cancellationToken))
        {
            chainEntry = default;
            return false;
        }

        TryGetAppendCallSite(invocation.Instance as IInvocationOperation, semanticModel, out var previousCallSite, cancellationToken);
        chainEntry = new AppendChainEntry(callSite, previousCallSite);

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

        if (!TryGetAppendCallDescriptor(invocation, semanticModel, out var appendCallDescriptor))
            return false;

        if (semanticModel.GetInterceptableLocation((InvocationExpressionSyntax)invocation.Syntax, cancellationToken) is not { } appendCallLocation)
            return false;

        appendCallSite = new AppendCallSite(
            appendCallDescriptor,
            appendCallLocation
        );

        return true;
    }

    private static bool IsPotentiallyChainableMethod(IMethodSymbol method)
        => IsPotentiallyChainableMethodName(method.Name)
           && IsLogMessageType(method.ContainingType);

    private static bool IsPotentiallyChainableMethodName(string methodName)
        => methodName is ZeroLogFacts.MethodNames.Append
            or ZeroLogFacts.MethodNames.AppendEnum
            or ZeroLogFacts.MethodNames.AppendKeyValue;

    private static bool IsLogMessageType(ITypeSymbol type)
        => type is
        {
            Name: ZeroLogFacts.ShortTypeNames.LogMessage,
            ContainingType: null,
            ContainingNamespace: { Name: ZeroLogFacts.Namespaces.ZeroLog, ContainingNamespace.IsGlobalNamespace: true }
        };

    private static bool TryGetAppendCallDescriptor(IInvocationOperation invocation,
                                                   SemanticModel semanticModel,
                                                   out AppendCallDescriptor descriptor)
    {
        var method = invocation.TargetMethod;

        if (!IsPotentiallyChainableMethod(method))
            goto noMatch;

        switch (method.Name)
        {
            case ZeroLogFacts.MethodNames.Append when method.Parameters.Length is 1 or 2:
            {
                if (method.Parameters[0].Type is not { } valueParameterType)
                    goto noMatch;

                if (method.Parameters.Length == 2 && method.Parameters[1].Type.SpecialType != SpecialType.System_String)
                    goto noMatch;

                if (GetSupportedTypeName(valueParameterType, semanticModel.Compilation, out var appendedType) is not { } underlyingTypeName)
                    goto noMatch;

                var valueString = appendedType is AppendedType.String
                    ? GetStringArgument(invocation.GetArgumentByParameterIndex(0))
                    : StringArgument.None;

                var formatString = GetStringArgument(invocation.GetArgumentByParameterIndex(1));

                descriptor = new AppendCallDescriptor(
                    appendedType,
                    underlyingTypeName,
                    StringArgument.None,
                    valueString,
                    formatString
                );

                return true;
            }

            case ZeroLogFacts.MethodNames.AppendEnum when method.Parameters.Length is 1:
            {
                if (method.Parameters[0].Type is not { } valueParameterType)
                    goto noMatch;

                if (GetSupportedTypeName(valueParameterType, semanticModel.Compilation, out var appendedType) is not { } underlyingTypeName)
                    goto noMatch;

                descriptor = new AppendCallDescriptor(
                    appendedType,
                    underlyingTypeName,
                    StringArgument.None,
                    StringArgument.None,
                    StringArgument.None
                );

                return true;
            }

            case ZeroLogFacts.MethodNames.AppendKeyValue when method.Parameters.Length is 2:
            {
                if (method.Parameters[0].Type.SpecialType is not SpecialType.System_String)
                    goto noMatch;

                if (method.Parameters[1].Type is not { } valueParameterType)
                    goto noMatch;

                if (GetSupportedTypeName(valueParameterType, semanticModel.Compilation, out var appendedType) is not { } underlyingTypeName)
                    goto noMatch;

                var keyString = GetStringArgument(invocation.GetArgumentByParameterIndex(0));

                var valueString = appendedType is AppendedType.String
                    ? GetStringArgument(invocation.GetArgumentByParameterIndex(1))
                    : StringArgument.None;

                descriptor = new AppendCallDescriptor(
                    appendedType,
                    underlyingTypeName,
                    keyString,
                    valueString,
                    StringArgument.None
                );

                return true;
            }
        }

        noMatch:
        descriptor = default;
        return false;

        static StringArgument GetStringArgument(IArgumentOperation? argument)
            => argument is null
                ? StringArgument.None
                : argument.TryGetConstantString(out var value)
                    ? StringArgument.Constant(value)
                    : StringArgument.Variable;
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

        var specialValueTypeName = underlyingType.SpecialType switch
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

        if (specialValueTypeName is not null)
        {
            appendedType = isNullableValueType ? AppendedType.NullableValueType : AppendedType.ValueType;
            return specialValueTypeName;
        }

        var fullTypeName = underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        switch (fullTypeName)
        {
            case "global::System.Guid":
            case "global::System.TimeSpan":
            case "global::System.DateTimeOffset":
            case "global::System.DateOnly":
            case "global::System.TimeOnly":
                appendedType = isNullableValueType ? AppendedType.NullableValueType : AppendedType.ValueType;
                return fullTypeName;
        }

        if (underlyingType is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && enumType.HasAssemblyAccessibility(compilation))
        {
            appendedType = isNullableValueType ? AppendedType.NullableEnum : AppendedType.Enum;
            return fullTypeName;
        }

        appendedType = AppendedType.None;
        return null;
    }

    private static ImmutableArray<AppendChain> ToAppendChains(ImmutableArray<AppendChainEntry> chainEntries, CancellationToken cancellationToken)
    {
        var chains = chainEntries.Where(static i => IsFirstCallSite(i))
                                 .Select(static i => new List<AppendCallSite> { i.CallSite })
                                 .ToImmutableArray();

        var previousToNext = chainEntries.Where(static i => !IsFirstCallSite(i))
                                         .GroupBy(static i => i.PreviousCallSite)
                                         .Select(static i => (Previous: i.Key, Next: i.UniqueOrDefault().CallSite))
                                         .Where(static i => i.Next.IsDefined())
                                         .ToDictionary(static i => i.Previous, static i => i.Next);

        foreach (var chain in chains)
        {
            for (var current = chain[0]; previousToNext.TryGetValue(current, out var next); current = next)
                chain.Add(next);
        }

        return chains.Where(ShouldIntercept)
                     .Select(static i => new AppendChain(i.ToImmutableArray()))
                     .ToImmutableArray();

        static bool IsFirstCallSite(in AppendChainEntry entry)
            => !entry.PreviousCallSite.IsDefined();

        static bool ShouldIntercept(List<AppendCallSite> callSites)
            => callSites.Count > 1 && callSites.Any(static i => IncludesConstantString(i.Descriptor));

        static bool IncludesConstantString(in AppendCallDescriptor call)
            => call.KeyString.Kind is StringArgumentKind.Constant
               || call.ValueString.Kind is StringArgumentKind.Constant
               || call.FormatString.Kind is StringArgumentKind.Constant;
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
                    callDescriptor.KeyString.Kind,
                    callDescriptor.ValueString.Kind,
                    callDescriptor.FormatString.Kind,
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
                            ValueString = StringArgument.Constant(lastCall.ValueString.Value + call.ValueString.Value)
                        };

                        continue;
                    }
                }

                calls.Add(call);
            }

            return chain with { Body = new ChainBody(calls.ToImmutable()) };
        }

        static bool CanConcat(in AppendCallDescriptor call)
            => call.ValueString is { Kind: StringArgumentKind.Constant, Value: not null }
               && call.KeyString.Kind is StringArgumentKind.None
               && call.FormatString.Kind is StringArgumentKind.None;
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

    private enum StringArgumentKind : byte
    {
        None,
        Constant,
        Variable
    }

    private readonly struct StringArgument : IEquatable<StringArgument>
    {
        private static readonly string _variableInstance = new('V', 1);
        private static readonly string _nullInstance = new('N', 1);

        private readonly string? _value;

        public static StringArgument None => default;
        public static StringArgument Variable => new(_variableInstance);
        public static StringArgument Constant(string? value) => new(value ?? _nullInstance);

        public StringArgumentKind Kind => _value is null
            ? StringArgumentKind.None
            : ReferenceEquals(_value, _variableInstance)
                ? StringArgumentKind.Variable
                : StringArgumentKind.Constant;

        public string? Value => ReferenceEquals(_value, _variableInstance) || ReferenceEquals(_value, _nullInstance) ? null : _value;

        private StringArgument(string? value)
            => _value = value;

        public override bool Equals(object? obj)
            => obj is StringArgument other && Equals(other);

        public bool Equals(StringArgument other)
            => Kind == other.Kind && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override int GetHashCode()
            => _value?.GetHashCode() ?? 0;

        public override string ToString()
            => Kind switch
            {
                StringArgumentKind.None     => "<none>",
                StringArgumentKind.Variable => "<variable>",
                StringArgumentKind.Constant => Value is null ? "<constant null>" : $"<constant \"{Value}\">",
                _                           => "<unknown>"
            };
    }

    private readonly record struct AppendCallDescriptor(
        AppendedType AppendedType,
        string UnderlyingTypeName,
        StringArgument KeyString,
        StringArgument ValueString,
        StringArgument FormatString
    );

    private readonly record struct AppendCallSite(
        AppendCallDescriptor Descriptor,
        InterceptableLocation Location
    )
    {
        public bool IsDefined() => Descriptor.AppendedType is not AppendedType.None;
    }

    private readonly record struct AppendChainEntry(
        AppendCallSite CallSite,
        AppendCallSite PreviousCallSite
    );

    private readonly record struct AppendChain(ImmutableArray<AppendCallSite> CallSites)
    {
        public bool Equals(AppendChain other)
            => CallSites.HasEqualValues(other.CallSites);

        public override int GetHashCode()
            => CallSites.GetValuesHashCode();

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
            => Calls.HasEqualValues(other.Calls);

        public override int GetHashCode()
            => Calls.GetValuesHashCode();
    }

    private readonly record struct InterceptorDescriptor(
        ChainIdentifier? ChainId,
        AppendedType AppendedType,
        StringArgumentKind KeyStringKind,
        StringArgumentKind ValueStringKind,
        StringArgumentKind FormatStringKind,
        string UnderlyingTypeName
    ) : IComparable<InterceptorDescriptor>
    {
        public int CompareTo(InterceptorDescriptor other)
        {
            var a = (ChainId?.Index ?? int.MaxValue, AppendedType, KeyStringKind, ValueStringKind, FormatStringKind);
            var b = (other.ChainId?.Index ?? int.MaxValue, other.AppendedType, other.KeyStringKind, other.ValueStringKind, other.FormatStringKind);

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
                    GenerateInitialization(chainDefinitions);
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

                    [global::System.Diagnostics.Conditional("ZEROLOG_GENERATED")]
                    [global::System.AttributeUsage(global::System.AttributeTargets.Method)]
                    file sealed class ModuleInitializerAttribute : global::System.Attribute
                    { }
                }

                """
            );
        }

        private void GenerateChainClass(in ChainDefinition chainDefinition)
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
                        switch (call.KeyString.Kind)
                        {
                            case StringArgumentKind.Constant:
                                _writer.AppendLine($"context.SetKeyString({FormatStringLiteral(call.KeyString.Value)});");
                                break;

                            case StringArgumentKind.Variable:
                                _writer.AppendLine("context.SetKeyString();");
                                break;
                        }

                        switch (call.FormatString.Kind)
                        {
                            case StringArgumentKind.Constant:
                                _writer.AppendLine($"context.SetFormatString({FormatStringLiteral(call.FormatString.Value)});");
                                break;

                            case StringArgumentKind.Variable:
                                _writer.AppendLine("context.SetFormatString();");
                                break;
                        }

                        switch (call.AppendedType)
                        {
                            case AppendedType.String when call.ValueString.Kind is StringArgumentKind.Constant:
                                _writer.AppendLine($"context.OutputString({FormatStringLiteral(call.ValueString.Value)});");
                                break;

                            case AppendedType.String when call.ValueString.Kind is StringArgumentKind.Variable:
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

                if (call.KeyStringKind is not StringArgumentKind.None)
                    _writer.Append("string key, ");

                _writer.Append(call.UnderlyingTypeName);

                if (call.AppendedType is AppendedType.NullableValueType or AppendedType.NullableEnum or AppendedType.String)
                    _writer.Append("?");

                _writer.Append(" value");

                if (call.FormatStringKind is not StringArgumentKind.None)
                    _writer.Append(", string format");

                _writer.AppendLine(")");

                using (_writer.WriteBlock())
                {
                    if (call.ChainId is { } chainId)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendChain}(log, global::{ZeroLogFacts.Namespaces.Generated}.{chainId}.Instance);");

                    if (call.KeyStringKind is StringArgumentKind.Variable)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendValue}(log, key);");

                    if (call.ValueStringKind is StringArgumentKind.Variable)
                        _writer.AppendLine($"global::{ZeroLogFacts.FullTypeNames.InternalApi.Chain}.{ZeroLogFacts.MethodNames.InternalApi.AppendValue}(log, value);");

                    if (call.FormatStringKind is StringArgumentKind.Variable)
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
        }

        private void GenerateInitialization(ImmutableArray<ChainDefinition> chainDefinitions)
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
                        // Ignore exceptions during module initialization.
                    }

                    return;

                    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                    static void AttachEventHandler()
                        => global::{{ZeroLogFacts.FullTypeNames.LogManager}}.{{ZeroLogFacts.ShortTypeNames.InternalApi}}.{{ZeroLogFacts.EventNames.Initialized}} += OnInitialized;
                }

                """
            );

            using (_writer.WriteBlock("public static void OnInitialized()"))
            {
                // Don't ignore exceptions at ZeroLog initialization. If anything goes wrong here,
                // chains used at runtime will also throw, so we want to know about it as early as possible.

                foreach (var chainDefinition in chainDefinitions)
                    _writer.AppendLine($"global::{ZeroLogFacts.Namespaces.Generated}.{chainDefinition.ChainId}.Instance.{ZeroLogFacts.MethodNames.InternalApi.Initialize}();");
            }
        }
    }
}

#endif
