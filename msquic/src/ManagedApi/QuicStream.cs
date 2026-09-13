using static Managed.Transport.MsQuic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Transport;
using static Managed.Transport.MsQuic.QUIC_STREAM_EVENT_TYPE;

namespace Managed.Transport.Api;

/// <summary>A peer application aborted one stream direction.</summary>
public sealed class QuicStreamAbortedException : System.IO.IOException
{
    public ulong ApplicationErrorCode { get; }
    internal QuicStreamAbortedException(string operation, ulong applicationErrorCode)
        : base(operation + " (application error " + applicationErrorCode + ")") => ApplicationErrorCode = applicationErrorCode;
}

/// <summary>An owned QUIC stream with independent read and write shutdown.</summary>
public sealed partial class QuicStream : QuicObject
{
    private readonly QuicConnection connection;
    private readonly bool canRead, canWrite;
    private readonly object receiveCalls = new();
    private readonly SemaphoreSlim sendAdmission = new(1, 1);
    private readonly Dictionary<nint, QuicSendOperation> sends = new();
    private readonly TaskCompletionSource started = NewCompletion();
    private readonly TaskCompletionSource writeShutdown = NewCompletion();
    private readonly TaskCompletionSource shutdown = NewCompletion();
    private TaskCompletionSource? receiveDrain, callbacksDrain;
    private Task? completeWrites;
    private TaskCompletionSource<QuicReceiveLease?>? receiveWaiter;
    private QuicReceiveLease? receiveOffer;
    private bool offerDelivered, startRequested, readEnded, writeEnded, finSubmitted, registered, peerFinObserved;
    private Exception? readError, writeError;
    private int receiveCompletions, callbacks;
    private ulong id;

    private QuicStream(QuicConnection connection, bool unidirectional, bool remote) : base(connection.Runtime)
    {
        this.connection = connection;
        canRead = !unidirectional || remote; canWrite = !unidirectional || !remote;
        readEnded = !canRead; writeEnded = !canWrite;
        if (!canWrite) writeShutdown.TrySetResult();
    }
    private static TaskCompletionSource NewCompletion() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool CanRead => canRead;
    public bool CanWrite => canWrite;
    public ulong Id { get { lock (Gate) { if (!started.Task.IsCompletedSuccessfully) throw new InvalidOperationException("Stream has not started"); return id; } } }

    internal static async ValueTask<QuicStream> CreateAsync(QuicConnection owner, QuicStreamOpenOptions openOptions,
        CancellationToken cancellationToken)
    {
        ValidateFlags((uint)openOptions, 5, 15, nameof(openOptions));
        cancellationToken.ThrowIfCancellationRequested();
        var stream = new QuicStream(owner, ((uint)openOptions & 1) != 0, remote: false);
        try
        {
            using (var initialization = stream.EnterOperation()) stream.OpenCore(openOptions);
            cancellationToken.ThrowIfCancellationRequested();
            return stream;
        }
        catch { await stream.DisposeAsync().ConfigureAwait(false); throw; }
    }
    internal static async ValueTask<QuicStream> OpenAsync(QuicConnection owner, QuicStreamOpenOptions openOptions,
        QuicStreamStartOptions startOptions, CancellationToken cancellationToken)
    {
        ValidateFlags((uint)startOptions, 15, 31, nameof(startOptions));
        var stream = await CreateAsync(owner, openOptions, cancellationToken).ConfigureAwait(false);
        try
        {
            await stream.StartAsync(startOptions, cancellationToken).ConfigureAwait(false);
            return stream;
        }
        catch { await stream.DisposeAsync().ConfigureAwait(false); throw; }
    }
    private unsafe void OpenCore(QuicStreamOpenOptions options)
    {
        using var admission = connection.AdmitStream(this); registered = true;
        QUIC_HANDLE* opened = null;
        uint status = Runtime.Api->StreamOpen(connection.StreamParentHandle, (QUIC_STREAM_OPEN_FLAGS)options, &Callback, Context, &opened);
        Handle = opened;
        QuicError.ThrowIfFailed(status, "StreamOpen");
        if (opened == null) throw new InvalidOperationException("StreamOpen returned no handle");
    }
    internal static unsafe QuicStream Accept(QuicConnection owner, QUIC_HANDLE* handle, QUIC_STREAM_OPEN_FLAGS flags)
    {
        if (handle == null) throw new ArgumentNullException(nameof(handle));
        ValidateFlags((uint)flags, 5, 15, nameof(flags));
        var stream = new QuicStream(owner, ((uint)flags & 1) != 0, remote: true);
        try
        {
            using var admission = owner.AdmitStream(stream); stream.registered = true;
            uint length = sizeof(ulong); ulong value = 0;
            QuicError.ThrowIfFailed(owner.Runtime.Api->GetParam(handle, MsQuic.QUIC_PARAM_STREAM_ID, &length, &value), "accepted stream ID");
            stream.id = value; stream.startRequested = true;
            void* callbackContext = stream.Context;
            // No fallible setup follows this ownership commitment. Before this
            // point, a failed PEER_STREAM_STARTED status makes C close the raw
            // stream. Afterward the connection callback must return success.
            stream.Handle = handle;
            owner.Runtime.Api->SetCallbackHandler(handle, (void*)(delegate*<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, uint>)&Callback, callbackContext);
            stream.started.TrySetResult();
            return stream;
        }
        catch (Exception error)
        {
            if (stream.Handle != null)
            {
                // A managed exception after commitment cannot transfer the
                // handle back to C; retain ownership and fail the connection.
                stream.RecordCallbackFailure(error); owner.Fault(error);
                return stream;
            }
            _ = stream.DisposeAsync(); throw;
        }
    }

    public ValueTask StartAsync(QuicStreamStartOptions options = 0, CancellationToken cancellationToken = default)
    {
        ValidateFlags((uint)options, 15, 31, nameof(options)); cancellationToken.ThrowIfCancellationRequested();
        StartCore(options);
        return new ValueTask(started.Task.WaitAsync(cancellationToken));
    }
    private unsafe void StartCore(QuicStreamStartOptions options)
    {
        using var operation = EnterOperation();
        lock (Gate)
        {
            if (startRequested) throw new InvalidOperationException("Stream start already requested");
            startRequested = true;
        }
        uint status = Runtime.Api->StreamStart(Handle, (QUIC_STREAM_START_FLAGS)options);
        if (Failed(status)) { var error = QuicError.FromStatus(status, "StreamStart"); started.TrySetException(error); throw error; }
    }

    /// <summary>Copies the input for transport ownership. Cancellation after admission cancels the wait; it does not retract sent bytes.</summary>
    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, QuicSendOptions options = 0, CancellationToken cancellationToken = default)
    {
        if (((uint)options & 8u) != 0) throw new ArgumentException("Datagram priority is not a stream-send option", nameof(options));
        ValidateFlags((uint)options, 22, 255, nameof(options));
        if (buffer.Length > Runtime.Options.MaximumCopiedSendBytes) throw new ArgumentOutOfRangeException(nameof(buffer), "Send exceeds the configured copied-send limit");
        if (!canWrite) throw new InvalidOperationException("This unidirectional stream cannot send");
        await sendAdmission.WaitAsync(cancellationToken).ConfigureAwait(false);
        QuicSendOperation? send = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            send = SubmitSend(buffer, options);
        }
        finally { if (send == null) sendAdmission.Release(); }
        await send.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    private unsafe QuicSendOperation SubmitSend(ReadOnlyMemory<byte> buffer, QuicSendOptions options)
    {
        using var operation = EnterOperation();
        lock (Gate)
        {
            if (writeEnded || finSubmitted) throw writeError ?? new InvalidOperationException("Write direction is closed");
        }
        if (!connection.TryReserveBufferBytes(Math.Max(1, buffer.Length), out var budget))
            throw QuicError.FromStatus(12, "stream send buffer budget");
        QuicSendOperation send;
        bool fin = ((uint)options & 4) != 0;
        bool startsStream = false;
        try
        {
            send = new QuicSendOperation(buffer, budget!);
            lock (Gate)
            {
                if (writeEnded || finSubmitted) throw writeError ?? new InvalidOperationException("Write direction is closed");
                sends.Add(send.Token, send); if (fin) finSubmitted = true;
                startsStream = ((uint)options & 2) != 0 && !startRequested;
                if (startsStream) startRequested = true;
            }
        }
        catch { budget!.Dispose(); throw; }
        uint status;
        try { status = Runtime.Api->StreamSend(Handle, send.Buffers, 1, (QUIC_SEND_FLAGS)options, (void*)send.Token); }
        catch (Exception error)
        {
            // An unexpected exception gives no proof that C rejected ownership.
            // Retain buffers until callback or raw close, and fault the connection.
            send.Completion.TrySetException(error); connection.Fault(error); return send;
        }
        if (Failed(status))
        {
            lock (Gate) { if (fin) finSubmitted = false; if (startsStream) startRequested = false; }
            FinishSend(send.Token, QuicError.FromStatus(status, "StreamSend"));
        }
        return send;
    }
    private void FinishSend(nint token, Exception? error)
    {
        QuicSendOperation send;
        lock (Gate)
        {
            if (!sends.Remove(token, out send!)) throw new InvalidOperationException("Unknown or repeated stream send completion");
        }
        if (!send.Finish(error)) throw new InvalidOperationException("Stream send completed twice");
        sendAdmission.Release();
    }

    /// <summary>Requests FIN once. Cancellation cancels the wait after this shutdown request is admitted.</summary>
    public ValueTask CompleteWritesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!canWrite) throw new InvalidOperationException("This unidirectional stream cannot send");
        using var operation = EnterOperation();
        Task completion;
        lock (Gate) completion = completeWrites ??= Task.Run(FinishWritesAsync);
        return new ValueTask(completion.WaitAsync(cancellationToken));
    }
    private async Task FinishWritesAsync()
    {
        await sendAdmission.WaitAsync().ConfigureAwait(false);
        QuicSendOperation? send = null;
        try
        {
            bool needsFin;
            lock (Gate) needsFin = !finSubmitted && !writeEnded;
            if (needsFin) send = SubmitSend(ReadOnlyMemory<byte>.Empty, QuicSendOptions.Fin);
        }
        finally { if (send == null) sendAdmission.Release(); }
        if (send != null) await send.Completion.Task.ConfigureAwait(false);
        await writeShutdown.Task.ConfigureAwait(false);
    }
    public void AbortRead(ulong errorCode) => AbortDirection(errorCode, read: true);
    public void AbortWrite(ulong errorCode) => AbortDirection(errorCode, read: false);
    private unsafe void AbortDirection(ulong errorCode, bool read)
    {
        if (errorCode > (1UL << 62) - 1) throw new ArgumentOutOfRangeException(nameof(errorCode));
        if (read ? !canRead : !canWrite) throw new InvalidOperationException("Direction is unavailable");
        using var operation = EnterOperation();
        QuicError.ThrowIfFailed(Runtime.Api->StreamShutdown(Handle, (QUIC_STREAM_SHUTDOWN_FLAGS)(read ? 4 : 2), errorCode), "StreamShutdown");
        var error = new OperationCanceledException(read ? "QUIC receive aborted" : "QUIC send aborted");
        lock (Gate)
        {
            if (read) { readEnded = true; readError = error; receiveWaiter?.TrySetException(error); receiveWaiter = null; }
            else { writeEnded = true; writeError = error; writeShutdown.TrySetException(error); }
        }
    }

    public async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (!canRead) throw new InvalidOperationException("This unidirectional stream cannot receive");
        if (destination.Length == 0) { cancellationToken.ThrowIfCancellationRequested(); return 0; }
        using var offer = await ReceiveAsync(cancellationToken).ConfigureAwait(false);
        if (offer == null) return 0;
        int length = offer.CopyTo(destination.Span);
        offer.Complete((uint)length, resume: true);
        return length;
    }
    public ValueTask<QuicReceiveLease?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!canRead) throw new InvalidOperationException("This unidirectional stream cannot receive");
        using var operation = EnterOperation();
        TaskCompletionSource<QuicReceiveLease?> waiter;
        lock (Gate)
        {
            if (receiveWaiter != null || (receiveOffer != null && offerDelivered)) throw new InvalidOperationException("Only one receive operation or lease is allowed");
            if (readError != null) throw readError;
            if (receiveOffer != null) { offerDelivered = true; return ValueTask.FromResult<QuicReceiveLease?>(receiveOffer); }
            if (readEnded) return ValueTask.FromResult<QuicReceiveLease?>(null);
            receiveWaiter = waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        try { ResumeReceive(); }
        catch (Exception error) { lock (Gate) { if (ReferenceEquals(receiveWaiter, waiter)) receiveWaiter = null; waiter.TrySetException(error); } }
        return new ValueTask<QuicReceiveLease?>(WaitReceiveAsync(waiter, cancellationToken));
    }
    private async Task<QuicReceiveLease?> WaitReceiveAsync(TaskCompletionSource<QuicReceiveLease?> waiter, CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(() =>
        {
            lock (Gate)
            {
                // Once an offer was delivered, cancellation cannot abandon its
                // ownership invisibly. The completed receive wins that race.
                if (!ReferenceEquals(receiveWaiter, waiter)) return;
                receiveWaiter = null; waiter.TrySetCanceled(cancellationToken);
            }
        });
        return await waiter.Task.ConfigureAwait(false);
    }
    private unsafe void ResumeReceive()
    {
        lock (receiveCalls)
        {
            uint status = Runtime.Api->StreamReceiveSetEnabled(Handle, 1);
            if (Failed(status))
            {
                lock (Gate) { if (readEnded || IsClosing) return; }
                throw QuicError.FromStatus(status, "StreamReceiveSetEnabled");
            }
        }
    }

    internal void CompleteReceive(QuicReceiveLease offer, ulong consumed, bool resume, bool idempotent)
    {
        // Reserve call ordering before publishing availability for another read.
        // Otherwise ReceiveAsync could enable before this partial completion,
        // and C would subsequently pause the newly admitted read indefinitely.
        lock (receiveCalls)
        {
            bool enable;
            lock (Gate)
            {
                if (!offer.ClaimCompletion()) { if (idempotent) return; throw new ObjectDisposedException(nameof(QuicReceiveLease)); }
                if (!ReferenceEquals(receiveOffer, offer)) throw new InvalidOperationException("Receive offer does not belong to this stream");
                receiveOffer = null; offerDelivered = false; receiveCompletions++;
                enable = resume && !readEnded && !IsClosing;
            }
            try { CompleteReceiveCore(consumed, enable); }
            finally
            {
                offer.ReleaseBudget();
                lock (Gate) { receiveCompletions--; SignalReceiveDrain(); }
            }
        }
    }
    private unsafe void CompleteReceiveCore(ulong consumed, bool resume)
    {
        // The completing offer itself retains the raw handle while close waits
        // for receiveCompletions. This remains valid after admission has closed.
        Runtime.Api->StreamReceiveComplete(Handle, consumed);
        if (resume) ResumeReceive();
    }
    private void SignalReceiveDrain()
    {
        if (receiveOffer == null && receiveCompletions == 0) receiveDrain?.TrySetResult();
    }

    private static unsafe uint Callback(QUIC_HANDLE* stream, void* context, QUIC_STREAM_EVENT* notification)
    {
        var owner = FromContext<QuicStream>(context);
        lock (owner.Gate) { if (owner.callbacks++ == 0) owner.callbacksDrain = NewCompletion(); }
        var previousCallback = callbackOwner; callbackOwner = owner;
        try
        {
            uint status = owner.ProcessEvent(notification);
            // Preserve the receive PENDING result even when an observer faults.
            try { owner.NotifyApplication(notification); }
            catch (Exception error) { owner.RecordCallbackFailure(error); owner.connection.Fault(error); }
            return status;
        }
        catch (Exception error)
        {
            owner.RecordCallbackFailure(error); owner.connection.Fault(error);
            if (notification->Type == QUIC_STREAM_EVENT_RECEIVE)
            {
                var receive = notification->RECEIVE; receive.TotalBufferLength = 0; notification->RECEIVE = receive;
            }
            return 0;
        }
        finally
        {
            callbackOwner = previousCallback;
            lock (owner.Gate) { if (--owner.callbacks == 0) owner.callbacksDrain?.TrySetResult(); }
        }
    }
    // No implementation is included in the product. Source-linked transport
    // tests gate delivery of an actual native callback to prove wait cancellation.
    partial void ObserveStartCompletion();

    private unsafe uint ProcessEvent(QUIC_STREAM_EVENT* notification)
    {
        switch (notification->Type)
        {
            case QUIC_STREAM_EVENT_START_COMPLETE:
                var start = notification->START_COMPLETE;
                ObserveStartCompletion();
                lock (Gate) startRequested = true;
                if (Failed(start.Status)) started.TrySetException(QuicError.FromStatus(start.Status, "stream start completion"));
                else { lock (Gate) id = start.ID; started.TrySetResult(); }
                break;
            case QUIC_STREAM_EVENT_RECEIVE:
                var receive = notification->RECEIVE;
                if (((uint)receive.Flags & ~2u) != 0) throw new InvalidOperationException("Unexpected early-data or reserved receive flags");
                lock (Gate)
                {
                    if (IsClosing || readEnded) { receive.TotalBufferLength = 0; notification->RECEIVE = receive; return 0; }
                    if (receive.TotalBufferLength == 0 && (receive.Flags & QUIC_RECEIVE_FLAGS.QUIC_RECEIVE_FLAG_FIN) != 0) return 0;
                    if (receiveOffer != null) throw new InvalidOperationException("Multiple outstanding receive offers");
                    if (receive.TotalBufferLength > (ulong)Runtime.Options.MaximumReceiveOfferBytes ||
                        !connection.TryReserveBufferBytes(checked(2 * (long)receive.TotalBufferLength), out var budget))
                        throw QuicError.FromStatus(12, "stream receive buffer budget");
                    QuicReceiveLease offer;
                    try
                    {
                        offer = new QuicReceiveLease(this, receive.AbsoluteOffset, receive.TotalBufferLength, receive.Buffers, receive.BufferCount,
                            (receive.Flags & QUIC_RECEIVE_FLAGS.QUIC_RECEIVE_FLAG_FIN) != 0, budget!);
                        receiveDrain = NewCompletion();
                    }
                    catch { budget!.Dispose(); throw; }
                    receiveOffer = offer;
                    if (receiveWaiter != null) { offerDelivered = true; var waiter = receiveWaiter; receiveWaiter = null; waiter.TrySetResult(offer); }
                    return unchecked((uint)-2); // Actual pending receive, completed once by the lease owner.
                }
            case QUIC_STREAM_EVENT_IDEAL_SEND_BUFFER_SIZE:
                lock (Gate) idealSendBufferRecommendation = notification->IDEAL_SEND_BUFFER_SIZE.ByteCount;
                break;
            case QUIC_STREAM_EVENT_SEND_COMPLETE:
                var send = notification->SEND_COMPLETE;
                FinishSend((nint)send.ClientContext, send.Canceled == 0 ? null : new OperationCanceledException("QUIC send canceled by transport"));
                break;
            case QUIC_STREAM_EVENT_PEER_SEND_SHUTDOWN:
                lock (Gate) { peerFinObserved = true; readEnded = true; receiveWaiter?.TrySetResult(null); receiveWaiter = null; }
                break;
            case QUIC_STREAM_EVENT_PEER_SEND_ABORTED:
                EndRead(new QuicStreamAbortedException("Peer aborted QUIC send", notification->PEER_SEND_ABORTED.ErrorCode));
                break;
            case QUIC_STREAM_EVENT_PEER_RECEIVE_ABORTED:
                EndWrite(new QuicStreamAbortedException("Peer aborted QUIC receive", notification->PEER_RECEIVE_ABORTED.ErrorCode));
                break;
            case QUIC_STREAM_EVENT_SEND_SHUTDOWN_COMPLETE:
                if (notification->SEND_SHUTDOWN_COMPLETE.Graceful != 0) { lock (Gate) writeEnded = true; writeShutdown.TrySetResult(); }
                else EndWrite(new OperationCanceledException("QUIC write direction aborted"));
                break;
            case QUIC_STREAM_EVENT_SHUTDOWN_COMPLETE:
                var closed = notification->SHUTDOWN_COMPLETE;
                var failure = Failed(closed.ConnectionCloseStatus) ? QuicError.FromStatus(closed.ConnectionCloseStatus, "stream connection shutdown") : null;
                lock (Gate)
                {
                    readEnded = true; writeEnded = true;
                    if (failure != null) { readError ??= failure; writeError ??= failure; }
                    if (canRead && !peerFinObserved) readError ??= new OperationCanceledException("Stream closed before peer FIN");
                    if (readError != null) receiveWaiter?.TrySetException(readError); else receiveWaiter?.TrySetResult(null);
                    receiveWaiter = null;
                }
                if (!started.Task.IsCompleted) started.TrySetException(failure ?? new OperationCanceledException("Stream closed before start"));
                if (!writeShutdown.Task.IsCompleted) writeShutdown.TrySetException(failure ?? new OperationCanceledException("Stream write closed before graceful completion"));
                shutdown.TrySetResult();
                break;
        }
        return 0;
    }
    private void EndRead(Exception error)
    { lock (Gate) { readEnded = true; readError = error; receiveWaiter?.TrySetException(error); receiveWaiter = null; } }
    private void EndWrite(Exception error)
    { lock (Gate) { writeEnded = true; writeError = error; } writeShutdown.TrySetException(error); }

    public override ValueTask DisposeAsync() => new(CloseOnce(CloseCoreAsync));
    private async Task CloseCoreAsync()
    {
        QuicReceiveLease? undispatched;
        lock (Gate)
        {
            var error = new ObjectDisposedException(nameof(QuicStream));
            receiveWaiter?.TrySetException(error); receiveWaiter = null;
            undispatched = offerDelivered ? null : receiveOffer;
        }
        undispatched?.Dispose();
        RequestCloseShutdown();
        Task borrowed;
        lock (Gate) { SignalReceiveDrain(); borrowed = receiveDrain?.Task ?? Task.CompletedTask; }
        await borrowed.ConfigureAwait(false);
        if (HasNativeHandle) await Runtime.RunCleanup(CloseHandle).ConfigureAwait(false);
        Task activeCallbacks;
        lock (Gate) activeCallbacks = callbacksDrain?.Task ?? Task.CompletedTask;
        await activeCallbacks.ConfigureAwait(false);
        // Raw close has now ended callback ownership. If shutdown did not produce
        // a send completion, close is the final proof that C stopped using bytes.
        List<QuicSendOperation> abandoned;
        lock (Gate) { abandoned = new(sends.Values); sends.Clear(); }
        foreach (var send in abandoned) { send.Finish(new OperationCanceledException("Stream closed")); sendAdmission.Release(); }
        started.TrySetException(new ObjectDisposedException(nameof(QuicStream)));
        writeShutdown.TrySetException(new ObjectDisposedException(nameof(QuicStream)));
        lock (Gate) managedCallback = null;
        RetireContext();
        if (registered) { registered = false; connection.StreamClosed(this); }
    }
    private unsafe void RequestCloseShutdown()
    {
        if (Handle == null) return;
        uint status = Runtime.Api->StreamShutdown(Handle, QUIC_STREAM_SHUTDOWN_FLAGS.QUIC_STREAM_SHUTDOWN_FLAG_ABORT | QUIC_STREAM_SHUTDOWN_FLAGS.QUIC_STREAM_SHUTDOWN_FLAG_IMMEDIATE, 0);
        // Already-terminal streams can reject an additional shutdown; raw close
        // still owns cleanup. Preserve unexpected status as a callback diagnostic.
        if (Failed(status) && status != 1) RecordCallbackFailure(QuicError.FromStatus(status, "stream disposal shutdown"));
    }
    private unsafe void CloseHandle()
    {
        if (Handle == null) return;
        Runtime.Api->StreamClose(Handle); Handle = null;
    }
    private static bool Failed(uint status) => unchecked((int)status) > 0;
    private static void ValidateFlags(uint value, uint supported, uint known, string name)
    {
        if ((value & ~known) != 0) throw new ArgumentException("Unknown or reserved " + name + " flags", name);
        if ((value & ~supported) != 0) throw new NotSupportedException("Unsupported " + name + " flags");
    }
}
