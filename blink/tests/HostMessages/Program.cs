global using static MessageGcHooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value,string message){if(!value)throw new Exception(message);}
var barrier=new Barrier(2);
Task Run()=>Task.Run(()=> {
    var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
    try {
        Blink.BindHostIo(owner);MessageGcHooks.Barrier=barrier;
        Check(Blink.Errors()==0,"descriptor errors");
        int result=Blink.Exchange(8080);Check(result==0,"C exchange "+result);
        Check(owner.OpenDescriptors==3,"all socket descriptors closed");
    }
    finally {MessageGcHooks.Barrier=null;Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
});
await Task.WhenAll(Run(),Run()).WaitAsync(TimeSpan.FromSeconds(15));
barrier.Dispose();

foreach(int zero in new[]{0,1}) {
    var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
    int listener=owner.Socket().Value;Check(owner.Bind(listener,new(GuestEndpoint.Loopback,8080)).Succeeded,"bind cancel");
    Check(owner.Listen(listener,1).Succeeded,"listen cancel");
    int client=owner.Socket().Value;Check((await owner.ConnectAsync(client,new(GuestEndpoint.Loopback,8080))).Succeeded,"connect cancel");
    var accepted=await owner.AcceptAsync(listener);Check(accepted.Succeeded,"accept cancel");
    var done=new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
    var worker=new Thread(()=> {
        try {Blink.BindHostIo(owner);done.SetResult(Blink.WaitMessage(accepted.Value.Handle,zero));}
        catch(Exception e){done.TrySetException(e);}finally{Blink.UnbindHostIo();}
    }){IsBackground=true};
    worker.Start();
    var deadline=DateTime.UtcNow+TimeSpan.FromSeconds(5);
    while(owner.PendingSocketOperations==0 && !done.Task.IsCompleted && DateTime.UtcNow<deadline)await Task.Delay(1);
    Check(owner.PendingSocketOperations>0 && !done.Task.IsCompleted,"receive must actually wait, including zero capacity");
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    await owner.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
    Check(await done.Task.WaitAsync(TimeSpan.FromSeconds(5))==0,"cancellation preserves outputs and returns ECANCELED");
    Check(worker.Join(TimeSpan.FromSeconds(5)),"worker teardown");
    Check(owner.PendingSocketOperations==0,"no pending socket operation after disposal");
    Check(owner.PeerEndpoint(client).Error==GuestError.BadDescriptor,"disposed peer lookup");
}
Console.WriteLine("private TCP messages: peers, gather/scatter, short transfers, EOF: PASS");

public static class MessageGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void ForceGc()
    {
        if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("two-owner GC barrier");
        GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    }
}
