using System;
using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Tests;

[TestFixture]
public class LogManagerEnumTests
{
    [Test]
    public void should_register_all_assembly_enums()
    {
        EnumCache.IsRegistered(typeof(ConsoleColor)).ShouldBeFalse();
        LogManager.RegisterAllEnumsFrom(typeof(ConsoleColor).Assembly);
        EnumCache.IsRegistered(typeof(ConsoleColor)).ShouldBeTrue();
    }
}
