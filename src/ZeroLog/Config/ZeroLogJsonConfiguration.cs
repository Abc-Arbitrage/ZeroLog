namespace ZeroLog.Config
{
    public class ZeroLogJsonConfiguration : IHierarchicalConfiguration
    {
        public int LogMessagePoolSize { get; set; }
        public int LogMessageBufferSize { get; set; }
        public int LogMessageArgumentCapacity { get; set; }
        public bool LazyRegisterEnums { get; set; }
        public string? NullDisplayString { get; set; }
        public string? JsonSeparator { get; set; }

        public AppenderDefinition[] Appenders { get; set; } = new AppenderDefinition[0];

        public LoggerDefinition RootLogger { get; set; } = new LoggerDefinition();
        public LoggerDefinition[] Loggers { get; set; } = new LoggerDefinition[0];

        public ZeroLogJsonConfiguration()
        {
            ApplyInitializationConfig(new ZeroLogInitializationConfig());
        }

        internal void ApplyInitializationConfig(ZeroLogInitializationConfig config)
        {
            LogMessagePoolSize = config.LogMessagePoolSize;
            LogMessageBufferSize = config.LogMessageBufferSize;
            LogMessageArgumentCapacity = config.LogMessageArgumentCapacity;
        }

        internal ZeroLogInitializationConfig GetInitializationConfig()
            => new ZeroLogInitializationConfig
            {
                LogMessagePoolSize = LogMessagePoolSize,
                LogMessageBufferSize = LogMessageBufferSize,
                LogMessageArgumentCapacity = LogMessageArgumentCapacity,
            };
    }
}
