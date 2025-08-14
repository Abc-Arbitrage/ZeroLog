using System.Threading;

namespace ZeroLog.Configuration;

/// <summary>
/// Configuration used by a thread initializer.
/// </summary>
/// <remarks>
/// This is provided to the initializer on the thread it configures, which is executed on the same thread.
/// It is intended to be used to set the thread affinity (pinning the thread) or priority.
/// </remarks>
public sealed class ThreadConfiguration
{
    /// <summary>
    /// The thread to configure. Since the initializer is executed on the thread it configures,
    /// this will always be equal to <see cref="Thread.CurrentThread"/>.
    /// </summary>
    public Thread Thread { get; }

    internal ThreadConfiguration(Thread thread)
    {
        Thread = thread;
    }
}
