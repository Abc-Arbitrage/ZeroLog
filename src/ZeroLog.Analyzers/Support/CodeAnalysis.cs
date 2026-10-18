using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

#pragma warning disable CS9113

namespace System.Diagnostics.CodeAnalysis
{
    [Embedded]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class StringSyntaxAttribute(string syntax) : Attribute;

    [Embedded]
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute;
}

namespace Microsoft.CodeAnalysis
{
    [SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
    internal sealed partial class EmbeddedAttribute : Attribute;
}
