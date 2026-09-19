global using static ResourceGcHooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool condition,string detail){if(!condition)throw new Exception(detail);}
var barrier=new Barrier(2);
Task Worker(int capacity,int pid,ulong budget)
{
    var result=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var thread=new Thread(()=> {
        var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>(),descriptorLimit:capacity);
        try {
            Check(Blink.UnboundResources(19)==0,"unbound resource outputs");
            Blink.BindHostIo(owner);Blink.BindHostIdentity(new HostIdentity(pid));ResourceGcHooks.Barrier=barrier;
            Check(Blink.ResourceAbi()==0,"actual emitted resource ABI");
            var duplicates=new List<int>();
            for(int i=3;i<capacity;++i){var fd=owner.Duplicate(0);Check(fd.Succeeded,"fill descriptor table");duplicates.Add(fd.Value);}
            Check(owner.Duplicate(0).Error==GuestError.TooManyFiles,"actual descriptor capacity enforcement");
            Check(owner.DescriptorCapacity().Value==capacity,"query is capacity, not free count");
            int code=Blink.ResourcePolicy((ulong)capacity,(uint)pid);Check(code==0,"resource policy C line "+code);
            foreach(int fd in duplicates)Check(owner.Close(fd).Succeeded,"release descriptor duplicate");
            code=Blink.MemoryResourcePolicy(budget);Check(code==0,"mapping policy C line "+code);
            owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
            Check(Blink.UnboundResources(9)==0,"disposed owner output atomicity");
            Check(owner.DescriptorCapacity().Error==GuestError.BadDescriptor,"disposed capacity");
            result.SetResult();
        }
        catch(Exception failure){result.TrySetException(failure);}
        finally {ResourceGcHooks.Barrier=null;Blink.UnbindHostIdentity();Blink.UnbindHostIo();Blink.BlinkHostMemoryDisposeWorker();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
    }){IsBackground=true};
    thread.Start();return result.Task;
}
await Task.WhenAll(Worker(8,17,32768),Worker(19,29,65536)).WaitAsync(TimeSpan.FromSeconds(15));
barrier.Dispose();
Console.WriteLine("resource ABI and query/idempotent/error invariants: PASS");

public static class ResourceGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void ResourceGc()
    {
        if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("owner barrier timeout");
        GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    }
}
