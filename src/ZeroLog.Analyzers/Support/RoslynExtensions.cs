using Microsoft.CodeAnalysis;

namespace ZeroLog.Analyzers.Support;

internal static class RoslynExtensions
{
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
