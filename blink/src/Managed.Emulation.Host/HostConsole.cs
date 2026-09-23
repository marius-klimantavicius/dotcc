namespace Managed.Emulation.Host;

/// <summary>Bounded binary guest descriptors. Closing input delivers EOF;
/// stopping cancels pending guest operations without closing caller streams.</summary>
public sealed class HostConsole : IDisposable
{
    private readonly ByteQueue input, output, error;
    private readonly object gate = new();
    private readonly long outputLimit;
    private readonly int captureLimit;
    private readonly bool captureOnly;
    private readonly byte[] marker;
    private readonly int[] markerPrefix;
    private int markerPosition;
    private bool markerSeen;
    private readonly MemoryStream capturedOutput = new(), capturedError = new();
    private long reserved, emitted;
    private bool limitReached, truncated;
    private readonly CancellationTokenSource stopped = new();
    private readonly SemaphoreSlim outputWriter = new(1, 1), errorWriter = new(1, 1);

    public HostConsole(int bufferBytes = 65536, long outputLimit = 1048576,
        int captureLimit = 65536, bool captureOnly = false, byte[]? outputMarker = null)
    {
        if (bufferBytes is < 1 or > 1048576 || outputLimit < 0 || captureLimit is < 0 or > 1048576)
            throw new ArgumentOutOfRangeException(nameof(bufferBytes));
        input = new(bufferBytes); output = new(bufferBytes); error = new(bufferBytes);
        this.outputLimit = outputLimit; this.captureLimit = captureLimit; this.captureOnly = captureOnly;
        marker = outputMarker?.ToArray() ?? [];
        if (marker.Length > 4096) throw new ArgumentException("Output marker exceeds its bound.");
        markerPrefix = new int[marker.Length];
        for (int i = 1, j = 0; i < marker.Length; ++i)
        {
            while (j > 0 && marker[i] != marker[j]) j = markerPrefix[j - 1];
            if (marker[i] == marker[j]) ++j;
            markerPrefix[i] = j;
        }
        StandardInput = new QueueStream(input, false); StandardOutput = new QueueStream(output, true);
        StandardError = new QueueStream(error, true);
    }
    public Stream StandardInput { get; }
    public Stream StandardOutput { get; }
    public Stream StandardError { get; }
    public bool OutputLimitReached { get { lock (gate) return limitReached; } }
    public bool CaptureTruncated { get { lock (gate) return truncated; } }
    public bool OutputMarkerSeen { get { lock (gate) return markerSeen; } }
    public long OutputBytes { get { lock (gate) return emitted; } }
    public (byte[] StandardOutput, byte[] StandardError) CapturedOutput
    { get { lock (gate) return (capturedOutput.ToArray(), capturedError.ToArray()); } }
    public void CloseInput() => input.Complete();
    public void CompleteOutput() { output.Complete(); error.Complete(); }
    public void Stop() { stopped.Cancel(); input.Complete(); CompleteOutput(); }
    public void Dispose() => Stop();

    public async Task<HostResult<int>> ReadInputAsync(Memory<byte> destination, CancellationToken cancellation = default, bool nonBlocking = false)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, stopped.Token);
        try { return HostResult<int>.Success(await input.ReadAsync(destination, linked.Token, nonBlocking).ConfigureAwait(false)); }
        catch (WouldBlockException) { return HostResult<int>.Failure(GuestError.Again); }
        catch (OperationCanceledException) { return HostResult<int>.Failure(GuestError.Canceled); }
    }
    public async Task<HostResult<int>> WriteOutputAsync(bool standardError, ReadOnlyMemory<byte> source,
        CancellationToken cancellation = default, bool nonBlocking = false)
    {
        if (source.IsEmpty) return HostResult<int>.Success(0);
        var writer = standardError ? errorWriter : outputWriter;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, stopped.Token);
        bool entered = false;
        try
        {
            if (nonBlocking)
            {
                if (!writer.Wait(0)) return HostResult<int>.Failure(GuestError.Again);
            }
            else await writer.WaitAsync(linked.Token).ConfigureAwait(false);
            entered = true;
            return await WriteOutputCoreAsync(standardError, source, linked.Token, nonBlocking).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return HostResult<int>.Failure(GuestError.Canceled); }
        finally { if (entered) writer.Release(); }
    }
    private async Task<HostResult<int>> WriteOutputCoreAsync(bool standardError, ReadOnlyMemory<byte> source,
        CancellationToken cancellation, bool nonBlocking)
    {
        if (source.Length == 0) return HostResult<int>.Success(0);
        if ((standardError ? error : output).Completed) return HostResult<int>.Failure((GuestError)32);
        int claimed;
        lock (gate)
        {
            claimed = (int)Math.Min(source.Length, outputLimit - reserved);
            if (claimed == 0) { limitReached = true; return HostResult<int>.Failure(GuestError.NoSpace); }
            reserved += claimed;
        }
        int written = 0;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, stopped.Token);
        try
        {
            linked.Token.ThrowIfCancellationRequested();
            written = captureOnly ? claimed : await (standardError ? error : output)
                .WriteAsync(source[..claimed], linked.Token, nonBlocking).ConfigureAwait(false);
            lock (gate)
            {
                emitted += written;
                if (!standardError && !markerSeen && marker.Length != 0)
                    foreach (byte value in source.Span[..written])
                    {
                        while (markerPosition > 0 && value != marker[markerPosition]) markerPosition = markerPrefix[markerPosition - 1];
                        if (value == marker[markerPosition]) ++markerPosition;
                        if (markerPosition == marker.Length) { markerSeen = true; break; }
                    }
                int keep = (int)Math.Min(written, captureLimit - capturedOutput.Length - capturedError.Length);
                if (keep > 0) (standardError ? capturedError : capturedOutput).Write(source.Span[..keep]);
                truncated |= keep != written;
            }
            return HostResult<int>.Success(written);
        }
        catch (WouldBlockException) { return HostResult<int>.Failure(GuestError.Again); }
        catch (OperationCanceledException) { return HostResult<int>.Failure(GuestError.Canceled); }
        catch (IOException) { return HostResult<int>.Failure((GuestError)32); }
        finally { lock (gate) reserved -= claimed - written; }
    }
    public short Readiness(int descriptor, short requested)
    {
        if (descriptor == 0) return (short)((input.Readable ? requested & 1 : 0) | (input.Completed ? 16 : 0));
        var queue = descriptor == 1 ? output : error;
        return (short)((queue.Completed ? 8 : 0) | (!queue.Completed && (captureOnly || queue.Writable) ? requested & 4 : 0));
    }
    public void CloseDescriptor(int descriptor)
    {
        if (descriptor == 0) input.Complete();
        else if (descriptor == 1) output.Complete();
        else if (descriptor == 2) error.Complete();
    }

    private sealed class WouldBlockException : Exception;
    private sealed class ByteQueue(int capacity)
    {
        private readonly object sync = new();
        private readonly byte[] bytes = new byte[capacity];
        private int head, count;
        private bool complete;
        private TaskCompletionSource changed = NewSignal();
        private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Readable { get { lock (sync) return count != 0 || complete; } }
        public bool Writable { get { lock (sync) return count < bytes.Length && !complete; } }
        public bool Completed { get { lock (sync) return complete; } }
        private void Pulse() { var previous = changed; changed = NewSignal(); previous.TrySetResult(); }
        public void Complete() { lock (sync) { complete = true; Pulse(); } }
        public async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken token, bool nonBlocking = false)
        {
            if (destination.IsEmpty) return 0;
            while (true)
            {
                token.ThrowIfCancellationRequested(); Task wait;
                lock (sync)
                {
                    if (count != 0)
                    {
                        int length = Math.Min(destination.Length, Math.Min(count, bytes.Length - head));
                        bytes.AsMemory(head, length).CopyTo(destination); head = (head + length) % bytes.Length;
                        count -= length; Pulse(); return length;
                    }
                    if (complete) return 0;
                    if (nonBlocking) throw new WouldBlockException();
                    wait = changed.Task;
                }
                await wait.WaitAsync(token).ConfigureAwait(false);
            }
        }
        public async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> source, CancellationToken token, bool nonBlocking = false)
        {
            if (source.IsEmpty) return 0;
            while (true)
            {
                token.ThrowIfCancellationRequested(); Task wait;
                lock (sync)
                {
                    if (complete) throw new IOException("Console descriptor is closed.");
                    if (count != bytes.Length)
                    {
                        int tail = (head + count) % bytes.Length;
                        int length = Math.Min(source.Length, Math.Min(bytes.Length - count, bytes.Length - tail));
                        source[..length].CopyTo(bytes.AsMemory(tail)); count += length; Pulse(); return length;
                    }
                    if (nonBlocking) throw new WouldBlockException();
                    wait = changed.Task;
                }
                await wait.WaitAsync(token).ConfigureAwait(false);
            }
        }
    }
    private sealed class QueueStream(ByteQueue queue, bool reading) : Stream
    {
        private bool disposed;
        public override bool CanRead => reading && !disposed;
        public override bool CanWrite => !reading && !disposed;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
        public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!reading) throw new NotSupportedException();
            return queue.ReadAsync(buffer, cancellationToken);
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (reading) throw new NotSupportedException();
            while (!buffer.IsEmpty) buffer = buffer[(await queue.WriteAsync(buffer, cancellationToken).ConfigureAwait(false))..];
        }
        protected override void Dispose(bool disposing) { disposed = true; queue.Complete(); base.Dispose(disposing); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
