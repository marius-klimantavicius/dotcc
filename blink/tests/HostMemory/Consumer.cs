using MemoryApi = Managed.Emulation.MemoryHost;

if (MemoryApi.MemorySelfTest() != 0) return 1;
using var barrier = new global::System.Threading.Barrier(2);
global::System.Exception failure = null;
var threads = new global::System.Threading.Thread[2];
for (int i = 0; i < threads.Length; ++i)
{
    threads[i] = new global::System.Threading.Thread(() =>
    {
        try
        {
            if (MemoryApi.MemoryWorkerBegin() != 0) throw new global::System.Exception("worker begin failed");
            barrier.SignalAndWait();
            global::System.GC.Collect(global::System.GC.MaxGeneration,
                global::System.GCCollectionMode.Forced, blocking: true, compacting: true);
            barrier.SignalAndWait();
            if (MemoryApi.MemoryWorkerFinish() != 0) throw new global::System.Exception("worker state or ownership changed");
        }
        catch (global::System.Exception error) { failure = error; }
    });
    threads[i].Start();
}
foreach (var thread in threads) thread.Join();
if (failure != null) throw failure;
return 0;
