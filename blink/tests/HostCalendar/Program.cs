using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool yes) { if(!yes)throw new Exception("calendar assertion failed"); }
Blink.BindHostEnvironment(new HostEnvironment());
Check(Blink.Calendar()==0 && Blink.CalendarPrivate(1)==0);Blink.UnbindHostEnvironment();Check(Blink.CalendarPrivate(0)==0);
using var barrier=new Barrier(2);Exception? failure=null;Thread[] workers=new Thread[2];
for(int i=0;i<2;i++) {
    long seconds=i==0?-1:1700000000;
    workers[i]=new Thread(()=> {
        try {
            Blink.BindHostEnvironment(new HostEnvironment(new FixedClock(seconds)));
            Check(barrier.SignalAndWait(TimeSpan.FromSeconds(10)));GC.Collect(2,GCCollectionMode.Forced,true,true);
            Check(Blink.TimeValue()==seconds);Blink.UnbindHostEnvironment();
        }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
    }){IsBackground=true};
}
foreach(var worker in workers)worker.Start();foreach(var worker in workers)Check(worker.Join(TimeSpan.FromSeconds(10)));
if(failure!=null)throw failure;
sealed class FixedClock(long seconds):TimeProvider { public override DateTimeOffset GetUtcNow()=>DateTimeOffset.FromUnixTimeSeconds(seconds); }
