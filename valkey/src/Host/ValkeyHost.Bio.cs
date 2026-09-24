using System;
using System.Collections.Generic;
using System.Threading;
using Libc = Managed.Database.ValkeyCore.Libc;
using mutexQueue = Managed.Database.ValkeyCore.mutexQueue;

namespace Managed.Database;

/// <summary>Cooperative lifetime control for the translated upstream BIO queues.</summary>
public static unsafe partial class ValkeyHost
{
    private sealed class BioWorker
    {
        internal nint Queue;
        internal long Thread;
        internal bool Started;
        internal bool StopSent;
    }

    private sealed partial class HostState
    {
        internal readonly List<BioWorker> BioWorkers = new();
        internal nint BioSentinel;
        internal int BioStopping;
    }

    // Initialization and stop run on the owning executor. Worker callbacks only
    // read the immutable sentinel and the published stopping flag. All calls,
    // including translated queue operations, require the bound runtime owner.
    public static void BioQueueCreated(ValkeyCore core, void* queue)
    {
        if (queue == null) throw new InvalidOperationException("BIO queue allocation failed.");
        var state = For(core);
        if (state.BioSentinel == 0)
        {
            var sentinel = Libc.malloc(1);
            if (sentinel == null) throw new OutOfMemoryException("BIO stop marker allocation failed.");
            state.BioSentinel = (nint)sentinel;
        }
        state.BioWorkers.Add(new BioWorker { Queue = (nint)queue });
    }

    public static void BioWorkerStarted(ValkeyCore core, void* queue, long thread)
    {
        foreach (var worker in For(core).BioWorkers)
        {
            if (worker.Queue != (nint)queue) continue;
            worker.Thread = thread;
            worker.Started = true;
            return;
        }
        throw new InvalidOperationException("BIO worker has no registered queue.");
    }

    public static int BioJobAllowed(ValkeyCore core) => Volatile.Read(ref For(core).BioStopping) == 0 ? 1 : 0;

    public static int BioShouldStop(ValkeyCore core, void* job)
    {
        var sentinel = For(core).BioSentinel;
        return sentinel != 0 && (nint)job == sentinel ? 1 : 0;
    }

    /// <summary>Drain normal work before joining. During terminal failure discard
    /// queued callbacks; the owner arena reclaims their remaining payloads after
    /// all workers join. A failed join retains queues and the marker for retry.</summary>
    public static int BioStop(ValkeyCore core, int abandon)
    {
        var state = For(core);
        Volatile.Write(ref state.BioStopping, 1);
        foreach (var worker in state.BioWorkers)
        {
            if (worker.Queue == 0) continue;
            if (abandon != 0)
            {
                DrainBioQueue(core, state, worker);
                worker.StopSent = false;
            }
            if (worker.Started && !worker.StopSent)
            {
                // Normal-priority FIFO insertion follows all submitted jobs.
                core.mutexQueueAdd((mutexQueue*)worker.Queue, (void*)state.BioSentinel);
                worker.StopSent = true;
            }
        }
        foreach (var worker in state.BioWorkers)
        {
            if (!worker.Started) continue;
            if (Libc.pthread_join(worker.Thread, null) != 0) return -1;
            worker.Started = false;
        }
        foreach (var worker in state.BioWorkers)
        {
            if (worker.Queue == 0) continue;
            // A faulted worker can leave jobs behind even on a normal stop.
            DrainBioQueue(core, state, worker);
            core.mutexQueueRelease((mutexQueue*)worker.Queue);
            worker.Queue = 0;
        }
        if (state.BioSentinel != 0)
        {
            Libc.free((void*)state.BioSentinel);
            state.BioSentinel = 0;
        }
        return 0;
    }

    private static void DrainBioQueue(ValkeyCore core, HostState state, BioWorker worker)
    {
        void* job;
        while ((job = core.mutexQueuePop((mutexQueue*)worker.Queue, 0)) != null)
            if ((nint)job != state.BioSentinel) core.valkey_free(job);
    }
}
