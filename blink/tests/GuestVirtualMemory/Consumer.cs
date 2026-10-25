using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length != 5 || !Enum.TryParse<ExecutionMode>(args[0], out var mode) || !Enum.IsDefined(mode))
    throw new ArgumentException("Consumer InProcess|SeparateProcess guest.elf sha256 report.json virtualMemoryMiB");
const long budget = 40_000_000;
const int seconds = 60;
int virtualMiB = int.Parse(args[4]);
if (virtualMiB is not (0 or 1024)) throw new ArgumentException("virtual budget");
byte[] expected = System.Text.Encoding.UTF8.GetBytes(virtualMiB == 0
    ? "guest-virtual-memory: default limit retained: PASS\n"
    : "guest-virtual-memory: sparse reservation and backing: PASS\n");
var report = new VirtualReport { Mode = mode.ToString(), ImageSha256 = args[2], VirtualMemoryMiB = virtualMiB };
BlinkMachine? machine = null;
MachineRun? run = null;
try
{
    byte[] image = File.ReadAllBytes(args[1]);
    if (Convert.ToHexStringLower(SHA256.HashData(image)) != args[2]) throw new IOException("Guest identity differs");
    machine = new(new MachineOptions
    {
        ExecutionMode = mode, MemoryLimit = 32 * 1024 * 1024,
        VirtualMemoryLimit = virtualMiB == 0 ? null : virtualMiB * 1024L * 1024, ThreadLimit = 4, DescriptorLimit = 16,
        InstructionLimit = budget, ExecutionDeadline = TimeSpan.FromSeconds(seconds), Network = NetworkPolicy.Isolated
    });
    machine.ImportImage([new ImageFile("/bin/virtual-memory", image, true)]);
    run = await machine.StartAsync(new ExecutionOptions
    {
        Executable = "/bin/virtual-memory",
        Readiness = new ReadinessOptions { Kind = ReadinessKind.Started, Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 1024, OutputLimit = 1024 }
    });
    report.WorkerProcessId = run.WorkerProcessId;
    report.Result = await run.WaitAsync().WaitAsync(TimeSpan.FromSeconds(seconds + 10));
    report.Passed = report.Result is
    {
        Reason: RunExitReason.Exited, ExitCode: 0, Signal: 0, Halt: 0, Diagnostic: null,
        ResourcesReleased: true, CaptureTruncated: false
    } && report.Result.Instructions is > 0 and <= budget && report.Result.OutputBytes == expected.Length &&
        report.Result.StandardOutput.AsSpan().SequenceEqual(expected) && report.Result.StandardError.Length == 0;
}
catch (Exception error) { report.Error = error.ToString(); }
finally
{
    if (run != null)
    {
        report.WorkerProcessId ??= run.WorkerProcessId;
        try
        {
            if (!run.Completion.IsCompleted) await run.StopAsync().WaitAsync(TimeSpan.FromSeconds(10));
            report.Result ??= await run.WaitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await run.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)); report.RunDisposed = true;
        }
        catch (Exception error) { report.CleanupError = error.ToString(); }
    }
    if (machine != null)
    {
        try { await machine.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10)); report.MachineDisposed = true; }
        catch (Exception error) { report.CleanupError = (report.CleanupError ?? "") + error; }
    }
    if (report.WorkerProcessId is int pid)
    {
        try { using var process = Process.GetProcessById(pid); report.WorkerGone = process.HasExited; }
        catch (ArgumentException) { report.WorkerGone = true; }
    }
    report.Passed &= report.RunDisposed && report.MachineDisposed && report.WorkerGone && report.Error == null &&
        report.CleanupError == null && (mode == ExecutionMode.SeparateProcess ? report.WorkerProcessId is > 0 : report.WorkerProcessId == null);
    File.WriteAllText(args[3], JsonSerializer.Serialize(report, VirtualJson.Default.VirtualReport));
}
Console.WriteLine($"virtual-memory {mode} {(report.Passed ? "PASS" : "FAIL")}");
return report.Passed ? 0 : 1;

internal sealed class VirtualReport
{
    public bool Passed { get; set; }
    public int VirtualMemoryMiB { get; set; }
    public string Mode { get; set; } = "";
    public string ImageSha256 { get; set; } = "";
    public string HostRuntime { get; set; } = RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot";
    public int? WorkerProcessId { get; set; }
    public bool WorkerGone { get; set; } = true;
    public bool RunDisposed { get; set; }
    public bool MachineDisposed { get; set; }
    public string? Error { get; set; }
    public string? CleanupError { get; set; }
    public MachineRunResult? Result { get; set; }
}
[JsonSerializable(typeof(VirtualReport))]
internal partial class VirtualJson : JsonSerializerContext;
