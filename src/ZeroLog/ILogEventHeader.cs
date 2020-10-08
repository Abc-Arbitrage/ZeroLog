using System;
using System.Threading;

namespace ZeroLog
{
    public interface ILogEventHeader
    {
        Level Level { get; }
        DateTime Timestamp { get; }
        Thread? Thread { get; }
        string Name { get; }
    }
}
