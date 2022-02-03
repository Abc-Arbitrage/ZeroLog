using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NFluent;
using NUnit.Framework;
using ZeroLog.Tests.Support;
using ZeroLog.Support;

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

            Check.That(IsUnmanaged<GenericStruct<GenericStruct<int>>>()).IsTrue();
            Check.That(IsUnmanaged<GenericStruct<GenericStruct<int?>>?>()).IsTrue();

            Check.That(IsUnmanaged<GenericStruct<GenericStruct<string>>>()).IsFalse();
            Check.That(IsUnmanaged<GenericStruct<GenericStruct<string>>?>()).IsFalse();

            bool IsUnmanaged<T>()
            {
                var expectedResult = !RuntimeHelpers.IsReferenceOrContainsReferences<T>();
                TypeUtil.GetIsUnmanagedSlow(typeof(T)).ShouldEqual(expectedResult);
                return expectedResult;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct UnmanagedStruct
        {
            public int Field;
            public int* Field2;
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

        [StructLayout(LayoutKind.Auto)]
        private struct GenericStruct<T>
        {
            public T Field;
        }
    }
}
