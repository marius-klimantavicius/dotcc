global using static HostSignalTestHooks;

internal static class HostSignalTestHooks
{
    [System.ThreadStatic] private static string initialHostMask;

    // Test-only observation on the thread executing translated C. No P/Invoke
    // or host signal installation is present in the product adapter.
    public static void DotccSignalCheckHostMask()
    {
        string current = System.Array.Find(
            System.IO.File.ReadAllLines("/proc/thread-self/status"),
            line => line.StartsWith("SigBlk:", System.StringComparison.Ordinal));
        if (current == null) throw new System.InvalidOperationException("Host mask unavailable");
        initialHostMask ??= current;
        if (current != initialHostMask)
            throw new System.InvalidOperationException("Adapter changed the real host thread mask");
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
                          blocking: true, compacting: true);
        System.GC.WaitForPendingFinalizers();
    }
}
