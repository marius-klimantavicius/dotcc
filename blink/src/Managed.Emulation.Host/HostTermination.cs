namespace Managed.Emulation.Host;

public enum HostTerminationKind { Exit, ImmediateExit, Abort }

/// <summary>An unexpected host termination call escaped the guest exit trap.
/// The owning worker must report failure and discard its complete C state.</summary>
public sealed class HostTerminationException(HostTerminationKind kind, int requestedStatus)
    : Exception($"Unexpected host {kind} request ({requestedStatus}).")
{
    public HostTerminationKind Kind { get; } = kind;
    public int RequestedStatus { get; } = requestedStatus;
}
