using System;

namespace ZeroLog
{
    public interface ILogEventHeader
    {
        Level Level { get; }
        DateTime Timestamp { get; }
        int ThreadId { get; }
        string Name { get; }
    }
}