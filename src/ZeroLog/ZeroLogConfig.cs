using JetBrains.Annotations;

namespace ZeroLog
{
    public class ZeroLogConfig
    {
        [NotNull]
        private string _nullDisplayString = "null";

        [NotNull]
        private string _truncatedMessageSuffix = " [TRUNCATED]";

        public bool LazyRegisterEnums { get; set; }
        public bool FlushAppenders { get; set; } = true;

        public string NullDisplayString
        {
            [NotNull] get => _nullDisplayString;
            set => _nullDisplayString = value ?? string.Empty;
        }

        public string TruncatedMessageSuffix
        {
            [NotNull] get => _truncatedMessageSuffix;
            set => _truncatedMessageSuffix = value ?? string.Empty;
        }

        internal ZeroLogConfig()
        {
        }
    }
}
