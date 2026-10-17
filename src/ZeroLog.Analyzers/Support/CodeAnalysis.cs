using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace System.Diagnostics.CodeAnalysis
{
    [Embedded]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class StringSyntaxAttribute : Attribute
    {
        public StringSyntaxAttribute(string syntax)
        { }
    }
}

namespace Microsoft.CodeAnalysis
{
    [SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
    internal sealed partial class EmbeddedAttribute : Attribute;
}
