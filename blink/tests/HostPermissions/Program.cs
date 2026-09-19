global using static PermissionGcHooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value,string message){if(!value)throw new Exception(message);}
var barrier=new Barrier(2);
Task Worker(uint mask,int pid)
{
    var completion=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    new Thread(()=> {
        Exception? failure=null;
        var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{{"/image","immutable"u8.ToArray()}});
        try {
            Check(Blink.UnboundPermissions(19)==0,"unbound permission policy");
            Blink.BindHostIo(owner);Blink.BindHostIdentity(new HostIdentity(pid));PermissionGcHooks.Barrier=barrier;
            int result=Blink.Permissions(mask);Check(result==0,"C permission line "+result);
            Check(owner.OpenDescriptors==3,"all descriptors closed");
            Check((owner.Stat("/work/open-mode").Value.Mode&0x1ff)==0x194,"actual creation bits");
            owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
            Check(Blink.UnboundPermissions(9)==0,"disposed permission policy");
        } catch(Exception error){failure=error;}
        finally {PermissionGcHooks.Barrier=null;Blink.UnbindHostIdentity();Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
        if(failure==null)completion.SetResult();else completion.SetException(failure);
    }){IsBackground=true}.Start();
    return completion.Task;
}
await Task.WhenAll(Worker(0x17,17),Worker(0x3f,29)).WaitAsync(TimeSpan.FromSeconds(20));
barrier.Dispose();
Console.WriteLine("private permission/ownership/creation invariants: PASS");
public static class PermissionGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void PermissionsGc(){if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("owner barrier");GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();}
}
