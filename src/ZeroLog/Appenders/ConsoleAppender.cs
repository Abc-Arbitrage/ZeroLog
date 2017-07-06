using System;
using System.IO;
using ZeroLog.Appenders.Builders;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : AppenderBase<DefaultAppenderConfig>
    {
        public const string DefaultPrefixPattern = "%time - %level - %logger || ";

        private readonly Stream _output;

        public ConsoleAppender()
        {
            _output = Console.OpenStandardOutput();
        }

        public ConsoleAppender(string prefixPattern = DefaultPrefixPattern)
            : this()
        {
            Configure(prefixPattern);
        }

        public override void Configure(DefaultAppenderConfig parameters)
        {
            Configure(parameters?.PrefixPattern ?? DefaultPrefixPattern);
        }

        public override void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            switch (logEventHeader.Level)
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
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case Level.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case Level.Finest:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            WritePrefix(_output, logEventHeader);

            NewlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += NewlineBytes.Length;

            _output.Write(messageBytes, 0, messageLength);
        }

        public override void Close()
        {
        }
    }
}
