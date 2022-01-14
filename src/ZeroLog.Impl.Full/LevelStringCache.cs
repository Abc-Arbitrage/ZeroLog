using System;
using System.Linq;

namespace ZeroLog
{
    internal static class LevelStringCache
    {
        private static readonly string[] _levelStrings;

        static LevelStringCache()
        {
            _levelStrings = Enum.GetNames(typeof(Level)).Select(x => x.ToUpperInvariant()).ToArray();
        }

        public static string GetLevelString(Level level) => _levelStrings[(byte)level];
    }
}
