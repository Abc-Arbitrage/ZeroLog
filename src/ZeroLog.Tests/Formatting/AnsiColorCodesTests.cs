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

    [Test]
    [TestCase("reset", 0)]
    [TestCase(" reset ", 0)]
    [TestCase("bold", 1)]
    [TestCase("red", 31)]
    [TestCase("blue", 34)]
    [TestCase("dark blue", 34)]
    [TestCase("bright blue", 94)]
    [TestCase(" bright blue ", 94)]
    [TestCase("foreground blue", 34)]
    [TestCase("foreground dark blue", 34)]
    [TestCase("foreground bright blue", 94)]
    [TestCase("fg blue", 34)]
    [TestCase("fg dark blue", 34)]
    [TestCase("fg bright blue", 94)]
    [TestCase("background blue", 44)]
    [TestCase("background dark blue", 44)]
    [TestCase("background bright blue", 104)]
    [TestCase("bg blue", 44)]
    [TestCase("bg dark blue", 44)]
    [TestCase("bg bright blue", 104)]
    public void should_parse_sgr_codes(string input, int expectedResult)
    {
        AnsiColorCodes.TryParseSgrCode(input, out var result).ShouldBeTrue();
        result.ShouldEqual(expectedResult);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("foo")]
    [TestCase("blue blue")]
    [TestCase("dark bright blue")]
    [TestCase("dark dark blue")]
    [TestCase("bright bright blue")]
    [TestCase("fg bg blue")]
    [TestCase("fg fg blue")]
    [TestCase("bg fg blue")]
    [TestCase("bg bg blue")]
    [TestCase("foreground background blue")]
    [TestCase("background foreground")]
    [TestCase("red blue")]
    [TestCase("reversed blue")]
    public void should_not_parse_sgr_codes(string input)
        => AnsiColorCodes.TryParseSgrCode(input, out _).ShouldBeFalse();
}
