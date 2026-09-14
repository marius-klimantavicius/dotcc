using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool yes) { if(!yes)throw new Exception("identity details failed"); }
static unsafe void Private(int pid,string name)
{
    byte[] bytes=System.Text.Encoding.ASCII.GetBytes(name+"\0");
    fixed(byte* p=bytes) Check(Blink.PrivateDetails(pid,p)==0);
}
Blink.BindHostIdentity(new HostIdentity());
Check(Blink.Details()==0);Private(1,"blink-1");Blink.UnbindHostIdentity();Check(Blink.UnboundDetails()==0);
using var barrier=new Barrier(2);Exception? failure=null;
Thread[] workers=new Thread[2];
for(int i=0;i<2;i++) {
    int pid=101+i;string name="instance-"+pid;
    workers[i]=new Thread(()=> {
        try {
            Blink.BindHostIdentity(new HostIdentity(pid,0,name));
            Check(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));
            GC.Collect(2,GCCollectionMode.Forced,true,true);Private(pid,name);
            Blink.UnbindHostIdentity();Check(Blink.UnboundDetails()==0);
        }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
    }){IsBackground=true};
}
foreach(var worker in workers)worker.Start();
foreach(var worker in workers)Check(worker.Join(TimeSpan.FromSeconds(10)));
if(failure!=null)throw failure;
