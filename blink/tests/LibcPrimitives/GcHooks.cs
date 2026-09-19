global using static LibcPrimitivesGcHooks;

internal static class LibcPrimitivesGcHooks
{
    // Added by the owning test consumer, never by editing generated C#.
    public static void ForceGc()
    {
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
                          blocking: true, compacting: true);
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
                          blocking: true, compacting: true);
    }
}
