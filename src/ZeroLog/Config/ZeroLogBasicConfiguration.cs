using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog.Config
{
    public class ZeroLogBasicConfiguration
    {
        public ICollection<IAppender> Appenders { get; set; } = new List<IAppender>();
        public Level Level { get; set; } = Level.Trace;
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; } = LogEventPoolExhaustionStrategy.Default;
        public LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; set; } = LogEventArgumentExhaustionStrategy.Default;

        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
        public int LogEventArgumentCapacity { get; set; }

        public ZeroLogBasicConfiguration()
        {
            ApplyInitializationConfig(new ZeroLogInitializationConfig());
        }

        internal void ApplyInitializationConfig(ZeroLogInitializationConfig config)
        {
            LogEventQueueSize = config.LogEventQueueSize;
            LogEventBufferSize = config.LogEventBufferSize;
            LogEventArgumentCapacity = config.LogEventArgumentCapacity;
        }

        internal ZeroLogInitializationConfig ToInitializationConfig()
        {
            return new ZeroLogInitializationConfig
            {
                LogEventQueueSize = LogEventQueueSize,
                LogEventBufferSize = LogEventBufferSize,
                LogEventArgumentCapacity = LogEventArgumentCapacity
            };
        }
    }
}
