namespace ZeroLog
{
    public class ZeroLogInitializationConfig
    {
        public int LogEventQueueSize { get; set; } = 1024;
        public int LogEventBufferSize { get; set; } = 128;
        public int LogEventArgumentCapacity { get; set; } = 32;
    }
}
