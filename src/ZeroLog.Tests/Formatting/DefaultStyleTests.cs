using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Shouldly;
using ZeroLog.Formatting;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class DefaultStyleTests
{
    [Test]
    [TestCaseSource(nameof(GetDefaultStyles))]
    public void should_be_valid(PropertyInfo property)
    {
        var style = property.GetValue(null).ShouldBeOfType<DefaultStyle>();
        var formatter = style.Formatter.ShouldBeOfType<DefaultFormatter>();
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
