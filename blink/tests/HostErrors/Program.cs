using System;
using System.Threading;
using Managed.Emulation;
if(Blink.ErrorProbe()!=0)throw new Exception("error constant probe failed");
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int value){var thread=new Thread(()=>{
  try{
    if(Blink.ErrorSetGet(value)!=value)throw new Exception("errno assignment failed");
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    if(Blink.ErrorRead()!=value)throw new Exception("errno storage crossed workers");
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
});thread.Start();return thread;}
var first=Start(36);var second=Start(132);first.Join();second.Join();if(failure!=null)throw failure;
