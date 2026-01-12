using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class DocumentationTests
{
    private static Dictionary<string, XElement> _members;

    [Test]
    [TestCaseSource(nameof(GetDocumentedMembers))]
    public void should_have_valid_documentation(XElement member)
    {
        if (member.Element("inheritdoc") == null)
        {
            member.Elements("summary").Count().ShouldEqual(1);
            member.Elements("remarks").Count().ShouldBeLessThanOrEqualTo(1);
        }

        foreach (var elem in member.Elements())
        {
            var text = Regex.Replace(elem.Value, @"^\s*Default:.*", "", RegexOptions.Multiline).Trim();

            if (text.Length != 0)
                text.ShouldEndWith(".");
        }
    }

    private static Dictionary<string, XElement> GetMembers()
    {
        if (_members != null)
            return _members;

        var xmlFilePath = Path.ChangeExtension(typeof(LogManager).Assembly.Location, ".xml");
        var members = XDocument.Load(xmlFilePath).Root!.Element("members")!.Elements("member");
        var membersDict = members.Select(i => (key: i.Attribute("name")!.Value, elem: i))
                                 .Where(i => FilterKey(i.key))
                                 .ToDictionary(i => i.key, i => i.elem);

        membersDict.Remove("P:System.Text.RegularExpressions.Generated.Utilities.WordCharBitmap");

        _members ??= membersDict;
        return _members;

        static bool FilterKey(string key)
        {
            if (key.EndsWith("Regex"))
                return false;

            if (key.StartsWith("P:System"))
                return false;

            return true;
        }
    }

    private static IEnumerable<ITestCaseData> GetDocumentedMembers()
        => GetMembers().Select(pair => new TestCaseData(pair.Value).SetCategory("Documentation").SetName(pair.Key));
}
