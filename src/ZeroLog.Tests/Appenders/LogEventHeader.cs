using System;
using System.Threading;

namespace ZeroLog.Tests.Appenders
{
    public class LogEventHeader : ILogEventHeader
    {
        public Level Level { get; set; }
        public DateTime Timestamp { get; set; }
        public Thread Thread { get; set; }
        public string Name { get; set; }
    }
}
