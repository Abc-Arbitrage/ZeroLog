using JetBrains.Annotations;

namespace ZeroLog
{
    public class ZeroLogConfig
    {
        [NotNull]
        private string _nullDisplayString = "null";

        public bool LazyRegisterEnums { get; set; }

        public string NullDisplayString
        {
            [NotNull] get => _nullDisplayString;
            set => _nullDisplayString = value ?? string.Empty;
        }

        internal ZeroLogConfig()
        {
        }
    }
}
