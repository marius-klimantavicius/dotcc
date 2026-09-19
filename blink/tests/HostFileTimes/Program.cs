using System;
using System.Collections.Generic;
using Managed.Emulation;
using Managed.Emulation.Host;
static void Check(bool ok){if(!ok)throw new Exception("file timestamp assertion failed");}
var image=new Dictionary<string,ReadOnlyMemory<byte>>{["/data/image"]="immutable"u8.ToArray()};
var owner=new InstanceIo(image);var other=new InstanceIo(image);
try {
 Blink.BindHostIo(owner);
 Check(Blink.TimesLayout()==0 && Blink.TimesProbe()==0 && Blink.TimesPrivate()==0);
 var before=owner.Stat("/data/work").Value;
 Check(other.Stat("/data/work").Error==GuestError.NoEntry);
 GC.Collect(2,GCCollectionMode.Forced,true,true);GC.WaitForPendingFinalizers();
 Check(owner.Stat("/data/work").Value==before);
 int fd=owner.OpenFile("/data/work",FileAccessMode.Read).Value;
 var explicitTime=new VirtualFileTimeUpdate(FileTimeSelection.Explicit,new(17,19));
 var omit=new VirtualFileTimeUpdate(FileTimeSelection.Omit);
 Check(owner.SetFileTimes(fd,explicitTime,omit).Succeeded);
 var changed=owner.FStat(fd).Value;Check(changed.AccessSubtick==19 && changed.ModifyTicks==before.ModifyTicks && changed.ModifySubtick==before.ModifySubtick);
 var invalid=new VirtualFileTimeUpdate((FileTimeSelection)999);
 Check(owner.SetFileTimes(fd,explicitTime,invalid).Error==GuestError.Invalid && owner.FStat(fd).Value==changed);
 Check(owner.SetFileTimes(1,explicitTime,omit).Error==GuestError.Unsupported);
 Check(owner.Close(fd).Succeeded);
 owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
 Check(owner.SetFileTimesAt(-100,"/data/work",omit,omit).Error==GuestError.BadDescriptor);
 Blink.UnbindHostIo();
 unsafe {Check(Blink.blink_host_futimens(0,null)==-1);}
} finally {Blink.UnbindHostIo();owner.DisposeAsync().AsTask().GetAwaiter().GetResult();other.DisposeAsync().AsTask().GetAwaiter().GetResult();}
