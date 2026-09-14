using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("clock bridge assertion failed");}
Blink.BindHostEnvironment(new HostEnvironment());
Check(Blink.ClockProbe()==0 && Blink.ClockPrivateErrors()==0 && Blink.UsesClockNanosleep()==0);
Blink.UnbindHostEnvironment();Check(Blink.ClockUnbound()==0);
var cases=new (long ticks,long frequency,long sec,long us,long resolution_sec,long resolution_nsec)[]{
  (12_340_005_678,10_000_000,1234,567,0,100),
  (-1,2,-1,999999,0,500000000),
  (0,1,0,0,1,0),
  (10_000_000,3,1,0,0,333333334),
  (20_000_000,long.MaxValue,2,0,0,100)
};
foreach(var item in cases){
  Blink.BindHostEnvironment(new HostEnvironment(new FixedTime(item.ticks,item.frequency)));
  Check(Blink.ClockInjected(item.sec,item.us,item.resolution_sec,item.resolution_nsec)==0);
  Blink.UnbindHostEnvironment();
}
Blink.BindHostEnvironment(new HostEnvironment(new FailingTime()));Check(Blink.ClockProviderError(0)==0);Blink.UnbindHostEnvironment();
Blink.BindHostEnvironment(new HostEnvironment(new FixedTime(0,0)));Check(Blink.ClockProviderError(1)==0);Blink.UnbindHostEnvironment();
Exception? failure=null;
using var barrier=new Barrier(2);
Thread Start(int tag){var worker=new Thread(()=>{
  try{
    Blink.BindHostEnvironment(new HostEnvironment(new FixedTime(tag*10_000_000L,10_000_000)));
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.ClockInjected(tag,0,0,100)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostEnvironment();}
});worker.Start();return worker;}
var first=Start(71);var second=Start(92);first.Join();second.Join();if(failure!=null)throw failure;
sealed class FixedTime(long ticks,long frequency):TimeProvider{
  public override DateTimeOffset GetUtcNow()=>DateTimeOffset.UnixEpoch.AddTicks(ticks);
  public override long GetTimestamp()=>0;
  public override long TimestampFrequency=>frequency;
}
sealed class FailingTime:TimeProvider{
  public override DateTimeOffset GetUtcNow()=>throw new InvalidOperationException("injected clock failure");
}
