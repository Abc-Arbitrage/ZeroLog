using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog.Config
{
    public class ZeroLogBasicConfiguration
    {
        public ICollection<Appender> Appenders { get; set; } = new List<Appender>();
        public Level Level { get; set; } = Level.Trace;
        public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; } = LogMessagePoolExhaustionStrategy.Default;

        public int LogMessagePoolSize { get; set; }
        public int LogMessageBufferSize { get; set; }
        public int LogMessageArgumentCapacity { get; set; }

        public ZeroLogBasicConfiguration()
        {
            ApplyInitializationConfig(new ZeroLogInitializationConfig());
        }

        internal void ApplyInitializationConfig(ZeroLogInitializationConfig config)
        {
            LogMessagePoolSize = config.LogMessagePoolSize;
            LogMessageBufferSize = config.LogMessageBufferSize;
            LogMessageArgumentCapacity = config.LogMessageArgumentCapacity;
        }

        internal ZeroLogInitializationConfig ToInitializationConfig()
        {
            return new ZeroLogInitializationConfig
            {
                LogMessagePoolSize = LogMessagePoolSize,
                LogMessageBufferSize = LogMessageBufferSize,
                LogMessageArgumentCapacity = LogMessageArgumentCapacity
            };
        }
    }
}
