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
    [TestCase("reset", "0")]
    [TestCase(" reset ", "0")]
    [TestCase("normal", "0")]
    [TestCase("default", "0")]
    [TestCase("bold", "1")]
    [TestCase("red", "31")]
    [TestCase("blue", "34")]
    [TestCase("dark blue", "34")]
    [TestCase("bright blue", "94")]
    [TestCase(" bright blue ", "94")]
    [TestCase("foreground blue", "34")]
    [TestCase("foreground dark blue", "34")]
    [TestCase("foreground bright blue", "94")]
    [TestCase("fg blue", "34")]
    [TestCase("fg dark blue", "34")]
    [TestCase("fg bright blue", "94")]
    [TestCase("background blue", "44")]
    [TestCase("background dark blue", "44")]
    [TestCase("background bright blue", "104")]
    [TestCase("bg blue", "44")]
    [TestCase("bg dark blue", "44")]
    [TestCase("bg bright blue", "104")]
    [TestCase("default foreground", "39")]
    [TestCase("default bg", "49")]
    [TestCase("bold", "1")]
    [TestCase("not italic", "23")]
    [TestCase("gray", "90")]
    [TestCase("gray bg", "100")]
    [TestCase("42", "42")]
    [TestCase("41, 42, 43", "41;42;43")]
    [TestCase("reset, bold, red, dark blue background", "0;1;31;44")]
    [TestCase("#102030", "38;2;16;32;48")]
    [TestCase("foreground #102030", "38;2;16;32;48")]
    [TestCase("#102030 bg", "48;2;16;32;48")]
    [TestCase("background #102030", "48;2;16;32;48")]
    [TestCase("reset, bold, #102030 foreground, #405060 background", "0;1;38;2;16;32;48;48;2;64;80;96")]
    public void should_parse_sgr_codes(string input, string expectedResult)
    {
        AnsiColorCodes.TryParse(input, out var result).ShouldBeTrue();
        result.ShouldStartWith("\e[");
        result.ShouldEndWith("m");
        result.Substring(2, result.Length - 3).ShouldEqual(expectedResult);
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
    [TestCase("bright")]
    [TestCase("dark")]
    [TestCase("reversed blue")]
    [TestCase("default blue foreground")]
    [TestCase("default blue")]
    [TestCase("default bright")]
    [TestCase("default default")]
    [TestCase("gray gray")]
    [TestCase("42 42")]
    [TestCase("#102030 #405060")]
    [TestCase("#102030 blue")]
    [TestCase("blue #102030")]
    [TestCase("#102030 dark")]
    [TestCase("#102030 dark blue")]
    [TestCase("#102030 bright")]
    [TestCase("#102030 bright blue")]
    [TestCase("dark #102030")]
    [TestCase("bright #102030")]
    [TestCase("fg #102030 bg")]
    [TestCase("default #FFA0A0")]
    [TestCase("FFA0A0")]
    [TestCase("#FFA0A00")]
    [TestCase("#00G000")]
    public void should_not_parse_sgr_codes(string input)
        => AnsiColorCodes.TryParse(input, out _).ShouldBeFalse();
}
