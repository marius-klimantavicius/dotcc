using System.Net;
using System.Net.Sockets;
using System.Text;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

if (args.Length == 1 && args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: ManagedConsumer [SERVICE_ELF INSTANCE_FILE]");
    Console.WriteLine("Runs the pinned local service through the translated interpreter and a C# execution owner.");
    return 0;
}
if (args.Length != 0 && args.Length != 2)
{
    Console.Error.WriteLine("Usage: ManagedConsumer [SERVICE_ELF INSTANCE_FILE]");
    return 2;
}
string image = args.Length == 2 ? args[0] : "blink/build/guest/service";
string data = args.Length == 2 ? args[1] : "blink/tests/ServiceFixture/instance.txt";
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>
{
    ["/bin/service"] = File.ReadAllBytes(image),
    ["/data/fixture"] = File.ReadAllBytes(data),
}, executablePaths: new HashSet<string> { "/bin/service" });
var stop = new HostExecutionStop(TimeSpan.FromSeconds(60));
var execution = new GuestExecution(io, stop);
var completion = new TaskCompletionSource<GuestExecutionResult>(TaskCreationOptions.RunContinuationsAsynchronously);
// Every translated call and binding stays on this one thread. The application
// uses only the thread-safe IO/publication surface and the stop owner elsewhere.
var worker = new Thread(() =>
{
    try
    {
        completion.SetResult(execution.Run("/bin/service", ["service", "8080", "/data/fixture"],
            ["LANG=C"], 100_000_000));
    }
    catch (Exception error) { completion.SetException(error); }
}) { Name = "Blink interpreter" };
worker.Start();
try
{
    while (Encoding.UTF8.GetString(io.CapturedOutput.StandardOutput) != "READY 8080\n")
    {
        if (completion.Task.IsCompleted) throw new InvalidOperationException("Guest ended before readiness.", completion.Task.Exception);
        stop.Token.ThrowIfCancellationRequested();
        await Task.Delay(10, stop.Token);
    }
    IPEndPoint? published = null;
    for (int fd = 3; fd < 128; ++fd)
    {
        var local = io.LocalEndpoint(fd);
        if (!local.Succeeded || local.Value.Port != 8080) continue;
        var endpoint = io.Publish(fd);
        if (endpoint.Succeeded) { published = endpoint.Value; break; }
    }
    if (published == null || !IPAddress.IsLoopback(published.Address))
        throw new InvalidOperationException("Ready guest listener could not be published on loopback.");
    byte[] health = await Request(published, "GET /health HTTP/1.1\r\nHost: fixture\r\n\r\n", stop.Token);
    RequireResponse(health, "ok\n");
    Console.WriteLine("Guest health: ok");
    byte[] stopped = await Request(published,
        "POST /stop HTTP/1.1\r\nHost: fixture\r\nContent-Length: 0\r\n\r\n", stop.Token);
    RequireResponse(stopped, "stopped\n");
    GuestExecutionResult result = await completion.Task.WaitAsync(TimeSpan.FromSeconds(15));
    if (!result.Exited || result.ExitStatus != 0 || result.StopReason != HostExecutionStopReason.None ||
        result.Signal != 0 || !result.MemoryReleased || stop.NotificationFailure != null)
        throw new InvalidOperationException($"Guest did not exit normally: {result}");
    if (Encoding.UTF8.GetString(io.CapturedOutput.StandardOutput) != "READY 8080\nSTOPPED\n" ||
        io.CapturedOutput.StandardError.Length != 0)
        throw new InvalidOperationException("Guest output differs from the pinned service contract.");
    Console.WriteLine("Guest stopped: exit 0");
    return 0;
}
finally
{
    stop.RequestStop();
    // Never dispose guest storage while the execution thread can still use it.
    if (!worker.Join(TimeSpan.FromSeconds(15)))
        throw new TimeoutException("Interpreter did not return after stop; owner storage remains alive.");
    try { await io.DisposeAsync(); }
    finally { stop.Dispose(); }
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
