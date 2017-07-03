namespace ZeroLog.Config
{
    public class RootDefinition
    {
        public Level DefaultLevel { get; set; }
        public LogEventPoolExhaustionStrategy DefaultLogEventPoolExhaustionStrategy { get; set; }

        public string[] AppenderReferences { get; set; }

        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
    }
}
