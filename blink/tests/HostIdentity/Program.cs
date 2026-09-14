using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("Identity assertion failed"); }
Blink.BindHostIdentity(new HostIdentity());
Check(Blink.Pid()==1 && Blink.Parent()==0 && Blink.User()==0 && Blink.Group()==0);
Check(Blink.main()==0);
Blink.UnbindHostIdentity();
Check(Blink.Pid()==-1 && Blink.User()==uint.MaxValue);
using var barrier=new Barrier(2);
Exception? failure=null;
Thread[] workers=new Thread[2];
for(int i=0;i<workers.Length;i++) {
    int pid=111+i;
    workers[i]=new Thread(()=> {
        try {
            Blink.BindHostIdentity(new HostIdentity(pid,7));
            Check(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));
            GC.Collect(2,GCCollectionMode.Forced,true,true);
            Check(Blink.Pid()==pid && Blink.Parent()==7 && Blink.User()==0 && Blink.Group()==0);
            Blink.UnbindHostIdentity();Check(Blink.Pid()==-1);
        } catch(Exception error) { Interlocked.CompareExchange(ref failure,error,null); }
    }) { IsBackground=true };
}
foreach(var worker in workers)worker.Start();
foreach(var worker in workers)Check(worker.Join(TimeSpan.FromSeconds(10)));
if(failure!=null)throw failure;
