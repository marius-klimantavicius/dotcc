using System.Net.Sockets;
using System.Runtime.InteropServices;
using RuntimeLibc = Managed.Security.S2n.S2nTls.Libc;

namespace Managed.Security.S2n;

public readonly record struct S2nIoResult(int Count, bool WouldBlock = false);

/// <summary>Nonblocking byte transport. Receive returns zero only on EOF. WaitAsync
/// must honor cancellation and may return spuriously. Callbacks must never block.</summary>
public interface IS2nTransport : IDisposable
{
    S2nIoResult Receive(Span<byte> destination);
    S2nIoResult Send(ReadOnlySpan<byte> source);
    ValueTask WaitAsync(bool reading, CancellationToken cancellationToken);
}

public sealed class S2nSocketTransport : IS2nTransport
{
    private readonly Socket _socket;
    private readonly bool _ownsSocket, _originalBlocking;
    private int _disposed;
    public S2nSocketTransport(Socket socket, bool ownsSocket = true)
    {
        ArgumentNullException.ThrowIfNull(socket);
        _socket = socket; _ownsSocket = ownsSocket; _originalBlocking = socket.Blocking;
        socket.Blocking = false;
    }
    public S2nIoResult Receive(Span<byte> destination)
    {
        int count = _socket.Receive(destination, SocketFlags.None, out SocketError error);
        return Result(count, error);
    }
    public S2nIoResult Send(ReadOnlySpan<byte> source)
    {
        int count = _socket.Send(source, SocketFlags.None, out SocketError error);
        return Result(count, error);
    }
    private static S2nIoResult Result(int count, SocketError error)
    {
        if (error is SocketError.WouldBlock or SocketError.IOPending or SocketError.NoBufferSpaceAvailable) return new(0, true);
        if (error != SocketError.Success) throw new SocketException((int)error);
        return new(count);
    }
    public async ValueTask WaitAsync(bool reading, CancellationToken cancellationToken)
    {
        // No receive is issued outside the C callback: encrypted bytes cannot be
        // consumed by a readiness probe. A cancellable bounded poll also works
        // for borrowed sockets without closing a caller's descriptor to wake it.
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            if (_socket.Poll(0, reading ? SelectMode.SelectRead : SelectMode.SelectWrite)) return;
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        }
    }
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        if (_ownsSocket) _socket.Dispose();
        else { try { _socket.Blocking = _originalBlocking; } catch (ObjectDisposedException) { } }
    }
}

internal sealed class IoCallbackState(IS2nTransport transport)
{
    internal readonly IS2nTransport Transport = transport;
    internal long Calls, BytesSent, BytesReceived;
    internal static unsafe int Receive(void* context, byte* buffer, uint count) => Invoke(context, buffer, count, false);
    internal static unsafe int Send(void* context, byte* buffer, uint count) => Invoke(context, buffer, count, true);
    private static unsafe int Invoke(void* context, byte* buffer, uint count, bool sending)
    {
        try
        {
            var state = (IoCallbackState)GCHandle.FromIntPtr((nint)context).Target!;
            Interlocked.Increment(ref state.Calls);
            CallbackScope.Current?.Cancellation.ThrowIfCancellationRequested();
            int length = checked((int)Math.Min(count, int.MaxValue));
            S2nIoResult result = sending ? state.Transport.Send(new ReadOnlySpan<byte>(buffer, length))
                : state.Transport.Receive(new Span<byte>(buffer, length));
            if (result.Count < 0 || result.Count > length || (result.WouldBlock && result.Count != 0))
                throw new IOException("Transport returned an invalid byte count.");
            if (result.WouldBlock) { RuntimeLibc.errno = RuntimeLibc.EAGAIN; return -1; }
            if (sending && length != 0 && result.Count == 0) throw new IOException("Transport made no progress on send.");
            if (sending) Interlocked.Add(ref state.BytesSent, result.Count);
            else Interlocked.Add(ref state.BytesReceived, result.Count);
            return result.Count;
        }
        catch (Exception error)
        {
            CallbackScope.Capture(error);
            RuntimeLibc.errno = RuntimeLibc.EIO;
            return -1;
        }
    }
}
