using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Support;

internal static class TypeUtil
{
    public static IntPtr GetTypeHandleSlow(Type type)
        => type.TypeHandle.Value;

#if NET7_0_OR_GREATER
    public static Type? GetTypeFromHandle(IntPtr typeHandle)
        => Type.GetTypeFromHandle(RuntimeTypeHandle.FromIntPtr(typeHandle));
#else
    public static Type? GetTypeFromHandle(IntPtr typeHandle)
        => _getTypeFromHandleFunc?.Invoke(typeHandle);

    private static readonly Func<IntPtr, Type>? _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

    private static Func<IntPtr, Type>? BuildGetTypeFromHandleFunc()
    {
        var method = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(IntPtr) }, null);
        if (method == null)
            return null;

        var param = Parameter(typeof(IntPtr));

        return Lambda<Func<IntPtr, Type>>(
            Call(method, param),
            param
        ).Compile();
    }
#endif

    public static bool GetIsUnmanagedSlow(Type type)
    {
        return !(bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences), BindingFlags.Static | BindingFlags.Public)!
                                            .MakeGenericMethod(type)
                                            .Invoke(null, null)!;
    }
}

internal static class TypeUtil<T>
{
    public static readonly IntPtr TypeHandle = TypeUtil.GetTypeHandleSlow(typeof(T));
}

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
internal static class TypeUtilSlow<T>
{
    // Initializing this type will allocate

    private static readonly Type? _underlyingType = Nullable.GetUnderlyingType(typeof(T));

    public static readonly TypeCode UnderlyingTypeCode = Type.GetTypeCode(_underlyingType);
}
