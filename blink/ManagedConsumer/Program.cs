using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

if (args.Length == 1 && args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: ManagedConsumer");
    Console.WriteLine("Runs the translated Blink normal instruction sample once, then exits.");
    Console.WriteLine("Shows arithmetic, bounded execution and normal guest exits; no HTTP service.");
    return 0;
}
if (args.Length != 0)
{
    Console.Error.WriteLine("Usage: ManagedConsumer [--help]");
    return 2;
}

// Host bindings are thread-local. Keep all translated calls and unbinding on
// this thread. One process owns one run; upstream static caches cannot be reused
// after the translated driver's teardown.
using var variables = new HostVariables();
using var sleep = new HostSleep();
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
var directories = new HostDirectories(io);
try
{
    Blink.BindHostIo(io);
    Blink.BindHostDirectories(directories);
    Blink.BindHostVariables(variables);
    Blink.BindHostSleep(sleep);
    Blink.BindHostEnvironment(new HostEnvironment());
    Blink.BindHostIdentity(new HostIdentity());

    // This translated owning driver initializes the real upstream interpreter,
    // executes its fixed normal cases, runs exit callbacks and frees its memory.
    // It verifies the instruction results and returns nonzero on a mismatch.
    int result = Blink.main();
    if (result != 0)
        Console.Error.WriteLine($"Translated Blink sample failed with status {result}.");
    return result;
}
finally
{
    Blink.UnbindHostIdentity();
    Blink.UnbindHostEnvironment();
    Blink.UnbindHostDirectories();
    directories.Dispose();
    Blink.UnbindHostIo();
    Blink.UnbindHostVariables();
    Blink.UnbindHostSleep();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
