using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool value){if(!value)throw new Exception("private directory assertion failed");}
static Dictionary<string,ReadOnlyMemory<byte>> Image()=>new(){["/data/image"]="alpha"u8.ToArray(),["/data/sub/child"]="x"u8.ToArray()};
static void Stop(InstanceIo owner)=>owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
static string Read(HostDirectories directories,nuint token){var result=directories.Read(token);Check(result.Succeeded && result.Value!=0);return Marshal.PtrToStringUTF8((nint)result.Value)!;}
static int Count(HostDirectories directories,nuint token){int count=0;while(true){var result=directories.Read(token);Check(result.Succeeded);if(result.Value==0)return count;++count;}}
var host=new InstanceIo(Image());using var dirs=new HostDirectories(host);
Blink.BindHostIo(host);Blink.BindHostDirectories(dirs);
Check(Blink.DirectoryLayout()==0 && Blink.DirectoryProbe()==0 && Blink.DirectoryPrivate()==0);
try{Blink.BindHostDirectories(dirs);throw new Exception("double binding accepted");}catch(InvalidOperationException){}
// Snapshot isolation, ordinal listing, true inode identity, and pointer stability.
var old=dirs.Open("/data");Check(old.Succeeded && host.GetDescriptorFlags(dirs.Descriptor(old.Value).Value).Value==1);Check(Read(dirs,old.Value)==".");
var dot=dirs.Read(old.Value);Check(dot.Succeeded);GC.Collect(2,GCCollectionMode.Forced,true,true);
Check(Marshal.PtrToStringUTF8((nint)dot.Value)=="..");
var inode=host.Stat("/data/image").Value.Inode;
var entry=dirs.Read(old.Value);Check(entry.Succeeded && (ulong)Marshal.ReadInt64((nint)entry.Value,256)==inode && Marshal.ReadByte((nint)entry.Value,264)==8);
int added=host.OpenFile("/data/new",FileAccessMode.Write,true).Value;Check(host.Close(added).Succeeded);
Check(dirs.Seek(old.Value,0).Succeeded && Count(dirs,old.Value)==4);
var fresh=dirs.Open("/data");Check(fresh.Succeeded && Count(dirs,fresh.Value)==5);Check(dirs.Close(fresh.Value).Succeeded && dirs.Close(old.Value).Succeeded);
// External descriptor replacement must neither redirect a stream nor let its
// eventual close release the new descriptor's unrelated open description.
int fd=host.OpenFile("/data",FileAccessMode.Read,allowDirectory:true).Value;
var replaced=dirs.Open(fd);Check(replaced.Succeeded && host.GetDescriptorFlags(fd).Value==0);
int file=host.OpenFile("/data/image",FileAccessMode.Read).Value;Check(host.DuplicateTo(file,fd).Succeeded);
Check(dirs.Read(replaced.Value).Error==GuestError.BadDescriptor && dirs.Close(replaced.Value).Error==GuestError.BadDescriptor);
Check(host.FStat(fd).Value.Length==5 && host.Close(fd).Succeeded && host.Close(file).Succeeded);
// Stream and descriptor positions are independent snapshots, while dup keeps
// the shared description alive until its final reference closes.
fd=host.OpenFile("/data",FileAccessMode.Read,allowDirectory:true).Value;int duplicate=host.Duplicate(fd).Value;
var left=dirs.Open(fd);var right=dirs.Open(duplicate);Check(left.Succeeded && right.Succeeded);
Check(Read(dirs,left.Value)=="." && Read(dirs,left.Value)==".." && Read(dirs,right.Value)==".");
Check(dirs.Close(left.Value).Succeeded && host.FStat(duplicate).Succeeded && dirs.Close(right.Value).Succeeded);
// A lease from another owner cannot mutate reference counts or descriptors.
var other=new InstanceIo(Image());fd=host.OpenFile("/data",FileAccessMode.Read,allowDirectory:true).Value;
var lease=host.AcquireDirectory(fd,16,1000);Check(lease.Succeeded);
Check(other.ReleaseDirectory(lease.Value).Error==GuestError.BadDescriptor && host.DirectoryDescriptor(lease.Value).Succeeded);
Check(host.ReleaseDirectory(lease.Value).Succeeded);Stop(other);
Blink.UnbindHostDirectories();dirs.Dispose();Check(dirs.ActiveStreams==0 && dirs.SnapshotNameBytes==0);
Blink.BindHostDirectories(dirs);Check(Blink.DirectoryUnavailable(9)==0);Blink.UnbindHostDirectories();Check(Blink.DirectoryUnavailable(19)==0);Blink.UnbindHostIo();Stop(host);
// Maximum valid name and explicit acquisition failure for the next byte.
foreach(int length in new[]{255,256}) {
  var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/long/"+new string('n',length)]=Array.Empty<byte>()});
  using var listing=new HostDirectories(owner);Blink.BindHostIo(owner);Blink.BindHostDirectories(listing);
  if(length==256){int before=owner.OpenDescriptors;Check(Blink.DirectoryLong()==0 && owner.OpenDescriptors==before);}
  else{var stream=listing.Open("/long");Check(stream.Succeeded);Read(listing,stream.Value);Read(listing,stream.Value);Check(Read(listing,stream.Value).Length==255);}
  Blink.UnbindHostDirectories();listing.Dispose();Blink.UnbindHostIo();Stop(owner);
}
// Active-stream quota and total context-registration quota preserve caller fds.
var empty=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());using(var listing=new HostDirectories(empty)) {
  var tokens=new List<nuint>();for(int i=0;i<HostDirectories.MaximumStreams;++i){var result=listing.Open("/");Check(result.Succeeded);tokens.Add(result.Value);}
  int caller=empty.OpenFile("/",FileAccessMode.Read,allowDirectory:true).Value;
  Check(listing.Open(caller).Error==GuestError.TooManyFiles && empty.FStat(caller).Succeeded);Check(empty.Close(caller).Succeeded);
  foreach(var token in tokens)Check(listing.Close(token).Succeeded);
  for(int i=tokens.Count;i<HostDirectories.MaximumRegistrations;++i){var result=listing.Open("/");Check(result.Succeeded && listing.Close(result.Value).Succeeded);}
  int before=empty.OpenDescriptors;Check(listing.Open("/").Error==GuestError.TooManyFiles && empty.OpenDescriptors==before);
}Stop(empty);
// Entry quota is independent of stream count; all snapshot storage is released.
var manyImage=new Dictionary<string,ReadOnlyMemory<byte>>();for(int i=0;i<1023;++i)manyImage["/n"+i]=Array.Empty<byte>();
var many=new InstanceIo(manyImage);using(var listing=new HostDirectories(many)) {
  var tokens=new List<nuint>();for(int i=0;i<3;++i){var result=listing.Open("/");Check(result.Succeeded);tokens.Add(result.Value);}
  Check(listing.Open("/").Error==GuestError.NoMemory);foreach(var token in tokens)Check(listing.Close(token).Succeeded);
  Check(listing.SnapshotNameBytes==0 && listing.Open("/").Succeeded);
}Stop(many);
var largeNames=new Dictionary<string,ReadOnlyMemory<byte>>();
for(int i=0;i<1023;++i)largeNames["/"+i.ToString("D4")+new string('n',251)]=Array.Empty<byte>();
var named=new InstanceIo(largeNames);using(var listing=new HostDirectories(named)) {
  var stream=listing.Open("/");Check(stream.Succeeded && listing.SnapshotNameBytes==1023*256+5);
  int before=named.OpenDescriptors;Check(listing.Open("/").Error==GuestError.NoMemory && named.OpenDescriptors==before);
  Check(listing.Close(stream.Value).Succeeded && listing.SnapshotNameBytes==0 && listing.Open("/").Succeeded);
}Stop(named);
// Owner disposal cancels streams; later End releases storage without IO access.
var canceled=new InstanceIo(Image());using(var listing=new HostDirectories(canceled)){
  var stream=listing.Open("/data");Check(stream.Succeeded);Stop(canceled);
  Check(listing.Read(stream.Value).Error==GuestError.BadDescriptor && listing.Close(stream.Value).Error==GuestError.BadDescriptor);
}
Exception? failure=null;using var barrier=new Barrier(2);
Thread Start(string name){var thread=new Thread(()=>{
  var owner=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{["/"+name]=Array.Empty<byte>()});using var listing=new HostDirectories(owner);
  try{
    Blink.BindHostIo(owner);Blink.BindHostDirectories(listing);var stream=listing.Open("/");Check(stream.Succeeded && Blink.DirectoryWorkerSetup()==0);
    barrier.SignalAndWait();GC.Collect(2,GCCollectionMode.Forced,true,true);barrier.SignalAndWait();
    Read(listing,stream.Value);Read(listing,stream.Value);Check(Read(listing,stream.Value)==name && Blink.DirectoryWorkerFinish(name=="first"?1:2)==0);
  }catch(Exception error){Interlocked.CompareExchange(ref failure,error,null);}
  finally{Blink.UnbindHostDirectories();listing.Dispose();Blink.UnbindHostIo();Stop(owner);}
});thread.Start();return thread;}
var first=Start("first");var second=Start("second");first.Join();second.Join();if(failure!=null)throw failure;
