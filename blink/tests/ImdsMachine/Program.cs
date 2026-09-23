using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Emulation;
using Managed.Emulation.Host;

if (args is ["--machine-worker"])
    return await MachineWorkerHost.RunAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());
if (args.Length != 3 || args[1] is not ("InProcess" or "SeparateProcess")) return 80;
string elf = Path.GetFullPath(args[0]), mode = args[1], output = Path.GetFullPath(args[2]);
Directory.CreateDirectory(output);
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var results = new ConcurrentDictionary<string, MachineRunResult>();
var workers = new ConcurrentDictionary<string, (int? Pid, string? Executable)>();
await using var first = Create("first");
await using var second = Create("second");
bool passed = false;
string? failure = null;
try
{
    await Task.WhenAll(Run(first, "first", "first"), Run(second, "second", "second"));
    await Run(first, "restart", "first");
    passed = true;
}
catch (Exception error) { failure = error.ToString(); }
finally
{
    using var file = File.Create(Path.Combine(output, "result.json"));
    using var writer = new Utf8JsonWriter(file, new JsonWriterOptions { Indented = true });
    writer.WriteStartObject(); writer.WriteBoolean("passed", passed); writer.WriteString("mode", mode);
    writer.WriteBoolean("consumer_dynamic_code_supported", System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported);
    writer.WriteString("guest_sha256", Hash(elf)); writer.WriteString("error", failure);
    writer.WriteStartArray("runs");
    foreach (var (name, result) in results.OrderBy(pair => pair.Key))
    {
        writer.WriteStartObject(); writer.WriteString("name", name); writer.WriteString("reason", result.Reason.ToString());
        if (workers.TryGetValue(name, out var worker))
        {
            if (worker.Pid is int pid) writer.WriteNumber("worker_pid", pid); else writer.WriteNull("worker_pid");
            writer.WriteString("worker_executable", worker.Executable);
            if (worker.Executable != null) writer.WriteString("worker_executable_sha256", Hash(worker.Executable));
        }
        writer.WriteNumber("exit_code", result.ExitCode); writer.WriteNumber("instructions", result.Instructions);
        writer.WriteNumber("signal", result.Signal); writer.WriteNumber("halt", result.Halt);
        writer.WriteBoolean("resources_released", result.ResourcesReleased); writer.WriteString("diagnostic", result.Diagnostic);
        writer.WriteString("stdout", Encoding.UTF8.GetString(result.StandardOutput));
        writer.WriteString("stderr", Encoding.UTF8.GetString(result.StandardError)); writer.WriteEndObject();
    }
    writer.WriteEndArray(); writer.WriteEndObject();
}
if (!passed) { Console.Error.WriteLine(failure); return 1; }
Console.WriteLine($"PASS {mode}: NativeAOT AWS SDK IMDSv2 credentials/cache/refresh, independent machines and restart");
return 0;

BlinkMachine Create(string name)
{
    var machine = new BlinkMachine(new MachineOptions
    {
        ExecutionMode = Enum.Parse<ExecutionMode>(mode), MemoryLimit = 128L << 20,
        InstructionLimit = 300_000_000, ExecutionDeadline = TimeSpan.FromMinutes(2),
        Environment = new Dictionary<string, string>
        {
            ["LANG"] = "C", ["DOTNET_GCHeapHardLimit"] = "1000000",
            ["DOTNET_GCRegionRange"] = "2000000", ["DOTNET_GCRegionSize"] = "100000",
            ["DOTNET_EnableDiagnostics"] = "0", ["AWS_EC2_METADATA_V1_DISABLED"] = "true"
        },
        Metadata = new ImdsV2Options { InstanceId = "i-" + name, RoleName = "test-role",
            Credentials = new("TESTACCESS" + name, "TESTSECRET", "TESTSESSION", DateTimeOffset.UtcNow.AddHours(1)) }
    });
    try { machine.MountDirectory("/work", Path.GetDirectoryName(elf)!, MountAccess.ReadOnly); }
    catch { machine.DisposeAsync().AsTask().GetAwaiter().GetResult(); throw; }
    return machine;
}
async Task Run(BlinkMachine machine, string name, string identity)
{
    await using var run = await machine.StartAsync(new ExecutionOptions
    {
        Executable = "/work/" + Path.GetFileName(elf), WorkingDirectory = "/work",
        Arguments = ["i-" + identity, "TESTACCESS" + identity]
    }, deadline.Token);
    Check(run.ExecutionMode == Enum.Parse<ExecutionMode>(mode), "requested execution mode");
    if (run.WorkerProcessId is int pid)
    {
        using var worker = Process.GetProcessById(pid);
        workers[name] = (pid, worker.MainModule?.FileName ?? throw new InvalidOperationException("Worker executable unavailable"));
        Check(mode == "SeparateProcess" && pid != Environment.ProcessId, "separate worker");
    }
    else { workers[name] = (null, null); Check(mode == "InProcess", "no worker for in-process execution"); }
    var result = results[name] = await run.WaitAsync(deadline.Token);
    await File.WriteAllBytesAsync(Path.Combine(output, name + ".stdout"), result.StandardOutput);
    await File.WriteAllBytesAsync(Path.Combine(output, name + ".stderr"), result.StandardError);
    Check(result.Reason == RunExitReason.Exited && result.ExitCode == 0 && result.Halt == 0 && result.Signal == 0,
        name + " normal process exit: " + result.Reason + " halt=" + result.Halt + " signal=" + result.Signal + " " + result.Diagnostic + " " + Encoding.UTF8.GetString(result.StandardError));
    Check(result.ResourcesReleased && !result.CaptureTruncated && result.Diagnostic == null && result.Instructions > 0, name + " complete cleanup");
    Check(Encoding.UTF8.GetString(result.StandardOutput) == "IMDS PASS i-" + identity + "\n" && result.StandardError.Length == 0, name + " exact guest output");
}
static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Check(bool success, string message) { if (!success) throw new InvalidOperationException(message); }
