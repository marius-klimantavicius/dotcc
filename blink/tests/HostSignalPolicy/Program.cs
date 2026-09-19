global using static SignalPolicyGcHooks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(int code) { if(code!=0)throw new Exception("signal policy C line "+code); }
using var barrier=new Barrier(2);
Task Worker(int pid)
{
    var completion=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var thread=new Thread(()=> {
        try {
            Check(Blink.SignalPolicyUnbound());
            Blink.BindHostIdentity(new HostIdentity(pid));SignalPolicyGcHooks.Barrier=barrier;
            Check(Blink.SignalPolicyCommon());Check(Blink.SignalPolicyPrivate(pid));
            Blink.UnbindHostIdentity();Check(Blink.SignalPolicyUnbound());
            completion.SetResult();
        } catch(Exception error){completion.TrySetException(error);}
        finally{Blink.UnbindHostIdentity();SignalPolicyGcHooks.Barrier=null;}
    }){IsBackground=true};
    thread.Start();return completion.Task;
}
await Task.WhenAll(Worker(17),Worker(29)).WaitAsync(TimeSpan.FromSeconds(15));
Console.WriteLine("signal policy common invariants: PASS");

public static class SignalPolicyGcHooks
{
    [ThreadStatic]public static Barrier? Barrier;
    public static void SignalPolicyGc()
    {
        if(Barrier!=null && !Barrier.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("barrier timeout");
        GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
    }
}
