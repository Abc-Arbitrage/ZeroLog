using System;
using System.Diagnostics;
using System.IO;
using ZeroLog.Appenders;
using ZeroLog.Formatting;

namespace ZeroLog.Tests;

internal class PerformanceAppender : Appender
{
    private readonly MessageReceived[] _messages;
    private int _count;

    public PerformanceAppender(int expectedEntries)
    {
        _messages = new MessageReceived[expectedEntries];
        for (int i = 0; i < expectedEntries; i++)
        {
            _messages[i] = new MessageReceived(new char[30]);
        }
    }

    public override void WriteMessage(LoggedMessage message)
    {
        var messageSpan = message.Message;
        messageSpan.CopyTo(_messages[_count].StartTimestampInChars);
        _messages[_count].MessageLength = messageSpan.Length;
        _messages[_count].EndTimestamp = Stopwatch.GetTimestamp();
        _count++;
    }

    private struct MessageReceived
    {
        public readonly char[] StartTimestampInChars;
        public int MessageLength;
        public long EndTimestamp;

        public MessageReceived(char[] startTimestampInChars)
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
                var startTime = long.Parse(messageReceived.StartTimestampInChars.AsSpan(0, messageReceived.MessageLength));
                fileStream.WriteLine(ToMicroseconds(messageReceived.EndTimestamp - startTime));
            }
        }
    }

    private static double ToMicroseconds(long ticks)
    {
        return unchecked(ticks * 1000000 / (double)(Stopwatch.Frequency));
    }
}
