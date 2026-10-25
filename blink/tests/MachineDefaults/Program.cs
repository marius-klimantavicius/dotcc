using System.Text.Json;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value, string message) { if (!value) throw new Exception(message); }
static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception("Host result: " + result.Error);
static void Invalid(Action action)
{
    try { action(); } catch (ArgumentException) { return; }
    throw new Exception("Invalid configuration was accepted");
}
static void Defaults(MachineOptions options)
{
    Check(options.MemoryLimit == 256L << 20 && options.WritableStorageLimit == 512L << 20 &&
        options.FileNodeLimit == 8192 && options.DescriptorLimit == 1024 && options.ThreadLimit == 64,
        "service resource defaults");
    Check(options.InstructionLimit == null && options.ExecutionDeadline == null, "service lifetime defaults");
    Check(options.PipeBufferBytes == 65536 && options.PipeStorageLimit == 1048576 && options.PendingPipeOperations == 128,
        "bounded pipe defaults");
}
static async Task<MachineStart> RoundTrip(MachineOptions options, ConsoleOptions console, bool allowElfInterpreter = false)
{
    options = options.Freeze(); console = console.Freeze();
    // ProcessMachineRun converts the mode to InProcess for the worker; all
    // resource values travel in this same source-generated start-v1 frame.
    var start = new MachineStart(options with { ExecutionMode = ExecutionMode.InProcess, Worker = null },
        new("/bin/service", [], "/", [], new(), console.BufferBytes, console.OutputLimit, console.CaptureBytes, console.DrainTimeout,
            allowElfInterpreter),
        new("/private-root", []));
    using var wire = new MemoryStream();
    await InstanceProtocol.WriteAsync(wire, new MachineFrame("start-v1", Start: start), MachineJson.Default.MachineFrame);
    wire.Position = 0;
    var result = await InstanceProtocol.ReadAsync(wire, MachineJson.Default.MachineFrame);
    Check(result?.Kind == "start-v1" && result.Start != null, "machine frame round trip");
    return result!.Start!;
}

Defaults(new MachineOptions());
// start-v1 sends every option. Source-generated init-only deserialization
// zeroes omitted fields, which must fail validation rather than weaken limits.
Invalid(() => JsonSerializer.Deserialize("{}", MachineJson.Default.MachineOptions)!.Freeze());
foreach (var mode in new[] { ExecutionMode.InProcess, ExecutionMode.SeparateProcess })
{
    var options = new MachineOptions { ExecutionMode = mode }.Freeze(); Defaults(options);
    var execution = new ExecutionOptions { Executable = "/bin/service" }.Freeze();
    Check(!execution.AllowElfInterpreter, "ELF interpreter defaults to disabled after freeze");
    Check((execution with { AllowElfInterpreter = true }).Freeze().AllowElfInterpreter, "explicit ELF interpreter opt-in survives freeze");
    var console = execution.Console;
    Check(console.OutputLimit == long.MaxValue && console.CaptureBytes == 65536 && console.BufferBytes == 65536,
        "bounded console retention with lifetime counter ceiling");
    var start = await RoundTrip(options, console); Defaults(start.Options.Freeze());
    Check(!start.Execution.AllowElfInterpreter, "worker default remains static-only");
    Check((await RoundTrip(options, console, true)).Execution.AllowElfInterpreter, "worker framing retains explicit interpreter opt-in");
    Check(start.Execution.OutputLimit == long.MaxValue && start.Execution.CaptureBytes == 65536 && start.Execution.BufferBytes == 65536,
        "worker console defaults survive source-generated framing");
}
var low = new MachineOptions { MemoryLimit = 65536, WritableStorageLimit = 32, FileNodeLimit = 4,
    DescriptorLimit = 8, ThreadLimit = 2, PipeBufferBytes = 8, PipeStorageLimit = 32,
    PendingPipeOperations = 4, InstructionLimit = 123, ExecutionDeadline = TimeSpan.FromSeconds(5) }.Freeze();
var lowConsole = new ConsoleOptions { BufferBytes = 4, CaptureBytes = 3, OutputLimit = 9 }.Freeze();
var custom = await RoundTrip(low, lowConsole);
Check(custom.Options.Freeze() with { Environment = low.Environment, Network = low.Network } == low &&
    custom.Execution.BufferBytes == 4 && custom.Execution.CaptureBytes == 3 && custom.Execution.OutputLimit == 9,
    "explicit small options survive framing without default replacement");
Invalid(() => (low with { MemoryLimit = (256L << 20) + 4096 }).Freeze());
Invalid(() => (low with { MemoryLimit = 8193 }).Freeze());
Invalid(() => (low with { InstructionLimit = 0 }).Freeze());
Invalid(() => (low with { ThreadLimit = 257 }).Freeze());
Invalid(() => (low with { DescriptorLimit = 4097 }).Freeze());
Invalid(() => (low with { FileNodeLimit = 65537 }).Freeze());
Invalid(() => (low with { WritableStorageLimit = (long)int.MaxValue + 1 }).Freeze());
Invalid(() => (lowConsole with { OutputLimit = -1 }).Freeze());
Invalid(() => (lowConsole with { CaptureBytes = 1048577 }).Freeze());

var defaults = new MachineOptions().Freeze();
long allocated = GC.GetAllocatedBytesForCurrentThread();
using (var fs = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(),
    defaults.WritableStorageLimit, defaults.DescriptorLimit, nodeLimit: defaults.FileNodeLimit))
{
    Check(GC.GetAllocatedBytesForCurrentThread() - allocated < 1 << 20, "storage quota is not preallocated");
    await using var io = new InstanceIo(fs, descriptorLimit: defaults.DescriptorLimit);
    for (int i = 0; i < 140; ++i) Value(io.OpenFile("/shared", FileAccessMode.Read | FileAccessMode.Write, create: true));
    Check(io.OpenDescriptors == 143, "descriptor policy supports more than former 128 default");
    for (int i = 0; i < 1030; ++i)
    {
        int fd = Value(fs.Open("/node-" + i, FileAccessMode.Write, create: true)); Value(fs.Close(fd));
    }
    Check(fs.NodeCount == 1032, "node policy supports more than former 1024 default");
}
using (var fs = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(),
    low.WritableStorageLimit, low.DescriptorLimit, nodeLimit: low.FileNodeLimit))
{
    await using var io = new InstanceIo(fs, descriptorLimit: low.DescriptorLimit);
    for (int i = 0; i < 5; ++i) Value(io.OpenFile("/shared", FileAccessMode.Write, create: true));
    Check(io.OpenFile("/shared", FileAccessMode.Write).Error == GuestError.TooManyFiles, "custom low descriptor bound remains effective");
    int fd = Value(fs.Open("/data", FileAccessMode.Write, create: true));
    Check(Value(fs.Write(fd, new byte[40])) == 32 && fs.Write(fd, new byte[1]).Error == GuestError.NoSpace,
        "custom low storage bound remains effective");
    Value(fs.Close(fd));
}

await using (var run = new ConsoleProbe(new ExecutionOptions { Executable = "/unused" }))
{
    byte[] chunk = new byte[65536];
    for (int i = 0; i < 20; ++i) Check(Value(await run.Emit(i % 2 != 0, chunk)) == chunk.Length, "machine console passes former lifetime output limit");
    var result = await run.Complete();
    Check(result.OutputBytes == 1310720 && result.StandardOutput.Length + result.StandardError.Length == 65536 &&
        result.CaptureTruncated && result.Reason == RunExitReason.Exited, "machine console retains bounded capture without lifetime termination");
}
await using (var run = new ConsoleProbe(new ExecutionOptions { Executable = "/unused", Console = lowConsole }))
{
    Check(Value(await run.Emit(false, new byte[12])) == 9 && (await run.Emit(true, new byte[1])).Error == GuestError.NoSpace,
        "machine console preserves explicit low output limit");
    var result = await run.Complete();
    Check(result.OutputBytes == 9 && result.StandardOutput.Length == 3 && result.CaptureTruncated, "custom capture remains bounded");
}
Console.WriteLine("machine default contracts: PASS (no translated guest execution)");

sealed class ConsoleProbe : MachineRun
{
    internal ConsoleProbe(ExecutionOptions options) : base(options.Freeze()) { StartConsole(); }
    public override ExecutionMode ExecutionMode => ExecutionMode.InProcess;
    public override int? WorkerProcessId => null;
    internal Task<HostResult<int>> Emit(bool error, byte[] bytes) => Console.WriteOutputAsync(error, bytes);
    internal async Task<MachineRunResult> Complete()
    {
        await FinishAsync(new(RunExitReason.Exited, 0, 0, 0, 0, [], [], false, 0, true, null));
        return await Completion;
    }
    protected override void RequestStop() => Console.Stop();
    public override Task<MachineRunResult> KillAsync(CancellationToken cancellation = default) => throw new NotSupportedException();
    protected override ValueTask DisposeExecutionAsync() => ValueTask.CompletedTask;
}
