using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("private access assertion failed");}
var host=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>> {
  ["/a/image"]="data"u8.ToArray(),["/a/executable"]="code"u8.ToArray(),["/other/image"]="data"u8.ToArray()
}, executablePaths:new HashSet<string>{"/a/executable"});
Blink.BindHostIo(host);Check(Blink.AccessProbe()==0 && Blink.AccessPrivate()==0);
Check(host.GetWorkingDirectory().Value=="/other");
Check(host.AccessAt(-100,"created",6).Succeeded);
host.DisposeAsync().AsTask().GetAwaiter().GetResult();Check(Blink.AccessUnavailable(9)==0);
Blink.UnbindHostIo();Check(Blink.AccessUnavailable(19)==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(bool executable){var thread=new Thread(()=>{
  var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/same"]="data"u8.ToArray()},executablePaths:executable ? new HashSet<string>{"/same"} : null);
  try{
    Blink.BindHostIo(owner);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.AccessWorker(executable?1:0)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
});thread.Start();return thread;}
var first=Start(false);var second=Start(true);first.Join();second.Join();if(failure!=null)throw failure;
