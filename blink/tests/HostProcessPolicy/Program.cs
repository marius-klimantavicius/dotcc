using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool yes) { if(!yes)throw new Exception("process policy failed"); }
int hostPid=Environment.ProcessId;
Check(Blink.ProcessPolicy(0)==0);
using var barrier=new Barrier(2);Exception? failure=null;Thread[] workers=new Thread[2];
for(int i=0;i<2;i++) {
    int pid=101+i;
    workers[i]=new Thread(()=> {
        try {
            Blink.BindHostIdentity(new HostIdentity(pid));
            Check(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));GC.Collect(2,GCCollectionMode.Forced,true,true);
            Check(Blink.ProcessPolicy(1)==0 && Environment.ProcessId==hostPid);
            Blink.UnbindHostIdentity();Check(Blink.ProcessPolicy(0)==0);
        }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
    }){IsBackground=true};
}
foreach(var worker in workers)worker.Start();foreach(var worker in workers)Check(worker.Join(TimeSpan.FromSeconds(10)));
if(failure!=null)throw failure;
Console.WriteLine("private process creation/exec denied, no children, immutable identity: PASS");
