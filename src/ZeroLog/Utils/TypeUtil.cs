using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Utils
{
    internal static class TypeUtil
    {
        private static readonly Func<IntPtr, Type> _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetTypeHandle<T>()
            => Cache<T>.TypeHandle;

        public static IntPtr GetTypeHandleSlow(Type type)
            => type.TypeHandle.Value;

        public static Type GetTypeFromHandle(IntPtr typeHandle)
            => _getTypeFromHandleFunc?.Invoke(typeHandle);

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

        private struct Cache<T>
        {
            public static readonly IntPtr TypeHandle = GetTypeHandleSlow(typeof(T));
        }
    }
}
