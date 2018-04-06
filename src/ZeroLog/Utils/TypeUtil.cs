using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using InlineIL;
using JetBrains.Annotations;
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

        public static int SizeOfSlow(Type type)
        {
            if (type == null)
                return 0;

            // ReSharper disable once PossibleNullReferenceException
            return (int)typeof(TypeUtil).GetMethod(nameof(SizeOf), BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null)
                                        .MakeGenericMethod(type)
                                        .Invoke(null, ArrayUtil.Empty<object>());
        }

        private static int SizeOf<T>()
        {
            IL.Emit(OpCodes.Sizeof, typeof(T));
            return IL.Return<int>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>([UsedImplicitly] ref TFrom source)
        {
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ret);
            throw IL.Unreachable();
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
    }

    internal static class TypeUtil<T>
    {
        public static readonly IntPtr TypeHandle = TypeUtil.GetTypeHandleSlow(typeof(T));
        public static readonly IntPtr TypeHandleNullableUnwrapped = TypeUtil.GetTypeHandleSlow(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        public static readonly bool IsEnum = typeof(T).IsEnum;
        public static readonly bool IsNullableEnum = Nullable.GetUnderlyingType(typeof(T))?.IsEnum == true;
        public static readonly int SizeOfNullableValue = TypeUtil.SizeOfSlow(Nullable.GetUnderlyingType(typeof(T)));
    }
}
