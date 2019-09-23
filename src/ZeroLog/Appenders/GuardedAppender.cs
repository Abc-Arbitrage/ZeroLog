using System;
using System.Text;
using ZeroLog.Utils;

namespace ZeroLog.Appenders
{
    internal class GuardedAppender : IAppender
    {
        private readonly TimeSpan _quarantineDelay;
        private DateTime? _nextActivationTime;

        internal IAppender Appender { get; }

        public GuardedAppender(IAppender appender, TimeSpan quarantineDelay)
        {
            Appender = appender;
            _quarantineDelay = quarantineDelay;
            _nextActivationTime = null;
        }

        public string Name
        {
            get => Appender.Name;
            set => Appender.Name = value;
        }

        public void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            if (_nextActivationTime.HasValue && _nextActivationTime.Value > SystemDateTime.UtcNow)
                return;

            try
            {
                Appender.WriteEvent(logEventHeader, messageBytes, messageLength);
                _nextActivationTime = null;
            }
            catch (Exception)
            {
                _nextActivationTime = SystemDateTime.UtcNow + _quarantineDelay;
            }
        }

        public void SetEncoding(Encoding encoding)
            => Appender.SetEncoding(encoding);

        public void Flush()
            => Appender.Flush();

        public void Dispose()
            => Appender.Dispose();
    }
}
