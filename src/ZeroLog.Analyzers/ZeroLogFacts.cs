namespace ZeroLog.Analyzers;

internal static class ZeroLogFacts
{
    public static bool IsLogLevelName(string? value)
        => value is nameof(LogLevel.Trace)
            or nameof(LogLevel.Debug)
            or nameof(LogLevel.Info)
            or nameof(LogLevel.Warn)
            or nameof(LogLevel.Error)
            or nameof(LogLevel.Fatal);

    public static class Namespaces
    {
        public const string ZeroLog = "ZeroLog";
        public const string Generated = "ZeroLog.Generated";
    }

    public static class ShortTypeNames
    {
        public const string LogManager = "LogManager";
        public const string LogMessage = "LogMessage";
        public const string InternalApi = "InternalApi";
        public const string Chain = "Chain";
    }

    public static class FullTypeNames
    {
        public const string Log = $"{Namespaces.ZeroLog}.Log";
        public const string LogManager = $"{Namespaces.ZeroLog}.LogManager";
        public const string LogMessage = $"{Namespaces.ZeroLog}.LogMessage";

        public static class InternalApi
        {
            public const string Chain = $"{LogMessage}.{ShortTypeNames.InternalApi}.{ShortTypeNames.Chain}";
            public const string ChainExecutionContext = $"{Chain}.ChainExecutionContext";
        }
    }

    public static class MetadataNames
    {
        public const string LogManagerInternalApi = $"{FullTypeNames.LogManager}+{ShortTypeNames.InternalApi}";
        public const string LogMessageInternalApiChain = $"{FullTypeNames.LogMessage}+{ShortTypeNames.InternalApi}+{ShortTypeNames.Chain}";
    }

    public static class MethodNames
    {
        public const string Append = "Append";
        public const string AppendEnum = "AppendEnum";
        public const string AppendKeyValue = "AppendKeyValue";
        public const string WithException = "WithException";
        public const string GetLogger = "GetLogger";
        public const string Log = "Log";

        public static class InternalApi
        {
            public const string AppendChain = "AppendChain";
            public const string AppendValue = "AppendValue";
            public const string AppendEnumValue = "AppendEnumValue";
        }
    }

    public static class ParameterNames
    {
        public const string FormatString = "format";
    }

    public static class EventNames
    {
        public const string Initialized = "Initialized";
    }

    public static class FieldNames
    {
        public const string Version = "Version";
    }
}
