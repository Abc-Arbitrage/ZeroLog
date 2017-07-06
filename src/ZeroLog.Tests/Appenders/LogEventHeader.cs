using System;

namespace ZeroLog.Tests.Appenders
{
    public class LogEventHeader : ILogEventHeader
    {
        public Level Level { get; set; }
        public DateTime Timestamp { get; set; }
        public int ThreadId { get; set; }
        public string Name { get; set; }
    }
}