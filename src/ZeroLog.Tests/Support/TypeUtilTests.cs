using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using ZeroLog.Support;

namespace ZeroLog.Tests.Support;

public class TypeUtilTests
{
    [Test]
    public void should_round_trip_enum()
    {
        var typeHandle = TypeUtil<DayOfWeek>.TypeHandle;
        var type = TypeUtil.GetTypeFromHandle(typeHandle);

        type.ShouldEqual(typeof(DayOfWeek));
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
