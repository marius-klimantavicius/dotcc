using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Managed.PGlite;

namespace Npgsql.Internal;

// Carries original Npgsql frontend bytes and original backend response bytes.
// A write can end anywhere, even inside the five-byte header or a parameter.
// Only whole validated frames cross the synchronous engine boundary.
internal sealed class EmbeddedStream(PgliteWorker worker, Action onClosed) : Stream
{
    internal PgliteWorker Worker { get; } = worker;
    readonly SemaphoreSlim _writeLock = new(1);
    readonly object _readLock = new();
    readonly Queue<byte[]> _responses = new();
    TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    Exception? _writeError;
    byte[] _pending = [];
    int _responseOffset;
    bool _disposed;

    public override bool CanRead => !_disposed;
    public override bool CanWrite => !_disposed;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Flush() { ObjectDisposedException.ThrowIf(_disposed, this); }
    public override Task FlushAsync(CancellationToken cancellationToken)
    { EmbeddedPolicy.CheckCancellation(cancellationToken); Flush(); return Task.CompletedTask; }

    public override void Write(byte[] buffer, int offset, int count)
        => WriteAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();
    public override void Write(ReadOnlySpan<byte> buffer)
        => Write(buffer.ToArray(), 0, buffer.Length);
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EmbeddedPolicy.CheckCancellation(cancellationToken);
        await _writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var input = new byte[checked(_pending.Length + buffer.Length)];
            _pending.CopyTo(input, 0);
            buffer.Span.CopyTo(input.AsSpan(_pending.Length));
            int offset = 0;
            while (input.Length - offset >= 5)
            {
                int length = BinaryPrimitives.ReadInt32BigEndian(input.AsSpan(offset + 1));
                if (length < 4 || length == int.MaxValue) throw new IOException("Invalid embedded frontend frame length.");
                if (length > input.Length - offset - 1) break;
                byte type = input[offset];
                if (type is (byte)'d' or (byte)'c' or (byte)'f') throw EmbeddedPolicy.CopyUnsupported();
                if (type == 'X') throw new IOException("Embedded termination is owned by connection disposal.");
                offset = checked(offset + length + 1);
            }
            _pending = input.AsSpan(offset).ToArray();
            if (offset == 0) return;
            byte[] frames = input.AsSpan(0, offset).ToArray();
            byte[] response = await Worker.InvokeAsync(db => db.ExecProtocolRaw(frames)).ConfigureAwait(false);
            if (response.Length != 0)
                lock (_readLock) { _responses.Enqueue(response); SignalChanged(); }
        }
        catch (Exception error)
        {
            lock (_readLock) { _writeError = error; SignalChanged(); }
            throw;
        }
        finally { _writeLock.Release(); }
    }

    // Called under _readLock. Complete the previous generation, so a producer
    // cannot race between the reader's availability check and awaiting data.
    void SignalChanged()
    {
        var previous = _changed;
        _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        previous.TrySetResult();
    }

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty) return 0;
        // Sync Npgsql reads can overlap an asynchronous batch writer too.
        for (;;)
        {
            Task changed;
            lock (_readLock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_responses.TryPeek(out var response))
                {
                    int count = Math.Min(buffer.Length, response.Length - _responseOffset);
                    response.AsSpan(_responseOffset, count).CopyTo(buffer);
                    _responseOffset += count;
                    if (_responseOffset == response.Length) { _responses.Dequeue(); _responseOffset = 0; }
                    return count;
                }
                if (_writeError != null) throw new IOException("The embedded frontend writer failed.", _writeError);
                changed = _changed.Task;
            }
            changed.GetAwaiter().GetResult();
        }
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        // Npgsql's internal timeout token is inert for this profile. Public
        // running tokens are rejected before any command/reader operation.
        cancellationToken.ThrowIfCancellationRequested();
        if (buffer.IsEmpty) return 0;
        for (;;)
        {
            Task changed;
            lock (_readLock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                if (_responses.Count != 0) return Read(buffer.Span);
                if (_writeError != null) throw new IOException("The embedded frontend writer failed.", _writeError);
                changed = _changed.Task;
            }
            await changed.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
    protected override void Dispose(bool disposing)
    {
        if (!disposing || _disposed) return;
        _writeLock.Wait();
        try
        {
            if (_disposed) return;
            try { Worker.Dispose(); }
            finally
            {
                _disposed = true;
                lock (_readLock) { _responses.Clear(); _pending = []; SignalChanged(); }
                // Managed extension/storage cleanup can fail after the engine
                // closed successfully. Release only with that actual proof;
                // genuine engine shutdown failures retain the exclusion lease.
                if (Worker.Closed) onClosed();
            }
        }
        finally { _writeLock.Release(); }
        base.Dispose(disposing);
    }
}
