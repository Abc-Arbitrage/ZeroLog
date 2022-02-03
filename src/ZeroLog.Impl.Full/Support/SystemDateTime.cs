using System;

namespace ZeroLog.Support
{
    internal static class SystemDateTime
    {
        private static DateTime? _pausedUtcNow;

        public static DateTime UtcNow => _pausedUtcNow ?? DateTime.UtcNow;

        public static void Reset()
        {
            _pausedUtcNow = null;
        }

        public static IDisposable Set(DateTime? utcNow = null)
        {
            _pausedUtcNow = utcNow ?? throw new ArgumentNullException();
            return Scope.Instance;
        }

        public static IDisposable PauseTime()
        {
            return Set(DateTime.UtcNow);
        }

        public static void AddToPausedTime(TimeSpan timeSpan)
        {
            _pausedUtcNow = _pausedUtcNow?.Add(timeSpan);
        }

        private class Scope : IDisposable
        {
            public static readonly Scope Instance = new Scope();

            private Scope()
            {
            }

            public void Dispose()
            {
                Reset();
            }
        }
    }
}
