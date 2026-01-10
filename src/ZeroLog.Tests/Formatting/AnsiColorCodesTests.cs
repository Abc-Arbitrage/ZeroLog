using NUnit.Framework;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class AnsiColorCodesTests
{
    [Test]
    [TestCase("", false)]
    [TestCase("foo", false)]
    [TestCase("\e[31mfoo", true)]
    public void should_detect_ansi_codes(string value, bool expectedResult)
        => AnsiColorCodes.HasAnsiCode(value).ShouldEqual(expectedResult);

    [Test]
    [TestCase("", "")]
    [TestCase("foo", "foo")]
    [TestCase("foo\e[31mbar\e[0mbaz", "foobarbaz")]
    public void should_remove_ansi_codes(string value, string expectedResult)
        => AnsiColorCodes.RemoveAnsiCodes(value).ShouldEqual(expectedResult);

    [Test]
    [TestCase("", 0)]
    [TestCase("foo", 3)]
    [TestCase("foo\e[31mbar\e[0mbaz", 9)]
    public void should_calculate_length_without_ansi_codes(string value, int expectedResult)
        => AnsiColorCodes.LengthWithoutAnsiCodes(value).ShouldEqual(expectedResult);
}
