using System;

namespace ZeroLog.Config
{
    [Obsolete("Use " + nameof(ZeroLogJsonConfiguration) + " instead")]
    public class ZeroLogConfiguration : ZeroLogJsonConfiguration
    {
    }
}
