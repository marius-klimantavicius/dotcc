using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

// Supplementary startup diagnostics using the existing scalar syscall hook.
// Public BlinkMachine Probe remains the lifecycle/network qualification path.
if (args.Length is not (7 or 8)) throw new ArgumentException("Trace closure.json host|smoke|workload|service budget seconds memoryMiB wx-default|diagnostic-no-wx report.json [virtualMemoryMiB]");
using var closure = JsonDocument.Parse(File.ReadAllBytes(args[0]));
string root = closure.RootElement.GetProperty("Images").GetString()!;
string scenario = args[1];
ulong budget = ulong.Parse(args[2]); int seconds = int.Parse(args[3]), memory = int.Parse(args[4]);
int virtualMemory = args.Length == 8 ? int.Parse(args[7]) : 0;
bool diagnosticNoWx = args[5] switch { "wx-default" => false, "diagnostic-no-wx" => true, _ => throw new ArgumentException("WX policy") };
var image = new Dictionary<string, ReadOnlyMemory<byte>>();
var executablePaths = new HashSet<string>();
foreach (var file in closure.RootElement.GetProperty("Files").EnumerateArray())
{
    byte[] bytes = File.ReadAllBytes(Path.Combine(root, file.GetProperty("File").GetString()!));
    if (Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != file.GetProperty("Sha256").GetString()) throw new IOException("Image changed");
    string path = file.GetProperty("GuestPath").GetString()!;
    image.Add(path, bytes);
    if (file.GetProperty("Executable").GetBoolean()) executablePaths.Add(path);
}
var environment = closure.RootElement.GetProperty("Environment").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString()!);
if (diagnosticNoWx) environment["DOTNET_EnableWriteXorExecute"] = "0";
string[] arguments = scenario switch
{
    "host" => ["--info"], "smoke" => ["/apps/console/CoreClrConsole.dll", "smoke"],
    "workload" => ["/apps/console/CoreClrConsole.dll", "workload", "/work"],
    "service" => ["/apps/service/CoreClrService.dll", "8080"], _ => throw new ArgumentException("scenario")
};
using var fs = new MountedFileSystem(new VirtualFileSystem(image, writableLimit: 512L * 1024 * 1024,
    descriptorLimit: 1024, imageLimit: 512L * 1024 * 1024, executablePaths: executablePaths, nodeLimit: 8192), ownsRoot: true);
fs.Mount("/dev", VirtualFileSystem.CreateDeviceFileSystem(1024), readOnly: true, ownsSource: true);
await using var io = new InstanceIo(fs, descriptorLimit: 1024, outputLimit: 65536);
using var stop = new HostExecutionStop(TimeSpan.FromSeconds(seconds));
var owner = new ThreadedGuestExecution(io, stop, (ulong)memory * 1024 * 1024, maximumWorkers: 64,
    virtualMemoryLimitBytes: virtualMemory == 0 ? null : (ulong)virtualMemory * 1024 * 1024);
ThreadedGuestExecutionResult? result = null;
string? error = null;
const int FirstLimit = 4096, TailLimit = 512;
var first = new List<ThreadedSyscallObservation>();
var tail = new Queue<ThreadedSyscallObservation>();
var counts = new Dictionary<ulong, long>();
var traceSync = new object();
long observations = 0;
var thread = new Thread(() =>
{
    try
    {
        result = owner.Run("/dotnet/dotnet", ["/dotnet/dotnet", .. arguments],
            environment.Select(p => p.Key + "=" + p.Value).ToArray(), budget,
            syscallTrace: observation =>
            {
                lock (traceSync)
                {
                    ++observations;
                    counts[observation.Number] = counts.GetValueOrDefault(observation.Number) + 1;
                    if (first.Count < FirstLimit) first.Add(observation);
                    else { if (tail.Count == TailLimit) tail.Dequeue(); tail.Enqueue(observation); }
                }
            }, workingDirectory: "/work", allowElfInterpreter: true);
    }
    catch (Exception exception) { error = exception.ToString(); }
}) { IsBackground = true, Name = "CoreCLR startup trace" };
thread.Start();
if (!thread.Join(TimeSpan.FromSeconds(seconds + 10)))
{
    stop.RequestStop();
    if (!thread.Join(TimeSpan.FromSeconds(5))) throw new TimeoutException("Trace owner did not quiesce; process must be terminated by attempt owner");
}
var captured = io.CapturedOutput;
var report = new
{
    DiagnosticOnly = true, PublicApiQualification = false, Scenario = scenario, DiagnosticNoWx = diagnosticNoWx,
    MemoryMiB = memory, VirtualMemoryMiB = virtualMemory, Budget = budget, Seconds = seconds, Error = error, Result = result,
    Stdout = Encoding.UTF8.GetString(captured.StandardOutput), Stderr = Encoding.UTF8.GetString(captured.StandardError),
    Observations = observations, FirstLimit, TailLimit, Truncated = observations > first.Count + tail.Count,
    SyscallCounts = counts, First = first, Tail = tail.ToArray(), Quiescent = owner.IsQuiescent
};
File.WriteAllText(args[6], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"coreclr-trace {scenario} observations={observations} quiescent={owner.IsQuiescent}");
return error == null && result?.Exited == true && result.ExitStatus == 0 && owner.IsQuiescent ? 0 : 1;
