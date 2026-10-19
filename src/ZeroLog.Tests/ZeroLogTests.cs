namespace ZeroLog.Tests;

public static class ZeroLogTests
{
    public static bool SourceGeneratorsEnabled
#if ZEROLOG_SOURCE_GENERATORS
        => true;
#else
        => false;
#endif
}
