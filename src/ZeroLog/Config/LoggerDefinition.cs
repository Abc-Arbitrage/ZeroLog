using System;

namespace ZeroLog.Config
{
    public class LoggerDefinition
    {
        public string? Name { get; set; }
        public Level Level { get; set; }
        public bool IncludeParentAppenders { get; set; }
        public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; }
        public string[] AppenderReferences { get; set; } = Array.Empty<string>();

        public LoggerDefinition()
        {
        }

        public LoggerDefinition(string name, Level level, bool includeParentAppenders, LogMessagePoolExhaustionStrategy logMessagePoolExhaustionStrategy, params string[] appenderReferences)
        {
            Name = name;
            Level = level;
            IncludeParentAppenders = includeParentAppenders;
            LogMessagePoolExhaustionStrategy = logMessagePoolExhaustionStrategy;
            AppenderReferences = appenderReferences;
        }
    }
}
