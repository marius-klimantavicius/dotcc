using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

if (args.Length != 1) return 80;
byte[] service = File.ReadAllBytes(args[0]);
if (Convert.ToHexString(SHA256.HashData(service)).ToLowerInvariant() != "916eeadfde4092c55bd2d05d5cacbe20482f4cabda8c0436fcfb68d690f2a9b8") return 81;
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/bin/service"] = service },
    executablePaths: new HashSet<string> { "/bin/service" });
var directories = new HostDirectories(io);
try
{
    Blink.BindHostIo(io);
    Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);
    Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());
    Blink.BindHostIdentity(new HostIdentity());
    GC.Collect(2, GCCollectionMode.Forced, true, true);
    int result = Blink.ElfLoadingRun();
    if (io.OpenDescriptors != 3) return 82;
    if (Blink.BlinkHostMemoryBytes() != 0 || Blink.BlinkHostMemoryMappings() != 0) return 83;
    return result;
}
catch (HostTerminationException error)
{
    Console.Error.WriteLine($"loader host termination: {error}");
    var captured = io.CapturedOutput;
    Console.Error.Write(System.Text.Encoding.UTF8.GetString(captured.StandardError));
    return 84;
}
finally
{
    // Normal C path has already freed machines and run exit callbacks. An
    // exceptional unwind skips callbacks and discards this entire process;
    // upstream static cache references must never be reused after disposal.
    Blink.BlinkHostExitCallbacksEnd();
    Blink.BlinkHostSignalActionsEnd();
    Blink.BlinkHostMemoryDisposeWorker();
    Blink.UnbindHostIdentity();
    Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();
    directories.Dispose();
    Blink.UnbindHostIo();
    Blink.UnbindHostVariables();
    Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
