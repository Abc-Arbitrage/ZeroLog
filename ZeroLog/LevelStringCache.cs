using System;

namespace ZeroLog
{
    public class LevelStringCache
    {
        private static readonly string[] _levelStrings;

        static LevelStringCache()
        {
            _levelStrings = Enum.GetNames(typeof(Level));
        }

        public static string GetLevelString(Level level) => _levelStrings[(byte)level];
    }
}