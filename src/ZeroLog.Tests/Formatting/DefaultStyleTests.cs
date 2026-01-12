using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class DefaultStyleTests
{
    [Test]
    [TestCaseSource(nameof(GetDefaultStyles))]
    public void should_be_valid(PropertyInfo property)
    {
        var style = property.GetValue(null).ShouldBe<DefaultStyle>();
        var formatter = style.Formatter.ShouldBe<DefaultFormatter>();
        PatternWriter.IsValidPattern(formatter.MessagePatternWriter.Pattern).ShouldBeTrue();
    }

    private static IEnumerable<ITestCaseData> GetDefaultStyles()
    {
        return GetStyles(typeof(DefaultStyle));

        static IEnumerable<ITestCaseData> GetStyles(Type type)
            => type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                   .Where(p => p.PropertyType == typeof(DefaultStyle))
                   .Select(i => new TestCaseData(i).SetName($"{i.DeclaringType?.FullName}.{i.Name}"))
                   .Concat(
                       type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
                           .SelectMany(GetStyles)
                   );
    }
}
