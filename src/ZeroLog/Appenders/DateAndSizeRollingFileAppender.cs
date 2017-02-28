using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ZeroLog.Appenders
{
    public class DateAndSizeRollingFileAppender : AppenderBase
    {
        private readonly object _lock = new object();
        private DateTime _currentDateTime = DateTime.UtcNow;
        private int _currentFileSize;
        private string _directory;
        private int _rollingFileNumber;
        private Stream _stream;

        /// <summary>
        /// Gets or sets if the appender should always call <see cref="M:System.IO.TextWriter.Flush" /> on the
        /// current file writer after every log event.
        /// </summary>
        public bool AutoFlush { get; set; }

        /// <summary>
        /// Gets or sets the file name extension to use for the rolling files. Defaults to "txt".
        /// </summary>
        public string FilenameExtension { get; set; }

        /// <summary>
        /// Gets or sets the root path and file name used by this appender, not including the file extension.
        /// </summary>
        public string FilenameRoot { get; set; }

        /// <summary>
        /// Gets or sets the maximum permitted file size in bytes. Once a file exceeds this value it will
        /// be closed and the next log file will be created. Defaults to 4 MB.
        /// If the size is 0, the feature is disabled.
        /// </summary>
        public int MaxFileSizeInBytes { get; set; }

        internal string CurrentFileName { get; private set; }

        /// <summary>
        /// Initialises a new instance of the class.
        /// </summary>
        public DateAndSizeRollingFileAppender(string filenameRoot, int maxFileSizeInBytes = 200 * 1024 * 1024, string extension = "log", string prefixPattern = "%time - %level - %logger || ")
            : base(prefixPattern)
        {
            FilenameRoot = filenameRoot;
            MaxFileSizeInBytes = maxFileSizeInBytes;
            FilenameExtension = extension;

            Open();
        }

        public override void WriteEvent(ILogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            var stream = _stream;
            if (stream == null)
                return;

            if (messageLength + 1 >= messageBytes.Length)
                throw new ApplicationException($"{nameof(messageBytes)} must be big enough to also contain the new line characters");

            WritePrefix(stream, logEvent);

            NewlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += NewlineBytes.Length;

            stream.Write(messageBytes, 0, messageLength);

            if (AutoFlush)
                stream.Flush();

            _currentFileSize = (int)stream.Length;
            CheckRollFile();
        }

        private void Open()
        {
            if (FilenameRoot == "")
                throw new ArgumentException("FilenameRoot name was not supplied for RollingFileAppender");

            try
            {
                FilenameRoot = Path.GetFullPath(FilenameRoot);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not resolve the full path to the log file", ex);
            }

            _directory = Path.GetDirectoryName(FilenameRoot);
            if (!Directory.Exists(_directory))
            {
                try
                {
                    Directory.CreateDirectory(_directory);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Could not create directory for log file '{_directory}'", ex);
                }
            }

            FilenameExtension = FilenameExtension ?? "";
            _rollingFileNumber = FindLastRollingFileNumber();
            CurrentFileName = GetCurrentFileName();
            _stream = OpenFile(CurrentFileName);
            CheckRollFile();
        }

        public override void Close()
        {
            lock (_lock)
            {
                CloseWriter();
            }
        }

        private void CloseWriter()
        {
            var stream = _stream;
            if (stream == null)
                return;

            Flush();
            _stream = null;
            Thread.MemoryBarrier();
            stream.Close();
        }

        /// <summary>
        /// Flushes the current log file writer.
        /// </summary>
        public void Flush()
        {
            lock (_lock)
            {
                if (_stream == null)
                    return;
                _stream.Flush();
            }
        }

        private bool CheckRollFile()
        {
            var now = DateTime.UtcNow;
            var maxSizeReached = MaxFileSizeInBytes > 0 && _currentFileSize >= MaxFileSizeInBytes;
            var dateReached = _currentDateTime.Date != now.Date;

            if (!maxSizeReached && !dateReached)
                return true;

            lock (_lock)
            {
                CloseWriter();

                if (maxSizeReached)
                    ++_rollingFileNumber;

                if (dateReached)
                {
                    _currentDateTime = now;
                    _rollingFileNumber = 0;
                }

                CurrentFileName = GetCurrentFileName();
                _stream = OpenFile(CurrentFileName);
            }
            return false;
        }

        private int FindLastRollingFileNumber()
        {
            var fileNumber = 0;
            var root = FilenameRoot + ".";
            var extension = FilenameExtension.Length == 0 ? "" : "." + FilenameExtension;
            foreach (var filename in Directory.EnumerateFiles(_directory).Select(f => f.ToUpper()))
            {
                if (filename.StartsWith(root, StringComparison.OrdinalIgnoreCase) && filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    var rootLength = root.Length;
                    var extensionLength = extension.Length;
                    int tempNumber;
                    if (filename.Length - rootLength - extensionLength > 0 && int.TryParse(filename.Substring(rootLength, filename.Length - rootLength - extensionLength), out tempNumber))
                        fileNumber = Math.Max(fileNumber, tempNumber);
                }
            }
            return fileNumber;
        }

        private string GetCurrentFileName()
        {
            return $"{FilenameRoot}.{_currentDateTime:yyyyMMdd}.{_rollingFileNumber:D3}{(FilenameExtension.Length == 0 ? "" : "." + FilenameExtension)}";
        }

        private Stream OpenFile(string filename)
        {
            var fullPath = Path.GetFullPath(filename);
            try
            {
                var fileStream = File.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _currentFileSize = (int)fileStream.Length;
                return fileStream;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Could not open log file '{fullPath}'", ex);
            }
        }
    }
}
