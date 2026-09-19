global using static ResourceGcHooks;
using global::System;
using global::System.Collections.Generic;
using global::System.Threading;
using global::System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;
Blink.ResourceSeedAbi();
var barrier = new Barrier(2);
Task Worker(int capacity, ulong budget) {
    var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    new Thread(() => {
        var owner = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), descriptorLimit:capacity);
        try {
            unsafe { if (Blink.BlinkHostInitializeBoundResourceLimits(null)!=-1 || Blink.Libc.errno!=19) throw new Exception("unbound"); }
            Blink.BindHostIo(owner);ResourceGcHooks.Barrier=barrier;
            var code=Blink.ResourceSeedProbe(budget,(ulong)capacity);
            if(code!=0)throw new Exception("C resource seed line "+code);
            owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
            unsafe { if(Blink.BlinkHostInitializeBoundResourceLimits(null)!=-1 || Blink.Libc.errno!=9)throw new Exception("disposed"); }
            completion.SetResult();
        }catch(Exception e){completion.TrySetException(e);}
        finally {ResourceGcHooks.Barrier=null;Blink.UnbindHostIo();Blink.BlinkHostMemoryDisposeWorker();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
    }) { IsBackground=true }.Start();return completion.Task;
}
await Task.WhenAll(Worker(8,32769),Worker(19,65537)).WaitAsync(TimeSpan.FromSeconds(20));
Console.WriteLine("guest resource seed pointer/owner invariants: PASS");
public static class ResourceGcHooks {
    [ThreadStatic]public static Barrier? Barrier;
    public static void ResourceGc(){if(Barrier!=null&&!Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("barrier");GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}
}
