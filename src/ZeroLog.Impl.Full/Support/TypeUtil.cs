using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Support;

internal static class TypeUtil
{
    private static readonly Func<IntPtr, Type>? _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

    public static IntPtr GetTypeHandleSlow(Type type)
        => type.TypeHandle.Value;

    public static Type? GetTypeFromHandle(IntPtr typeHandle)
        => _getTypeFromHandleFunc?.Invoke(typeHandle);

    private static Func<IntPtr, Type>? BuildGetTypeFromHandleFunc()
    {
        // TODO: Use RuntimeTypeHandle.FromIntPtr in .NET 7 (see #47)

        var method = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(IntPtr) }, null);
        if (method == null)
            return null;

        var param = Parameter(typeof(IntPtr));

        return Lambda<Func<IntPtr, Type>>(
            Call(method, param),
            param
        ).Compile();
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
