#if NET8_0_OR_GREATER

using System;

namespace ZeroLog.Tests.Support;

internal class TestTimeProvider : TimeProvider
{
    public static DateTime ExampleTimestamp { get; } = new(2020, 1, 2, 3, 4, 5, 6, 7);

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public override DateTimeOffset GetUtcNow()
        => Timestamp;
}

#endif
