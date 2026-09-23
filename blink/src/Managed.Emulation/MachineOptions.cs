using System.Collections.ObjectModel;
using System.Net;
using Managed.Emulation.Host;

namespace Managed.Emulation;

public enum ExecutionMode { InProcess, SeparateProcess }
public enum MachineState { Created, Running, Exited, Disposed }
public enum MountAccess { ReadWrite, ReadOnly, CopyOnWrite }
public enum RunExitReason { Exited, Stopped, Deadline, InstructionLimit, OutputLimit, GuestSignal, ExecutionFailure, Killed }
public enum ReadinessKind { Started, ListeningPorts, OutputMarker }
public enum ConsoleInterruptPolicy { None, Stop }
public sealed record MachineCapabilities(ExecutionMode ExecutionMode, bool CooperativeStop,
    bool ForceTermination, bool HardenedSandbox, bool PersistentHostUnixModes,
    bool HostWideResourceEnforcement);

public sealed record PortPublication(ushort GuestPort, string HostAddress = "127.0.0.1", int HostPort = 0);
public sealed record OutboundDestination(string Address, ushort Port);
public sealed record NetworkPolicy
{
    public static NetworkPolicy Isolated { get; } = new();
    public PortPublication[] Publications { get; init; } = [];
    public OutboundDestination[] OutboundDestinations { get; init; } = [];
    internal NetworkPolicy Freeze()
    {
        if (Publications == null || Publications.Length > 32 || OutboundDestinations == null)
            throw new ArgumentException("Invalid network policy.");
        if (OutboundDestinations.Length > 128 || OutboundDestinations.Any(p => p == null || p.Port == 0 ||
            !IPAddress.TryParse(p.Address, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork))
            throw new ArgumentException("Outbound grants require numeric IPv4 addresses and nonzero ports.");
        if (Publications.Any(p => p == null || p.GuestPort == 0 || p.HostPort is < 0 or > 65535 ||
            !IPAddress.TryParse(p.HostAddress, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) ||
            Publications.Select(p => p.GuestPort).Distinct().Count() != Publications.Length)
            throw new ArgumentException("Publications require unique guest ports and explicit IPv4 host addresses.");
        return this with { Publications = Publications.ToArray(), OutboundDestinations = OutboundDestinations.ToArray() };
    }
}
public sealed record MachineOptions
{
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.InProcess;
    public long MemoryLimit { get; init; } = 64L * 1024 * 1024;
    public long WritableStorageLimit { get; init; } = 16L * 1024 * 1024;
    public int FileNodeLimit { get; init; } = 1024;
    public int DescriptorLimit { get; init; } = 128;
    public int ThreadLimit { get; init; } = 16;
    public int PipeBufferBytes { get; init; } = 65536;
    public long PipeStorageLimit { get; init; } = 1048576;
    public int PendingPipeOperations { get; init; } = 128;
    public long? InstructionLimit { get; init; } = 100_000_000;
    public TimeSpan? ExecutionDeadline { get; init; }
    public IReadOnlyDictionary<string, string> Environment { get; init; } = new Dictionary<string, string>();
    public NetworkPolicy Network { get; init; } = NetworkPolicy.Isolated;
    /// <summary>Optional private IMDSv2 endpoint at guest 169.254.169.254:80.</summary>
    public ImdsV2Options? Metadata { get; init; }
    public WorkerLaunch? Worker { get; init; }

    internal MachineOptions Freeze()
    {
        if (!Enum.IsDefined(ExecutionMode) || MemoryLimit is < 8192 or > 268435456 || MemoryLimit % 4096 != 0 ||
            WritableStorageLimit is < 0 or > int.MaxValue || FileNodeLimit is < 1 or > 65536 || DescriptorLimit is < 3 or > 4096 ||
            ThreadLimit is < 1 or > 256 || PipeBufferBytes is < 1 or > 1048576 || PipeStorageLimit < PipeBufferBytes ||
            PendingPipeOperations is < 1 or > 4096 || InstructionLimit is < 1 || ExecutionDeadline is { } deadline && deadline <= TimeSpan.Zero)
            throw new ArgumentException("A requested resource limit is outside the supported range.");
        if (Environment == null || Network == null) throw new ArgumentException("Machine configuration is incomplete.");
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Environment) { GuestText.EnvironmentEntry(pair.Key, pair.Value); environment.Add(pair.Key, pair.Value); }
        GuestText.EnvironmentSize(environment);
        if (ExecutionMode == ExecutionMode.InProcess && Worker != null)
            throw new ArgumentException("Worker location applies only to explicit separate-process execution.");
        return this with { Environment = new ReadOnlyDictionary<string, string>(environment), Network = Network.Freeze(),
            Metadata = Metadata?.Snapshot(),
            Worker = Worker == null ? null : Worker with { Arguments = Worker.Arguments?.ToArray() ?? throw new ArgumentException("Missing worker arguments.") } };
    }
}
public sealed record ReadinessOptions
{
    public ReadinessKind Kind { get; init; } = ReadinessKind.Started;
    public byte[] OutputMarker { get; init; } = [];
    public TimeSpan? Timeout { get; init; }
}
public sealed record ConsoleOptions
{
    public Stream? Input { get; init; }
    public Stream? Output { get; init; }
    public Stream? Error { get; init; }
    public bool LeaveOpen { get; init; } = true;
    public bool RedirectInput { get; init; }
    public bool RedirectOutput { get; init; }
    public int BufferBytes { get; init; } = 65536;
    public int CaptureBytes { get; init; } = 65536;
    public long OutputLimit { get; init; } = 1048576;
    public TimeSpan DrainTimeout { get; init; } = TimeSpan.FromSeconds(1);
    public ConsoleInterruptPolicy InterruptPolicy { get; init; }
    public static ConsoleOptions AttachCurrent(bool leaveOpen = true, ConsoleInterruptPolicy interrupt = ConsoleInterruptPolicy.Stop)
        => new() { Input = System.Console.OpenStandardInput(), Output = System.Console.OpenStandardOutput(),
            Error = System.Console.OpenStandardError(), LeaveOpen = leaveOpen, InterruptPolicy = interrupt };
    internal ConsoleOptions Freeze()
    {
        if (BufferBytes is < 1 or > 1048576 || CaptureBytes is < 0 or > 1048576 || OutputLimit < 0 ||
            DrainTimeout < TimeSpan.Zero || DrainTimeout > TimeSpan.FromMinutes(1) ||
            Input is { CanRead: false } || Output is { CanWrite: false } || Error is { CanWrite: false } ||
            Input != null && RedirectInput || RedirectOutput && (Output != null || Error != null) || !Enum.IsDefined(InterruptPolicy))
            throw new ArgumentException("Invalid console configuration.");
        return this with { };
    }
}
public sealed record ExecutionOptions
{
    public required string Executable { get; init; }
    /// <summary>Arguments after argv[0]. The executable path is supplied once.</summary>
    public string[] Arguments { get; init; } = [];
    public string WorkingDirectory { get; init; } = "/";
    public IReadOnlyDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();
    public ConsoleOptions Console { get; init; } = new();
    public ReadinessOptions Readiness { get; init; } = new();
    internal ExecutionOptions Freeze()
    {
        GuestText.Path(Executable); GuestText.Path(WorkingDirectory);
        if (Arguments == null || Arguments.Length > 256 || Environment == null || Console == null || Readiness == null)
            throw new ArgumentException("Execution configuration is incomplete.");
        foreach (string argument in Arguments) GuestText.Value(argument);
        var environment = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var pair in Environment) { GuestText.EnvironmentEntry(pair.Key, pair.Value ?? ""); environment.Add(pair.Key, pair.Value); }
        if (!Enum.IsDefined(Readiness.Kind) || Readiness.OutputMarker == null || Readiness.OutputMarker.Length > 4096 ||
            Readiness.Kind == ReadinessKind.OutputMarker && Readiness.OutputMarker.Length == 0 ||
            Readiness.Timeout is { } timeout && timeout <= TimeSpan.Zero)
            throw new ArgumentException("Invalid readiness condition.");
        return this with { Arguments = Arguments.ToArray(), Environment = new ReadOnlyDictionary<string, string?>(environment),
            Console = Console.Freeze(), Readiness = Readiness with { OutputMarker = Readiness.OutputMarker.ToArray() } };
    }
}
public sealed record MachineRunResult(RunExitReason Reason, int ExitCode, int Signal, int Halt,
    long Instructions, byte[] StandardOutput, byte[] StandardError, bool CaptureTruncated,
    long OutputBytes, bool ResourcesReleased, string? Diagnostic);
public sealed class MachineStopTimeoutException(string message) : TimeoutException(message);

internal static class GuestText
{
    internal static void Value(string value)
    {
        if (value == null || value.Length > 4096 || value.Contains('\0')) throw new ArgumentException("Invalid guest string.");
        _ = new System.Text.UTF8Encoding(false, true).GetByteCount(value);
    }
    internal static void Path(string value)
    {
        Value(value);
        if (!value.StartsWith('/') || value.Contains("//", StringComparison.Ordinal) ||
            value.Length > 1 && value.EndsWith('/') || value.Split('/').Any(p => p is "." or ".."))
            throw new ArgumentException("An absolute normalized guest path is required.");
    }
    internal static void EnvironmentEntry(string name, string value)
    {
        Value(name); Value(value);
        if (name.Length == 0 || name.Contains('=')) throw new ArgumentException("Invalid environment name.");
    }
    internal static void EnvironmentSize(IReadOnlyDictionary<string, string> environment)
    {
        if (environment.Count > 256 || environment.Sum(p => System.Text.Encoding.UTF8.GetByteCount(p.Key) + System.Text.Encoding.UTF8.GetByteCount(p.Value) + 2) > 65536)
            throw new ArgumentException("Guest environment exceeds its byte or entry bound.");
    }
}
