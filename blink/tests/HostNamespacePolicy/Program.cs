global using static NamespacePolicyGcHooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(int code){if(code!=0)throw new Exception("namespace policy C line "+code);}
using var barrier=new Barrier(2);
Task Worker()
{
    var completion=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var thread=new Thread(()=> {
        var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/image"]=new byte[]{1,2,3}});
        try {
            Check(Blink.NamespacePolicyUnavailable(19));
            Blink.BindHostIo(owner);NamespacePolicyGcHooks.Barrier=barrier;
            Check(Blink.NamespacePolicyCommon());
            var file=owner.Stat("work");var image=owner.Stat("image");var capacity=owner.FileSystemCapacity("/");
            int descriptors=owner.OpenDescriptors;
            Check(Blink.NamespacePolicyPrivate());
            if(owner.Stat("work")!=file || owner.Stat("image")!=image || owner.FileSystemCapacity("/")!=capacity || owner.OpenDescriptors!=descriptors)
                throw new Exception("refused operation changed namespace metadata, quota or descriptors");
            owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
            Check(Blink.NamespacePolicyUnavailable(9));
            completion.SetResult();
        }catch(Exception error){completion.TrySetException(error);}
        finally{Blink.UnbindHostIo();NamespacePolicyGcHooks.Barrier=null;owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
    }){IsBackground=true};
    thread.Start();return completion.Task;
}
await Task.WhenAll(Worker(),Worker()).WaitAsync(TimeSpan.FromSeconds(15));
Console.WriteLine("namespace and network profile invariants: PASS");
public static class NamespacePolicyGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void NamespacePolicyGc()
    {
        if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("barrier timeout");
        GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    }
}
