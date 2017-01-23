using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ZeroLog.Appenders
{
    class PerformanceAppender : IAppender
    {
        private readonly MessageReceived[] _messages;
        private int _count;

        public PerformanceAppender(int expectedEntries)
        {
            _messages = new MessageReceived[expectedEntries];
            for (int i = 0; i < expectedEntries; i++)
            {
                _messages[i] = new MessageReceived(new byte[30]);
            }
        }

        public void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            Array.Copy(messageBytes, _messages[_count].StartTimestampInChars, messageLength);
            _messages[_count].MessageLength = messageLength;
            _messages[_count].EndTimestamp = Stopwatch.GetTimestamp();
            _count++;
        }

        public void SetEncoding(Encoding encoding)
        {
        }

        public void Close()
        {
        }

        private struct MessageReceived
        {
            public readonly byte[] StartTimestampInChars;
            public int MessageLength;
            public long EndTimestamp;

            public MessageReceived(byte[] startTimestampInChars)
            {
                StartTimestampInChars = startTimestampInChars;
                EndTimestamp = 0;
                MessageLength = 0;
            }
        }

        public void PrintTimeTaken()
        {
            var totalTimeCsv = "total-time.csv";
            if (File.Exists(totalTimeCsv))
                File.Delete(totalTimeCsv);

            using (var fileStream = new StreamWriter(File.OpenWrite(totalTimeCsv)))
            {
                for (int i = 0; i < _count; i++)
                {
                    var messageReceived = _messages[i];
                    var timestampString = Encoding.Default.GetString(messageReceived.StartTimestampInChars, 0, messageReceived.MessageLength);
                    var startTime = long.Parse(timestampString);
                    fileStream.WriteLine(ToMicroseconds(messageReceived.EndTimestamp - startTime));
                }
            }
        }

        private static double ToMicroseconds(long ticks)
        {
            return unchecked(ticks * 1000000 / (double)(Stopwatch.Frequency));
        }
    }
}