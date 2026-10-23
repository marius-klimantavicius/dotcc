using Managed.Security.S2n;
unsafe
{
    if (S2nTls.HostAtExit(null) != -1 || S2nTls.HostAtExit(&Callbacks.First) != 0 || S2nTls.HostAtExit(&Callbacks.Second) != 0)
        throw new InvalidOperationException("atexit registration failure");
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    Console.Write("main;");
}
static unsafe class Callbacks
{
    public static void First() => Console.Write("1");
    public static void Second()
    {
        Console.Write("2");
        // C permits a callback to add another termination callback.
        if (S2nTls.HostAtExit(&Third) != 0) throw new InvalidOperationException("atexit during drain");
    }
    public static void Third() => Console.Write("3");
}
