using System;
using System.IO;
using System.Linq;

namespace ZeroLog.Appenders
{
    public class DateAndSizeRollingFileAppender : StreamAppender
    {
        public const int DefaultMaxSize = 200 * 1024 * 1024;
        public const string DefaultExtension = "log";
        public const string DefaultPrefixPattern = "%time - %level - %logger || ";

        private DateTime _currentDate = DateTime.UtcNow.Date;
        private int _rollingFileNumber;
        private long _fileSize;

        /// <summary>
        /// Gets or sets the file name extension to use for the rolling files. Defaults to "txt".
        /// </summary>
        public string FilenameExtension { get; set; } = default!;

        /// <summary>
        /// Gets or sets the root path and file name used by this appender, not including the file extension.
        /// </summary>
        public string FilenameRoot { get; set; } = default!;

        /// <summary>
        /// Gets or sets the maximum permitted file size in bytes. Once a file exceeds this value it will
        /// be closed and the next log file will be created. Defaults to 200 MB.
        /// If the size is 0, the feature is disabled.
        /// </summary>
        public int MaxFileSizeInBytes { get; set; }

        internal string? CurrentFileName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DateAndSizeRollingFileAppender(string filePathRoot, int maxFileSizeInBytes = DefaultMaxSize, string? extension = DefaultExtension, string prefixPattern = DefaultPrefixPattern)
            : base(prefixPattern)
        {
            FilenameRoot = filePathRoot;
            MaxFileSizeInBytes = maxFileSizeInBytes;
            FilenameExtension = extension ?? string.Empty;

            Init();
        }

        private void Init()
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

            var directory = Path.GetDirectoryName(FilenameRoot) ?? throw new ApplicationException($"Could not resolve the directory of {FilenameRoot}");

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

            FilenameExtension ??= "";
            _rollingFileNumber = FindLastRollingFileNumber(directory);
        }

        public override void WriteMessage(FormattedLogMessage message)
        {
            CheckRollFile(message.Message.Timestamp);

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
            _stream = OpenFile(CurrentFileName);
            FileOpened(_stream);
            _fileSize = _stream.Length;
        }

        private void CloseStream()
        {
            var stream = _stream;
            if (stream == null)
                return;

            FileClosing(stream);
            Flush();

            _stream = null;
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

            if (!maxSizeReached && !dateReached && _stream != null)
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
            var root = FilenameRoot + ".";
            var extension = FilenameExtension.Length == 0 ? "" : "." + FilenameExtension;
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
            return $"{FilenameRoot}.{_currentDate:yyyyMMdd}.{_rollingFileNumber:D3}{(FilenameExtension.Length == 0 ? "" : "." + FilenameExtension)}";
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
