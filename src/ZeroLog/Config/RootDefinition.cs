namespace ZeroLog.Config
{
    public class RootDefinition
    {
        public int LogEventQueueSize { get; set; } = 10;
        public int LogEventBufferSize { get; set; } = 10;
        public Level DefaultLevel { get; set; }
        public LogEventPoolExhaustionStrategy DefaultLogEventPoolExhaustionStrategy { get; set; }
        public string[] AppenderReferences { get; set; }
    }
}
