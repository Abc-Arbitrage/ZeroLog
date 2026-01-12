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
    [TestCase("1\e[0m👩🏽‍🚒3👨🏼‍👩🏽‍👧‍👦🏻5👩‍👧‍👦7", 7)]
    public void should_calculate_visible_text_length(string value, int expectedResult)
        => AnsiColorCodes.GetVisibleTextLength(value).ShouldEqual(expectedResult);
}
