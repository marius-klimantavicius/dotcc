using System;
using System.Collections.Generic;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
if(args.Length != 0) return 2;
// One owner per discarded guest-environment process; all upstream state is discarded after both cycles.
using var variables=new HostVariables();
using var sleep=new HostSleep();
var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>());
var directories=new HostDirectories(io);
try {
    Blink.BindHostIo(io);Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment(TimeProvider.System, new BclHostEntropy()));Blink.BindHostIdentity(new HostIdentity(73));
    Console.WriteLine("provider timestamp_frequency=" + TimeProvider.System.TimestampFrequency.ToString(System.Globalization.CultureInfo.InvariantCulture));
    GC.Collect(2,GCCollectionMode.Forced,true,true);
    int result = Blink.GuestEnvironmentRun();
    if (result != 0) return result;
    if (io.OpenDescriptors != 3) throw new InvalidOperationException("Guest descriptor cleanup leaked private handles");
    return 0;
}
finally {
    try { Blink.GuestEnvironmentDestroy(); }
    finally {
    try { if (Blink.GuestEnvironmentRelease() != 0) throw new InvalidOperationException("Guest-memory owner cleanup failed"); }
    finally {
    Blink.UnbindHostIdentity();Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();directories.Dispose();Blink.UnbindHostIo();
    Blink.UnbindHostVariables();Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    }
}
