using System;
using System.IO;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : AppenderBase<DefaultAppenderConfig>
    {
        public const string DefaultPrefixPattern = "%time - %level - %logger || ";

        private readonly Stream _output;

        public ConsoleAppender()
            : this(DefaultPrefixPattern)
        {
        }

        public ConsoleAppender(string prefixPattern)
        {
            _output = Console.OpenStandardOutput();
            Configure(prefixPattern);
        }

        public override void Configure(DefaultAppenderConfig parameters)
        {
            Configure(parameters?.PrefixPattern ?? DefaultPrefixPattern);
        }

        public override void WriteMessage(LogMessage message, byte[] messageBytes, int messageLength)
        {
            Console.BackgroundColor = ConsoleColor.Black;

            switch (message.Level)
            {
                case Level.Fatal:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                    break;
                case Level.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Level.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Level.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case Level.Debug:
                case Level.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            WriteMessageToStream(_output, message, messageBytes, messageLength);
        }
    }
}
