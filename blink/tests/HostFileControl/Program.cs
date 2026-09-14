using System;
using System.Collections.Generic;
using System.IO;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if(!value)throw new Exception("file control assertion failed"); }
var image=new Dictionary<string,ReadOnlyMemory<byte>> { ["/data/image"]="alpha"u8.ToArray() };
var owner=new InstanceIo(image);
try {
  Blink.BindHostIo(owner);
  Check(Blink.ControlProbe()==0 && Blink.ControlPrivateProbe()==0);
  var first=owner.OpenFile("/data/flags",FileAccessMode.Read|FileAccessMode.Write,closeOnExecFlag:true);
  Check(first.Succeeded);
  var copy=owner.Duplicate(first.Value);Check(copy.Succeeded);
  var retained=owner.FStat(copy.Value).Value;
  GC.Collect(2,GCCollectionMode.Forced,true,true);
  Check(owner.CloseOnExecDescriptors().Value==1 && owner.GetDescriptorFlags(first.Value).Error==GuestError.BadDescriptor);
  Check(owner.GetDescriptorFlags(copy.Value).Value==0 && owner.FStat(copy.Value).Value.Inode==retained.Inode);
  Check(owner.SetStatusFlags(copy.Value,1024).Succeeded);
  Check(owner.Seek(copy.Value,0,SeekOrigin.Begin).Succeeded);
  Check(owner.WriteAsync(copy.Value,"!"u8.ToArray()).GetAwaiter().GetResult().Value==1);
  Check(owner.FStat(copy.Value).Value.Length==retained.Length+1);
  Check(owner.Close(copy.Value).Succeeded);
  // FD reuse must not inherit flags from the prior occupant.
  var reused=owner.OpenFile("/data/image",FileAccessMode.Read);
  Check(reused.Succeeded && owner.GetDescriptorFlags(reused.Value).Value==0);
  Check(owner.Close(reused.Value).Succeeded);
  var socket=owner.Socket();Check(socket.Succeeded);
  Check(owner.GetStatusFlags(socket.Value).Value==2);
  Check(owner.SetStatusFlags(socket.Value,2048).Error==GuestError.Unsupported);
  Check(owner.GetStatusFlags(socket.Value).Value==2 && owner.Close(socket.Value).Succeeded);
  var small=new InstanceIo(image,descriptorLimit:5);
  try {
    var d=small.OpenFileAt(-100,"/data",FileAccessMode.Read,requireDirectory:true);Check(d.Succeeded);
    var f=small.OpenFileAt(d.Value,"image",FileAccessMode.Read);Check(f.Succeeded);
    Check(small.OpenFileAt(d.Value,"new",FileAccessMode.Write,create:true).Error==GuestError.TooManyFiles);
    Check(small.Stat("/data/new").Error==GuestError.NoEntry);
    Check(small.Duplicate(f.Value).Error==GuestError.TooManyFiles);
  } finally { small.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
} finally {
  Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
