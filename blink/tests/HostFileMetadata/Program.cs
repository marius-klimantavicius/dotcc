using System;
using System.Collections.Generic;
using System.Threading;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if (!value) throw new Exception("metadata assertion failed"); }
var image = new Dictionary<string,ReadOnlyMemory<byte>> {
    ["/data/image"] = "alpha"u8.ToArray(), ["/data/executable"] = "ELF"u8.ToArray()
};
var owner = new InstanceIo(image, executablePaths:new HashSet<string>{"/data/executable"});
var other = new InstanceIo(image);
try {
    Blink.BindHostIo(owner);
    Check(Blink.MetadataLayout()==0 && Blink.MetadataProbe()==0 && Blink.MetadataPrivateErrors()==0);
    Check(other.Stat("/data/work").Error==GuestError.NoEntry);
    Check((other.Stat("/data/executable").Value.Mode&0x49)==0);
    var first = owner.OpenFile("/data/work",FileAccessMode.Read|FileAccessMode.Write);
    Check(first.Succeeded);
    var before=owner.FStat(first.Value).Value;
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    Check(owner.WriteAsync(first.Value,"new"u8.ToArray()).GetAwaiter().GetResult().Value==3);
    var after=owner.FStat(first.Value).Value;
    Check(before.Inode==after.Inode && after.Length==3 && after.ModifyTicks>0);
    Check(owner.Stat("/data/work").Value==after);
    var copy=owner.Duplicate(first.Value);Check(copy.Succeeded);
    Check(owner.Close(first.Value).Succeeded && owner.FStat(copy.Value).Value.Inode==after.Inode);
    Check(owner.Close(copy.Value).Succeeded && owner.FStat(copy.Value).Error==GuestError.BadDescriptor);
    Blink.UnbindHostIo();
    // Thread-local binding is absent after unbind; no generic host fallback.
    unsafe { Blink.blink_host_stat_record value=default; Check(Blink.blink_host_fstat(0,&value)==-1); }
} finally {
    Blink.UnbindHostIo();
    owner.DisposeAsync().AsTask().GetAwaiter().GetResult();
    other.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
