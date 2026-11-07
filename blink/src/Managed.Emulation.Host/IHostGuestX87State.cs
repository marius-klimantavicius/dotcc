using System;

namespace Managed.Emulation.Host;

// ProducerProtocol1. Values are a wire contract, not instruction dispatch IDs.
public enum HostX87Transfer : int
{
    LinuxSave = 1, LinuxValidate = 2, LinuxRestore = 3, LinuxReset = 4,
    Fx64Save = 5, Fx64Restore = 6, FxLong32Save = 7, FxLong32Restore = 8,
    FxLegacySave = 9, FxLegacyRestore = 10,
    Env14Save = 11, Env14Restore = 12, Env28Save = 13, Env28Restore = 14,
    Save94 = 15, Restore94 = 16, Save108 = 17, Restore108 = 18,
    CpuReset = 19, Finit = 20, EnvironmentSaved = 21, LinuxHandlerEnter = 22,
}

public enum HostX87TransferResult : int { Inactive = 0, Complete = 1, BadImage = -1 }

public static class HostX87StateProtocol
{
    public const uint Version = 1;
    public static int ImageSize(HostX87Transfer operation) => operation switch
    {
        HostX87Transfer.LinuxSave or HostX87Transfer.LinuxValidate or HostX87Transfer.LinuxRestore or
        HostX87Transfer.Fx64Save or HostX87Transfer.Fx64Restore or HostX87Transfer.FxLong32Save or
        HostX87Transfer.FxLong32Restore or HostX87Transfer.FxLegacySave or HostX87Transfer.FxLegacyRestore => 512,
        HostX87Transfer.Env14Save or HostX87Transfer.Env14Restore => 14,
        HostX87Transfer.Env28Save or HostX87Transfer.Env28Restore => 28,
        HostX87Transfer.Save94 or HostX87Transfer.Restore94 => 94,
        HostX87Transfer.Save108 or HostX87Transfer.Restore108 => 108,
        HostX87Transfer.CpuReset or HostX87Transfer.Finit or HostX87Transfer.LinuxReset or
        HostX87Transfer.EnvironmentSaved or HostX87Transfer.LinuxHandlerEnter => 0,
        _ => throw new InvalidOperationException("unknown x87 producer operation")
    };
}

/// <summary>Copy-only bridge at a fully returned native boundary. Implementer
/// must validate exact current Machine, owning worker/generation and lifetime,
/// including recursive signal entry. No async fault-stack callback is allowed.</summary>
public interface IHostGuestX87State
{
    // Null means inactive only. An active ownership/protocol mismatch must throw.
    // Pointer remains valid through all native frames; no movable managed image.
    nint GetX87RegisterFile(nint machine, uint producerProtocol);
    // image is an owned host staging slice, NEVER a guest address or user-store
    // alias. Length is exact for the operation. BadImage must change no state.
    HostX87TransferResult TransferX87State(nint machine, uint producerProtocol,
        HostX87Transfer operation, Span<byte> image);
    // Tripwire for compatibility helpers. Active must throw before any legacy
    // f64 read/write or MMX use of XMM storage; inactive is a no-op.
    void RejectLegacyX87State(nint machine, uint producerProtocol);
}
