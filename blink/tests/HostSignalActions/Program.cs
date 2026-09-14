using System;
using System.Runtime.InteropServices;
using System.Threading;
using Managed.Emulation;
static void Check(bool value){if(!value)throw new Exception("private signal action assertion failed");}
var before=Native.Snapshot(10);var beforeSecond=Native.Snapshot(12);
Check(Blink.BlinkHostSignalActionsBegin()==0);
Check(Blink.ActionProbe()==0 && Blink.ActionPrivate()==0);
Blink.BlinkHostSignalActionsEnd();Check(Native.Snapshot(10)==before && Native.Snapshot(12)==beforeSecond);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(bool second){var worker=new Thread(()=>{
  try{
    Check(Blink.BlinkHostSignalActionsBegin()==0 && Blink.ActionWorkerSet(second?1:0)==0);
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.ActionWorkerRead(second?1:0)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.BlinkHostSignalActionsEnd();}
});worker.Start();return worker;}
var first=Start(false);var second=Start(true);first.Join();second.Join();if(failure!=null)throw failure;
Check(Native.Snapshot(10)==before && Native.Snapshot(12)==beforeSecond);
static unsafe class Native {
  [StructLayout(LayoutKind.Sequential)] struct Action {
    public nuint Handler;public fixed ulong Mask[16];public int Flags;public nuint Restorer;
  }
  [DllImport("libc",EntryPoint="sigaction",SetLastError=true)]
  private static extern int Sigaction(int signal,Action* action,Action* old);
  [DllImport("libc",EntryPoint="sigprocmask",SetLastError=true)]
  private static extern int Sigprocmask(int how,void* mask,void* old);
  public static (nuint,ulong,int,nuint,ulong) Snapshot(int signal){
    Action action=default;ulong* mask=stackalloc ulong[16];
    if(sizeof(Action)!=152 || Sigaction(signal,null,&action)!=0 || Sigprocmask(2,null,mask)!=0)throw new Exception("native query failed");
    return(action.Handler,action.Mask[0],action.Flags,action.Restorer,mask[0]);
  }
}
