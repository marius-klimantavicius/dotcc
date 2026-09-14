using System;
using System.Collections.Generic;
using Managed.Emulation;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

// One process is one discarded core worker. No translated call follows main.
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
try
{
    Blink.BindHostIo(io);
    Blink.BindHostVariables(variables);
    Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());
    Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    return Blink.main();
}
finally
{
    Blink.UnbindHostIdentity();
    Blink.UnbindHostEnvironment();
    Blink.UnbindHostIo();
    Blink.UnbindHostVariables();
    Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
