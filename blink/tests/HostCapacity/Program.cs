using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("private capacity assertion failed");}
var host=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/data/image"]="alpha"u8.ToArray()},writableLimit:17);
Blink.BindHostIo(host);Check(Blink.CapacityLayout()==0 && Blink.CapacityProbe()==0 && Blink.CapacityPrivate()==0);
Check(Blink.CapacityExact(22,17,3)==0);
int fd=host.OpenFile("/data/work",FileAccessMode.Read|FileAccessMode.Write,true).Value;
Check(Blink.CapacityExact(22,17,4)==0);
int copy=host.Duplicate(fd).Value;
Check(host.WriteAt(fd,"abc"u8,0).Value==3 && Blink.CapacityExact(22,14,4)==0);
Check(host.TruncateFile(copy,17).Succeeded && Blink.CapacityExact(22,0,4)==0);
Check(host.WriteAt(fd,"X"u8,17).Error==GuestError.NoSpace);
Check(host.TruncateFile(fd,18).Error==GuestError.NoSpace && Blink.CapacityExact(22,0,4)==0);
Check(host.TruncateFile("/data/work",2).Succeeded && Blink.CapacityExact(22,15,4)==0);
Check(host.Close(fd).Succeeded && host.FileSystemCapacity(copy).Value.FreeBytes==15);
Check(host.TruncateFile(copy,0).Succeeded && Blink.CapacityExact(22,17,4)==0 && host.Close(copy).Succeeded);
Check(host.ChangeDirectory("/data").Succeeded && Blink.CapacityRelative()==0);
int socket=host.Socket().Value;Check(Blink.CapacityFdUnsupported(socket)==0 && host.Close(socket).Succeeded);
host.DisposeAsync().AsTask().GetAwaiter().GetResult();Check(Blink.CapacityUnavailable(9)==0);
Blink.UnbindHostIo();Check(Blink.CapacityUnavailable(19)==0);
// Node and path limits are independent of free byte capacity.
using(var limited=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),writableLimit:8,nodeLimit:2,pathBytesLimit:100)) {
  Check(limited.Capacity("/").Value.TotalNodes==2 && limited.Capacity("/").Value.FreeNodes==1);
  int empty=limited.Open("/a",FileAccessMode.Write,true).Value;
  Check(limited.Capacity(empty).Value.FreeNodes==0 && limited.Capacity(empty).Value.FreeBytes==8);
  Check(limited.Open("/b",FileAccessMode.Write,true).Error==GuestError.NoSpace);
}
using(var limited=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),writableLimit:8,nodeLimit:8,pathBytesLimit:1)) {
  Check(limited.Capacity("/").Value.FreeNodes==7 && limited.Capacity("/").Value.FreeBytes==8);
  Check(limited.Open("/a",FileAccessMode.Write,true).Error==GuestError.NoSpace);
}
using(var names=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),nodeLimit:2,pathBytesLimit:5000)) {
  Check(names.Open("/"+new string('n',4095),FileAccessMode.Write,true).Succeeded);
  Check(names.Open("/"+new string('n',4096),FileAccessMode.Write,true).Error==GuestError.NameTooLong);
}
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(int limit,int imageLength){var thread=new Thread(()=>{
  var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/image"]=new byte[imageLength]},writableLimit:limit);
  try{
    Blink.BindHostIo(owner);barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Check(Blink.CapacityExact((ulong)(limit+imageLength),(ulong)limit,2)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();}
});thread.Start();return thread;}
var first=Start(0,0);var second=Start(97,7);first.Join();second.Join();if(failure!=null)throw failure;
