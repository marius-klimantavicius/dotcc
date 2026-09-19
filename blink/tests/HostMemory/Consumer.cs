using MemoryApi = Managed.Emulation.MemoryHost;

if (MemoryApi.MemorySelfTest() != 0) return 1;
using var barrier = new global::System.Threading.Barrier(2);
global::System.Exception failure = null;
var addresses = new ulong[2];
var threads = new global::System.Threading.Thread[2];
for (int i = 0; i < threads.Length; ++i)
{
    int worker = i;
    threads[i] = new global::System.Threading.Thread(() =>
    {
        try
        {
            if (MemoryApi.MemoryWorkerBegin() != 0) throw new global::System.Exception("worker begin failed");
            addresses[worker] = MemoryApi.MemoryWorkerAddress();
            barrier.SignalAndWait();
            if (addresses[worker] == 0 || addresses[worker] == addresses[1-worker])
                throw new global::System.Exception("owners share a backing allocation");
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
