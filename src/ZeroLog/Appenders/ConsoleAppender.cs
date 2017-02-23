using System;
using System.IO;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : AppenderBase
    {
        private readonly Stream _output;

        public ConsoleAppender(string prefixPattern = "%time - %level - %logger || ") : base(prefixPattern)
        {
            _output = Console.OpenStandardOutput();
        }

        public override void WriteEvent(ILogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            switch (logEvent.Level)
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

            WritePrefix(_output, logEvent);

            NewlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += NewlineBytes.Length;

            _output.Write(messageBytes, 0, messageLength);
        }

        public override void Close()
        {
        }
    }
}