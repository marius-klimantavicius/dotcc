using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

if (args.Length != 1) return 80;
byte[] image = File.ReadAllBytes(args[0]);
if (Convert.ToHexString(SHA256.HashData(image)).ToLowerInvariant() != "FIXTURE_SHA256") return 81;
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/bin/tls-fixture"] = image },
    executablePaths: new HashSet<string> { "/bin/tls-fixture" });
var directories = new HostDirectories(io);
try
{
    Blink.BindHostIo(io); Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables); Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment()); Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    int result = Blink.TlsLoadingRun();
    var output = io.CapturedOutput;
    if (output.StandardOutput.Length != 0 || output.StandardError.Length != 0) return 82;
    if (io.OpenDescriptors != 3) return 83;
    if (Blink.BlinkHostMemoryBytes() != 0 || Blink.BlinkHostMemoryMappings() != 0) return 84;
    return result;
}
catch (HostTerminationException error)
{
    Console.Error.WriteLine($"TLS fixture host termination: {error}");
    return 85;
}
finally
{
    Blink.BlinkHostExitCallbacksEnd(); Blink.BlinkHostSignalActionsEnd(); Blink.BlinkHostMemoryDisposeWorker();
    Blink.UnbindHostIdentity(); Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories(); directories.Dispose(); Blink.UnbindHostIo();
    Blink.UnbindHostVariables(); Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
