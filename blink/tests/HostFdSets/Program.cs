using System;
using System.Threading;
using Managed.Emulation;

if (Blink.FdSetProbe() != 0) throw new Exception("fd-set comparison failed");
using var barrier = new Barrier(2);
Exception? failure = null;
void Worker(int seed)
{
    try
    {
        Blink.FdSetStore(seed);
        if (!barrier.SignalAndWait(TimeSpan.FromSeconds(10))) throw new Exception("fd-set worker timeout");
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        if (!barrier.SignalAndWait(TimeSpan.FromSeconds(10))) throw new Exception("fd-set GC timeout");
        int result = Blink.FdSetVerify(seed);
        if (result != 0) throw new Exception($"fd-set worker {seed} storage check {result} failed");
    }
    catch (Exception error) { Interlocked.CompareExchange(ref failure, error, null); }
}
var first = new Thread(() => Worker(2));
var second = new Thread(() => Worker(5));
first.Start(); second.Start();
if (!first.Join(TimeSpan.FromSeconds(15)) || !second.Join(TimeSpan.FromSeconds(15))) throw new Exception("fd-set worker did not exit");
if (failure is not null) throw failure;
return 0;
