using System;
using System.Diagnostics;
using System.IO;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public class DateAndSizeRollingFileAppender : StreamAppender
{
    private const int _initializeOnFirstAppend = -1;

    private DateTime _currentDate;
    private int _currentFileNumber;

    /// <summary>
    /// Directory where to put the log files.
    /// </summary>
    public string FileDirectory { get; }

    /// <summary>
    /// File name prefix to use for rolling files. Default to "LogFile".
    /// </summary>
    public string FileNamePrefix { get; init; } = "LogFile";

    /// <summary>
    /// File name extension to use for the rolling files. Defaults to "log".
    /// </summary>
    public string FileExtension { get; init; } = "log";

    /// <summary>
    /// Gets or sets the maximum permitted file size in bytes. Once a file exceeds this value it will
    /// be closed and the next log file will be created. Defaults to 200 MB.
    /// If the size is 0, the feature is disabled.
    /// </summary>
    public long MaxFileSizeInBytes { get; init; } = 200 * 1024 * 1024;

    public DateAndSizeRollingFileAppender(string directory)
    {
        PrefixPattern = "%time - %level - %logger || ";
        FileDirectory = Path.GetFullPath(directory);

        _currentFileNumber = _initializeOnFirstAppend;
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        CheckRollFile(message.Timestamp);

        base.WriteMessage(message);
    }

    public override void Dispose()
    {
        CloseStream();
        base.Dispose();
    }

    private void OpenStream()
    {
        Debug.Assert(Stream is null);

        if (_currentFileNumber == _initializeOnFirstAppend)
        {
            Directory.CreateDirectory(FileDirectory);

            _currentDate = DateTime.UtcNow.Date;
            _currentFileNumber = FindLastRollingFileNumber(DateOnly.FromDateTime(_currentDate));
        }

        var fileName = Path.Combine(FileDirectory, GetFileName(DateOnly.FromDateTime(_currentDate), _currentFileNumber));

        try
        {
            Stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
        }
        catch (Exception ex)
        {
            throw new IOException($"Could not open log file '{fileName}'", ex);
        }

        FileOpened();
    }

    private void CloseStream()
    {
        if (Stream is null)
            return;

        FileClosing();
        Flush();

        Stream.Dispose();
        Stream = null;
    }

    /// <summary>
    /// Called after a file is opened. You may use this to write a header.
    /// </summary>
    protected virtual void FileOpened()
    {
    }

    /// <summary>
    /// Called before a file is closed. You may use this to write a footer.
    /// </summary>
    protected virtual void FileClosing()
    {
    }

    /// <summary>
    /// Returns the file name without the directory for the given parameters.
    /// </summary>
    /// <param name="date">The file date</param>
    /// <param name="number">The file number</param>
    protected virtual string GetFileName(DateOnly date, int number)
        => $"{FileNamePrefix}.{date:yyyyMMdd}.{number:D3}{(string.IsNullOrEmpty(FileExtension) ? "" : "." + FileExtension)}";

    private void CheckRollFile(DateTime timestamp)
    {
        if (Stream is null)
        {
            OpenStream();
            return;
        }

        // FileStream.Position has been optimized in .NET 6, it no longer performs a syscall
        var maxSizeReached = MaxFileSizeInBytes > 0 && Stream.Position >= MaxFileSizeInBytes;
        var dateReached = _currentDate != timestamp.Date;

        if (!maxSizeReached && !dateReached)
            return;

        CloseStream();

        if (maxSizeReached)
            ++_currentFileNumber;

        if (dateReached)
        {
            _currentDate = timestamp.Date;
            _currentFileNumber = 0;
        }

        OpenStream();
    }

    private int FindLastRollingFileNumber(DateOnly date)
    {
        var nextFileNumber = 0;
        string? lastExistingFileName = null;

        while (true)
        {
            var fileName = Path.Combine(FileDirectory, GetFileName(date, nextFileNumber));

            // Sanity check
            if (string.Equals(fileName, lastExistingFileName, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                return nextFileNumber;

            if (!File.Exists(fileName))
            {
                if (nextFileNumber == 0)
                    return 0;

                break;
            }

            lastExistingFileName = fileName;
            ++nextFileNumber;
        }

        if (lastExistingFileName != null)
        {
            // Find out if we can append to the latest file

            try
            {
                // Really open the file to check if it's not locked

                // ReSharper disable once RedundantArgumentDefaultValue
                using var handle = File.OpenHandle(lastExistingFileName, FileMode.Append, FileAccess.Write, FileShare.Read);

                if (MaxFileSizeInBytes > 0 && RandomAccess.GetLength(handle) < MaxFileSizeInBytes)
                    return nextFileNumber - 1;
            }
            catch
            {
                // Cannot open the file, create a new one
                return nextFileNumber;
            }
        }

        return nextFileNumber;
    }
}
