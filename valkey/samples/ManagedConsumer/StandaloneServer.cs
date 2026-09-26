using System.Runtime.InteropServices;
using System.Threading.Channels;
using Managed.Valkey;

internal static class StandaloneServer
{
    internal static async Task RunAsync(ValkeyOptions options)
    {
        // Signals only enqueue a request. Shutdown and persistence run on the
        // server's executor, and a failed SAVE leaves the endpoint available.
        var stops = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions { SingleReader = true });
        ConsoleCancelEventHandler cancel = (_, signal) =>
        {
            signal.Cancel = true;
            stops.Writer.TryWrite(true);
        };
        Console.CancelKeyPress += cancel;
        try
        {
            using var terminate = OperatingSystem.IsWindows() ? null
                : PosixSignalRegistration.Create(PosixSignal.SIGTERM, signal =>
                {
                    signal.Cancel = true;
                    stops.Writer.TryWrite(true);
                });
            await using var server = new ValkeyServer(options);
            var endpoint = await server.Endpoint;
            Console.WriteLine($"Valkey ready at redis://{endpoint.Address}:{endpoint.Port}");
            Console.WriteLine("Data directory: " + Path.GetFullPath(options.DataDirectory));
            Console.WriteLine(string.IsNullOrEmpty(options.Password)
                ? "Authentication: none." : "Authentication: password required (user: default).");
            Console.WriteLine("Press Ctrl+C to stop, or send SHUTDOWN from a client.");

            while (true)
            {
                var requested = stops.Reader.ReadAsync().AsTask();
                await Task.WhenAny(server.Completion, requested);
                if (server.Completion.IsCompleted)
                {
                    await server.Completion; // Surface terminal failures as an unsuccessful process exit.
                    return;
                }
                await requested;
                try
                {
                    await server.StopAsync(options.DisposeMode);
                    return;
                }
                catch (ValkeyStopException error) when (server.IsRunning)
                {
                    Console.Error.WriteLine("Shutdown failed: " + error.Message);
                    Console.Error.WriteLine("Server is still running. Fix the persistence error and press Ctrl+C to retry, "
                        + "or explicitly send SHUTDOWN NOSAVE to discard unsaved changes.");
                }
            }
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
            stops.Writer.TryComplete();
        }
    }
}
