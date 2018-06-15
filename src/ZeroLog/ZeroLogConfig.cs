using JetBrains.Annotations;

namespace ZeroLog
{
    public class ZeroLogConfig
    {
        [NotNull]
        private string _nullDisplayString = "null";

        public bool LazyRegisterEnums { get; set; }
        public bool FlushAppenders { get; set; } = true;

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
