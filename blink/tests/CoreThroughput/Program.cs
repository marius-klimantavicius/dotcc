using System;
using System.Collections.Generic;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
if(args.Length!=2 || !uint.TryParse(args[0],out uint iterations) || !uint.TryParse(args[1],out uint samples))return 2;
// One owner per discarded benchmark process; preparation and teardown are untimed.
using var variables=new HostVariables();
using var sleep=new HostSleep();
var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
var directories=new HostDirectories(io);
try {
    Blink.BindHostIo(io);Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    return Blink.ThroughputMain(iterations,samples);
}
finally {
    try { Blink.ThroughputDestroy(); }
    finally {
    try { Blink.ThroughputRelease(); }
    finally {
    Blink.UnbindHostIdentity();Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();directories.Dispose();Blink.UnbindHostIo();
    Blink.UnbindHostVariables();Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    }
}
