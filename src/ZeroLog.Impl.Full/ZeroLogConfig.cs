using ZeroLog.Configuration;

namespace ZeroLog;

public class ZeroLogConfig
{
    // TODO refactor this

    private string _nullDisplayString = "null";
    private string _truncatedMessageSuffix = " [TRUNCATED]";
    private string _jsonSeparator = " ~~ ";

    public bool LazyRegisterEnums { get; set; }
    public bool FlushAppenders { get; set; } = true;

    public string NullDisplayString
    {
        get => _nullDisplayString;
        set => _nullDisplayString = value ?? string.Empty;
    }

    public string TruncatedMessageSuffix
    {
        get => _truncatedMessageSuffix;
        set => _truncatedMessageSuffix = value ?? string.Empty;
    }

    public string JsonSeparator
    {
        get => _jsonSeparator;
        set => _jsonSeparator = value ?? string.Empty;
    }

    internal ZeroLogConfig()
    {
    }

    internal void UpdateFrom(ZeroLogConfiguration config)
    {
        LazyRegisterEnums = config.AutoRegisterEnums;

        NullDisplayString = config.NullDisplayString;
        TruncatedMessageSuffix = config.TruncatedMessageSuffix;
        JsonSeparator = config.JsonSeparator;
    }
}
