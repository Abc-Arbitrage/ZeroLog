using System;

namespace ZeroLog
{
    public class ZeroLogInitializationConfig
    {
        public int LogEventQueueSize { get; set; } = 1024;
        public int LogEventBufferSize { get; set; } = 128;
        public int LogEventArgumentCapacity { get; set; } = 32;

        internal void Validate()
        {
            if (LogEventQueueSize <= 0)
                throw new InvalidOperationException($"{nameof(LogEventQueueSize)} must be positive");

            if (LogEventBufferSize <= 0)
                throw new InvalidOperationException($"{nameof(LogEventBufferSize)} must be positive");

            if (LogEventArgumentCapacity <= 0)
                throw new InvalidOperationException($"{nameof(LogEventArgumentCapacity)} must be positive");
        }
    }
}
