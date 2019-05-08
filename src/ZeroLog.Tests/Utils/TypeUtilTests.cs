using System;
using System.Runtime.InteropServices;
using NFluent;
using NUnit.Framework;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Utils
{
    public class TypeUtilTests
    {
        [Test]
        public void should_round_trip_enum()
        {
            var typeHandle = TypeUtil<DayOfWeek>.TypeHandle;
            var type = TypeUtil.GetTypeFromHandle(typeHandle);

            Check.That(type).IsEqualTo(typeof(DayOfWeek));
        }

        [Test]
        public void should_identify_unmanaged_types()
        {
            Check.That(IsUnmanaged<int>()).IsTrue();
            Check.That(IsUnmanaged<int?>()).IsTrue();

            Check.That(IsUnmanaged<string>()).IsFalse();

            Check.That(IsUnmanaged<DayOfWeek>()).IsTrue();
            Check.That(IsUnmanaged<DayOfWeek?>()).IsTrue();

            Check.That(IsUnmanaged<UnmanagedStruct>()).IsTrue();
            Check.That(IsUnmanaged<UnmanagedStruct?>()).IsTrue();

            Check.That(IsUnmanaged<UnmanagedStructNested>()).IsTrue();
            Check.That(IsUnmanaged<UnmanagedStructNested?>()).IsTrue();

            Check.That(IsUnmanaged<ManagedStruct>()).IsFalse();
            Check.That(IsUnmanaged<ManagedStruct?>()).IsFalse();

            Check.That(IsUnmanaged<ManagedStructNested>()).IsFalse();
            Check.That(IsUnmanaged<ManagedStructNested?>()).IsFalse();

            Check.That(IsUnmanaged<UnmanagedAutoLayoutStruct>()).IsTrue();
            Check.That(IsUnmanaged<UnmanagedAutoLayoutStruct?>()).IsTrue();

            bool IsUnmanaged<T>()
            {
                var genericResult = TypeUtil.GetIsUnmanagedSlow<T>();
                var nonGenericResult = TypeUtil.GetIsUnmanagedSlow(typeof(T));
                Check.That(nonGenericResult).IsEqualTo(genericResult);
                return genericResult;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UnmanagedStruct
        {
            public int Field;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UnmanagedStructNested
        {
            public UnmanagedStruct Field;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ManagedStruct
        {
            public string Field;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ManagedStructNested
        {
            public ManagedStruct Field;
        }

        [StructLayout(LayoutKind.Auto)]
        private struct UnmanagedAutoLayoutStruct
        {
            public int Field;
        }
    }
}
