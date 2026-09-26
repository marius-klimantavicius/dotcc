using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

if (args.Length > 0 && args[0] == "--serve")
{
    int port = 8080;
    var mode = ExecutionMode.InProcess;
    if (args.Length is < 2 or > 4 ||
        args.Length >= 3 && (!int.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out port) || port is < 0 or > 65535) ||
        args.Length == 4 && (!Enum.TryParse(args[3], out mode) || !Enum.IsDefined(mode)))
    {
        Console.Error.WriteLine("Usage: ManagedConsumer --serve GUEST_ELF [HOST_PORT] [InProcess|SeparateProcess]");
        return 2;
    }
    return await KestrelServer.RunAsync(args[1], port, mode);
}

if (args.Length >= 3 && args[0] is "--run" or "--run-process")
{
    await using var machine = new BlinkMachine(new MachineOptions
    {
        ExecutionMode = args[0] == "--run-process" ? ExecutionMode.SeparateProcess : ExecutionMode.InProcess,
        Environment = new Dictionary<string, string> { ["LANG"] = "C" }
    });
    machine.MountDirectory("/work", args[1]);
    var result = await machine.ExecuteAsync(new ExecutionOptions
    {
        Executable = args[2], WorkingDirectory = "/work", Arguments = args[3..],
        Console = ConsoleOptions.AttachCurrent()
    });
    if (result.Diagnostic != null) Console.Error.WriteLine(result.Diagnostic);
    return result.Reason == RunExitReason.Exited ? result.ExitCode : 1;
}

if (args.Length == 1 && args[0] is not ("--help" or "-h"))
    return await MachineDemo.RunAsync(args[0]);
if (args.Length == 2 && Enum.TryParse<ExecutionMode>(args[1], out var machineMode))
    return await MachineDemo.RunAsync(args[0], machineMode);

if (args.Length == 1 && args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: ManagedConsumer GUEST_ELF [InProcess|SeparateProcess]");
    Console.WriteLine("       ManagedConsumer --serve GUEST_ELF [HOST_PORT] [InProcess|SeparateProcess]");
    Console.WriteLine("       --serve keeps Kestrel running for browser access until Ctrl+C (default host port: 8080).");
    Console.WriteLine("       ManagedConsumer --run HOST_DIRECTORY /work/PROGRAM [ARGUMENT ...]");
    Console.WriteLine("       ManagedConsumer --run-process HOST_DIRECTORY /work/PROGRAM [ARGUMENT ...]");
    Console.WriteLine("Runs the ASP.NET Core Kestrel NativeAOT service, checks HTTP, then restarts it and requests cooperative stop.");
    Console.WriteLine("Mounts the ELF directory at /work. Default is in-process; separate-process workers deploy automatically.");
    return 0;
}
if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: ManagedConsumer GUEST_ELF WORKER_EXECUTABLE");
    return 2;
}
string workerPath = Path.GetFullPath(args[1]);
var launch = workerPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
    ? new WorkerLaunch("dotnet", [workerPath]) : new WorkerLaunch(workerPath, []);
var options = new InstanceOptions
{
    Executable = "/bin/kestrel-service",
    Image = [new ImageFile("/bin/kestrel-service", File.ReadAllBytes(args[0]), Executable: true)],
    Arguments = ["kestrel-service", "8080"],
    Environment = ["LANG=C", "DOTNET_GCHeapHardLimit=1000000",
        "DOTNET_GCRegionRange=2000000", "DOTNET_GCRegionSize=100000",
        "DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false", "DOTNET_EnableDiagnostics=0"],
    MemoryLimit = 128 * 1024 * 1024,
    DescriptorLimit = 128,
    OutputLimit = 16384,
    InstructionLimit = 100_000_000,
    WallClockMilliseconds = 60_000,
    PublishedPorts = [8080]
};
string? evidenceDirectory = Environment.GetEnvironmentVariable("BLINK_SAMPLE_EVIDENCE_DIRECTORY");
if (evidenceDirectory != null) Directory.CreateDirectory(evidenceDirectory);
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(3));
int first = await RunOnce(false);
int restarted = await RunOnce(true);
if (first == restarted) throw new InvalidOperationException("Restart did not create a fresh worker.");
Console.WriteLine("Restart and cooperative stop: passed");
return 0;

async Task<int> RunOnce(bool cooperativeStop)
{
    await using var instance = await BlinkInstance.StartAsync(launch, options, deadline.Token);
    PublishedEndpoint[] endpoints = await instance.Ready.WaitAsync(deadline.Token);
    if (endpoints.Length != 1 || endpoints[0].GuestPort != 8080)
        throw new InvalidOperationException("Unexpected guest publication.");
    var endpoint = new IPEndPoint(IPAddress.Loopback, endpoints[0].HostPort);
    RequireResponse(await Request(endpoint, "GET /health HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n",
        cooperativeStop ? "restart-health" : "health", deadline.Token), "ok\n");
    Console.WriteLine("Guest health: ok");
    InstanceResult result;
    if (cooperativeStop)
        result = await instance.StopAsync(TimeSpan.FromSeconds(10), deadline.Token);
    else
    {
        RequireResponse(await Request(endpoint,
            "POST /stop HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n", "stop", deadline.Token), "stopped\n");
        result = await instance.Completion.WaitAsync(deadline.Token);
    }
    RequireCleanup(result, cooperativeStop);
    if (result.Reason != (cooperativeStop ? "stopped" : "exited") ||
        (!cooperativeStop && result.ExitStatus != 0) || result.Signal != 0 || result.Halt != 0 ||
        result.StandardError.Length != 0 || result.Instructions <= 0 || result.Instructions > options.InstructionLimit)
        throw new InvalidOperationException("Guest did not finish with the expected result: " + result.Reason);
    string expectedOutput = cooperativeStop ? "READY 8080\n" : "READY 8080\nSTOPPED\n";
    if (Encoding.UTF8.GetString(result.StandardOutput) != expectedOutput)
        throw new InvalidOperationException("Guest output differs from the service contract.");
    if (evidenceDirectory != null)
    {
        string prefix = cooperativeStop ? "cooperative" : "normal";
        await File.WriteAllTextAsync(Path.Combine(evidenceDirectory, prefix + "-cleanup.json"), result.Detail!, deadline.Token);
        await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, prefix + ".stdout"), result.StandardOutput, deadline.Token);
        await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, prefix + ".stderr"), result.StandardError, deadline.Token);
        using var evidence = File.Create(Path.Combine(evidenceDirectory, prefix + "-result.json"));
        using var writer = new Utf8JsonWriter(evidence);
        writer.WriteStartObject();
        writer.WriteNumber("workerProcessId", instance.WorkerProcessId);
        writer.WriteNumber("workerExitCode", result.WorkerExitCode);
        writer.WriteString("reason", result.Reason);
        writer.WriteNumber("exitStatus", result.ExitStatus);
        writer.WriteNumber("signal", result.Signal);
        writer.WriteNumber("halt", result.Halt);
        writer.WriteNumber("instructions", result.Instructions);
        writer.WriteEndObject();
    }
    if (!cooperativeStop) Console.WriteLine("Guest stopped: exit 0");
    return instance.WorkerProcessId;
}

static void RequireCleanup(InstanceResult result, bool cooperativeStop)
{
    if (result.WorkerExitCode != 0 || result.WorkerDiagnostics.Length != 0 || result.Detail == null)
        throw new InvalidOperationException("Worker did not report clean completion.");
    using var document = JsonDocument.Parse(result.Detail);
    var detail = document.RootElement;
    if (detail.GetProperty("stopReason").GetString() != (cooperativeStop ? "Requested" : "None"))
        throw new InvalidOperationException("Guest stop outcome differs from the requested lifecycle.");
    foreach (string key in new[] { "joined", "quiescent", "ioDisposed", "memoryReleased" })
        if (!detail.GetProperty(key).GetBoolean())
            throw new InvalidOperationException("Worker cleanup incomplete: " + key);
    if (detail.GetProperty("notificationFailure").GetBoolean() ||
        detail.GetProperty("error").ValueKind != JsonValueKind.Null)
        throw new InvalidOperationException("Worker reported a cleanup or execution error.");
}

async Task<byte[]> Request(IPEndPoint endpoint, string request, string label, CancellationToken cancellation)
{
    using var client = new TcpClient(AddressFamily.InterNetwork);
    await client.ConnectAsync(endpoint.Address, endpoint.Port, cancellation);
    using NetworkStream stream = client.GetStream();
    await stream.WriteAsync(Encoding.ASCII.GetBytes(request), cancellation);
    using var response = new MemoryStream();
    byte[] buffer = new byte[1024];
    while (true)
    {
        int count = await stream.ReadAsync(buffer, cancellation);
        if (count == 0) break;
        if (response.Length + count > 4096) throw new InvalidOperationException("Response exceeds the sample bound.");
        response.Write(buffer, 0, count);
    }
    byte[] actual = response.ToArray();
    if (evidenceDirectory != null)
    {
        await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, label + ".request"), Encoding.ASCII.GetBytes(request), cancellation);
        await File.WriteAllBytesAsync(Path.Combine(evidenceDirectory, label + ".response"), actual, cancellation);
    }
    return actual;
}
static void RequireResponse(byte[] response, string body)
{
    int boundary = response.AsSpan().IndexOf("\r\n\r\n"u8);
    if (boundary < 0) throw new InvalidOperationException("Incomplete HTTP response.");
    string[] lines = Encoding.ASCII.GetString(response, 0, boundary).Split("\r\n");
    if (lines[0] != "HTTP/1.1 200 OK") throw new InvalidOperationException("Unexpected HTTP status.");
    var headers = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (string line in lines.Skip(1))
    {
        int colon = line.IndexOf(':');
        if (colon <= 0 || !headers.TryAdd(line[..colon].ToLowerInvariant(), line[(colon + 1)..].Trim()))
            throw new InvalidOperationException("Invalid or duplicate HTTP header.");
    }
    if (!headers.Remove("date", out string? date) ||
        !DateTimeOffset.TryParseExact(date, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
        throw new InvalidOperationException("Invalid RFC1123 Date.");
    var expected = new Dictionary<string, string>
    {
        ["content-type"] = "text/plain",
        ["content-length"] = Encoding.ASCII.GetByteCount(body).ToString(CultureInfo.InvariantCulture),
        ["connection"] = "close",
        ["server"] = "Kestrel"
    };
    if (headers.Count != expected.Count || expected.Any(pair => !headers.TryGetValue(pair.Key, out string? value) || value != pair.Value) ||
        !response.AsSpan(boundary + 4).SequenceEqual(Encoding.ASCII.GetBytes(body)))
        throw new InvalidOperationException("Service response differs from the pinned Kestrel contract.");
}
