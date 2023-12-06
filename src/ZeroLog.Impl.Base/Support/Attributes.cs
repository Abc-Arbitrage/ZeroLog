#if NETSTANDARD

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
    internal sealed class NotNullAttribute : Attribute;
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public InterpolatedStringHandlerArgumentAttribute(string argument)
            => Arguments = [argument];

        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments)
            => Arguments = arguments;

        public string[] Arguments { get; }
    }
}

#endif
