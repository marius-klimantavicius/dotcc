using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

// Diagnostic timing observations, never timing assertions. The caller freezes
// the assembly closure and runs builds and measurements serially.
if (args.Length < 3) throw new ArgumentException("Bench loop|host|prefix|service input output [trials] [serviceSeconds] [serviceBudget]");
string mode = args[0];
int trials = args.Length > 3 ? int.Parse(args[3]) : 6;
int seconds = mode == "service" ? args.Length > 4 ? int.Parse(args[4]) : 180 : 60;
ulong serviceBudget = args.Length > 5 ? ulong.Parse(args[5]) : 150_000_000UL;
if (seconds is < 1 or > 7200 || serviceBudget is < 1 or > 3_000_000_000UL || trials is < 1 or > 20)
    throw new ArgumentOutOfRangeException(nameof(args));
var rows = new List<Measurement>();
for (int trial = 0; trial < trials; ++trial)
{
    var row = new Measurement { Mode = mode, Trial = trial, Warmup = trial == 0,
        HostDynamicCode = System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled };
    var import = Stopwatch.StartNew();
    var image = new Dictionary<string, ReadOnlyMemory<byte>>();
    var executables = new HashSet<string>();
    string executable, directory;
    string[] arguments, environment;
    if (mode == "loop")
    {
        executable = "/bin/loop"; directory = "/";
        image.Add(executable, File.ReadAllBytes(args[1])); executables.Add(executable);
        arguments = [executable]; environment = [];
    }
    else
    {
        using var closure = JsonDocument.Parse(File.ReadAllBytes(args[1]));
        var root = closure.RootElement;
        foreach (var file in root.GetProperty("Files").EnumerateArray())
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(root.GetProperty("Images").GetString()!, file.GetProperty("File").GetString()!));
            if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.GetProperty("Sha256").GetString())
                throw new IOException("Guest input changed");
            string path = file.GetProperty("GuestPath").GetString()!;
            image.Add(path, bytes);
            if (file.GetProperty("Executable").GetBoolean()) executables.Add(path);
        }
        environment = root.GetProperty("Environment").EnumerateObject().Select(p => p.Name + "=" + p.Value.GetString()).ToArray();
        executable = root.TryGetProperty("Executable", out var entry) ? entry.GetString()! : "/dotnet/dotnet";
        directory = "/work";
        arguments = mode == "host" ? [executable, "--info"] :
            executable == "/dotnet/dotnet" ? [executable, "/apps/service/CoreClrService.dll", "8080"] : [executable, "8080"];
    }
    using var fs = new MountedFileSystem(new VirtualFileSystem(image, writableLimit: 512L << 20,
        descriptorLimit: 1024, imageLimit: 512L << 20, executablePaths: executables, nodeLimit: 8192), ownsRoot: true);
    fs.Mount("/dev", VirtualFileSystem.CreateDeviceFileSystem(1024), readOnly: true, ownsSource: true);
    await using var io = new InstanceIo(fs, descriptorLimit: 1024, outputLimit: 65536);
    using var stop = new HostExecutionStop(TimeSpan.FromSeconds(seconds));
    var owner = new ThreadedGuestExecution(io, stop, mode == "loop" ? 32UL << 20 : 256UL << 20,
        maximumWorkers: 64, virtualMemoryLimitBytes: mode == "loop" ? null : 4UL << 30);
    row.ImportMs = import.Elapsed.TotalMilliseconds;
    ulong budget = mode == "prefix" ? 2_000_000UL : mode == "service" ? serviceBudget : 40_000_000UL;
    long allocated = GC.GetTotalAllocatedBytes(true);
    using var process = Process.GetCurrentProcess();
    var cpu = process.TotalProcessorTime;
    var wall = Stopwatch.StartNew();
    ThreadedGuestExecutionResult? result = null;
    Exception? executionError = null;
    var thread = new Thread(() =>
    {
        try { result = owner.Run(executable, arguments, environment, budget, workingDirectory: directory, allowElfInterpreter: mode != "loop"); }
        catch (Exception error) { executionError = error; }
    });
    thread.Start();
    try
    {
        if (mode == "service")
        {
            while (!Encoding.UTF8.GetString(io.CapturedOutput.StandardOutput).Contains("SERVICE READY\n", StringComparison.Ordinal))
            {
                stop.Token.ThrowIfCancellationRequested();
                if (!thread.IsAlive) throw new IOException("Guest ended before readiness");
                await Task.Delay(10, stop.Token);
            }
            row.ReadinessInstructions = owner.InstructionsCompleted;
            row.ReadinessMs = wall.Elapsed.TotalMilliseconds;
            row.ReadinessCpuMs = (process.TotalProcessorTime - cpu).TotalMilliseconds;
            IPEndPoint? endpoint = null;
            for (int fd = 0; fd < 1024; ++fd)
            {
                var local = io.LocalEndpoint(fd);
                if (!local.Succeeded || local.Value.Port != 8080) continue;
                var published = io.Publish(fd);
                if (published.Succeeded) { endpoint = published.Value; break; }
            }
            if (endpoint == null) throw new IOException("Guest listener not published");
            foreach (var request in new[] { ("GET", "/health", "", "CORECLR SERVICE OK\n"),
                ("GET", "/health", "", "CORECLR SERVICE OK\n"), ("GET", "/health", "", "CORECLR SERVICE OK\n"),
                ("POST", "/echo", "payload", "CORECLR:payload\n"), ("POST", "/echo", "payload", "CORECLR:payload\n"),
                ("POST", "/stop", "", "STOP OK\n") })
            {
                ulong startInstructions = owner.InstructionsCompleted;
                var requestTime = Stopwatch.StartNew(); var requestCpu = process.TotalProcessorTime;
                using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(seconds) };
                using var message = new HttpRequestMessage(new HttpMethod(request.Item1), $"http://{endpoint.Address}:{endpoint.Port}{request.Item2}")
                    { Content = new StringContent(request.Item3) };
                using var response = await client.SendAsync(message, stop.Token);
                string body = await response.Content.ReadAsStringAsync(stop.Token);
                if (response.StatusCode != HttpStatusCode.OK || body != request.Item4) throw new IOException("HTTP semantics differ");
                row.Requests.Add(new(request.Item2, requestTime.Elapsed.TotalMilliseconds,
                    (process.TotalProcessorTime - requestCpu).TotalMilliseconds, owner.InstructionsCompleted - startInstructions, body));
            }
        }
    }
    catch (Exception error) { row.Error = error.ToString(); stop.RequestStop(); }
    if (!thread.Join(TimeSpan.FromSeconds(seconds + 10)))
    {
        stop.RequestStop();
        if (!thread.Join(TimeSpan.FromSeconds(5)))
        {
            row.Error = "Guest owner failed to quiesce after stop; process discarded without disposing live owner resources";
            rows.Add(row);
            File.WriteAllText(args[2], JsonSerializer.Serialize(rows, BenchJson.Default.ListMeasurement));
            Environment.Exit(2);
        }
    }
    wall.Stop(); process.Refresh();
    row.WallMs = wall.Elapsed.TotalMilliseconds;
    row.CpuMs = (process.TotalProcessorTime - cpu).TotalMilliseconds;
    row.AllocatedBytes = GC.GetTotalAllocatedBytes(true) - allocated;
    row.PeakWorkingSet = process.PeakWorkingSet64;
    row.Instructions = result?.Instructions ?? 0;
    row.StopReason = result?.StopReason.ToString();
    row.RetainedBytes = result?.RetainedBytesBeforeRelease ?? 0;
    row.RetainedMappings = result?.RetainedMappingsBeforeRelease ?? 0;
    row.Threads = result?.Threads.Select(t => new ThreadMeasurement(t.GuestThreadId, t.Termination,
        t.Status, t.Halt, t.Signal, t.InstructionPointer, t.FirstTrap)).ToList() ?? [];
    row.Quiescent = owner.IsQuiescent;
    row.ResourcesReleased = result?.AllWorkersJoined == true && result.MemoryReleased;
    row.Stdout = Encoding.UTF8.GetString(io.CapturedOutput.StandardOutput);
    row.Stderr = Encoding.UTF8.GetString(io.CapturedOutput.StandardError);
    row.ExecutionError = executionError?.ToString();
    row.Error ??= executionError?.ToString();
    row.Passed = row.Error == null && row.Stderr == "" && row.Quiescent && row.ResourcesReleased && stop.NotificationFailure == null &&
        result != null && (mode == "prefix" ? result.StopReason == HostExecutionStopReason.Budget && result.Instructions == budget :
            result.Exited && result.ExitStatus == 0 && result.StopReason == HostExecutionStopReason.None) &&
        (mode != "host" || row.Stdout.Contains("10.0.12", StringComparison.Ordinal)) &&
        (mode != "service" || row.Stdout == "SERVICE READY\nSERVICE STOPPED\n");
    await io.DisposeAsync();
    row.Passed &= io.OpenDescriptors == 0 && io.PendingPipeOperations == 0;
    rows.Add(row);
    File.WriteAllText(args[2], JsonSerializer.Serialize(rows, BenchJson.Default.ListMeasurement));
    if (!row.Passed) return 1;
}
return 0;

internal sealed class Measurement
{
    public string Mode { get; set; } = "";
    public int Trial { get; set; }
    public bool Warmup { get; set; }
    public bool HostDynamicCode { get; set; }
    public bool Passed { get; set; }
    public double ImportMs { get; set; }
    public double WallMs { get; set; }
    public double CpuMs { get; set; }
    public double ReadinessMs { get; set; }
    public double ReadinessCpuMs { get; set; }
    public ulong ReadinessInstructions { get; set; }
    public long AllocatedBytes { get; set; }
    public long PeakWorkingSet { get; set; }
    public ulong Instructions { get; set; }
    public ulong RetainedBytes { get; set; }
    public ulong RetainedMappings { get; set; }
    public string? StopReason { get; set; }
    public bool Quiescent { get; set; }
    public bool ResourcesReleased { get; set; }
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public string? Error { get; set; }
    public string? ExecutionError { get; set; }
    public List<RequestMeasurement> Requests { get; set; } = [];
    public List<ThreadMeasurement> Threads { get; set; } = [];
}
internal sealed record RequestMeasurement(string Path, double WallMs, double CpuMs, ulong Instructions, string Body);
internal sealed record ThreadMeasurement(int Id, string Termination, int Status, int Halt, int Signal, ulong IP, string? FirstTrap);
[JsonSerializable(typeof(List<Measurement>))]
internal partial class BenchJson : JsonSerializerContext;
