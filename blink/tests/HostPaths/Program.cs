using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool condition){if(!condition)throw new Exception("path boundary assertion failed");}
static InstanceIo Owner()=>new(new Dictionary<string,ReadOnlyMemory<byte>> {
  ["/a/seed"]="seed"u8.ToArray(),["/a/b/leaf"]="leaf"u8.ToArray(),
  ["/other/seed"]="another"u8.ToArray(),["/é/leaf"]="leaf"u8.ToArray() });
var host=Owner();Blink.BindHostIo(host);
Check(Blink.PathProbe()==0 && Blink.PathPrivate()==0);
// Public APIs inherit cwd unless an explicit override is supplied.
Check(host.GetWorkingDirectory().Value=="/a");
Check(host.Stat("seed").Value.Length==4 && host.Stat("seed","/other").Value.Length==7);
int file=host.OpenFile("seed",FileAccessMode.Read).Value;Check(host.FStat(file).Value.Length==4);Check(host.Close(file).Succeeded);
file=host.OpenFile("seed",FileAccessMode.Read,cwd:"/other").Value;Check(host.FStat(file).Value.Length==7);Check(host.Close(file).Succeeded);
unsafe{
  void* saved=Blink.SavePaths();Check(saved!=null);
  Check(host.ChangeDirectory("/other").Succeeded);
  GC.Collect(2,GCCollectionMode.Forced,true,true);
  host.DisposeAsync().AsTask().GetAwaiter().GetResult();
  Check(Blink.CheckSavedPaths(saved)==0);
}
Check(Blink.PathUnavailable(9)==0);Blink.UnbindHostIo();Check(Blink.PathUnavailable(19)==0);
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(bool second){var worker=new Thread(()=>{
  var owner=Owner();
  try{
    Blink.BindHostIo(owner);Check(owner.ChangeDirectory(second ? "/other" : "/a").Succeeded);
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.PathWorker(second?1:0)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
});worker.Start();return worker;}
var a=Start(false);var b=Start(true);a.Join();b.Join();if(failure!=null)throw failure;
