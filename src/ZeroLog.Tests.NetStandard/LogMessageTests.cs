using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Tests.NetStandard;

[TestFixture]
public class LogMessageTests
{
    [Test]
    public void should_return_empty_string()
    {
        LogMessage.Empty
                  .Append(42)
                  .ToString()
                  .ShouldBe(string.Empty);
    }
}
