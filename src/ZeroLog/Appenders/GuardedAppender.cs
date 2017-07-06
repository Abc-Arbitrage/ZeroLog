using System;
using System.Text;
using ZeroLog.Utils;

namespace ZeroLog.Appenders
{
    internal class GuardedAppender : IAppender
    {
        private readonly IAppender _appender;
        private readonly TimeSpan _quarantineDelay;
        private DateTime? _nextActivationTime;

        public GuardedAppender(IAppender appender, TimeSpan quarantineDelay)
        {
            _appender = appender;
            _quarantineDelay = quarantineDelay;
            _nextActivationTime = null;
        }

        public string Name
        {
            get => _appender.Name;
            set => _appender.Name = value;
        }

        public void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            if (_nextActivationTime.HasValue && _nextActivationTime.Value > SystemDateTime.UtcNow)
                return;

            try
            {
                _appender.WriteEvent(logEventHeader, messageBytes, messageLength);
                _nextActivationTime = null;
            }
            catch (Exception)
            {
                _nextActivationTime = SystemDateTime.UtcNow + _quarantineDelay;
            }
        }

        public void SetEncoding(Encoding encoding)
        {
            _appender.SetEncoding(encoding);
        }

        public void Close()
        {
            _appender.Close();
        }
    }
}