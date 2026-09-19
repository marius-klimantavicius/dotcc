namespace Managed.Emulation.Host;

public readonly record struct PipePair(int Read, int Write);

/// <summary>Private byte-stream pipes. End descriptions and in-flight leases
/// own a bounded ring; no operating-system descriptor or handle is created.</summary>
public sealed class VirtualPipes : IAsyncDisposable
{
    public const int AtomicWriteLimit = 4096;
    private sealed class Pipe(int capacity, ulong inode)
    {
        internal byte[] Bytes = new byte[capacity];
        internal readonly ulong Inode = inode;
        internal int Head, Count;
        internal End Reader = null!, Writer = null!;
        internal TaskCompletionSource Changed = NewSignal();
        internal VirtualFileTime Access = VirtualFileTime.UtcNow, Modify = VirtualFileTime.UtcNow, Change = VirtualFileTime.UtcNow;
    }
    private sealed class End(Pipe pipe, bool writing)
    {
        internal readonly Pipe Pipe = pipe;
        internal readonly bool Writing = writing;
        internal bool Closing;
        internal int Operations;
        internal bool Alive => !Closing || Operations != 0;
    }
    private readonly object sync = new();
    private readonly Dictionary<int, End> ends = new();
    private readonly HashSet<Pipe> pipes = new();
    private readonly HashSet<Task> pending = new();
    private readonly int capacity, descriptorLimit, operationLimit;
    private readonly long byteLimit;
    private long allocated;
    private ulong nextInode = 1UL << 63;
    private bool disposed;
    private TaskCompletionSource? shutdown;
    public VirtualPipes(int descriptorLimit = 128, int capacity = 65536, long byteLimit = 1048576, int operationLimit = 128)
    {
        if (descriptorLimit < 2 || capacity < AtomicWriteLimit || capacity > 1048576 || byteLimit < 0 || operationLimit < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        this.capacity = capacity; this.byteLimit = byteLimit;
        this.descriptorLimit = descriptorLimit; this.operationLimit = operationLimit;
    }
    public long AllocatedBytes { get { lock(sync) return allocated; } }
    public int PendingOperations { get { lock(sync) return pending.Count; } }
    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static HostResult<T> Fail<T>(GuestError error) => HostResult<T>.Failure(error);
    public HostResult<PipePair> Create()
    {
        lock(sync)
        {
            if (disposed) return Fail<PipePair>(GuestError.BadDescriptor);
            if (ends.Count > descriptorLimit - 2) return Fail<PipePair>(GuestError.TooManyFiles);
            if (capacity > byteLimit - allocated) return Fail<PipePair>(GuestError.NoMemory);
            try {
                int read = 0; while(ends.ContainsKey(read)) ++read;
                int write = read + 1; while(ends.ContainsKey(write)) ++write;
                var pipe = new Pipe(capacity, nextInode);
                pipe.Reader = new(pipe, false); pipe.Writer = new(pipe, true);
                ends.EnsureCapacity(ends.Count + 2); pipes.EnsureCapacity(pipes.Count + 1);
                ends.Add(read, pipe.Reader); ends.Add(write, pipe.Writer); pipes.Add(pipe);
                ++nextInode; allocated += capacity;
                return HostResult<PipePair>.Success(new(read, write));
            } catch(OutOfMemoryException) { return Fail<PipePair>(GuestError.NoMemory); }
        }
    }
    private static void Pulse(Pipe pipe)
    {
        var old = pipe.Changed; pipe.Changed = NewSignal(); old.TrySetResult();
    }
    private void Release(Pipe pipe)
    {
        if (pipe.Reader.Alive || pipe.Writer.Alive) return;
        if (pipes.Remove(pipe)) { allocated -= pipe.Bytes.Length; pipe.Bytes = []; }
    }
    public HostResult<int> Close(int handle)
    {
        lock(sync)
        {
            if (!ends.Remove(handle, out var end)) return Fail<int>(GuestError.BadDescriptor);
            end.Closing = true; Pulse(end.Pipe); Release(end.Pipe);
            return HostResult<int>.Success(0);
        }
    }
    public HostResult<VirtualFileStat> Stat(int handle)
    {
        lock(sync)
        {
            if (disposed || !ends.TryGetValue(handle, out var end)) return Fail<VirtualFileStat>(GuestError.BadDescriptor);
            var p=end.Pipe;
            return HostResult<VirtualFileStat>.Success(new(0,false,false,p.Inode,0x1000|0x180,
                p.Access.Ticks,p.Modify.Ticks,p.Change.Ticks,AccessSubtick:p.Access.Subtick,ModifySubtick:p.Modify.Subtick,ChangeSubtick:p.Change.Subtick));
        }
    }
    public Task<HostResult<int>> ReadAsync(int handle, Memory<byte> destination, bool nonblocking = false, CancellationToken cancellation = default)
        => Transfer(handle, destination, default, false, nonblocking, cancellation);
    public Task<HostResult<int>> WriteAsync(int handle, ReadOnlyMemory<byte> source, bool nonblocking = false, CancellationToken cancellation = default)
        => Transfer(handle, default, source, true, nonblocking, cancellation);
    private async Task<HostResult<int>> Transfer(int handle, Memory<byte> destination, ReadOnlyMemory<byte> source,
        bool writing, bool nonblocking, CancellationToken cancellation)
    {
        End end; var completion=NewSignal(); int written=0;
        lock(sync)
        {
            if (cancellation.IsCancellationRequested) return Fail<int>(GuestError.Canceled);
            if (disposed || !ends.TryGetValue(handle,out end!)) return Fail<int>(GuestError.BadDescriptor);
            if (end.Writing != writing) return Fail<int>(GuestError.BadDescriptor);
            if (pending.Count >= operationLimit) return Fail<int>(GuestError.Again);
            ++end.Operations; pending.Add(completion.Task);
        }
        try
        {
            while(true)
            {
                Task changed;
                lock(sync)
                {
                    if (disposed) return written != 0 ? HostResult<int>.Success(written) : Fail<int>(GuestError.Canceled);
                    var pipe=end.Pipe;
                    if ((writing ? source.Length : destination.Length) == 0) return HostResult<int>.Success(0);
                    if (!writing)
                    {
                        if (pipe.Count != 0)
                        {
                            int count=Math.Min(destination.Length,pipe.Count);
                            int first=Math.Min(count,pipe.Bytes.Length-pipe.Head);
                            pipe.Bytes.AsSpan(pipe.Head,first).CopyTo(destination.Span);
                            pipe.Bytes.AsSpan(0,count-first).CopyTo(destination.Span[first..]);
                            pipe.Head=(pipe.Head+count)%pipe.Bytes.Length;pipe.Count-=count;
                            pipe.Access=VirtualFileTime.UtcNow;Pulse(pipe);
                            return HostResult<int>.Success(count);
                        }
                        if (!pipe.Writer.Alive) return HostResult<int>.Success(0);
                    }
                    else
                    {
                        if (!pipe.Reader.Alive) return written != 0 ? HostResult<int>.Success(written) : Fail<int>(GuestError.BrokenPipe);
                        int free=pipe.Bytes.Length-pipe.Count;
                        if (free > 0 && (source.Length > AtomicWriteLimit || free >= source.Length))
                        {
                            int count=Math.Min(free,source.Length-written);
                            int tail=(pipe.Head+pipe.Count)%pipe.Bytes.Length;
                            int first=Math.Min(count,pipe.Bytes.Length-tail);
                            source.Span.Slice(written,first).CopyTo(pipe.Bytes.AsSpan(tail));
                            source.Span.Slice(written+first,count-first).CopyTo(pipe.Bytes);
                            pipe.Count+=count;written+=count;pipe.Modify=pipe.Change=VirtualFileTime.UtcNow;Pulse(pipe);
                            if (written == source.Length || nonblocking) return HostResult<int>.Success(written);
                            continue;
                        }
                    }
                    if (nonblocking) return Fail<int>(GuestError.Again);
                    changed=pipe.Changed.Task;
                }
                await changed.WaitAsync(cancellation).ConfigureAwait(false);
            }
        }
        catch(OperationCanceledException) { return written != 0 ? HostResult<int>.Success(written) : Fail<int>(GuestError.Canceled); }
        finally
        {
            lock(sync) { --end.Operations; pending.Remove(completion.Task); Pulse(end.Pipe); Release(end.Pipe); }
            completion.TrySetResult();
        }
    }
    public HostResult<short> Readiness(int handle, short requested)
    {
        lock(sync)
        {
            if (disposed || !ends.TryGetValue(handle,out var end)) return Fail<short>(GuestError.BadDescriptor);
            var p=end.Pipe; int result=0;
            if (end.Writing)
            {
                if (p.Bytes.Length-p.Count >= AtomicWriteLimit) result |= requested & (4|256);
                if (!p.Reader.Alive) result |= 8;
            }
            else
            {
                if (p.Count != 0) result |= requested & (1|64);
                if (!p.Writer.Alive) result |= 16;
            }
            return HostResult<short>.Success((short)result);
        }
    }
    public async ValueTask DisposeAsync()
    {
        TaskCompletionSource completion; Task[] operations=[]; bool owner;
        lock(sync)
        {
            owner=shutdown==null;shutdown??=NewSignal();completion=shutdown;
            if(owner)
            {
                disposed=true;operations=pending.ToArray();ends.Clear();
                foreach(var pipe in pipes.ToArray()) { pipe.Reader.Closing=pipe.Writer.Closing=true;Pulse(pipe);Release(pipe); }
            }
        }
        if(owner)
        {
            try { await Task.WhenAll(operations).ConfigureAwait(false); completion.TrySetResult(); }
            catch(Exception error) { completion.TrySetException(error); }
        }
        await completion.Task.ConfigureAwait(false);
    }
}
