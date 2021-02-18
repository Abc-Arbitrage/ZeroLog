using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace ZeroLog.Utils
{
    internal static class HighResolutionDateTime
    {
#if NETCOREAPP

        public static DateTime UtcNow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => DateTime.UtcNow;
        }

#else

        private static readonly bool _isAvailable = CheckAvailability();

        [SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long fileTime);

        public static DateTime UtcNow
        {
            get
            {
                if (!_isAvailable)
                    return DateTime.UtcNow;

                GetSystemTimePreciseAsFileTime(out var fileTime);
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }

        [DebuggerStepThrough]
        private static bool CheckAvailability()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

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
#endif
    }
}
