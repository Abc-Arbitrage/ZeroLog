namespace ZeroLog.Analyzers;

internal static class ZeroLogFacts
{
    public const string Namespace = "ZeroLog";

    public static bool IsLogLevelName(string? value)
        => value is nameof(LogLevel.Trace)
            or nameof(LogLevel.Debug)
            or nameof(LogLevel.Info)
            or nameof(LogLevel.Warn)
            or nameof(LogLevel.Error)
            or nameof(LogLevel.Fatal);

    public static class ShortTypeNames
    {
        public const string LogManager = "LogManager";
        public const string LogMessage = "LogMessage";
        public const string InternalApi = "InternalApi";
    }

    public static class FullTypeNames
    {
        public const string Log = "ZeroLog.Log";
        public const string LogManager = "ZeroLog.LogManager";
        public const string LogMessage = "ZeroLog.LogMessage";
        public const string Chain = "ZeroLog.LogMessage.InternalApi.Chain";
        public const string ChainExecutionContext = "ZeroLog.LogMessage.InternalApi.Chain.ChainExecutionContext";
    }

    public static class MethodNames
    {
        public const string Append = "Append";
        public const string AppendEnum = "AppendEnum";
        public const string AppendKeyValue = "AppendKeyValue";
        public const string AppendChain = "AppendChain";
        public const string AppendChainItem = "AppendChainItem";
        public const string WithException = "WithException";
        public const string GetLogger = "GetLogger";
        public const string Log = "Log";
    }

    public static class ParameterNames
    {
        public const string FormatString = "format";
    }

    public static class EventNames
    {
        public const string Initialized = "Initialized";
    }
}
