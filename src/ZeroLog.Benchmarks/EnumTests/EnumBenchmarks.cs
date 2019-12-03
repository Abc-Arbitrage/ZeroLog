using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using InlineIL;
using static InlineIL.IL.Emit;

namespace ZeroLog.Benchmarks.EnumTests
{
    public static class EnumBenchmarksRunner
    {
        public static void Run()
        {
            Validate();
            BenchmarkRunner.Run<EnumBenchmarks>();
        }

        private static void Validate()
        {
            var benchmarks = new EnumBenchmarks();
            var expected = benchmarks.Typeof();

            if (benchmarks.TypeofCached() != expected)
                throw new InvalidOperationException();

            if (benchmarks.TypedRef() != expected)
                throw new InvalidOperationException();

            if (benchmarks.TypeHandleIl() != expected)
                throw new InvalidOperationException();
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48), SimpleJob(RuntimeMoniker.NetCoreApp30)]
    public unsafe class EnumBenchmarks
    {
        [Benchmark(Baseline = true)]
        public IntPtr Typeof() => TypeofImpl<SomeEnum>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr TypeofImpl<T>()
            where T : struct
        {
            return typeof(T).TypeHandle.Value;
        }

        [Benchmark]
        public IntPtr TypeofCached() => TypeofCachedImpl<SomeEnum>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr TypeofCachedImpl<T>()
            where T : struct
        {
            return Cache<T>.TypeHandle;
        }

        private struct Cache<T>
        {
            public static readonly IntPtr TypeHandle = typeof(T).TypeHandle.Value;
        }

        [Benchmark]
        public IntPtr TypedRef() => TypedRefImpl<SomeEnum>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr TypedRefImpl<T>()
            where T : struct
        {
            var value = default(T);
            var typedRef = __makeref(value);
            return ((IntPtr*)&typedRef)[1];
        }

        [Benchmark]
        public IntPtr TypeHandleIl() => TypeHandleIlImpl<SomeEnum>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr TypeHandleIlImpl<T>()
            where T : struct
        {
            IL.DeclareLocals(
                false,
                new LocalVar(typeof(RuntimeTypeHandle))
            );

            Ldtoken(typeof(T));
            Stloc_0();
            Ldloca_S(0);
            Call(new MethodRef(typeof(RuntimeTypeHandle), "get_" + nameof(RuntimeTypeHandle.Value)));
            return IL.Return<IntPtr>();
        }
    }

    public enum SomeEnum
    {
        Foo,
        Bar,
        Baz
    }
}
