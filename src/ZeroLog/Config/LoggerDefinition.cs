namespace ZeroLog.Config
{
    public class LoggerDefinition
    {
        public string Name { get; set; }
        public Level Level { get; set; }
        public bool IncludeParentAppenders { get; set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; }
        public string[] AppenderReferences { get; set; }

        public LoggerDefinition()
        {
        }

        public LoggerDefinition(string name, Level level, bool includeParentAppenders, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, params string[] appenderReferences)
        {
            Name = name;
            Level = level;
            IncludeParentAppenders = includeParentAppenders;
            LogEventPoolExhaustionStrategy = logEventPoolExhaustionStrategy;
            AppenderReferences = appenderReferences;
        }
    }
}