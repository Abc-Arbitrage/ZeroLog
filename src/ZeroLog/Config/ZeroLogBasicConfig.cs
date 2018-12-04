using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog.Config
{
    public class ZeroLogBasicConfig
    {
        public ICollection<IAppender> Appenders { get; set; } = new List<IAppender>();
        public Level Level { get; set; } = Level.Finest;
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; } = LogEventPoolExhaustionStrategy.Default;
        public LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; set; } = LogEventArgumentExhaustionStrategy.Default;

        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
        public int LogEventArgumentCapacity { get; set; }

        public ZeroLogBasicConfig()
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
