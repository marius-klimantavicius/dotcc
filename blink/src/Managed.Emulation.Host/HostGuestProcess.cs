namespace Managed.Emulation.Host;

/// <summary>Both execution APIs share one process-discard lifecycle. Upstream
/// static caches are not certified for a second guest after their backing is
/// released, including when initialization of the first guest failed.</summary>
public static class HostGuestProcess
{
    private static int used;
    public static void BeginOnce()
    {
        if (Interlocked.Exchange(ref used, 1) != 0)
            throw new InvalidOperationException("The translated guest may execute only once in this process; discard it after execution.");
    }
}
