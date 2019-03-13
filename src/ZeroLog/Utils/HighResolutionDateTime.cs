using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ZeroLog.Utils
{
    internal static class HighResolutionDateTime
    {
        private static readonly bool _isAvailable = CheckAvailability();

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static DateTime UtcNow
        {
            get
            {
                if (!_isAvailable)
                    return DateTime.UtcNow;

	            GetSystemTimePreciseAsFileTime(out var filetime);
                return DateTime.FromFileTimeUtc(filetime);
            }
        }

        [DebuggerStepThrough]
        private static bool CheckAvailability()
        {
            try
            {
	            GetSystemTimePreciseAsFileTime(out _);
                return true;
            }
            catch (DllNotFoundException)
            {
	            // Not running Windows 8 or higher.
	            return false;
            }
            catch (EntryPointNotFoundException)
            {
                // Not running Windows 8 or higher.
                return false;
            }
        }
    }
}
