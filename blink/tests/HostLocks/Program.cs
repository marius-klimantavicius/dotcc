global using static LocksGcHooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(int code){if(code!=0)throw new Exception("locks C line "+code);}
using var barrier=new Barrier(2);
Task Worker()
{
    var completion=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var thread=new Thread(()=> {
        var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
        try {
            Check(Blink.LocksUnbound(19));
            Blink.BindHostIo(owner);LocksGcHooks.Barrier=barrier;
            Check(Blink.LocksCommon());Check(Blink.LocksPrivate());
            owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
            Check(Blink.LocksUnbound(9));
            completion.SetResult();
        }catch(Exception error){completion.TrySetException(error);}
        finally{Blink.UnbindHostIo();LocksGcHooks.Barrier=null;owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
    }){IsBackground=true};
    thread.Start();return completion.Task;
}
await Task.WhenAll(Worker(),Worker()).WaitAsync(TimeSpan.FromSeconds(15));
Console.WriteLine("private file locking common invariants: PASS");
public static class LocksGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void LocksGc()
    {
        if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("barrier timeout");
        GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    }
}
