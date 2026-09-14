using System;
using System.Threading;
using Managed.Emulation;
static void Check(bool value){if(!value)throw new Exception("exit callback assertion failed");}
Check(Blink.BlinkHostExitCallbacksBegin()==0 && Blink.SetupCallbacks()==0);
GC.Collect(2,GCCollectionMode.Forced,true,true);
Check(Blink.BlinkHostExitCallbacksRun()==0 && Blink.CallbackResult()==0);
Check(Blink.PrivateCallbackChecks()==0 && Blink.JumpCallbackChecks()==0);
Check(Blink.SetupThrowingCallback()==0);
try{Blink.BlinkHostExitCallbacksRun();throw new Exception("callback exception swallowed");}
catch(InvalidOperationException error) when(error.Message=="intentional exit callback failure"){}
Check(Blink.CheckThrownCallback()==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int value){var worker=new Thread(()=>{
  try{
    Check(Blink.WorkerSetup(value)==0);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.WorkerFinish(value)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.BlinkHostExitCallbacksEnd();}
});worker.Start();return worker;}
var first=Start(71);var second=Start(92);first.Join();second.Join();if(failure!=null)throw failure;
namespace Managed.Emulation {
  public static partial class Blink {
    public static void ThrowingExitCallback()=>throw new InvalidOperationException("intentional exit callback failure");
  }
}
