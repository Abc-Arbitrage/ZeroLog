using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ZeroLog.Utils
{
    public static class HighResolutionDateTime
    {
        private static readonly bool _isAvailable;

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static DateTime UtcNow
        {
            get
            {
                if (!_isAvailable)
                    return DateTime.UtcNow;

                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);

                return DateTime.FromFileTimeUtc(filetime);
            }
        }

        [DebuggerStepThrough]
        static HighResolutionDateTime()
        {
            try
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                _isAvailable = true;
            }
            catch (EntryPointNotFoundException)
            {
                // Not running Windows 8 or higher.
                _isAvailable = false;
            }
        }
    }
}