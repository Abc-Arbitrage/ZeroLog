namespace ZeroLog.Config
{
    public class ZeroLogConfiguration
    {
        public RootDefinition Root { get; set; }
        public AppenderDefinition[] Appenders { get; set; }
        public LoggerDefinition[] Loggers { get; set; }
    }
}
