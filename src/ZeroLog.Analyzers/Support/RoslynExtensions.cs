using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ZeroLog.Analyzers.Support;

internal static class RoslynExtensions
{
    public static IncrementalValueProvider<bool> IsGeneratorEnabled(this IncrementalGeneratorInitializationContext context, string generatorName)
        => context.AnalyzerConfigOptionsProvider
                  .Select(static (options, _) => options.GlobalOptions.TryGetValue("build_property.ZeroLogEnabledGenerators", out var value) ? value : "")
                  .Select((value, _) => value.Contains(generatorName, StringComparison.OrdinalIgnoreCase));

    public static IncrementalValueProvider<ImmutableArray<T>> EmptyIfFalse<T>(this IncrementalValueProvider<(ImmutableArray<T> items, bool isEnabled)> source)
        => source.Select(static (pair, _) => pair.isEnabled ? pair.items : ImmutableArray<T>.Empty);

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> source)
        where T : class
        => source.Where(static value => value is not null)!;

    public static IncrementalValuesProvider<T> WithLambdaComparer<T>(this IncrementalValuesProvider<T> source, Func<T, T, bool> equals, Func<T, int> getHashCode)
        => source.WithComparer(new LambdaComparer<T>(equals, getHashCode));

    extension(InvocationExpressionSyntax invocation)
    {
        public string? InvokedMethodName => invocation.Expression switch
        {
            MemberAccessExpressionSyntax i => i.Name.Identifier.ValueText,
            SimpleNameSyntax i             => i.Identifier.ValueText,
            _                              => null
        };
    }

    extension(ISymbol symbol)
    {
        public bool HasAttribute(INamedTypeSymbol attributeType)
            => symbol.GetAttribute(attributeType) is not null;

        public AttributeData? GetAttribute(INamedTypeSymbol attributeType)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                    return attribute;
            }

            return null;
        }
    }

    extension(IInvocationOperation invocation)
    {
        public IArgumentOperation? GetArgumentByParameterIndex(int index)
        {
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter?.Ordinal == index)
                    return argument;
            }

            return null;
        }
    }

    extension(IArgumentOperation argument)
    {
        public bool TryGetConstantString(out string? value)
        {
            if (argument.Value.ConstantValue is { HasValue: true, Value: var valueOrNull })
            {
                value = valueOrNull as string;
                return true;
            }

            value = null;
            return false;
        }
    }

    private sealed class LambdaComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
            => x is not null
                ? y is not null && equals(x, y)
                : y is null;

        public int GetHashCode(T obj)
            => getHashCode(obj);
    }
}
