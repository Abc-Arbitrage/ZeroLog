﻿using System;
using System.Runtime.CompilerServices;
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

    [Test]
    public void should_identify_unmanaged_types()
    {
        IsUnmanaged<int>().ShouldBeTrue();
        IsUnmanaged<int?>().ShouldBeTrue();

        IsUnmanaged<string>().ShouldBeFalse();

        IsUnmanaged<DayOfWeek>().ShouldBeTrue();
        IsUnmanaged<DayOfWeek?>().ShouldBeTrue();

        IsUnmanaged<UnmanagedStruct>().ShouldBeTrue();
        IsUnmanaged<UnmanagedStruct?>().ShouldBeTrue();

        IsUnmanaged<UnmanagedStructNested>().ShouldBeTrue();
        IsUnmanaged<UnmanagedStructNested?>().ShouldBeTrue();

        IsUnmanaged<ManagedStruct>().ShouldBeFalse();
        IsUnmanaged<ManagedStruct?>().ShouldBeFalse();

        IsUnmanaged<ManagedStructNested>().ShouldBeFalse();
        IsUnmanaged<ManagedStructNested?>().ShouldBeFalse();

        IsUnmanaged<UnmanagedAutoLayoutStruct>().ShouldBeTrue();
        IsUnmanaged<UnmanagedAutoLayoutStruct?>().ShouldBeTrue();

        IsUnmanaged<GenericStruct<GenericStruct<int>>>().ShouldBeTrue();
        IsUnmanaged<GenericStruct<GenericStruct<int?>>?>().ShouldBeTrue();

        IsUnmanaged<GenericStruct<GenericStruct<string>>>().ShouldBeFalse();
        IsUnmanaged<GenericStruct<GenericStruct<string>>?>().ShouldBeFalse();

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
