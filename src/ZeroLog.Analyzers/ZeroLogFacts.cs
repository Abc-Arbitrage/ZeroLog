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

    public static class TypeNames
    {
        public const string Log = "ZeroLog.Log";
        public const string LogMessage = "ZeroLog.LogMessage";
        public const string PatternWriter = "ZeroLog.Formatting.PatternWriter";
        public const string DefaultFormatter = "ZeroLog.Formatting.DefaultFormatter";
    }

    public static class MethodNames
    {
        public const string Append = "Append";
        public const string AppendEnum = "AppendEnum";
        public const string WithException = "WithException";
        public const string Log = "Log";
    }

    public static class ParameterNames
    {
        public const string FormatString = "format";
    }

    public static class PropertyNames
    {
        public const string PrefixPattern = "PrefixPattern";
    }
}
