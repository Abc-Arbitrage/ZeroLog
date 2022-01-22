namespace ZeroLog.Analyzers;

internal static class ZeroLogFacts
{
    public static bool IsLogLevelName(string? value)
        => value is "Trace" or "Debug" or "Info" or "Warn" or "Error" or "Fatal";

    public static class TypeNames
    {
        public const string Log = "ZeroLog.Log";
        public const string LogMessage = "ZeroLog.LogMessage";
    }

    public static class MethodNames
    {
        public const string Append = "Append";
        public const string AppendEnum = "AppendEnum";
        public const string Log = "Log";
    }

    public static class ParameterNames
    {
        public const string FormatString = "format";
    }
}
