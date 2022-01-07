#if NETCOREAPP

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AllowNullAttribute))]
[assembly: TypeForwardedTo(typeof(DisallowNullAttribute))]
[assembly: TypeForwardedTo(typeof(DoesNotReturnAttribute))]
[assembly: TypeForwardedTo(typeof(DoesNotReturnIfAttribute))]
[assembly: TypeForwardedTo(typeof(MaybeNullAttribute))]
[assembly: TypeForwardedTo(typeof(MaybeNullWhenAttribute))]
[assembly: TypeForwardedTo(typeof(NotNullAttribute))]
[assembly: TypeForwardedTo(typeof(NotNullIfNotNullAttribute))]
[assembly: TypeForwardedTo(typeof(NotNullWhenAttribute))]

[assembly: TypeForwardedTo(typeof(InterpolatedStringHandlerAttribute))]
[assembly: TypeForwardedTo(typeof(InterpolatedStringHandlerArgumentAttribute))]

#else

// ReSharper disable CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class AllowNullAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue)
        {
            ParameterValue = parameterValue;
        }

        public bool ParameterValue { get; }
    }

    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
    internal sealed class MaybeNullAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }

    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }

    [AttributeUsageAttribute(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerAttribute : Attribute
    {
        public InterpolatedStringHandlerAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public InterpolatedStringHandlerArgumentAttribute(string argument) => Arguments = new string[] { argument };

        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) => Arguments = arguments;

        public string[] Arguments { get; }
    }
}

#endif
