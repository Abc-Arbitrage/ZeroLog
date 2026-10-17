using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

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
        => source.Where(static value => value is not null)!;

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
}
