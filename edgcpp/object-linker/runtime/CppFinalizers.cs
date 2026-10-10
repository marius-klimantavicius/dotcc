#nullable enable
namespace Edg.Runtime;

// Each translated assembly uses Program; an explicit state owner may instead
// own an instance. The registry stores callback addresses, never target storage.
public sealed unsafe class CppFinalizers
{
    internal readonly struct Entry(nint function, nint argument, bool noArguments = false)
    {
        internal readonly nint Function = function, Argument = argument;
        internal readonly bool NoArguments = noArguments;
    }

    public static CppFinalizers Program { get; } = new();
    internal readonly object Sync = new();
    internal readonly global::System.Collections.Generic.List<Entry> Normal = new();
    internal readonly global::System.Collections.Generic.List<Entry> Quick = new();
    private int state, drainingThread;
    internal int State { get { lock (Sync) return state; } }

    public int Register(delegate* managed<void*, void> callback, void* argument)
        => Register(Normal, (nint)callback, (nint)argument);

    internal int Register(global::System.Collections.Generic.List<Entry> entries,
        nint function, nint argument, bool noArguments = false)
    {
        try
        {
            lock (Sync)
            {
                if (state == 2) return -1;
                entries.Add(new Entry(function, argument, noArguments));
                return 0;
            }
        }
        catch (global::System.OutOfMemoryException) { return -1; }
    }

    // Pop before invoking and invoke outside the registry lock. A callback can
    // register another callback, which becomes the next one to run.
    internal void Drain(global::System.Collections.Generic.List<Entry> entries,
        delegate* managed<global::System.Exception, bool> handleTransfer = null,
        bool closeWhenEmpty = false)
    {
        for (;;)
        {
            Entry entry;
            lock (Sync)
            {
                if (entries.Count == 0)
                {
                    if (closeWhenEmpty)
                    {
                        state = 2;
                        drainingThread = 0;
                        global::System.Threading.Monitor.PulseAll(Sync);
                    }
                    return;
                }
                int last = entries.Count - 1;
                entry = entries[last];
                entries.RemoveAt(last);
            }
            try
            {
                if (entry.NoArguments) ((delegate* managed<void>)entry.Function)();
                else ((delegate* managed<void*, void>)entry.Function)((void*)entry.Argument);
            }
            catch (global::System.Exception error)
            {
                if (handleTransfer != null && handleTransfer(error)) continue;
                global::System.Environment.FailFast("Exception escaped translated finalizer", error);
            }
        }
    }

    public void DrainProcess() => Drain(Normal);

    // The caller must first stop source work using the library. Finalization is
    // explicit and idempotent; it never terminates the managed host process.
    public void FinalizeLibrary()
    {
        int thread = global::System.Environment.CurrentManagedThreadId;
        lock (Sync)
        {
            while (state == 1)
            {
                if (drainingThread == thread) return;
                global::System.Threading.Monitor.Wait(Sync);
            }
            if (state == 2) return;
            state = 1;
            drainingThread = thread;
        }
        Drain(Normal, closeWhenEmpty: true);
    }
}
