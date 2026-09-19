using System;
using System.Collections.Generic;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
if(args.Length != 0) return 2;
// One owner per discarded guest-memory process; all upstream state is discarded after both cycles.
using var variables=new HostVariables();
using var sleep=new HostSleep();
var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
var directories=new HostDirectories(io);
try {
    Blink.BindHostIo(io);Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    return Blink.GuestMemoryRun();
}
finally {
    try { Blink.GuestMemoryDestroy(); }
    finally {
    try { if (Blink.GuestMemoryRelease() != 0) throw new InvalidOperationException("Guest-memory owner cleanup failed"); }
    finally {
    Blink.UnbindHostIdentity();Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();directories.Dispose();Blink.UnbindHostIo();
    Blink.UnbindHostVariables();Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    }
}
