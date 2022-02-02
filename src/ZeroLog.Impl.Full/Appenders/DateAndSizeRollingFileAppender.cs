using System;
using System.IO;
using System.Linq;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders
{
    public class DateAndSizeRollingFileAppender : StreamAppender
    {
        private DateTime _currentDate = DateTime.UtcNow.Date;
        private int _rollingFileNumber;
        private long _fileSize;

        /// <summary>
        /// Gets or sets the file name extension to use for the rolling files. Defaults to "log".
        /// </summary>
        public string FileExtension { get; set; } = "log";

        /// <summary>
        /// Gets or sets the root path and file name used by this appender, not including the file extension.
        /// </summary>
        public string FileNameRoot { get; set; }

        /// <summary>
        /// Gets or sets the maximum permitted file size in bytes. Once a file exceeds this value it will
        /// be closed and the next log file will be created. Defaults to 200 MB.
        /// If the size is 0, the feature is disabled.
        /// </summary>
        public int MaxFileSizeInBytes { get; set; } = 200 * 1024 * 1024;

        internal string? CurrentFileName { get; private set; }

        public DateAndSizeRollingFileAppender(string fileNameRoot)
        {
            PrefixPattern = "%time - %level - %logger || ";
            FileNameRoot = fileNameRoot;

            Init();
        }

        private void Init()
        {
            if (string.IsNullOrEmpty(FileNameRoot))
                throw new ArgumentException("FilenameRoot name was not supplied for RollingFileAppender");

            try
            {
                FileNameRoot = Path.GetFullPath(FileNameRoot);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not resolve the full path to the log file", ex);
            }

            var directory = Path.GetDirectoryName(FileNameRoot) ?? throw new ApplicationException($"Could not resolve the directory of {FileNameRoot}");

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Could not create directory for log file '{directory}'", ex);
                }
            }

            FileExtension ??= "";
            _rollingFileNumber = FindLastRollingFileNumber(directory);
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
            CurrentFileName = GetCurrentFileName();
            Stream = OpenFile(CurrentFileName);
            FileOpened(Stream);
            _fileSize = Stream.Length;
        }

        private void CloseStream()
        {
            var stream = Stream;
            if (stream == null)
                return;

            FileClosing(stream);
            Flush();

            Stream = null;
            _fileSize = 0;
            stream.Dispose();
        }

        /// <summary>
        /// Called after a file is opened. You may use this to write a header.
        /// </summary>
        protected virtual void FileOpened(Stream stream)
        {
        }

        /// <summary>
        /// Called before a file is closed. You may use this to write a footer.
        /// </summary>
        protected virtual void FileClosing(Stream stream)
        {
        }

        private void CheckRollFile(DateTime timestamp)
        {
            var maxSizeReached = MaxFileSizeInBytes > 0 && _fileSize >= MaxFileSizeInBytes;
            var dateReached = _currentDate != timestamp.Date;

            if (!maxSizeReached && !dateReached && Stream != null)
                return;

            CloseStream();

            if (maxSizeReached)
                ++_rollingFileNumber;

            if (dateReached)
            {
                _currentDate = timestamp.Date;
                _rollingFileNumber = 0;
            }

            OpenStream();
        }

        private int FindLastRollingFileNumber(string directory)
        {
            var fileNumber = 0;
            var root = FileNameRoot + ".";
            var extension = FileExtension.Length == 0 ? "" : "." + FileExtension;
            foreach (var filename in Directory.EnumerateFiles(directory).Select(f => f.ToUpper()))
            {
                if (filename.StartsWith(root, StringComparison.OrdinalIgnoreCase) && filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    var rootLength = root.Length;
                    var extensionLength = extension.Length;
                    if (filename.Length - rootLength - extensionLength > 0 && int.TryParse(filename.Substring(rootLength, filename.Length - rootLength - extensionLength), out var tempNumber))
                        fileNumber = Math.Max(fileNumber, tempNumber);
                }
            }

            return fileNumber;
        }

        private string GetCurrentFileName()
        {
            return $"{FileNameRoot}.{_currentDate:yyyyMMdd}.{_rollingFileNumber:D3}{(FileExtension.Length == 0 ? "" : "." + FileExtension)}";
        }

        private static Stream OpenFile(string filename)
        {
            var fullPath = Path.GetFullPath(filename);

            try
            {
                return new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read, 64 * 1024, FileOptions.SequentialScan);
            }
            catch (Exception ex)
            {
                throw new IOException($"Could not open log file '{fullPath}'", ex);
            }
        }
    }
}
