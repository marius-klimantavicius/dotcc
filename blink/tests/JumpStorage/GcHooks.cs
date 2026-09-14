global using static JumpStorageGcHooks;

internal static class JumpStorageGcHooks
{
    // Added by the owning test consumer, never by editing generated C#.
    public static void DotccTestCollect()
    {
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
                          blocking: true, compacting: true);
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced,
                          blocking: true, compacting: true);
    }
}
