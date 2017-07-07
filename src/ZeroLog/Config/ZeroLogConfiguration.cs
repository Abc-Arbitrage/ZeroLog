namespace ZeroLog.Config
{
    public class ZeroLogConfiguration
    {
        public int LogEventQueueSize { get; set; } = 1024;
        public int LogEventBufferSize { get; set; } = 128;

        public AppenderDefinition[] Appenders { get; set; } = new AppenderDefinition[0];

        public LoggerDefinition RootLogger { get; set; } = new LoggerDefinition();
        public LoggerDefinition[] Loggers { get; set; } = new LoggerDefinition[0];
    }
}
