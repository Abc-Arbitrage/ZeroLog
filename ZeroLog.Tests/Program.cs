using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeroLog.Tests
{
    public class Program
    {
        public static void Main(string [] args)
        {
            var test = new IntegrationTests();

            test.SetUp();
            try
            {
                test.should_test_append();
            }
            finally
            {
                test.Teardown();
            }

            Console.ReadLine();
        }

        public static void Main2()
        {
            var method = typeof(Program).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Single(x => x.Name.Contains("AppendWithBoxing"));
            Console.WriteLine(method.GetMethodBody().GetILAsByteArray().Length);

            var memo = new Memo();
            var b = (byte)1;

            const int count = 1000000000;

            var start = Environment.TickCount;
            for (int i = 0; i < count; i++)
                AppendWithMakeref(b, memo);

            Console.WriteLine("Using __makeref:  {0} ticks", Environment.TickCount - start);

            start = Environment.TickCount;
            for (int i = 0; i < count; i++)
                AppendWithBoxing(b, memo);

            Console.WriteLine("Using boxing: {0} ticks", Environment.TickCount - start);

            Console.WriteLine(memo);
            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendWithMakeref<T>(T arg, Memo m)
        {
            m.Set(__refvalue(__makeref(arg), byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendWithBoxing<T>(T arg, Memo m)
        {
            m.Set((byte)(object)arg);
        }


        private class Memo
        {
            private byte _b;

            public void Set(byte b)
            {
                _b = b;
            }

            public override string ToString()
            {
                return $"{nameof(_b)}: {_b}";
            }
        }
    }
}