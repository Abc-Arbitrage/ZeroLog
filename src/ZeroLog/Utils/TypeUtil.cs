using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using JetBrains.Annotations;
using static InlineIL.IL.Emit;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Utils
{
    internal static class TypeUtil
    {
        private static readonly Func<IntPtr, Type> _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

        public static IntPtr GetTypeHandleSlow(Type type)
            => type?.TypeHandle.Value ?? IntPtr.Zero;

        public static Type GetTypeFromHandle(IntPtr typeHandle)
            => _getTypeFromHandleFunc?.Invoke(typeHandle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            Ldarg(nameof(source));
            return ref IL.ReturnRef<TTo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            Sizeof(typeof(T));
            return IL.Return<int>();
        }

        private static Func<IntPtr, Type> BuildGetTypeFromHandleFunc()
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

        public static bool IsReferenceOrContainsReferences<T>()
            => !ByTypeValues<T>.IsPinnable;

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static class ByTypeValues<T>
        {
            public static bool IsPinnable { get; } = GetIsPinnable();

            private static bool GetIsPinnable()
            {
                if (default(T) == null)
                    return false;

                if (typeof(T).IsPrimitive)
                    return true;

                if (typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(f => !f.FieldType.IsValueType))
                    return false;

                try
                {
                    GCHandle.Alloc(default(T), GCHandleType.Pinned).Free();
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
        }
    }

    internal static class TypeUtil<T>
    {
        public static readonly IntPtr TypeHandle = TypeUtil.GetTypeHandleSlow(typeof(T));
        public static readonly bool IsEnum = typeof(T).IsEnum;
    }

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal static class TypeUtilNullable<T>
    {
        // Nullable-specific properties, initializing this type will allocate

        [CanBeNull]
        private static readonly Type _underlyingType = Nullable.GetUnderlyingType(typeof(T));

        public static readonly bool IsNullableEnum = _underlyingType?.IsEnum == true;
        public static readonly IntPtr UnderlyingTypeHandle = TypeUtil.GetTypeHandleSlow(_underlyingType);
        public static readonly TypeCode UnderlyingTypeCode = Type.GetTypeCode(_underlyingType);
    }
}
