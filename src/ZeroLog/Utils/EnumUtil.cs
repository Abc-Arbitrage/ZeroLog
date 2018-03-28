using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtraConstraints;
using static System.Linq.Expressions.Expression;

namespace ZeroLog.Utils
{
    internal static class EnumUtil
    {
        private static readonly Func<IntPtr, Type> _getTypeFromHandleFunc = BuildGetTypeFromHandleFunc();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetTypeHandle<[EnumConstraint] T>()
            where T : struct
            => Cache<T>.TypeHandle;

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
            where T : struct
        {
            public static readonly IntPtr TypeHandle = typeof(T).TypeHandle.Value;
        }
    }
}
