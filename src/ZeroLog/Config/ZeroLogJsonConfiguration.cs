namespace ZeroLog.Config
{
    public class ZeroLogJsonConfiguration : IHierarchicalConfiguration
    {
        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
        public int LogEventArgumentCapacity { get; set; }
        public bool LazyRegisterEnums { get; set; }
        public string? NullDisplayString { get; set; }

        public AppenderDefinition[] Appenders { get; set; } = new AppenderDefinition[0];

        public LoggerDefinition RootLogger { get; set; } = new LoggerDefinition();
        public LoggerDefinition[] Loggers { get; set; } = new LoggerDefinition[0];

        public ZeroLogJsonConfiguration()
        {
            ApplyInitializationConfig(new ZeroLogInitializationConfig());
        }

        internal void ApplyInitializationConfig(ZeroLogInitializationConfig config)
        {
            LogEventQueueSize = config.LogEventQueueSize;
            LogEventBufferSize = config.LogEventBufferSize;
            LogEventArgumentCapacity = config.LogEventArgumentCapacity;
        }

        internal ZeroLogInitializationConfig GetInitializationConfig()
            => new ZeroLogInitializationConfig
            {
                LogEventQueueSize = LogEventQueueSize,
                LogEventBufferSize = LogEventBufferSize,
                LogEventArgumentCapacity = LogEventArgumentCapacity,
            };
    }
}
