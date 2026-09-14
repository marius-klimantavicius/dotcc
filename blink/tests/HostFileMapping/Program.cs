using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value) { if(!value)throw new Exception("file mapping assertion failed"); }
byte[] contents=new byte[131123];for(int i=0;i<contents.Length;++i)contents[i]=(byte)(i%251);
var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>> { ["/image"]=contents,["/empty"]=Array.Empty<byte>() });
try {
  Blink.BindHostIo(owner);Check(Blink.FileMappingProbe()==0);
} finally { Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
using var ready=new CountdownEvent(2);using var release=new ManualResetEventSlim();
Exception? failure=null;
Thread Start(int tag) {
  var worker=new Thread(()=> {
    byte[] data=new byte[4096];data[0]=(byte)tag;
    var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/image"]=data});
    bool signaled=false;
    try {
      Blink.BindHostIo(io);Check(Blink.HoldFileMapping()==0);
      Blink.UnbindHostIo();io.DisposeAsync().AsTask().GetAwaiter().GetResult();
      ready.Signal();signaled=true;release.Wait();
      Check(Blink.ReleaseFileMapping(tag)==0);
    } catch(Exception error) {Interlocked.CompareExchange(ref failure,error,null);}
    finally {if(!signaled)ready.Signal();Blink.UnbindHostIo();io.DisposeAsync().AsTask().GetAwaiter().GetResult();}
  });worker.Start();return worker;
}
var one=Start(71);var two=Start(92);
Check(ready.Wait(TimeSpan.FromSeconds(10)));GC.Collect(2,GCCollectionMode.Forced,true,true);
release.Set();one.Join();two.Join();if(failure!=null)throw failure;
