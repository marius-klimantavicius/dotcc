using System.Net.Sockets;
using Managed.Security.S2n;

namespace ManagedTests;

// These supply byte transport faults only. TLS, allocation, credentials and all
// state transitions still execute in the translated product; no TLS test double.
internal sealed class PendingTransport : IS2nTransport
{
    internal readonly TaskCompletionSource Waiting = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal int Calls, Disposals;
    internal Action? OnReceive;
    public S2nIoResult Receive(Span<byte> destination)
    { Interlocked.Increment(ref Calls); OnReceive?.Invoke(); return new(0, true); }
    public S2nIoResult Send(ReadOnlySpan<byte> source) { Interlocked.Increment(ref Calls); return new(source.Length); }
    public async ValueTask WaitAsync(bool reading, CancellationToken cancellationToken)
    { Waiting.TrySetResult(); await Task.Delay(Timeout.Infinite, cancellationToken); }
    public void Dispose() => Interlocked.Increment(ref Disposals);
}

internal sealed class ShortTransport(Socket socket) : IS2nTransport
{
    private readonly S2nSocketTransport _inner = new(socket);
    private int _calls;
    public S2nIoResult Receive(Span<byte> destination)
        => Interlocked.Increment(ref _calls) % 3 == 0 ? new(0, true) : _inner.Receive(destination[..Math.Min(destination.Length, 17)]);
    public S2nIoResult Send(ReadOnlySpan<byte> source)
        => Interlocked.Increment(ref _calls) % 3 == 0 ? new(0, true) : _inner.Send(source[..Math.Min(source.Length, 19)]);
    public ValueTask WaitAsync(bool reading, CancellationToken cancellationToken) => _inner.WaitAsync(reading, cancellationToken);
    public void Dispose() => _inner.Dispose();
}

internal sealed class CorruptTransport(Socket socket) : IS2nTransport
{
    private readonly S2nSocketTransport _inner = new(socket);
    internal bool Corrupt;
    private int _position, _recordLength;
    public S2nIoResult Receive(Span<byte> destination) => _inner.Receive(destination);
    public S2nIoResult Send(ReadOnlySpan<byte> source)
    {
        if (!Corrupt) return _inner.Send(source);
        if (_position == 0)
        {
            if (source.Length < 5) throw new InvalidOperationException("Expected complete outgoing TLS header in the translated record buffer.");
            _recordLength = 5 + (source[3] << 8) + source[4];
        }
        byte[] copy = source.ToArray();
        int target = _recordLength - 1 - _position;
        if ((uint)target < (uint)copy.Length) copy[target] ^= 0x80;
        var result = _inner.Send(copy);
        _position += result.Count;
        if (_position >= _recordLength) Corrupt = false;
        return result;
    }
    public ValueTask WaitAsync(bool reading, CancellationToken cancellationToken) => _inner.WaitAsync(reading, cancellationToken);
    public void Dispose() => _inner.Dispose();
}
