#nullable enable
namespace Edg.Runtime;

// Itanium guard publication implemented with BCL synchronization. The storage
// belongs to translated static objects; the registry never owns native memory.
public static unsafe class CppGuards
{
    private sealed class Guard { internal int Thread; }
    private static readonly object Sync = new();
    private static readonly global::System.Collections.Generic.Dictionary<nint, Guard> Guards = new();

    public static int NeedsInitialization(ulong* address)
        => global::System.Threading.Volatile.Read(ref *(byte*)address) == 0 ? 1 : 0;

    public static int Acquire(ulong* address)
    {
        Guard guard;
        lock (Sync)
            if (!Guards.TryGetValue((nint)address, out guard!))
                Guards.Add((nint)address, guard = new Guard());
        global::System.Threading.Monitor.Enter(guard);
        if (NeedsInitialization(address) == 0)
        {
            global::System.Threading.Monitor.Exit(guard);
            return 0;
        }
        if (guard.Thread != 0)
            global::System.Environment.FailFast("Recursive local-static initialization");
        guard.Thread = global::System.Environment.CurrentManagedThreadId;
        return 1;
    }

    public static void Release(ulong* address) => Finish(address, true);
    public static void Abort(ulong* address) => Finish(address, false);

    private static void Finish(ulong* address, bool complete)
    {
        Guard guard;
        lock (Sync) guard = Guards[(nint)address];
        if (guard.Thread != global::System.Environment.CurrentManagedThreadId ||
            !global::System.Threading.Monitor.IsEntered(guard))
            global::System.Environment.FailFast("Local-static guard completed by a thread that does not own initialization");
        global::System.Threading.Volatile.Write(ref *(byte*)address, complete ? (byte)1 : (byte)0);
        guard.Thread = 0;
        global::System.Threading.Monitor.Exit(guard);
    }
}
