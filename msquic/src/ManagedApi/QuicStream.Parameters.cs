using static Managed.Transport.MsQuic;
namespace Managed.Transport.Api;

[Flags]
public enum QuicStreamShutdownOptions : uint
{
    None = 0, Graceful = 1, AbortSend = 2, AbortReceive = 4,
    Abort = AbortSend | AbortReceive, Immediate = 8, Inline = 16
}

/// <summary>Actual accumulated blocking durations, in microseconds.</summary>
public sealed record QuicStreamStatistics(ulong ConnectionScheduling, ulong ConnectionPacing,
    ulong ConnectionAmplificationProtection, ulong ConnectionCongestionControl, ulong ConnectionFlowControl,
    ulong StreamIdFlowControl, ulong StreamFlowControl, ulong Application);

public sealed partial class QuicStream
{
    [ThreadStatic] private static QuicStream? callbackOwner;
    private ulong? idealSendBufferRecommendation;

    /// <summary>The last copied recommendation callback, or null before one is observed.</summary>
    public ulong? LastIdealSendBufferRecommendation { get { lock (Gate) return idealSendBufferRecommendation; } }

    public unsafe ushort GetPriority(QuicParameterPriority priority = QuicParameterPriority.Normal)
    { using var operation = EnterOperation(); return ReadStreamParameter<ushort>(MsQuic.QUIC_PARAM_STREAM_PRIORITY, priority); }
    public unsafe void SetPriority(ushort priority, QuicParameterPriority parameterPriority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        QuicError.ThrowIfFailed(Runtime.Api->SetParam(Handle, QuicParameterDispatch.Apply(MsQuic.QUIC_PARAM_STREAM_PRIORITY, parameterPriority),
            sizeof(ushort), &priority), "Set stream priority");
    }
    public unsafe ulong GetIdealSendBufferSize(QuicParameterPriority priority = QuicParameterPriority.Normal)
    { using var operation = EnterOperation(); return ReadStreamParameter<ulong>(MsQuic.QUIC_PARAM_STREAM_IDEAL_SEND_BUFFER_SIZE, priority); }
    /// <summary>Queries actual early-data bytes after local close acknowledgment.
    /// This does not enable early data; the selected profile requires zero.</summary>
    public unsafe ulong GetObservedEarlyDataLength(QuicParameterPriority priority = QuicParameterPriority.Normal)
    { using var operation = EnterOperation(); return ReadStreamParameter<ulong>(MsQuic.QUIC_PARAM_STREAM_0RTT_LENGTH, priority); }
    public unsafe QuicStreamStatistics GetStatistics(QuicParameterPriority priority = QuicParameterPriority.Normal)
    {
        using var operation = EnterOperation();
        var value = ReadStreamParameter<QUIC_STREAM_STATISTICS>(MsQuic.QUIC_PARAM_STREAM_STATISTICS, priority);
        return new(value.ConnBlockedBySchedulingUs, value.ConnBlockedByPacingUs,
            value.ConnBlockedByAmplificationProtUs, value.ConnBlockedByCongestionControlUs,
            value.ConnBlockedByFlowControlUs, value.StreamBlockedByIdFlowControlUs,
            value.StreamBlockedByFlowControlUs, value.StreamBlockedByAppUs);
    }
    private unsafe T ReadStreamParameter<T>(uint parameter, QuicParameterPriority priority) where T : unmanaged
    {
        T value = default; uint length = (uint)sizeof(T);
        QuicError.ThrowIfFailed(Runtime.Api->GetParam(Handle, QuicParameterDispatch.Apply(parameter, priority), &length, &value), "Read stream parameter");
        if (length != sizeof(T)) throw new InvalidOperationException("Unexpected stream parameter size");
        return value;
    }

    /// <summary>Admits a native shutdown request. Completion remains asynchronous;
    /// CompleteWritesAsync observes graceful send completion. Inline is valid only
    /// while this stream's actual native callback is executing on the caller thread.</summary>
    public unsafe void Shutdown(QuicStreamShutdownOptions options, ulong applicationError = 0)
    {
        uint bits = (uint)options;
        if ((bits & ~31u) != 0 || bits == 0)
            throw new ArgumentException("Unknown or empty stream shutdown flags", nameof(options));
        QuicError.ErrorCode(applicationError);
        if ((bits & 1) != 0 && (bits & 14) != 0)
            throw new ArgumentException("Graceful shutdown cannot be combined with abort or immediate", nameof(options));
        if ((bits & 8) != 0 && bits != 14)
            throw new ArgumentException("Immediate shutdown requires exactly both abort directions", nameof(options));
        if ((bits & 16) != 0 && !ReferenceEquals(callbackOwner, this))
            throw new InvalidOperationException("Inline shutdown requires this stream's native callback context");
        if ((bits & 1) != 0 && !canWrite)
            throw new InvalidOperationException("This unidirectional stream cannot send");
        using var operation = EnterOperation();
        QuicError.ThrowIfFailed(Runtime.Api->StreamShutdown(Handle, (QUIC_STREAM_SHUTDOWN_FLAGS)bits,
            applicationError), "StreamShutdown");
        lock (Gate)
        {
            if ((bits & 1) != 0) finSubmitted = true;
            if ((bits & 2) != 0)
            {
                writeEnded = true; writeError = new OperationCanceledException("QUIC send aborted");
                writeShutdown.TrySetException(writeError);
            }
            if ((bits & 4) != 0)
            {
                readEnded = true; readError = new OperationCanceledException("QUIC receive aborted");
                receiveWaiter?.TrySetException(readError); receiveWaiter = null;
            }
        }
    }
}
