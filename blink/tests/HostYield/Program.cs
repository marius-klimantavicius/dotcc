using System;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("yield assertion failed");}
Blink.BindHostEnvironment(new HostEnvironment());Check(Blink.YieldProbe()==0);Blink.UnbindHostEnvironment();Check(Blink.YieldUnbound()==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int tag){var worker=new Thread(()=>{try{Blink.BindHostEnvironment(new HostEnvironment());barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();Check(Blink.YieldWorker(tag)==0);}catch(Exception e){Interlocked.CompareExchange(ref failure,e,null);}finally{Blink.UnbindHostEnvironment();}});worker.Start();return worker;}
var first=Start(71);var second=Start(92);first.Join();second.Join();if(failure!=null)throw failure;
