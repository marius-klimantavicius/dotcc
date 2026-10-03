#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Edg.Runtime;

public sealed unsafe partial class LibcxxThreads<TOwner> where TOwner : class
{
    private ThreadEntry? initialThread;
    private bool initialThreadExited, lastThreadExitClaimed;
    private delegate* managed<TOwner, void> lastThreadExit;

    // Only the executable entry establishes this boundary. Managed Run and
    // arbitrary host exports do not acquire source-process/thread semantics.
    public void EnterInitialThread(delegate* managed<TOwner, void> normalExit)
    {
        if (normalExit == null) throw new ArgumentNullException(nameof(normalExit));
        lock (gate)
        {
            if (disposed || initialThread != null || cCurrent.Value != null)
                throw new InvalidOperationException("Initial source thread already established");
            var entry = new ThreadEntry(this, 0, 0, false, true, true);
            retainOwner(owner);
            try
            {
                threads.Add(entry.Token, entry);
                cCurrent.Value = entry;
                initialThread = entry;
                lastThreadExit = normalExit;
                ++running;
            }
            catch { threads.Remove(entry.Token); releaseOwner(owner); throw; }
        }
    }

    public bool IsInitialThreadExit(CThreadExitTransfer exit)
    {
        var entry = cCurrent.Value;
        return entry != null && entry.Initial && ReferenceEquals(exit.Context, this)
            && exit.Token == entry.Token;
    }

    private bool CompleteSourceThread(ThreadEntry entry)
    {
        lock (gate)
        {
            if (entry.Completed) Environment.FailFast("Source thread completed twice");
            if (entry.Initial) initialThreadExited = true;
            entry.Completed = true;
            --running;
            if (entry.JoinState == 2) threads.Remove(entry.Token);
            Monitor.PulseAll(gate);
            if (initialThreadExited && running == 0 && !lastThreadExitClaimed)
            {
                lastThreadExitClaimed = true;
                return true;
            }
            return false;
        }
    }

    [DoesNotReturn]
    private void ExitLastSourceThread()
    {
        // Keep the last source identity available to atexit callbacks. Cleanup
        // has already run; process termination must never join/dispose workers.
        lastThreadExit(owner);
        Environment.FailFast("Initial-thread process exit callback returned");
    }

    [DoesNotReturn]
    public void ExitInitialThread(CThreadExitTransfer exit)
    {
        if (!IsInitialThreadExit(exit))
            throw new InvalidOperationException("Mismatched initial thread exit");
        var entry = cCurrent.Value!;
        entry.Result = exit.Result;
        try { releaseThread(owner); }
        catch (Exception error) when (Terminate(error)) { }
        bool lastSourceThread = CompleteSourceThread(entry);
        releaseOwner(owner);
        if (lastSourceThread) ExitLastSourceThread();
        cCurrent.Value = null;
        // This CLR thread is just a process waiter now. Joiners wait on logical
        // completion above, not its physical CLR lifetime. The actual last
        // source thread performs normal status-zero process termination.
        for (;;)
        {
            try { lock (gate) Monitor.Wait(gate); }
            catch (ThreadInterruptedException) { }
        }
    }
}
