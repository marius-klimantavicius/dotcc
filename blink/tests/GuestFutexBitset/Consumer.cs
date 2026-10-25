using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation;

if (args.Length is not (5 or 6) || !Enum.TryParse<ExecutionMode>(args[0], out var mode) || !Enum.IsDefined(mode))
    throw new ArgumentException("Consumer InProcess|SeparateProcess guest.elf sha256 report.json bitset|signal-stack|process-maps|getcpu|segment-read [dependencies.json]");
const long budget = 20_000_000;
const int seconds = 20;
string scenario = args[4];
byte[] expectedOutput = scenario switch
{
    "bitset" => [],
    "signal-stack" => "guest-signal-stack: guard=4096 registered=16384 onstack=2 returned=2 disabled=1\n"u8.ToArray(),
    "process-maps" => "guest-process-maps: main/worker stacks, rwx/none, private/shared, unmap: PASS\n"u8.ToArray(),
    "getcpu" => "guest-getcpu: main/worker affinity, libc, nullable outputs, width, cache: PASS\n"u8.ToArray(),
    "segment-read" => "guest-segment-read: selectors, registers, word stores, cross-page: PASS\n"u8.ToArray(),
    _ => throw new ArgumentException("Unknown scenario")
};
string output = Path.GetFullPath(args[3]);
var report = new BitsetReport { Mode = mode.ToString(), ImageSha256 = args[2], Scenario = scenario };
BlinkMachine? machine = null;
MachineRun? run = null;
long started = Stopwatch.GetTimestamp();
try
{
    byte[] image = File.ReadAllBytes(args[1]);
    if (Convert.ToHexStringLower(SHA256.HashData(image)) != args[2])
        throw new IOException("Fixture image hash differs");
    machine = new(new MachineOptions
    {
        ExecutionMode = mode, MemoryLimit = 64 * 1024 * 1024,
        ThreadLimit = 8, DescriptorLimit = 16,
        InstructionLimit = budget, ExecutionDeadline = TimeSpan.FromSeconds(seconds),
        Network = NetworkPolicy.Isolated
    });
    var images = new List<ImageFile> { new("/bin/futex-bitset", image, true) };
    if (scenario is "process-maps" or "getcpu")
    {
        if (args.Length != 6) throw new ArgumentException("Dynamic fixture requires retained dependencies");
        var dependencies = JsonSerializer.Deserialize(File.ReadAllText(args[5]), BitsetJson.Default.FixtureDependencies)!;
        foreach (var file in dependencies.Files)
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(dependencies.Images, file.File));
            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != file.Sha256)
                throw new IOException("Dependency image hash differs");
            images.Add(new(file.GuestPath, bytes, file.Executable));
        }
    }
    machine.ImportImage(images);
    run = await machine.StartAsync(new ExecutionOptions
    {
        Executable = "/bin/futex-bitset",
        AllowElfInterpreter = scenario is "process-maps" or "getcpu",
        Readiness = new ReadinessOptions { Kind = ReadinessKind.Started, Timeout = TimeSpan.FromSeconds(seconds) },
        Console = new ConsoleOptions { CaptureBytes = 1024, OutputLimit = 1024 }
    });
    report.WorkerProcessId = run.WorkerProcessId;
    report.Result = await run.WaitAsync().WaitAsync(TimeSpan.FromSeconds(seconds + 10));
    // Exit 0 is reached only after the original combined fixture's assertions:
    // realtime/monotonic absolute deadlines, selective bitsets, WAIT/WAKE,
    // clear-child-TID joins, and changed-word timeout without a wake.
    report.Passed = report.Result is
    {
        Reason: RunExitReason.Exited, ExitCode: 0, Signal: 0, Halt: 0,
        ResourcesReleased: true, CaptureTruncated: false,
        Diagnostic: null
    } && report.Result.Instructions is > 0 and <= budget &&
        report.Result.OutputBytes == expectedOutput.Length &&
        report.Result.StandardOutput.AsSpan().SequenceEqual(expectedOutput) && report.Result.StandardError.Length == 0;
}
catch (Exception error) { report.Error = error.ToString(); }
finally
{
    if (run != null)
    {
        report.WorkerProcessId ??= run.WorkerProcessId;
        try
        {
            if (!run.Completion.IsCompleted)
                await run.StopAsync().WaitAsync(TimeSpan.FromSeconds(10));
            report.Result ??= await run.WaitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            await run.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
            report.RunDisposed = true;
        }
        catch (Exception error) { report.CleanupError = error.ToString(); }
    }
    if (machine != null)
    {
        try
        {
            await machine.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
            report.MachineDisposed = true;
        }
        catch (Exception error) { report.CleanupError = (report.CleanupError ?? "") + error; }
    }
    if (report.WorkerProcessId is int pid)
    {
        try { using var process = Process.GetProcessById(pid); report.WorkerGone = process.HasExited; }
        catch (ArgumentException) { report.WorkerGone = true; }
    }
    report.ElapsedSeconds = Stopwatch.GetElapsedTime(started).TotalSeconds;
    report.Passed &= report.RunDisposed && report.MachineDisposed && report.WorkerGone &&
        report.Error == null && report.CleanupError == null &&
        (mode == ExecutionMode.SeparateProcess ? report.WorkerProcessId is > 0 : report.WorkerProcessId == null);
    File.WriteAllText(output, JsonSerializer.Serialize(report, BitsetJson.Default.BitsetReport));
}
Console.WriteLine($"guest-consumer {mode} {scenario} {(report.Passed ? "PASS" : "FAIL")}");
return report.Passed ? 0 : 1;

internal sealed class BitsetReport
{
    public bool Passed { get; set; }
    public string Mode { get; set; } = "";
    public string Scenario { get; set; } = "";
    public string ImageSha256 { get; set; } = "";
    public string HostRuntime { get; set; } = RuntimeFeature.IsDynamicCodeSupported ? "jit" : "nativeaot";
    public string HostFramework { get; set; } = RuntimeInformation.FrameworkDescription;
    public bool HostDynamicCodeCompiled { get; set; } = RuntimeFeature.IsDynamicCodeCompiled;
    public int? WorkerProcessId { get; set; }
    public bool WorkerGone { get; set; } = true;
    public bool RunDisposed { get; set; }
    public bool MachineDisposed { get; set; }
    public double ElapsedSeconds { get; set; }
    public string? Error { get; set; }
    public string? CleanupError { get; set; }
    public MachineRunResult? Result { get; set; }
}

[JsonSerializable(typeof(BitsetReport))]
[JsonSerializable(typeof(FixtureDependencies))]
internal partial class BitsetJson : JsonSerializerContext;
internal sealed record FixtureDependencies(string Images, DependencyFile[] Files);
internal sealed record DependencyFile(string GuestPath, string File, string Sha256, bool Executable);
