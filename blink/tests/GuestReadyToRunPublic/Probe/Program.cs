using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length != 8 || !Enum.TryParse<ExecutionMode>(args[0], out var mode) || !Enum.IsDefined(mode))
    throw new ArgumentException("Probe InProcess|SeparateProcess closure.json instructions seconds memoryMiB virtualMiB report.json events.jsonl");
long budget = long.Parse(args[2]);
int seconds = int.Parse(args[3]), memory = int.Parse(args[4]), virtualMemory = int.Parse(args[5]);
if (budget is < 1 or > 3_000_000_000 || seconds is < 1 or > 3600 || memory != 256 || virtualMemory != 4096)
    throw new ArgumentException("Finite bounds: instructions <= 3000000000, seconds <= 3600, backing 256 MiB, virtual 4096 MiB");
string output = Path.GetFullPath(args[6]);
var report = new Report { Mode = mode.ToString(), MemoryMiB = memory, VirtualMemoryMiB = virtualMemory,
    InstructionLimit = budget, ExecutionSeconds = seconds, EventsPath = Path.GetFullPath(args[7]) };
var total = Stopwatch.StartNew();
var execution = new Stopwatch();
using var events = new HostEvents(report.EventsPath, total, execution);
BlinkMachine? machine = null;
MachineRun? run = null;
Task? exitObserver = null;
CancellationTokenSource? deadline = null;
try
{
    var closure = JsonSerializer.Deserialize(File.ReadAllText(args[1]), EvidenceJson.Default.Closure)
        ?? throw new IOException("Missing closure");
    // Preserve the retained closure environment exactly, including default W^X,
    // tiering and ReadyToRun selection. There are no guest environment overrides.
    machine = new(new MachineOptions { ExecutionMode = mode, MemoryLimit = memory * 1024L * 1024,
        VirtualMemoryLimit = virtualMemory * 1024L * 1024, InstructionLimit = budget,
        ExecutionDeadline = TimeSpan.FromSeconds(seconds), Environment = closure.Environment,
        Network = new NetworkPolicy { Publications = [new(8080)] } });
    var import = Stopwatch.StartNew();
    machine.ImportImage(closure.Files.Select(file =>
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(closure.Images, file.File));
        if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.Sha256)
            throw new IOException("Closure changed: " + file.File);
        return new ImageFile(file.GuestPath, bytes, file.Executable);
    }));
    report.ImportSeconds = import.Elapsed.TotalSeconds;
    events.Emit("image_imported", phaseSeconds: report.ImportSeconds);
    deadline = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
    execution.Start();
    events.Emit("start_requested");
    var start = Stopwatch.StartNew();
    run = await machine.StartAsync(new ExecutionOptions { Executable = "/dotnet/dotnet",
        Arguments = ["/apps/service/CoreClrService.dll", "8080"], AllowElfInterpreter = true,
        WorkingDirectory = "/work", Readiness = new ReadinessOptions { Kind = ReadinessKind.ListeningPorts,
            Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 131072 } }, deadline.Token);
    report.StartSeconds = start.Elapsed.TotalSeconds;
    report.Worker = run.WorkerProcessId;
    events.Emit("start_returned", phaseSeconds: report.StartSeconds, worker: report.Worker);
    // This observer never controls the guest. Exceptions become retained
    // evidence errors; they cannot cancel or fail an otherwise running service.
    exitObserver = ObserveExitAsync(run, events, report, execution);
    var ready = Stopwatch.StartNew();
    var endpoints = await run.Ready.WaitAsync(deadline.Token);
    report.ReadyWaitSeconds = ready.Elapsed.TotalSeconds;
    report.ReadySinceStartSeconds = execution.Elapsed.TotalSeconds;
    var endpoint = endpoints.Single(p => p.GuestPort == 8080);
    events.Emit("ready", phaseSeconds: report.ReadyWaitSeconds, worker: report.Worker,
        instructions: AvailableInstructions(run));
    foreach (var request in new[] { ("GET", "/health", "", "CORECLR SERVICE OK\n"),
        ("POST", "/echo", "payload", "CORECLR:payload\n"), ("POST", "/stop", "", "STOP OK\n") })
    {
        // A request uses the remaining global execution budget, as well as the
        // existing finite HttpClient timeout. No networking policy is widened.
        using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false })
            { Timeout = TimeSpan.FromSeconds(seconds) };
        using var message = new HttpRequestMessage(new HttpMethod(request.Item1),
            $"http://{endpoint.HostAddress}:{endpoint.HostPort}{request.Item2}")
            { Content = new StringContent(request.Item3) };
        var http = Stopwatch.StartNew();
        events.Emit("http_requested", path: request.Item2, instructions: AvailableInstructions(run));
        using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
        double headersSeconds = http.Elapsed.TotalSeconds;
        string body = await response.Content.ReadAsStringAsync(deadline.Token);
        double elapsed = http.Elapsed.TotalSeconds, sinceStart = execution.Elapsed.TotalSeconds;
        report.Responses.Add(new(request.Item2, (int)response.StatusCode, body));
        report.HttpTimings.Add(new(request.Item2, elapsed, headersSeconds, sinceStart));
        events.Emit("http_response", path: request.Item2, status: (int)response.StatusCode, body: body,
            phaseSeconds: elapsed, instructions: AvailableInstructions(run));
        if (response.StatusCode != HttpStatusCode.OK || body != request.Item4)
            throw new IOException("HTTP comparison differs: " + request.Item2);
    }
    // /stop asks IHostApplicationLifetime to stop the app after its response.
    // Emulator StopAsync is only used in failure cleanup below.
    var wait = Stopwatch.StartNew();
    report.Result = await run.WaitAsync(deadline.Token);
    report.WaitSeconds = wait.Elapsed.TotalSeconds;
    await exitObserver;
    report.NormalWaitCompleted = true;
    report.Stdout = Encoding.UTF8.GetString(report.Result.StandardOutput);
    report.Stderr = Encoding.UTF8.GetString(report.Result.StandardError);
    events.Emit("wait_returned", phaseSeconds: report.WaitSeconds, instructions: report.Result.Instructions,
        reason: report.Result.Reason.ToString(), exitCode: report.Result.ExitCode);
    report.Passed = report.Result.Reason == RunExitReason.Exited && report.Result.ExitCode == 0 &&
        report.Result.Instructions > 0 && report.Result.Instructions <= budget && report.Result.ResourcesReleased &&
        !report.Result.CaptureTruncated && report.Stderr == "" &&
        report.Stdout == "SERVICE READY\nSERVICE STOPPED\n" && report.Responses.Count == 3;
}
catch (Exception error)
{
    report.Error = error.ToString();
    events.Emit("failure", detail: error.ToString(), instructions: run == null ? null : AvailableInstructions(run));
}
finally
{
    if (run != null)
    {
        report.Worker ??= run.WorkerProcessId;
        if (!run.Completion.IsCompleted)
        {
            report.CleanupStopRequested = true;
            report.Passed = false;
            events.Emit("cleanup_stop_requested", worker: report.Worker);
            try { await run.StopAsync(TimeSpan.FromSeconds(5)); }
            catch (Exception error)
            {
                report.CleanupErrors.Add(error.ToString());
                if (mode == ExecutionMode.SeparateProcess)
                {
                    report.CleanupKillRequested = true;
                    try
                    {
                        using var killBound = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await run.KillAsync(killBound.Token);
                    }
                    catch (Exception killError) { report.CleanupErrors.Add(killError.ToString()); }
                }
            }
        }
        try
        {
            using var cleanupBound = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            report.Result ??= await run.WaitAsync(cleanupBound.Token);
            report.Stdout = Encoding.UTF8.GetString(report.Result.StandardOutput);
            report.Stderr = Encoding.UTF8.GetString(report.Result.StandardError);
            if (exitObserver != null) await exitObserver.WaitAsync(cleanupBound.Token);
        }
        catch (Exception error) { report.CleanupErrors.Add(error.ToString()); }
        try { await run.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)); report.RunDisposed = true; }
        catch (Exception error) { report.CleanupErrors.Add(error.ToString()); }
    }
    if (machine != null)
        try { await machine.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5)); report.MachineDisposed = true; }
        catch (Exception error) { report.CleanupErrors.Add(error.ToString()); }
    if (report.Worker is int pid)
    {
        try { using var process = Process.GetProcessById(pid); report.WorkerGone = process.HasExited; }
        catch (ArgumentException) { report.WorkerGone = true; }
        catch (Exception error) { report.WorkerGone = false; report.CleanupErrors.Add(error.ToString()); }
    }
    deadline?.Dispose();
    report.TotalSeconds = total.Elapsed.TotalSeconds;
    events.Emit("disposed", worker: report.Worker, instructions: report.Result?.Instructions);
    events.Dispose();
    report.EventErrors = events.Errors.ToArray();
    report.Passed &= report.NormalWaitCompleted && !report.CleanupStopRequested && !report.CleanupKillRequested &&
        report.RunDisposed && report.MachineDisposed && report.WorkerGone && report.Error == null &&
        report.CleanupErrors.Count == 0 && report.EventErrors.Length == 0 && report.ObserverError == null &&
        (mode == ExecutionMode.SeparateProcess ? report.Worker is > 0 : report.Worker == null);
    File.WriteAllText(output, JsonSerializer.Serialize(report, EvidenceJson.Default.Report));
}
Console.WriteLine($"readytorun {mode} service {(report.Passed ? "PASS" : "FAIL")}");
return report.Passed ? 0 : 1;

static long? AvailableInstructions(MachineRun run) => run.Completion.IsCompletedSuccessfully
    ? run.Completion.Result.Instructions : null;

static async Task ObserveExitAsync(MachineRun run, HostEvents events, Report report, Stopwatch execution)
{
    try
    {
        var result = await run.Completion;
        report.GuestExitSinceStartSeconds = execution.Elapsed.TotalSeconds;
        events.Emit("guest_exit", instructions: result.Instructions, reason: result.Reason.ToString(), exitCode: result.ExitCode);
    }
    catch (Exception error) { report.ObserverError = error.ToString(); events.Emit("observer_failure", detail: error.ToString()); }
}

internal sealed record Closure(string Images, ImageEntry[] Files, Dictionary<string, string> Environment);
internal sealed record ImageEntry(string GuestPath, string File, string Sha256, bool Executable);
internal sealed record Response(string Path, int Status, string Body);
internal sealed record HttpTiming(string Path, double Seconds, double HeadersSeconds, double SinceStartSeconds);
internal sealed class Report
{
    public bool Passed { get; set; }
    public string HostRuntime { get; set; } = RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot";
    public string HostFramework { get; set; } = RuntimeInformation.FrameworkDescription;
    public bool HostDynamicCodeCompiled { get; set; } = RuntimeFeature.IsDynamicCodeCompiled;
    public string Mode { get; set; } = "";
    public string Scenario { get; set; } = "service";
    public bool DiagnosticNoWx { get; set; }
    public int MemoryMiB { get; set; }
    public int VirtualMemoryMiB { get; set; }
    public long InstructionLimit { get; set; }
    public int ExecutionSeconds { get; set; }
    public int? Worker { get; set; }
    public bool WorkerGone { get; set; } = true;
    public bool RunDisposed { get; set; }
    public bool MachineDisposed { get; set; }
    public bool NormalWaitCompleted { get; set; }
    public bool CleanupStopRequested { get; set; }
    public bool CleanupKillRequested { get; set; }
    public string? Error { get; set; }
    public string? ObserverError { get; set; }
    public List<string> CleanupErrors { get; set; } = [];
    public string[] EventErrors { get; set; } = [];
    public string EventsPath { get; set; } = "";
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public List<Response> Responses { get; set; } = [];
    public List<HttpTiming> HttpTimings { get; set; } = [];
    public MachineRunResult? Result { get; set; }
    public double ImportSeconds { get; set; }
    public double StartSeconds { get; set; }
    public double ReadyWaitSeconds { get; set; }
    public double ReadySinceStartSeconds { get; set; }
    public double WaitSeconds { get; set; }
    public double GuestExitSinceStartSeconds { get; set; }
    public double TotalSeconds { get; set; }
}
internal sealed record HostEvent(long Sequence, string Event, DateTimeOffset Utc, double ElapsedSeconds,
    double? SinceStartSeconds, double? PhaseSeconds, string? Path, int? Status, string? Body,
    int? Worker, long? Instructions, string? Reason, int? ExitCode, string? Detail);

internal sealed class HostEvents : IDisposable
{
    private readonly object gate = new();
    private readonly Stopwatch total, execution;
    private StreamWriter? writer;
    private long sequence;
    public List<string> Errors { get; } = [];
    public HostEvents(string path, Stopwatch total, Stopwatch execution)
    {
        this.total = total; this.execution = execution;
        try { writer = new(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read), new UTF8Encoding(false)); }
        catch (Exception error) { Errors.Add(error.ToString()); }
    }
    public void Emit(string name, double? phaseSeconds = null, string? path = null, int? status = null,
        string? body = null, int? worker = null, long? instructions = null, string? reason = null,
        int? exitCode = null, string? detail = null)
    {
        lock (gate)
        {
            if (writer == null) return;
            try
            {
                var row = new HostEvent(++sequence, name, DateTimeOffset.UtcNow, total.Elapsed.TotalSeconds,
                    execution.IsRunning ? execution.Elapsed.TotalSeconds : null, phaseSeconds, path, status,
                    body, worker, instructions, reason, exitCode, detail == null ? null : detail[..Math.Min(detail.Length, 4096)]);
                writer.WriteLine(JsonSerializer.Serialize(row, EvidenceJson.Default.HostEvent));
                writer.Flush();
            }
            catch (Exception error)
            {
                Errors.Add(error.ToString());
                try { writer.Dispose(); } catch (Exception closeError) { Errors.Add(closeError.ToString()); }
                writer = null;
            }
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            try { writer?.Dispose(); } catch (Exception error) { Errors.Add(error.ToString()); }
            writer = null;
        }
    }
}
[JsonSerializable(typeof(Report))]
[JsonSerializable(typeof(Closure))]
[JsonSerializable(typeof(HostEvent))]
internal partial class EvidenceJson : JsonSerializerContext;
