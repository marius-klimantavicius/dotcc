using System;
using System.Collections.Generic;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
if(args.Length!=1 || !int.TryParse(args[0],out int index) || index<0)return 2;
// A separate process owns each case. Native witnesses run only in the Python
// orchestrator, never in this actual translated-core consumer.
using var variables=new HostVariables();
using var sleep=new HostSleep();
var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
var directories=new HostDirectories(io);
try {
    Blink.BindHostIo(io);Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    return Blink.CpuConformanceRun(index);
}
finally {
    Blink.UnbindHostIdentity();Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();directories.Dispose();Blink.UnbindHostIo();
    Blink.UnbindHostVariables();Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
