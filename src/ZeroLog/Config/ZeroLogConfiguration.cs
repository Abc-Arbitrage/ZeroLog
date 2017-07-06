namespace ZeroLog.Config
{
    public class ZeroLogConfiguration
    {
        public RootDefinition Root { get; set; } = new RootDefinition();
        public AppenderDefinition[] Appenders { get; set; } = new AppenderDefinition[0];
        public LoggerDefinition[] Loggers { get; set; } = new LoggerDefinition[0];
    }
}
