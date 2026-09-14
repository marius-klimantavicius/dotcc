using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("sleep assertion failed");}
using(var owner=new HostSleep()) {
  Blink.BindHostSleep(owner);Blink.BindHostEnvironment(new HostEnvironment());
  Check(Blink.SleepProbe()==0 && Blink.SleepNull()==0);
  owner.Interrupt();Check(Blink.SleepInterrupted(1)==0);
  Blink.UnbindHostSleep();Blink.UnbindHostEnvironment();
}
Check(Blink.SleepErrorProbe(19)==0);
using(var disposed=new HostSleep()) {disposed.Dispose();Blink.BindHostSleep(disposed);Check(Blink.SleepErrorProbe(9)==0);Blink.UnbindHostSleep();}
foreach(bool dispose in new[]{false,true}) {
  using var owner=new HostSleep();Exception? failure=null;
  var worker=new Thread(()=>{try{Blink.BindHostSleep(owner);Check(Blink.SleepInterrupted(long.MaxValue)==0);}catch(Exception e){failure=e;}finally{Blink.UnbindHostSleep();}});
  worker.Start();Check(SpinWait.SpinUntil(()=>owner.IsWaiting,TimeSpan.FromSeconds(10)));
  Check(owner.Sleep(0,0).Error==16);GC.Collect(2,GCCollectionMode.Forced,true,true);
  if(dispose)owner.Dispose();else owner.Interrupt();
  Check(worker.Join(TimeSpan.FromSeconds(10)));if(failure!=null)throw failure;
  if(!dispose)Check(owner.Sleep(0,0).Error==0);
}
using(var left=new HostSleep())using(var right=new HostSleep()) {
  Exception? failure=null;
  Thread Start(HostSleep owner){var thread=new Thread(()=>{try{Blink.BindHostSleep(owner);Check(Blink.SleepInterrupted(60)==0);}catch(Exception e){Interlocked.CompareExchange(ref failure,e,null);}finally{Blink.UnbindHostSleep();}});thread.Start();return thread;}
  var first=Start(left);var second=Start(right);
  Check(SpinWait.SpinUntil(()=>left.IsWaiting && right.IsWaiting,TimeSpan.FromSeconds(10)));
  left.Interrupt();Check(first.Join(TimeSpan.FromSeconds(10)) && right.IsWaiting);
  GC.Collect(2,GCCollectionMode.Forced,true,true);right.Dispose();Check(second.Join(TimeSpan.FromSeconds(10)));
  if(failure!=null)throw failure;
}
