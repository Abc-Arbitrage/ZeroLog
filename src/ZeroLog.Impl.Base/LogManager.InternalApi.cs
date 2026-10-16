using System;
using System.ComponentModel;

namespace ZeroLog;

partial class LogManager
{
#pragma warning disable CS1591

    /// <summary>
    /// This class is for internal use only and should not be used directly by users.
    /// It may cause breaking changes or be removed in future versions without notice.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class InternalApi
    {
        public static event Action? Initialized;

        internal static void OnInitialized()
        {
            try
            {
                Initialized?.Invoke();
            }
            catch
            {
                // Ignore initialization exceptions to avoid breaking the application.
                // LogManager should not throw exceptions during initialization.
            }
        }
    }
}
