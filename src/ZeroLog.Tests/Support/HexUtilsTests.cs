using System;
using NUnit.Framework;
using ZeroLog.Formatting;

namespace ZeroLog.Tests.Support;

[TestFixture]
public unsafe class HexUtilsTests
{
    [Test]
    public void should_append_value_as_hex_1()
    {
        Span<char> buffer = new char[2 * sizeof(int)];
        var x = 0x1234abcd;
        var xPtr = (byte*)&x;
        HexUtils.AppendValueAsHex(xPtr, sizeof(int), buffer);
        var s = buffer.ToString();
        var expected = BitConverter.IsLittleEndian ? "cdab3412" : "1234abcd";
        Assert.That(s, Is.EqualTo(expected));
    }

    [Test]
    public void should_append_value_as_hex_2()
    {
        Span<char> buffer = new char[2 * sizeof(int)];
        var x = 0x01020304;
        var xPtr = (byte*)&x;
        HexUtils.AppendValueAsHex(xPtr, sizeof(int), buffer);
        var s = buffer.ToString();
        var expected = BitConverter.IsLittleEndian ? "04030201" : "01020304";
        Assert.That(s, Is.EqualTo(expected));
    }

    [Test]
    public void should_append_value_as_hex_3()
    {
        Span<char> buffer = new char[2 * sizeof(int)];
        var x = 0x10203040;
        var xPtr = (byte*)&x;
        HexUtils.AppendValueAsHex(xPtr, sizeof(int), buffer);
        var s = buffer.ToString();
        var expected = BitConverter.IsLittleEndian ? "40302010" : "10203040";
        Assert.That(s, Is.EqualTo(expected));
    }
}
