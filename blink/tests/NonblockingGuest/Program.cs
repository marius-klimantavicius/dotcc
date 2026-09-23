using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

if (args is ["--machine-worker"])
    return await MachineWorkerHost.RunAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());
if (args.Length != 3 || args[1] is not ("native" or "InProcess" or "SeparateProcess")) return 80;
string elf = Path.GetFullPath(args[0]), mode = args[1], output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(output);
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(4));
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
int port = ((IPEndPoint)listener.LocalEndpoint).Port;
var requests = new ConcurrentBag<string>();
var handlers = new ConcurrentBag<Task>();
var results = new ConcurrentDictionary<string, MachineRunResult>();
var machines = new ConcurrentDictionary<string, BlinkMachine>();
var workers = new ConcurrentDictionary<string, (int? Pid, string? Executable)>();
var initialRequests = new ConcurrentDictionary<string, byte>();
var bothInitialRequests = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
using var serverStop = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
Task server = Serve();
bool passed = false;
string? failure = null;
try
{
    // Independent simultaneous machines, then another execution in the first machine.
    await Task.WhenAll(Run("first"), Run("second"));
    await Run("restart");
    string[] expected = new[] { "first", "second", "restart" }
        .SelectMany(name => Enumerable.Range(0, 6).Select(index => "/" + name + "/" + index))
        .Order(StringComparer.Ordinal).ToArray();
    Check(requests.Order(StringComparer.Ordinal).SequenceEqual(expected), "exactly eighteen ordinary HTTP connections");
    passed = true;
}
catch (Exception error) { failure = error.ToString(); }
finally
{
    foreach (var machine in machines.Values)
    {
        try { await machine.DisposeAsync(); }
        catch (Exception error) { passed = false; failure ??= error.ToString(); }
    }
    serverStop.Cancel(); listener.Stop();
    try { await server; await Task.WhenAll(handlers); }
    catch (OperationCanceledException) when (serverStop.IsCancellationRequested) { }
    catch (Exception error) { passed = false; failure ??= error.ToString(); }
    using var file = File.Create(Path.Combine(output, "result.json"));
    using var writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject();
    writer.WriteBoolean("passed", passed); writer.WriteString("mode", mode);
    writer.WriteBoolean("consumer_dynamic_code_supported", System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported);
    writer.WriteBoolean("simultaneous_initial_requests", bothInitialRequests.Task.IsCompletedSuccessfully);
    writer.WriteString("guest_sha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(elf))).ToLowerInvariant());
    writer.WriteString("error", failure); writer.WriteNumber("host_port", port);
    writer.WriteStartArray("requests"); foreach (string path in requests.Order()) writer.WriteStringValue(path); writer.WriteEndArray();
    writer.WriteStartArray("runs");
    foreach (var (name, result) in results.OrderBy(pair => pair.Key))
    {
        writer.WriteStartObject(); writer.WriteString("name", name); writer.WriteString("reason", result.Reason.ToString());
        if (workers.TryGetValue(name, out var worker))
        {
            if (worker.Pid is int pid) writer.WriteNumber("worker_pid", pid); else writer.WriteNull("worker_pid");
            writer.WriteString("worker_executable", worker.Executable);
            if (worker.Executable != null)
                writer.WriteString("worker_executable_sha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(worker.Executable))).ToLowerInvariant());
        }
        writer.WriteNumber("exit_code", result.ExitCode); writer.WriteNumber("signal", result.Signal); writer.WriteNumber("halt", result.Halt);
        writer.WriteNumber("instructions", result.Instructions); writer.WriteBoolean("resources_released", result.ResourcesReleased);
        writer.WriteBoolean("capture_truncated", result.CaptureTruncated); writer.WriteString("diagnostic", result.Diagnostic);
        writer.WriteString("stdout", Encoding.UTF8.GetString(result.StandardOutput));
        writer.WriteString("stderr", Encoding.UTF8.GetString(result.StandardError)); writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteEndObject();
}
if (!passed) { Console.Error.WriteLine(failure); return 1; }
string lifecycle = mode == "native" ? "two simultaneous native processes and repeat" : "two simultaneous machines and same-machine restart";
Console.WriteLine($"PASS {mode}: NativeAOT HttpClient, {lifecycle}, eighteen fresh TCP connections, clean shutdown");
return 0;

async Task Run(string name)
{
    string url = "http://127.0.0.1:" + port;
    MachineRunResult result;
    var environment = new Dictionary<string, string>
    {
        ["LANG"] = "C", ["DOTNET_GCHeapHardLimit"] = "1000000",
        ["DOTNET_GCRegionRange"] = "2000000", ["DOTNET_GCRegionSize"] = "100000",
        ["DOTNET_EnableDiagnostics"] = "0"
    };
    if (mode == "native")
    {
        var start = new ProcessStartInfo(elf) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(url); start.ArgumentList.Add(name);
        foreach (var pair in environment) start.Environment[pair.Key] = pair.Value;
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Native guest did not start");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(), stderr = process.StandardError.ReadToEndAsync();
        try { await process.WaitForExitAsync(deadline.Token); }
        finally
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
        }
        result = new(RunExitReason.Exited, process.ExitCode, 0, 0, 0,
            Encoding.UTF8.GetBytes(await stdout), Encoding.UTF8.GetBytes(await stderr), false, 0, true, null);
    }
    else
    {
        var machine = machines.GetOrAdd(name == "restart" ? "first" : name, _ =>
        {
            var owner = new BlinkMachine(new MachineOptions
            {
                ExecutionMode = Enum.Parse<ExecutionMode>(mode), MemoryLimit = 128L << 20,
                InstructionLimit = 200_000_000, ExecutionDeadline = TimeSpan.FromMinutes(2),
                Environment = environment,
                Network = new() { OutboundDestinations = [new("127.0.0.1", checked((ushort)port))] }
            });
            try { owner.MountDirectory("/work", Path.GetDirectoryName(elf)!, MountAccess.ReadOnly); }
            catch { owner.DisposeAsync().AsTask().GetAwaiter().GetResult(); throw; }
            return owner;
        });
        await using var run = await machine.StartAsync(new ExecutionOptions
        {
            Executable = "/work/" + Path.GetFileName(elf), WorkingDirectory = "/work", Arguments = [url, name]
        }, deadline.Token);
        Check(run.ExecutionMode == Enum.Parse<ExecutionMode>(mode), name + " requested execution mode");
        if (run.WorkerProcessId is int pid)
        {
            using var worker = Process.GetProcessById(pid);
            workers[name] = (pid, worker.MainModule?.FileName ?? throw new InvalidOperationException("Worker executable unavailable"));
            Check(mode == "SeparateProcess" && pid != Environment.ProcessId, name + " actual separate worker");
        }
        else
        {
            workers[name] = (null, null);
            Check(mode == "InProcess", name + " in-process execution has no worker");
        }
        result = await run.WaitAsync(deadline.Token);
    }
    results[name] = result;
    await File.WriteAllBytesAsync(Path.Combine(output, name + ".stdout"), result.StandardOutput);
    await File.WriteAllBytesAsync(Path.Combine(output, name + ".stderr"), result.StandardError);
    Check(result.Reason == RunExitReason.Exited && result.ExitCode == 0 && result.Halt == 0 && result.Signal == 0,
        name + " normal process exit: " + result.Reason + " " + result.Diagnostic + " " + Encoding.UTF8.GetString(result.StandardError));
    Check(result.ResourcesReleased && !result.CaptureTruncated && result.Diagnostic == null, name + " complete cleanup");
    Check(Encoding.UTF8.GetString(result.StandardOutput) == "HTTP PASS " + name + " 6\n" && result.StandardError.Length == 0,
        name + " exact guest stdout/stderr");
    if (mode != "native") Check(result.Instructions > 0, name + " executed translated instructions");
}

async Task Serve()
{
    try
    {
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync(serverStop.Token);
            handlers.Add(Reply(client));
        }
    }
    catch (OperationCanceledException) when (serverStop.IsCancellationRequested) { }
    catch (SocketException) when (serverStop.IsCancellationRequested) { }
}

async Task Reply(TcpClient client)
{
    using (client)
    {
        using var stream = client.GetStream();
        byte[] buffer = new byte[8192];
        int count = 0;
        while (buffer.AsSpan(0, count).IndexOf("\r\n\r\n"u8) < 0)
        {
            Check(count < buffer.Length, "bounded HTTP request");
            int read = await stream.ReadAsync(buffer.AsMemory(count), serverStop.Token);
            Check(read != 0, "complete HTTP request headers"); count += read;
        }
        string[] request = Encoding.ASCII.GetString(buffer, 0, count).Split("\r\n")[0].Split(' ');
        Check(request.Length == 3 && request[0] == "GET" && request[2] == "HTTP/1.1" && request[1].StartsWith('/'), "ordinary HTTP/1.1 GET");
        requests.Add(request[1]);
        if (request[1] is "/first/0" or "/second/0")
        {
            initialRequests.TryAdd(request[1], 0);
            if (initialRequests.Count == 2) bothInitialRequests.TrySetResult();
            // An ordinary service-side rendezvous demonstrates both guests are
            // live together without assuming any minimum execution duration.
            await bothInitialRequests.Task.WaitAsync(serverStop.Token);
        }
        byte[] body = Encoding.UTF8.GetBytes("reply:" + request[1][1..] + "\n");
        byte[] header = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(header, serverStop.Token); await stream.WriteAsync(body, serverStop.Token);
    }
}

static void Check(bool value, string message)
{
    if (!value) throw new InvalidOperationException(message);
}
