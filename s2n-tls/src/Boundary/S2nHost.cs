#nullable enable
using System;
using System.Collections.Generic;

namespace Managed.Security.S2n;

public static unsafe partial class S2nTls
{
    private static readonly object AtExitGate = new();
    private static readonly Stack<nint> AtExitCallbacks = new();
    private static bool atExitRegistered, atExitCompleted;

    // The standard callback registry has process lifetime, as in native C.
    // The owning API disables upstream atexit and drains with cleanup_final;
    // this preserves the low-level explicit upstream enable_atexit control.
    public static int HostAtExit(delegate*<void> function)
    {
        if (function == null) return -1;
        try
        {
            lock (AtExitGate)
            {
                if (atExitCompleted) return -1;
                if (!atExitRegistered)
                {
                    AppDomain.CurrentDomain.ProcessExit += RunAtExit;
                    atExitRegistered = true;
                }
                AtExitCallbacks.Push((nint)function);
                return 0;
            }
        }
        catch { return -1; }
    }

    private static void RunAtExit(object? sender, EventArgs args)
    {
        while (true)
        {
            nint callback;
            lock (AtExitGate)
            {
                if (AtExitCallbacks.Count == 0) { atExitCompleted = true; return; }
                callback = AtExitCallbacks.Pop();
            }
            try { ((delegate*<void>)callback)(); }
            catch (Exception error)
            {
                Console.Error.WriteLine("s2n managed atexit callback failed: " + error.Message);
                Environment.ExitCode = 1;
            }
        }
    }
}
