using Managed.Emulation;

/// <summary>A mounted NativeAOT Kestrel guest with a browser-accessible listener.</summary>
internal static class KestrelServer
{
    public static async Task<int> RunAsync(string image, int hostPort, ExecutionMode mode)
    {
        string path = Path.GetFullPath(image);
        await using var machine = new BlinkMachine(new MachineOptions
        {
            ExecutionMode = mode,
            MemoryLimit = 128L << 20,
            InstructionLimit = null,
            ExecutionDeadline = null,
            Environment = new Dictionary<string, string>
            {
                ["LANG"] = "C",
                ["DOTNET_GCHeapHardLimit"] = "1000000",
                ["DOTNET_GCRegionRange"] = "2000000",
                ["DOTNET_GCRegionSize"] = "100000",
                ["DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE"] = "false",
                ["DOTNET_EnableDiagnostics"] = "0"
            },
            Network = new() { Publications = [new(8080, HostPort: hostPort)] }
        });
        machine.MountDirectory("/work", Path.GetDirectoryName(path)!, MountAccess.ReadOnly);
        Console.WriteLine($"Starting Kestrel in Blink ({mode}). Press Ctrl+C to stop.");
        await using var run = await machine.StartAsync(new ExecutionOptions
        {
            Executable = "/work/" + Path.GetFileName(path),
            Arguments = ["8080"],
            WorkingDirectory = "/work",
            Readiness = new() { Kind = ReadinessKind.ListeningPorts, Timeout = TimeSpan.FromSeconds(40) },
            Console = new()
            {
                Output = Console.OpenStandardOutput(),
                Error = Console.OpenStandardError(),
                CaptureBytes = 0,
                OutputLimit = long.MaxValue,
                InterruptPolicy = ConsoleInterruptPolicy.Stop
            }
        });
        await Task.WhenAny(run.Ready, run.Completion);
        if (run.Ready.IsCompletedSuccessfully)
        {
            var endpoint = (await run.Ready).Single();
            Console.WriteLine($"Open http://{endpoint.HostAddress}:{endpoint.HostPort}/health in your browser.");
            Console.WriteLine("The service stays running until Ctrl+C or POST /stop.");
        }
        else
        {
            // Observe readiness errors too, including an interrupt during startup.
            try { await run.Ready; }
            catch (Exception error) { Console.Error.WriteLine("Service did not become ready: " + error.Message); }
        }
        var result = await run.WaitAsync();
        Console.WriteLine($"Kestrel stopped: {result.Reason} (exit {result.ExitCode}).");
        if (result.Diagnostic != null) Console.Error.WriteLine(result.Diagnostic);
        if (!result.ResourcesReleased || result.Diagnostic != null) return 1;
        return result.Reason switch
        {
            RunExitReason.Stopped => 0,
            RunExitReason.Exited => result.ExitCode,
            _ => 1
        };
    }
}
