using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
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
        => _getTypeFromHandleFunc.Invoke(typeHandle);

    private static readonly Func<IntPtr, Type?> _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

    private static Func<IntPtr, Type?> BuildGetTypeFromHandleFunc()
    {
        // The GetTypeFromHandleUnsafe method is the preferred way to get a Type from a handle before .NET 7, as it dates back to the .NET Framework.
        var method = typeof(Type).GetMethod("GetTypeFromHandleUnsafe", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(IntPtr)], null);
        if (method is not null)
        {
            var param = Parameter(typeof(IntPtr));
            return Lambda<Func<IntPtr, Type?>>(Call(method, param), param).Compile();
        }

        // The GetTypeFromHandleUnsafe method can get trimmed away on .NET 6: ArgIterator is the only type which uses this internal method of the core library,
        // and since varargs are only supported on non-ARM Windows, GetTypeFromHandleUnsafe will get removed on other platforms such as Linux.
        // To get around this, we use __reftype to convert the handle, but we need to build a TypedReference equivalent manually.
        return static handle =>
        {
            IL.Push(new TypedReferenceLayout { Value = default, Type = handle });
            IL.Emit.Refanytype();
            IL.Emit.Call(MethodRef.Method(typeof(Type), nameof(Type.GetTypeFromHandle)));
            return IL.Return<Type?>();
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TypedReferenceLayout
    {
        public IntPtr Value;
        public IntPtr Type;
    }
#endif

    public static bool GetIsUnmanagedSlow(Type type)
    {
        return !(bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences), BindingFlags.Static | BindingFlags.Public)!
                                            .MakeGenericMethod(type)
                                            .Invoke(null, null)!;
    }

    /// <summary>
    /// Gets the types defined in the given assembly, except those which could not be loaded.
    /// </summary>
    [DebuggerStepThrough]
    public static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).ToArray()!;
        }
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
    public static readonly TypeCode UnderlyingEnumTypeCode = typeof(T).IsEnum ? Type.GetTypeCode(Enum.GetUnderlyingType(typeof(T))) : TypeCode.Empty;
}
