using System.Threading.Channels;

namespace Managed.Transport.Api;

public enum QuicDatagramSendResult { Lost = 3, Acknowledged = 4, AcknowledgedAfterLoss = 5, Canceled = 6 }

public sealed partial class QuicConnection
{
    private readonly Channel<ReceivedDatagram> datagrams = Channel.CreateBounded<ReceivedDatagram>(new BoundedChannelOptions(64) { FullMode = BoundedChannelFullMode.Wait });
    private readonly Dictionary<nint, DatagramSend> datagramSends = [];
    private long nextDatagram;
    private ushort maximumDatagramLength;
    private bool datagramSendEnabled;
    private sealed record ReceivedDatagram(byte[] Bytes, IDisposable Reservation);
    private sealed class DatagramSend(byte[] bytes, QUIC_BUFFER[] descriptors, IDisposable reservation)
    {
        internal readonly byte[] Bytes = bytes;
        internal readonly QUIC_BUFFER[] Descriptors = descriptors;
        internal readonly IDisposable Reservation = reservation;
        internal readonly TaskCompletionSource<QuicDatagramSendResult> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    public ushort MaximumDatagramSendLength { get { lock (Gate) return datagramSendEnabled ? maximumDatagramLength : (ushort)0; } }

    /// <summary>Copies one datagram. Cancellation after admission stops waiting; the native send retains its copied storage until final completion.</summary>
    public async ValueTask<QuicDatagramSendResult> SendDatagramAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DatagramSend send;
        using (EnterOperation()) send = SubmitDatagram(data.Span);
        return await send.Completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
    private unsafe DatagramSend SubmitDatagram(ReadOnlySpan<byte> data)
    {
        lock (Gate)
            if (!datagramSendEnabled || data.Length > maximumDatagramLength) throw new InvalidOperationException("Datagram exceeds the negotiated send limit or datagram sending is disabled.");
        if (data.Length > Runtime.Options.MaximumCopiedSendBytes || !TryReserveBufferBytes(Math.Max(1, data.Length), out var reservation))
            throw new InvalidOperationException("Datagram copy budget is exhausted.");
        DatagramSend? send = null; nint token = 0;
        try
        {
            byte[] bytes = GC.AllocateArray<byte>(Math.Max(1, data.Length), pinned: true); data.CopyTo(bytes);
            var descriptors = GC.AllocateArray<QUIC_BUFFER>(1, pinned: true);
            fixed (byte* pointer = bytes) descriptors[0] = new() { Buffer = pointer, Length = (uint)data.Length };
            send = new(bytes, descriptors, reservation!);
            lock (Gate)
            {
                if (datagramSends.Count >= 64) throw new InvalidOperationException("Pending datagram send limit reached.");
                token = checked((nint)checked(++nextDatagram));
                datagramSends.Add(token, send);
            }
            uint status;
            fixed (QUIC_BUFFER* buffers = descriptors)
                status = Runtime.Api->DatagramSend(Handle, buffers, 1, QUIC_SEND_FLAGS.QUIC_SEND_FLAG_NONE, (void*)token);
            if (QuicError.Failed(status))
            {
                lock (Gate) datagramSends.Remove(token);
                QuicError.ThrowIfFailed(status, "DatagramSend");
            }
            return send;
        }
        catch
        {
            if (token != 0) lock (Gate) datagramSends.Remove(token);
            reservation!.Dispose(); throw;
        }
    }
    public async ValueTask<ReadOnlyMemory<byte>> ReceiveDatagramAsync(CancellationToken cancellationToken = default)
    {
        using var operation = EnterOperation();
        var datagram = await datagrams.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        datagram.Reservation.Dispose();
        return datagram.Bytes; // Caller-owned managed memory remains valid independently of QUIC close.
    }
    private unsafe void ReceiveDatagram(QUIC_BUFFER* buffer)
    {
        if (buffer == null || buffer->Length > Runtime.Options.MaximumReceiveOfferBytes ||
            !TryReserveBufferBytes(Math.Max(1L, buffer->Length), out var reservation)) return;
        try
        {
            byte[] copy = new ReadOnlySpan<byte>(buffer->Buffer, checked((int)buffer->Length)).ToArray();
            if (datagrams.Writer.TryWrite(new(copy, reservation!))) reservation = null;
        }
        finally { reservation?.Dispose(); }
    }
    private unsafe void DatagramState(void* context, QUIC_DATAGRAM_SEND_STATE state)
    {
        if ((int)state < 3) return;
        DatagramSend send;
        lock (Gate)
            if (!datagramSends.Remove((nint)context, out send!)) throw new InvalidOperationException("Unknown final datagram send context.");
        send.Reservation.Dispose(); send.Completion.TrySetResult((QuicDatagramSendResult)state);
    }
    private void DrainDatagrams()
    {
        datagrams.Writer.TryComplete(new ObjectDisposedException(nameof(QuicConnection)));
        while (datagrams.Reader.TryRead(out var item)) item.Reservation.Dispose();
        lock (Gate)
            if (datagramSends.Count != 0) throw new InvalidOperationException("Native connection close left datagram sends outstanding.");
    }
}
