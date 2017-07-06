namespace ZeroLog.Config
{
    public class LoggerDefinition
    {
        public string Name { get; set; }
        public Level Level { get; set; }
        public bool IncludeParentAppenders { get; set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; }
        public string[] AppenderReferences { get; set; }
    }
}