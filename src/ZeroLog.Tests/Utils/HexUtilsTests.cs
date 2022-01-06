using System;
using System.Text.Formatting;
using NUnit.Framework;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Utils;

[TestFixture]
public unsafe class HexUtilsTests
{
    // TODO

    // [Test]
    // public void should_append_value_as_hex_1()
    // {
    //     StringBuffer buffer = new StringBuffer(1024);
    //     int x = 0x1234abcd;
    //     var bytes = BitConverter.GetBytes(x);
    //     byte* x_ptr = (byte*)&x;
    //     HexUtils.AppendValueAsHex(buffer, x_ptr, sizeof(int));
    //     var s = buffer.ToString();
    //     var expected = BitConverter.IsLittleEndian ? "cdab3412" : "1234abcd";
    //     Assert.That(s, Is.EqualTo(expected));
    // }
    //
    // [Test]
    // public void should_append_value_as_hex_2()
    // {
    //     StringBuffer buffer = new StringBuffer(1024);
    //     int x = 0x01020304;
    //     var bytes = BitConverter.GetBytes(x);
    //     byte* x_ptr = (byte*)&x;
    //     HexUtils.AppendValueAsHex(buffer, x_ptr, sizeof(int));
    //     var s = buffer.ToString();
    //     var expected = BitConverter.IsLittleEndian ? "04030201" : "01020304";
    //     Assert.That(s, Is.EqualTo(expected));
    // }
    //
    // [Test]
    // public void should_append_value_as_hex_3()
    // {
    //     StringBuffer buffer = new StringBuffer(1024);
    //     int x = 0x10203040;
    //     var bytes = BitConverter.GetBytes(x);
    //     byte* x_ptr = (byte*)&x;
    //     HexUtils.AppendValueAsHex(buffer, x_ptr, sizeof(int));
    //     var s = buffer.ToString();
    //     var expected = BitConverter.IsLittleEndian ? "40302010" : "10203040";
    //     Assert.That(s, Is.EqualTo(expected));
    // }
}
