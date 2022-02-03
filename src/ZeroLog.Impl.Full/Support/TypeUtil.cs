using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Support
{
    internal static class TypeUtil
    {
        private static readonly Func<IntPtr, Type>? _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

        public static IntPtr GetTypeHandleSlow(Type? type)
            => type?.TypeHandle.Value ?? IntPtr.Zero;

        public static Type? GetTypeFromHandle(IntPtr typeHandle)
            => _getTypeFromHandleFunc?.Invoke(typeHandle);

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
        public static readonly bool IsEnum = typeof(T).IsEnum;
    }

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal static class TypeUtilSlow<T>
    {
        // Initializing this type will allocate

        private static readonly Type? _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static readonly bool IsNullableEnum = _underlyingType?.IsEnum == true;
        public static readonly IntPtr UnderlyingTypeHandle = TypeUtil.GetTypeHandleSlow(_underlyingType);
        public static readonly TypeCode UnderlyingTypeCode = Type.GetTypeCode(_underlyingType);
    }
}
