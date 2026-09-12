namespace Managed.Transport.Api;

public sealed class QuicTransportException : IOException
{
    public uint Status { get; }
    public ulong? TransportError { get; }
    public ushort? TlsAlert => TransportError is >= 0x100 and <= 0x1ff ? (ushort)(TransportError.Value - 0x100) : null;
    public QuicTransportException(string operation, uint status, ulong? transportError = null)
        : base($"{operation} failed with QUIC status {status}" + (transportError is null ? "." : $" and transport error 0x{transportError:x}."))
    { Status = status; TransportError = transportError; }
}

internal static class QuicError
{
    internal static bool Failed(uint status) => unchecked((int)status) > 0;
    internal static void ThrowIfFailed(uint status, string operation)
    { if (Failed(status)) throw FromStatus(status, operation); }
    internal static Exception FromStatus(uint status, string operation) => status switch
    {
        95 => new NotSupportedException(operation + " is outside the qualified QUIC profile."),
        22 => new ArgumentException(operation + " received an invalid argument."),
        12 => new OutOfMemoryException(operation),
        _ => new QuicTransportException(operation, status)
    };
    internal static void ErrorCode(ulong error)
    { if (error > (1UL << 62) - 1) throw new ArgumentOutOfRangeException(nameof(error)); }
}

[Flags]
public enum QuicConnectionShutdownOptions : uint { None = 0, Silent = 1 }
[Flags]
public enum QuicStreamOpenOptions : uint { None = 0, Unidirectional = 1, DelayIdFlowControlUpdates = 4 }
[Flags]
public enum QuicStreamStartOptions : uint { None = 0, Immediate = 1, FailBlocked = 2, ShutdownOnFail = 4, IndicatePeerAccept = 8 }
[Flags]
public enum QuicSendOptions : uint { None = 0, Start = 2, Fin = 4, DelaySend = 16 }
public sealed record QuicCloseInfo(uint Status, ulong ErrorCode, bool PeerInitiated, bool ApplicationInitiated);
