using static Managed.Transport.QUIC_STREAM_EVENT_TYPE;

namespace Managed.Transport.Api;

public enum QuicStreamNotificationKind
{
    Started, ReceiveOffered, SendCompleted, PeerSendClosed, PeerSendAborted,
    PeerReceiveAborted, SendClosed, Closed, IdealSendBufferSize, PeerAccepted
}

/// <summary>A copied stream event. Receive payload ownership stays with ReceiveAsync.</summary>
public sealed record QuicStreamNotification(QuicStreamNotificationKind Kind,
    uint Status = 0, ulong ErrorCode = 0, ulong StreamId = 0,
    ulong AbsoluteOffset = 0, ulong ByteCount = 0, bool Fin = false,
    bool Canceled = false, bool Graceful = false);

/// <summary>Runs synchronously inside the actual stream callback, after owner state is updated.
/// Do not block waiting for QUIC work or disposal. Inline shutdown is permitted here.
/// Exceptions fault the connection and never escape into the translated core.</summary>
public delegate void QuicStreamCallback(QuicStream stream, QuicStreamNotification notification, object? context);

public sealed partial class QuicStream
{
    private QuicStreamCallback? managedCallback;

    /// <summary>Atomically replaces the callback and its managed context. An invocation
    /// already in progress retains its previous callback/context pair until it returns.
    /// Passing null removes the observer; the owning callback remains installed.</summary>
    public void SetCallbackHandler(QuicStreamCallback? callback, object? context = null)
    {
        using var operation = EnterOperation();
        lock (Gate) { managedCallback = callback; applicationContext = context; }
    }

    private unsafe void NotifyApplication(QUIC_STREAM_EVENT* value)
    {
        QuicStreamCallback? callback;
        object? context;
        lock (Gate) { callback = managedCallback; context = applicationContext; }
        if (callback == null) return;
        // Every field is copied before user code. No generated storage, raw
        // buffer, or callback-scoped pointer crosses the public boundary.
        QuicStreamNotification? notification = value->Type switch
        {
            QUIC_STREAM_EVENT_START_COMPLETE => new(QuicStreamNotificationKind.Started,
                Status: value->START_COMPLETE.Status, StreamId: value->START_COMPLETE.ID),
            QUIC_STREAM_EVENT_RECEIVE => new(QuicStreamNotificationKind.ReceiveOffered,
                AbsoluteOffset: value->RECEIVE.AbsoluteOffset, ByteCount: value->RECEIVE.TotalBufferLength,
                Fin: (value->RECEIVE.Flags & QUIC_RECEIVE_FLAGS.QUIC_RECEIVE_FLAG_FIN) != 0),
            QUIC_STREAM_EVENT_SEND_COMPLETE => new(QuicStreamNotificationKind.SendCompleted,
                Canceled: value->SEND_COMPLETE.Canceled != 0),
            QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN => new(QuicStreamNotificationKind.PeerSendClosed),
            QUIC_STREAM_EVENT_PEER_SEND_ABORTED => new(QuicStreamNotificationKind.PeerSendAborted,
                ErrorCode: value->PEER_SEND_ABORTED.ErrorCode),
            QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED => new(QuicStreamNotificationKind.PeerReceiveAborted,
                ErrorCode: value->PEER_RECEIVE_ABORTED.ErrorCode),
            QUIC_STREAM_EVENT_SEND_SHUTDOWN_COMPLETE => new(QuicStreamNotificationKind.SendClosed,
                Graceful: value->SEND_SHUTDOWN_COMPLETE.Graceful != 0),
            QUIC_STREAM_EVENT_SHUTDOWN_COMPLETE => new(QuicStreamNotificationKind.Closed,
                Status: value->SHUTDOWN_COMPLETE.ConnectionCloseStatus),
            QUIC_STREAM_EVENT_IDEAL_SEND_BUFFER_SIZE => new(QuicStreamNotificationKind.IdealSendBufferSize,
                ByteCount: value->IDEAL_SEND_BUFFER_SIZE.ByteCount),
            QUIC_STREAM_EVENT_PEER_ACCEPTED => new(QuicStreamNotificationKind.PeerAccepted),
            _ => null
        };
        if (notification == null) return;
        try { callback(this, notification, context); }
        catch (Exception error) { RecordCallbackFailure(error); connection.Fault(error); }
    }
}
