using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
if (args.Length != 4) return 2;
// These artifact paths belong only to the harness, never the guest namespace.
byte[] input = File.ReadAllBytes(args[0]);
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), input);
var directories = new HostDirectories(io);
try
{
    Blink.BindHostIo(io); Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables); Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment()); Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    int result = Blink.GuestStreamsRun();
    var captured = io.CapturedOutput;
    File.WriteAllBytes(args[1], captured.StandardOutput);
    File.WriteAllBytes(args[2], captured.StandardError);
    // Report storage belongs to the translated library. Copy it now; retain no
    // unmanaged pointer through final upstream/host teardown.
    unsafe
    {
        string report = Marshal.PtrToStringUTF8((nint)Blink.GuestStreamsReport(result))
            ?? throw new InvalidOperationException("Missing bounded guest-stream report");
        File.WriteAllText(args[3], report, new UTF8Encoding(false));
    }
    if (result != 0) return result;
    if (io.OpenDescriptors != 3) throw new InvalidOperationException("Standard descriptor lifetime differs");
    return 0;
}
finally
{
    try { Blink.GuestStreamsDestroy(); }
    finally
    {
        try
        {
            if (Blink.GuestStreamsRelease() != 0) throw new InvalidOperationException("Guest-stream owner cleanup failed");
        }
        finally
        {
            Blink.UnbindHostIdentity(); Blink.UnbindHostEnvironment();
            Blink.UnbindHostDirectories(); directories.Dispose(); Blink.UnbindHostIo();
            Blink.UnbindHostVariables(); Blink.UnbindHostSleep();
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
