using System;
using System.Collections.Generic;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool ok){if(!ok)throw new Exception("namespace model assertion failed");}
var image=new Dictionary<string,ReadOnlyMemory<byte>>{["/data/image"]="immutable"u8.ToArray()};
var owner=new InstanceIo(image);var other=new InstanceIo(image);
try {
 Blink.BindHostIo(owner);
 Check(Blink.NamespaceProbe()==0 && Blink.NamespacePrivate()==0);
 Check(owner.MakeDirectoryAt(-100,"/data/dir",0x1c0).Succeeded);
 int d=owner.OpenFile("/data/dir",FileAccessMode.Read,allowDirectory:true,requireDirectory:true).Value;
 Check(owner.RenameAt(-100,"/data/dir",-100,"/data/newdir").Succeeded);
 int file=owner.OpenFileAt(d,"file",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 Check(owner.Stat("/data/newdir/file").Succeeded && other.Stat("/data/newdir").Error==GuestError.NoEntry);
 Check(owner.UnlinkAt(d,"file").Succeeded);
 Check(owner.FStat(file).Value.Links==0);
 GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
 Check(owner.WriteAsync(file,"x"u8.ToArray()).GetAwaiter().GetResult().Value==1);
 Check(owner.Close(file).Succeeded && owner.UnlinkAt(-100,"/data/newdir",512).Succeeded);
 Check(owner.FStat(d).Value.Links==0 && owner.MakeDirectoryAt(d,"orphan",0x1c0).Error==GuestError.NoEntry);
 Check(owner.Close(d).Succeeded);
 Blink.UnbindHostIo();
 unsafe {Check(Blink.blink_host_unlink(null)==-1);}
} finally {Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();other.DisposeAsync().AsTask().GetAwaiter().GetResult();}
// Retained unlinked nodes consume bytes and inode quota until last description closes.
using(var fs=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),writableLimit:4,nodeLimit:2)) {
 int file=fs.Open("/x",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 Check(fs.Write(file,"abcd"u8).Value==4);int copy=fs.Duplicate(file).Value;
 Check(fs.Unlink("/x").Succeeded && fs.NodeCount==2 && fs.WritableBytes==4 && fs.FStat(file).Value.Links==0);
 Check(fs.MakeDirectory("/new",0x1c0).Error==GuestError.NoSpace);
 Check(fs.Close(file).Succeeded && fs.WritableBytes==4);
 Check(fs.Close(copy).Succeeded && fs.WritableBytes==0 && fs.NodeCount==1);
 Check(fs.MakeDirectory("/new",0x1c0).Succeeded);
}
// Rename quota failures leave paths, metadata and open descriptions unchanged.
using(var fs=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),pathBytesLimit:12)) {
 Check(fs.MakeDirectory("/a",0x1c0).Succeeded);int directory=fs.Open("/a",FileAccessMode.Read,allowDirectory:true).Value;
 int file=fs.Open("/a/f",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 var before=fs.FStat(file).Value;long names=fs.PathBytes;
 Check(fs.Rename("/a","/longname").Error==GuestError.NoSpace);
 Check(fs.PathBytes==names && fs.Stat("/a/f").Value==before && fs.DirectoryPath(directory).Value=="/a");
 Check(fs.Rename("/a","/b").Succeeded && fs.DirectoryPath(directory).Value=="/b");
 Check(fs.Unlink("/b",true).Error==GuestError.NotEmpty);
}
// Advisory locks follow descriptions across dup; final close releases the lock
// and a detached allocation without waiting for another lock operation.
using(var fs=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),writableLimit:4)) {
 int first=fs.Open("/locked",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 int duplicate=fs.Duplicate(first).Value;
 int independent=fs.Open("/locked",FileAccessMode.Read|FileAccessMode.Write).Value;
 Check(fs.Write(first,"lock"u8).Value==4 && fs.AdvisoryLock(first,6).Succeeded);
 Check(fs.Close(first).Succeeded && fs.AdvisoryLock(independent,6).Error==GuestError.Again);
 Check(fs.Close(duplicate).Succeeded && fs.AdvisoryLock(independent,6).Succeeded);
 Check(fs.Unlink("/locked").Succeeded && fs.WritableBytes==4);
 Check(fs.Close(independent).Succeeded && fs.WritableBytes==0 && fs.NodeCount==1);
}
// Replacement targets also retain their allocation until their last open fd closes.
using(var fs=new VirtualFileSystem(new Dictionary<string,ReadOnlyMemory<byte>>(),writableLimit:8,nodeLimit:4)) {
 int source=fs.Open("/source",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 int target=fs.Open("/target",FileAccessMode.Read|FileAccessMode.Write,create:true).Value;
 Check(fs.Write(source,"from"u8).Value==4 && fs.Write(target,"into"u8).Value==4);
 ulong inode=fs.FStat(source).Value.Inode;
 Check(fs.Rename("/source","/target").Succeeded && fs.WritableBytes==8 && fs.NodeCount==3);
 Check(fs.Stat("/target").Value.Inode==inode && fs.FStat(target).Value.Links==0);
 Check(fs.Close(target).Succeeded && fs.WritableBytes==4 && fs.NodeCount==2);
 Check(fs.Close(source).Succeeded && fs.Stat("/target").Value.Inode==inode);
}
// Directory snapshot leases survive unlink and fd reuse without retargeting.
var streams=new InstanceIo(image);
try {
 Check(streams.MakeDirectoryAt(-100,"/data/snapshot",0x1c0).Succeeded);
 int fd=streams.OpenFile("/data/snapshot",FileAccessMode.Read,allowDirectory:true,requireDirectory:true).Value;
 var lease=streams.AcquireDirectory(fd,100,4096);Check(lease.Succeeded);
 Check(streams.UnlinkAt(-100,"/data/snapshot",512).Succeeded && streams.Close(fd).Succeeded);
 Check(lease.Value.Entries.Count==2);
 int reused=streams.OpenFile("/data/image",FileAccessMode.Read).Value;
 Check(reused==fd && streams.ReleaseDirectory(lease.Value).Error==GuestError.BadDescriptor && streams.FStat(reused).Succeeded);
} finally {streams.DisposeAsync().AsTask().GetAwaiter().GetResult();}
