namespace ZeroLog.Config
{
    public interface IHierarchicalConfiguration
    {
        AppenderDefinition[] Appenders { get; }
        LoggerDefinition RootLogger { get; }
        LoggerDefinition[] Loggers { get; }
    }
}
