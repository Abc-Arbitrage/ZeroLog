using System;
using System.IO;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : IAppender
    {
        private readonly Stream _openStandardOutput;

        public ConsoleAppender()
        {
            _openStandardOutput = Console.OpenStandardOutput();
        }

        public Stream GetStream()
        {
            return _openStandardOutput;
        }
    }
}