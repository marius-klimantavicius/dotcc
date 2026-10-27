using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation;

if (args.Length is < 3 or > 7 || !Enum.TryParse<ExecutionMode>(args[0], out var mode))
    throw new ArgumentException("ServiceProbe InProcess|SeparateProcess closure.json report.json [seconds] [budget]");
int seconds = args.Length > 3 ? int.Parse(args[3]) : 180;
long budget = args.Length > 4 ? long.Parse(args[4]) : 150_000_000;
if (seconds is < 1 or > 7200 || budget is < 1 or > 3_000_000_000L) throw new ArgumentOutOfRangeException(nameof(args));
using var closure = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var root = closure.RootElement;
string executable = root.TryGetProperty("Executable", out var entry) ? entry.GetString()! : "/dotnet/dotnet";
var environment = root.GetProperty("Environment").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()!);
BlinkMachine? machine = null;
MachineRun? run = null;
MachineRunResult? result = null;
bool passed = false, runDisposed = false, machineDisposed = false, workerGone = false;
int? worker = null;
string? error = null, cleanupError = null;
var responses = new List<(string Path, int Status, string Body, double Ms)>();
var time = Stopwatch.StartNew(); double readyMs = 0;
try
{
    machine = new(new MachineOptions { ExecutionMode = mode, MemoryLimit = 256L << 20,
        VirtualMemoryLimit = 4L << 30, InstructionLimit = budget,
        ExecutionDeadline = TimeSpan.FromSeconds(seconds), Environment = environment,
        GuestJit = new() { Enabled = args.Length > 5 && args[5] == "guest-jit", AllowUnverifiedWindows = true,
            HotBlockThreshold = args.Length > 6 ? int.Parse(args[6]) : 16 },
        Network = new NetworkPolicy { Publications = [new(8080)] } });
    machine.ImportImage(root.GetProperty("Files").EnumerateArray().Select(file =>
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(root.GetProperty("Images").GetString()!, file.GetProperty("File").GetString()!));
        if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.GetProperty("Sha256").GetString()) throw new IOException("Image changed");
        return new ImageFile(file.GetProperty("GuestPath").GetString()!, bytes, file.GetProperty("Executable").GetBoolean());
    }));
    run = await machine.StartAsync(new ExecutionOptions { Executable = executable,
        Arguments = executable == "/dotnet/dotnet" ? ["/apps/service/CoreClrService.dll", "8080"] : ["8080"],
        WorkingDirectory = "/work", AllowElfInterpreter = true,
        Readiness = new ReadinessOptions { Kind = ReadinessKind.ListeningPorts, Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 65536 } });
    worker = run.WorkerProcessId;
    var endpoint = (await run.Ready).Single(p => p.GuestPort == 8080);
    readyMs = time.Elapsed.TotalMilliseconds;
    foreach (var request in new[] { ("GET", "/health", "", "CORECLR SERVICE OK\n"),
        ("GET", "/health", "", "CORECLR SERVICE OK\n"), ("GET", "/health", "", "CORECLR SERVICE OK\n"),
        ("POST", "/echo", "payload", "CORECLR:payload\n"), ("POST", "/echo", "payload", "CORECLR:payload\n"),
        ("POST", "/stop", "", "STOP OK\n") })
    {
        var timer = Stopwatch.StartNew();
        using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(seconds) };
        using var message = new HttpRequestMessage(new HttpMethod(request.Item1), $"http://{endpoint.HostAddress}:{endpoint.HostPort}{request.Item2}")
            { Content = new StringContent(request.Item3) };
        using var response = await client.SendAsync(message);
        string body = await response.Content.ReadAsStringAsync();
        responses.Add((request.Item2, (int)response.StatusCode, body, timer.Elapsed.TotalMilliseconds));
        if (response.StatusCode != HttpStatusCode.OK || body != request.Item4) throw new IOException("HTTP response differs");
    }
    result = await run.WaitAsync();
    passed = result.Reason == RunExitReason.Exited && result.ExitCode == 0 && result.ResourcesReleased &&
        !result.CaptureTruncated && result.StandardError.Length == 0 &&
        Encoding.UTF8.GetString(result.StandardOutput) == "SERVICE READY\nSERVICE STOPPED\n";
}
catch (Exception failure) { error = failure.ToString(); }
finally
{
    if (run != null)
        try
        {
            if (!run.Completion.IsCompleted) await run.StopAsync();
            result ??= await run.WaitAsync();
            await run.DisposeAsync(); runDisposed = true;
        }
        catch (Exception failure) { cleanupError = failure.ToString(); }
    if (machine != null)
        try { await machine.DisposeAsync(); machineDisposed = true; }
        catch (Exception failure) { cleanupError = (cleanupError ?? "") + failure; }
    if (worker is int pid)
        try { using var process = Process.GetProcessById(pid); workerGone = process.HasExited; }
        catch (ArgumentException) { workerGone = true; }
    else workerGone = true;
    passed &= error == null && cleanupError == null && runDisposed && machineDisposed && workerGone &&
        (mode == ExecutionMode.SeparateProcess ? worker is > 0 : worker == null);
    using var file = File.Create(args[2]); using var json = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });
    json.WriteStartObject(); json.WriteBoolean("passed", passed);
    json.WriteString("mode", mode.ToString()); json.WriteBoolean("host_dynamic_code", RuntimeFeature.IsDynamicCodeCompiled);
    json.WriteNumber("ready_ms", readyMs); json.WriteNumber("total_ms", time.Elapsed.TotalMilliseconds);
    json.WriteNumber("instructions", result?.Instructions ?? 0); json.WriteBoolean("resources_released", result?.ResourcesReleased == true);
    json.WriteStartObject("jit");
    json.WriteString("capability", result?.JitStatistics?.Capability);
    json.WriteNumber("compiled_blocks", result?.JitStatistics?.CompiledBlocks ?? 0);
    json.WriteNumber("native_entries", result?.JitStatistics?.NativeEntries ?? 0);
    json.WriteNumber("first_child_instructions", result?.JitStatistics?.FirstChildInstructions ?? -1);
    json.WriteNumber("native_at_first_child", result?.JitStatistics?.NativeInstructionsAtFirstChild ?? 0);
    json.WriteNumber("native_after_first_child", result?.JitStatistics is { FirstChildInstructions: >= 0 } stats
        ? stats.GuestInstructions - stats.NativeInstructionsAtFirstChild : 0);
    json.WriteNumber("guest_instructions", result?.JitStatistics?.GuestInstructions ?? 0);
    json.WriteNumber("interpreted_instructions", result?.JitStatistics?.InterpreterInstructions ?? 0);
    json.WriteNumber("peak_cache_bytes", result?.JitStatistics?.PeakCacheBytes ?? 0);
    json.WriteNumber("cache_bytes_after_release", result?.JitStatistics?.CacheBytes ?? 0);
    json.WriteEndObject();
    json.WriteString("reason", result?.Reason.ToString()); json.WriteString("stdout", result == null ? null : Encoding.UTF8.GetString(result.StandardOutput));
    json.WriteString("stderr", result == null ? null : Encoding.UTF8.GetString(result.StandardError));
    if (worker is int id) json.WriteNumber("worker", id); else json.WriteNull("worker");
    json.WriteBoolean("worker_gone", workerGone); json.WriteBoolean("run_disposed", runDisposed); json.WriteBoolean("machine_disposed", machineDisposed);
    json.WriteString("error", error); json.WriteString("cleanup_error", cleanupError);
    json.WriteStartArray("responses");
    foreach (var response in responses)
    {
        json.WriteStartObject(); json.WriteString("path", response.Path); json.WriteNumber("status", response.Status);
        json.WriteString("body", response.Body); json.WriteNumber("ms", response.Ms); json.WriteEndObject();
    }
    json.WriteEndArray(); json.WriteEndObject();
}
return passed ? 0 : 1;
