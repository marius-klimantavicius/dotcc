using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

if (args.Length == 1 && args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: ManagedConsumer GUEST_ELF WORKER_EXECUTABLE");
    Console.WriteLine("Runs the .NET NativeAOT service, checks HTTP, then restarts it and requests cooperative stop.");
    Console.WriteLine("WORKER_EXECUTABLE is the managed worker .dll or its published NativeAOT executable.");
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
    Executable = "/bin/dotnet-service",
    Image = [new ImageFile("/bin/dotnet-service", File.ReadAllBytes(args[0]), Executable: true)],
    Arguments = ["dotnet-service", "8080"],
    Environment = ["LANG=C", "DOTNET_GCHeapHardLimit=1000000",
        "DOTNET_GCRegionRange=2000000", "DOTNET_GCRegionSize=100000"],
    MemoryLimit = 64 * 1024 * 1024,
    DescriptorLimit = 128,
    OutputLimit = 16384,
    InstructionLimit = 20_000_000,
    WallClockMilliseconds = 30_000,
    PublishedPorts = [8080]
};
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
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
    RequireResponse(await Request(endpoint, "GET /health HTTP/1.1\r\nHost: fixture\r\n\r\n", deadline.Token), "ok\n");
    Console.WriteLine("Guest health: ok");
    InstanceResult result;
    if (cooperativeStop)
        result = await instance.StopAsync(TimeSpan.FromSeconds(10), deadline.Token);
    else
    {
        RequireResponse(await Request(endpoint,
            "POST /stop HTTP/1.1\r\nHost: fixture\r\nContent-Length: 0\r\n\r\n", deadline.Token), "stopped\n");
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

static async Task<byte[]> Request(IPEndPoint endpoint, string request, CancellationToken cancellation)
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
    return response.ToArray();
}
static void RequireResponse(byte[] response, string body)
{
    string expected = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {Encoding.ASCII.GetByteCount(body)}\r\nConnection: close\r\n\r\n{body}";
    if (!response.AsSpan().SequenceEqual(Encoding.ASCII.GetBytes(expected)))
        throw new InvalidOperationException("Service response differs from the pinned bytes.");
}
