using System.Threading;

namespace Managed.Database;

public static unsafe partial class ValkeyHost
{
    private sealed partial class HostState
    {
        internal bool IoWorkersStopped;
    }

    private static void StopIoWorkers(ValkeyCore core)
    {
        var state = For(core);
        if (state.IoWorkersStopped || !state.ServerEntered) return;
        if (core.Globals.server.io_threads_num > 1 && core.Globals.io_threads_initialized != 0)
        {
            // Shutdown has ended command execution. Upstream cancellation cleanup
            // flushes each worker's response backlog before joining; keep consuming
            // the bounded return queue so that this flush cannot block teardown.
            // The owner arena reclaims discarded responses after every worker joins.
            int stopping = 0;
            Exception? drainFailure = null;
            var drain = new Thread(() =>
            {
                try
                {
                    using var binding = core.__DotCcEnter();
                    void** responses = stackalloc void*[64];
                    while (Volatile.Read(ref stopping) == 0)
                    {
                        if (core.mpscDequeueBatch((ValkeyCore.mpscQueue*)
                            System.Runtime.CompilerServices.Unsafe.AsPointer(ref core.io_shared_outbox), responses, 64) == 0)
                            Thread.Yield();
                    }
                }
                catch (Exception error)
                {
                    // Let upstream's failed-server cleanup abandon a full return
                    // queue, then surface the error to quarantine this owner.
                    drainFailure = error;
                    Volatile.Write(ref core.Globals.server.crashed, 1);
                }
            }) { IsBackground = true, Name = "Valkey I/O shutdown" };
            drain.Start();
            try { core.killIOThreads(); }
            finally
            {
                Volatile.Write(ref stopping, 1);
                drain.Join();
            }
            if (drainFailure != null)
                throw new InvalidOperationException("I/O shutdown response drain failed.", drainFailure);
        }
        state.IoWorkersStopped = true;
    }
}
