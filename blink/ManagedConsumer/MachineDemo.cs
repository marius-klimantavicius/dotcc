using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Managed.Emulation;

internal static class MachineDemo
{
    private static string? evidenceDirectory;
    private static int exchange;
    public static async Task<int> RunAsync(string image, ExecutionMode mode = ExecutionMode.InProcess)
    {
        string path = Path.GetFullPath(image);
        evidenceDirectory = Environment.GetEnvironmentVariable("BLINK_SAMPLE_EVIDENCE_DIRECTORY");
        if (evidenceDirectory != null) Directory.CreateDirectory(evidenceDirectory);
        exchange = 0;
        var options = new MachineOptions
        {
            ExecutionMode = mode, MemoryLimit = 128L << 20, InstructionLimit = 100_000_000,
            ExecutionDeadline = TimeSpan.FromSeconds(60),
            Environment = new Dictionary<string, string> {
                ["LANG"] = "C", ["DOTNET_GCHeapHardLimit"] = "1000000", ["DOTNET_GCRegionRange"] = "2000000",
                ["DOTNET_GCRegionSize"] = "100000", ["DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE"] = "false",
                ["DOTNET_EnableDiagnostics"] = "0" },
            Network = new() { Publications = [new(8080)] }
        };
        await using var first = new BlinkMachine(options);
        await using var second = new BlinkMachine(options);
        first.MountDirectory("/work", Path.GetDirectoryName(path)!);
        second.MountDirectory("/work", Path.GetDirectoryName(path)!);
        var execution = new ExecutionOptions { Executable = "/work/" + Path.GetFileName(path), Arguments = ["8080"],
            WorkingDirectory = "/work", Readiness = new() { Kind = ReadinessKind.ListeningPorts, Timeout = TimeSpan.FromSeconds(40) } };
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var token = timeout.Token;
        var observations = new List<MachineDemoResult>();
        await using (var a = await first.StartAsync(execution, token))
        await using (var b = await second.StartAsync(execution, token))
        {
            var endpoints = await Task.WhenAll(a.Ready, b.Ready).WaitAsync(token);
            if (a.Completion.IsCompleted || b.Completion.IsCompleted || endpoints[0][0].HostPort == endpoints[1][0].HostPort)
                throw new InvalidOperationException("Independent services did not overlap on distinct publications.");
            await Health(endpoints[0], token); await Health(endpoints[1], token);
            await StopHttp(endpoints[0], token); await StopHttp(endpoints[1], token);
            observations.Add(Validate(await a.WaitAsync(token), a.WorkerProcessId, false));
            observations.Add(Validate(await b.WaitAsync(token), b.WorkerProcessId, false));
        }
        await using (var restarted = await first.StartAsync(execution, token))
        {
            await Health(await restarted.Ready.WaitAsync(token), token);
            observations.Add(Validate(await restarted.StopAsync(TimeSpan.FromSeconds(10), token), restarted.WorkerProcessId, true));
        }
        if (mode == ExecutionMode.InProcess && observations.Any(x => x.ProcessId != null) ||
            mode == ExecutionMode.SeparateProcess && observations.Select(x => x.ProcessId).Distinct().Count() != 3)
            throw new InvalidOperationException("Execution mode or worker lifetime differs.");
        string? directory = Environment.GetEnvironmentVariable("BLINK_SAMPLE_EVIDENCE_DIRECTORY");
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, "machine-results.json"),
                JsonSerializer.Serialize(observations.ToArray(), MachineDemoJson.Default.MachineDemoResultArray), token);
        }
        Console.WriteLine($"Kestrel {mode}: two concurrent machines, mounted ELF, HTTP stop, restart and cooperative stop passed");
        return 0;
    }
    private static MachineDemoResult Validate(MachineRunResult result, int? pid, bool stopped)
    {
        if (result.Reason != (stopped ? RunExitReason.Stopped : RunExitReason.Exited) || result.ExitCode != 0 ||
            result.Signal != 0 || result.Halt != 0 || !result.ResourcesReleased || result.StandardError.Length != 0 ||
            result.CaptureTruncated || result.Instructions is <= 0 or > 100_000_000 || result.Diagnostic != null ||
            Encoding.UTF8.GetString(result.StandardOutput) != (stopped ? "READY 8080\n" : "READY 8080\nSTOPPED\n"))
            throw new InvalidOperationException("Kestrel result differs: " + result);
        return new(pid, result.Reason.ToString(), result.Instructions, result.ResourcesReleased,
            Encoding.UTF8.GetString(result.StandardOutput));
    }
    private static Task Health(IReadOnlyList<PublishedEndpoint> endpoints, CancellationToken token)
        => Request(endpoints, "GET", "/health", "ok\n", token);
    private static Task StopHttp(IReadOnlyList<PublishedEndpoint> endpoints, CancellationToken token)
        => Request(endpoints, "POST", "/stop", "stopped\n", token);
    private static async Task Request(IReadOnlyList<PublishedEndpoint> endpoints, string method, string path, string body, CancellationToken token)
    {
        if (endpoints.Count != 1 || endpoints[0].GuestPort != 8080) throw new InvalidOperationException("Unexpected publication.");
        var endpoint = endpoints[0];
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(IPAddress.Parse(endpoint.HostAddress), endpoint.HostPort, token);
        byte[] request = Encoding.ASCII.GetBytes($"{method} {path} HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
        int sent = 0; while (sent < request.Length) { int n = await socket.SendAsync(request.AsMemory(sent), SocketFlags.None, token); if (n == 0) throw new IOException("Short request"); sent += n; }
        using var response = new MemoryStream(); byte[] buffer = new byte[1024];
        for (;;) { int n = await socket.ReceiveAsync(buffer, SocketFlags.None, token); if (n == 0) break; response.Write(buffer, 0, n); if (response.Length > 8192) throw new IOException("Response bound"); }
        if (evidenceDirectory != null)
        {
            string name = $"{exchange++:D2}-{(path == "/health" ? "health" : "stop")}";
            await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, name + ".request"), request, token);
            await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, name + ".response"), response.ToArray(), token);
        }
        string text = Encoding.ASCII.GetString(response.ToArray()); int boundary = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (boundary < 0) throw new IOException("Missing HTTP header");
        string[] lines = text[..boundary].Split("\r\n");
        var headers = lines.Skip(1).Select(x => x.Split(':', 2)).ToDictionary(x => x[0], x => x[1].Trim(), StringComparer.OrdinalIgnoreCase);
        if (lines[0] != "HTTP/1.1 200 OK" || text[(boundary + 4)..] != body ||
            headers.GetValueOrDefault("Content-Length") != Encoding.UTF8.GetByteCount(body).ToString(CultureInfo.InvariantCulture) ||
            headers.GetValueOrDefault("Content-Type") != "text/plain" || headers.GetValueOrDefault("Server") != "Kestrel" ||
            headers.GetValueOrDefault("Connection") != "close" || !DateTimeOffset.TryParseExact(headers.GetValueOrDefault("Date"), "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
            throw new IOException("Kestrel HTTP contract differs: " + text);
    }
}
internal sealed record MachineDemoResult(int? ProcessId, string Reason, long Instructions, bool ResourcesReleased, string Output);
[JsonSerializable(typeof(MachineDemoResult[]))] internal partial class MachineDemoJson : JsonSerializerContext;
