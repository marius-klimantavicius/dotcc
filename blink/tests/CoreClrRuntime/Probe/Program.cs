using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length is not (8 or 9 or 10) || !Enum.TryParse<ExecutionMode>(args[0], out var mode))
    throw new ArgumentException("Probe InProcess|SeparateProcess closure.json host|smoke|workload|service|memfd budget seconds memoryMiB wx-default|diagnostic-no-wx report.json");
var closure = JsonSerializer.Deserialize(File.ReadAllText(args[1]), EvidenceJson.Default.Closure)!;
string scenario = args[2], output = Path.GetFullPath(args[7]);
if (scenario is not ("host" or "smoke" or "workload" or "service" or "memfd")) throw new ArgumentException("scenario");
long budget = long.Parse(args[3]); int seconds = int.Parse(args[4]), memory = int.Parse(args[5]);
int virtualMemory = args.Length >= 9 ? int.Parse(args[8]) : 0;
bool diagnosticNoWx = args[6] switch { "wx-default" => false, "diagnostic-no-wx" => true, _ => throw new ArgumentException("WX policy") };
var report = new Report { Mode = mode.ToString(), Scenario = scenario, DiagnosticNoWx = diagnosticNoWx,
    MemoryMiB = memory, VirtualMemoryMiB = virtualMemory };
BlinkMachine? machine = null;
MachineRun? run = null;
try
{
    var environment = closure.Environment.ToDictionary(p => p.Key, p => p.Value);
    if (diagnosticNoWx) environment["DOTNET_EnableWriteXorExecute"] = "0";
    machine = new(new MachineOptions { ExecutionMode = mode, MemoryLimit = memory * 1024L * 1024,
        VirtualMemoryLimit = virtualMemory == 0 ? null : virtualMemory * 1024L * 1024,
        InstructionLimit = budget, ExecutionDeadline = TimeSpan.FromSeconds(seconds), Environment = environment,
        GuestJit = new() { Enabled = args.Length > 9 && args[9] == "guest-jit", AllowUnverifiedWindows = true },
        Network = scenario == "service" ? new NetworkPolicy { Publications = [new(8080)] } : NetworkPolicy.Isolated });
    machine.ImportImage(closure.Files.Select(file =>
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(closure.Images, file.File));
        if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.Sha256) throw new IOException("Closure changed: " + file.File);
        return new ImageFile(file.GuestPath, bytes, file.Executable);
    }));
    string[] arguments = scenario switch
    {
        "host" => ["--info"], "smoke" => ["/apps/console/CoreClrConsole.dll", "smoke"],
        "workload" => ["/apps/console/CoreClrConsole.dll", "workload", "/work"], "memfd" => [],
        _ => ["/apps/service/CoreClrService.dll", "8080"]
    };
    run = await machine.StartAsync(new ExecutionOptions { Executable = scenario == "memfd" ? "/bin/guest-memfd" : "/dotnet/dotnet", Arguments = arguments,
        AllowElfInterpreter = scenario != "memfd", WorkingDirectory = scenario == "memfd" ? "/" : "/work",
        Readiness = new ReadinessOptions { Kind = scenario == "service" ? ReadinessKind.ListeningPorts : ReadinessKind.Started,
            Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 65536, OutputLimit = 131072 } });
    report.Worker = run.WorkerProcessId;
    if (scenario == "service")
    {
        var endpoints = await run.Ready;
        var endpoint = endpoints.Single(p => p.GuestPort == 8080);
        foreach (var request in new[] { ("GET", "/health", "", "CORECLR SERVICE OK\n"),
            ("POST", "/echo", "payload", "CORECLR:payload\n"), ("POST", "/stop", "", "STOP OK\n") })
        {
            // First-request JIT work runs inside the interpreter too. Keep the
            // request within the same finite execution bound as guest startup.
            using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(seconds) };
            using var message = new HttpRequestMessage(new HttpMethod(request.Item1), $"http://{endpoint.HostAddress}:{endpoint.HostPort}{request.Item2}")
                { Content = new StringContent(request.Item3) };
            using var response = await client.SendAsync(message);
            string body = await response.Content.ReadAsStringAsync();
            report.Responses.Add(new(request.Item2, (int)response.StatusCode, body));
            if (response.StatusCode != HttpStatusCode.OK || body != request.Item4) throw new IOException("HTTP comparison differs");
        }
    }
    report.Result = await run.WaitAsync();
    report.Stdout = Encoding.UTF8.GetString(report.Result.StandardOutput);
    report.Stderr = Encoding.UTF8.GetString(report.Result.StandardError);
    if (scenario == "workload" && report.Result.ResourcesReleased)
    {
        string export = output + ".files";
        Directory.CreateDirectory(export);
        machine.ExportPrivateStorage(export);
        string file = Path.Combine(export, "work/output.txt");
        if (File.Exists(file)) report.Written = File.ReadAllText(file);
    }
    report.Passed = report.Result.Reason == RunExitReason.Exited && report.Result.ExitCode == 0 && report.Result.Instructions > 0 &&
        report.Result.ResourcesReleased && !report.Result.CaptureTruncated && report.Stderr == "" && (scenario switch
        {
            "host" => report.Stdout.Contains("10.0.12", StringComparison.Ordinal),
            "smoke" => report.Stdout == "CORECLR IL OK\n",
            "workload" => report.Stdout == "CORECLR WORKLOAD OK jit=2757056 threads=13 file=20\n" && report.Written == "guest-output\nappend\n",
            "memfd" => report.Stdout == "guest memfd aliases passed",
            _ => report.Responses.Count == 3 && report.Stdout == "SERVICE READY\nSERVICE STOPPED\n"
        });
}
catch (Exception error) { report.Error = error.ToString(); }
finally
{
    if (run != null)
    {
        report.Worker ??= run.WorkerProcessId;
        try
        {
            if (!run.Completion.IsCompleted) await run.StopAsync();
            report.Result ??= await run.WaitAsync();
            report.Stdout = Encoding.UTF8.GetString(report.Result.StandardOutput);
            report.Stderr = Encoding.UTF8.GetString(report.Result.StandardError);
            await run.DisposeAsync(); report.RunDisposed = true;
        }
        catch (Exception error) { report.CleanupError = error.ToString(); }
    }
    if (machine != null)
        try { await machine.DisposeAsync(); report.MachineDisposed = true; }
        catch (Exception error) { report.CleanupError = (report.CleanupError ?? "") + error; }
    if (report.Worker is int pid)
    {
        try { using var process = Process.GetProcessById(pid); report.WorkerGone = process.HasExited; }
        catch (ArgumentException) { report.WorkerGone = true; }
    }
    report.Passed &= report.RunDisposed && report.MachineDisposed && report.WorkerGone && report.CleanupError == null &&
        (mode == ExecutionMode.SeparateProcess ? report.Worker is > 0 : report.Worker == null);
    File.WriteAllText(output, JsonSerializer.Serialize(report, EvidenceJson.Default.Report));
}
Console.WriteLine($"coreclr {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");
return report.Passed ? 0 : 1;

internal sealed record Closure(string Images, ImageEntry[] Files, Dictionary<string, string> Environment);
internal sealed record ImageEntry(string GuestPath, string File, string Sha256, bool Executable);
internal sealed record Response(string Path, int Status, string Body);
internal sealed class Report
{
    public bool Passed { get; set; }
    public string HostRuntime { get; set; } = RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot";
    public string HostFramework { get; set; } = RuntimeInformation.FrameworkDescription;
    public bool HostDynamicCodeCompiled { get; set; } = RuntimeFeature.IsDynamicCodeCompiled;
    public string Mode { get; set; } = "";
    public string Scenario { get; set; } = "";
    public bool DiagnosticNoWx { get; set; }
    public int MemoryMiB { get; set; }
    public int VirtualMemoryMiB { get; set; }
    public int? Worker { get; set; }
    public bool WorkerGone { get; set; } = true;
    public bool RunDisposed { get; set; }
    public bool MachineDisposed { get; set; }
    public string? Error { get; set; }
    public string? CleanupError { get; set; }
    public string? Written { get; set; }
    public string Stdout { get; set; } = "";
    public string Stderr { get; set; } = "";
    public List<Response> Responses { get; set; } = [];
    public MachineRunResult? Result { get; set; }
}
[JsonSerializable(typeof(Report))]
[JsonSerializable(typeof(Closure))]
internal partial class EvidenceJson : JsonSerializerContext;
