namespace ZeroLog.Analyzers;

internal static class DiagnosticIds
{
    public const string Category = "ZeroLog";

    public const string DiscardedLogMessage = "ZL0001";
    public const string AllocatingStringInterpolation = "ZL0002";
    public const string UseStringInterpolation = "ZL0003";
    public const string InvalidPattern = "ZL0004";
    public const string UseAppend = "ZL0005";
}
