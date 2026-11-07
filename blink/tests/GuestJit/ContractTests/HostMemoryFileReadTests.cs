using Managed.Emulation.Host;

unsafe partial class Program
{
    // Parent runner hook. Authored only: no build or execution by R74.
    static void CheckHostMemoryFileRead()
    {
        for (int route = 0; route < 3; ++route) CheckHostMemoryFileReadRetry(route);
    }

    sealed class HostBackingReadProbe : IHostMemoryFileRead, IDisposable
    {
        internal int Open, Attempts, Begins, Ends, Active;
        internal readonly ManualResetEventSlim Rejected = new();
        public bool TryBeginMemoryFileRead()
        {
            Interlocked.Increment(ref Attempts);
            if (Volatile.Read(ref Open) == 0) { Rejected.Set(); return false; }
            Interlocked.Increment(ref Begins); Interlocked.Increment(ref Active);
            return true;
        }
        public void EndMemoryFileRead()
        {
            Interlocked.Increment(ref Ends); Interlocked.Decrement(ref Active);
        }
        public void Dispose() => Rejected.Dispose();
    }

    static void CheckHostMemoryFileReadRetry(int route)
    {
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var observer = new HostBackingReadProbe();
        int fd = io.CreateMemoryFile("read-exclusion", 0).Value;
        byte[] expected = new byte[8192]; expected.AsSpan(0, 4096).Fill(0x35); expected.AsSpan(4096).Fill(0xca);
        Check(io.WriteAt(fd, expected, 0).Value == expected.Length, "prepare two backing pages");
        var first = io.AcquireMemoryFilePage(fd, 0).Value;
        var second = io.AcquireMemoryFilePage(fd, 4096).Value;
        var mapping = io.BeginMapping(fd).Value;
        io.BindMemoryFileRead(observer); io.BindMemoryFileRead(observer);
        byte[] destination = new byte[8192];
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        Task<HostResult<int>>? pending = null;
        try
        {
            pending = Task.Run(() => route switch
            {
                0 => io.ReadAsync(fd, destination, cancellation.Token).GetAwaiter().GetResult(),
                1 => io.ReadAt(fd, destination, 0),
                _ => io.MappingReadAt(mapping, destination, 0)
            });
            Check(observer.Rejected.Wait(TimeSpan.FromSeconds(3)), "backing read attempted nonblocking admission");
            // A rejected reader must leave sync. Metadata uses that exact gate.
            var metadata = Task.Run(() => io.ReadAtLength(fd));
            Check(metadata.Wait(TimeSpan.FromSeconds(3)) && metadata.Result.Value == expected.Length,
                "pending backing read releases I/O gate before retry");
            Check(observer.Begins == 0 && observer.Ends == 0 && observer.Active == 0,
                "failed try-admission performs no counted access");
            Volatile.Write(ref observer.Open, 1);
            Check(pending.Wait(TimeSpan.FromSeconds(3)) && pending.Result.Value == expected.Length &&
                destination.AsSpan().SequenceEqual(expected), "all public backing read routes complete owned copies");
            Check(observer.Begins == 2 && observer.Begins == observer.Ends && observer.Active == 0,
                "one balanced bounded scope for each actual page copy");
            bool mismatch = false;
            using var other = new HostBackingReadProbe();
            try { io.UnbindMemoryFileRead(other); } catch (InvalidOperationException) { mismatch = true; }
            Check(mismatch, "backing reader owner cannot be replaced by unrelated cleanup");
        }
        finally
        {
            Volatile.Write(ref observer.Open, 1);
            if (pending != null) Check(pending.Wait(TimeSpan.FromSeconds(3)), "reader quiesces before observer unbind");
            io.UnbindMemoryFileRead(observer);
            io.EndMapping(mapping); io.Close(fd);
            InstanceIo.ReleaseMemoryFilePage(second.Lease); InstanceIo.ReleaseMemoryFilePage(first.Lease);
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
