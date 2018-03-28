using System;
using NFluent;
using NUnit.Framework;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Utils
{
    public class EnumUtilTests
    {
        [Test]
        public void should_round_trip_enum()
        {
            var typeHandle = EnumUtil.GetTypeHandle<DayOfWeek>();
            var type = EnumUtil.GetTypeFromHandle(typeHandle);

            Check.That(type).Equals(typeof(DayOfWeek));
        }

        [Test]
        public void should_not_crash_on_invalid_handle()
        {
            Check.That(EnumUtil.GetTypeFromHandle(IntPtr.Zero)).IsNull();
        }
    }
}
