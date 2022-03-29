using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        if (member.Element("inheritdoc") != null)
        {
            member.Elements("inheritdoc").Count().ShouldEqual(1);
            member.Elements().Count(i => i.Name.LocalName is not "inheritdoc").ShouldEqual(0);
        }
        else
        {
            member.Elements("summary").Count().ShouldEqual(1);
            member.Elements("remarks").Count().ShouldBeLessThanOrEqualTo(1);
        }

        foreach (var text in member.Elements().Select(i => i.Value.Trim()).Where(i => i.Length != 0))
        {
            if (!text.StartsWith("Default: "))
                text.ShouldEndWith(".");
        }
    }

    private static Dictionary<string, XElement> GetMembers()
    {
        if (_members != null)
            return _members;

        var xmlFilePath = Path.ChangeExtension(typeof(LogManager).Assembly.Location, ".xml");
        var members = XDocument.Load(xmlFilePath).Root!.Element("members")!.Elements("member");
        var membersDict = members.ToDictionary(i => i.Attribute("name")!.Value, i => i);

        _members ??= membersDict;
        return _members;
    }

    private static IEnumerable<ITestCaseData> GetDocumentedMembers()
        => GetMembers().Select(pair => new TestCaseData(pair.Value).SetCategory("Documentation").SetName(pair.Key));
}
