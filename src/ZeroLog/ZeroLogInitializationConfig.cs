using System;

namespace ZeroLog
{
    public class ZeroLogInitializationConfig
    {
        public int LogMessagePoolSize { get; set; } = 1024;
        public int LogMessageBufferSize { get; set; } = 128;
        public int LogMessageArgumentCapacity { get; set; } = 32;

        internal void Validate()
        {
            if (LogMessagePoolSize <= 0)
                throw new InvalidOperationException($"{nameof(LogMessagePoolSize)} must be positive");

            if (LogMessageBufferSize <= 0)
                throw new InvalidOperationException($"{nameof(LogMessageBufferSize)} must be positive");

            if (LogMessageArgumentCapacity <= 0)
                throw new InvalidOperationException($"{nameof(LogMessageArgumentCapacity)} must be positive");
        }
    }
}
