using Managed.Emulation.Host;

unsafe partial class Program
{
    // Parent integration: call CheckHostMemoryFileMutation() from Main. These
    // host-only contracts do not enter a guest or invoke emitted native code.
    static void CheckHostMemoryFileMutation()
    {
        CheckHostMemoryFileMutationLifetime();
        CheckHostRegularSharedMutation();
        CheckHostMemoryFileMutationOverlap();
    }

    sealed class HostMemoryFileMutationProbe : IHostMemoryFileMutation
    {
        internal int Attempts, Begins, Ends, Active, Peak;
        internal nint LastPage, WatchAddress;
        internal long LastLength;
        internal bool LastLogical, Reject;
        internal byte Before, After;
        internal Barrier? Overlap;

        public void BeginMemoryFileMutation(nint page, long length, bool logical)
        {
            Interlocked.Increment(ref Attempts);
            if (Reject) { Reject = false; throw new InvalidOperationException("Rejected before admission."); }
            int active = Interlocked.Increment(ref Active), peak;
            do { peak = Volatile.Read(ref Peak); }
            while (active > peak && Interlocked.CompareExchange(ref Peak, active, peak) != peak);
            if (Overlap != null && !Overlap.SignalAndWait(TimeSpan.FromSeconds(10)))
            {
                Interlocked.Decrement(ref Active); // Failed Begin owns its rollback.
                throw new TimeoutException("Concurrent host mutation did not overlap.");
            }
            LastPage = page; LastLength = length; LastLogical = logical;
            if (WatchAddress != 0) Before = ((byte*)WatchAddress)[0];
            Interlocked.Increment(ref Begins);
        }

        public void EndMemoryFileMutation()
        {
            if (WatchAddress != 0) After = ((byte*)WatchAddress)[0];
            Interlocked.Increment(ref Ends);
            Interlocked.Decrement(ref Active);
        }
    }

    static void CheckHostMemoryFileMutationLifetime()
    {
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        var observer = new HostMemoryFileMutationProbe();
        var other = new HostMemoryFileMutationProbe();
        io.BindMemoryFileMutation(observer);
        io.BindMemoryFileMutation(observer);
        bool replacementRejected = false, mismatchRejected = false;
        try { io.BindMemoryFileMutation(other); }
        catch (InvalidOperationException) { replacementRejected = true; }
        try { io.UnbindMemoryFileMutation(other); }
        catch (InvalidOperationException) { mismatchRejected = true; }
        Check(replacementRejected && mismatchRejected, "host mutation observer binding enforces owner identity");

        var opened = io.CreateMemoryFile("mutation-contract", 0);
        Check(opened.Succeeded, "create observed memory file");
        int fd = opened.Value;
        MemoryFilePage first = default, second = default;
        try
        {
            Check(io.WriteAt(fd, "ab"u8, 0).Succeeded && io.TruncateFile(fd, 8192).Succeeded,
                "prepare unpublished sparse memory file");
            var acquiredFirst = io.AcquireMemoryFilePage(fd, 0);
            var acquiredSecond = io.AcquireMemoryFilePage(fd, 4096);
            Check(acquiredFirst.Succeeded && acquiredSecond.Succeeded, "publish two backing leases");
            first = acquiredFirst.Value; second = acquiredSecond.Value;
            Check(observer.Begins == 0, "initial zeroing and setup before first lease need no revocation");
            Check(io.MemoryFileMutationOwned(first.Lease) && io.MemoryFileMutationOwned(second.Lease) &&
                !io.MemoryFileMutationOwned(0), "only active local observer-enrolled memfd leases qualify");
            io.UnbindMemoryFileMutation(observer);
            Check(!io.MemoryFileMutationOwned(first.Lease), "unbound memfd backing cannot grant native eligibility");
            io.BindMemoryFileMutation(observer);

            observer.WatchAddress = first.Address;
            Check(io.WriteAt(fd, "x"u8, 0).Value == 1 && observer.Before == (byte)'a' && observer.After == (byte)'x',
                "host write admission precedes bytes and End follows publication");
            Check(observer.LastPage == first.Address && observer.LastLength == 4096 && !observer.LastLogical,
                "nonextending host write identifies aligned physical backing");

            Check(io.Seek(fd, 0, SeekOrigin.Begin).Succeeded &&
                io.WriteAsync(fd, new byte[] { (byte)'y' }).GetAwaiter().GetResult().Value == 1 &&
                observer.Before == (byte)'x' && observer.After == (byte)'y' && observer.Active == 0,
                "public async entry completes its mutation scope synchronously");

            int ends = observer.Ends;
            observer.Reject = true;
            bool admissionRejected = false;
            try { io.WriteAt(fd, "z"u8, 0); }
            catch (InvalidOperationException) { admissionRejected = true; }
            Check(admissionRejected && ((byte*)first.Address)[0] == (byte)'y' && observer.Active == 0 && observer.Ends == ends,
                "failed Begin changes no bytes and has no unmatched End");
            int begins = observer.Begins;
            Check(!io.WriteAt(fd, "z"u8, -1).Succeeded && io.WriteAt(fd, ReadOnlySpan<byte>.Empty, 0).Value == 0 &&
                !io.TruncateFile(fd, -1).Succeeded && observer.Begins == begins,
                "invalid and empty operations admit no writer");

            observer.WatchAddress = first.Address + 1;
            Check(io.TruncateFile(fd, 1).Succeeded && observer.Before == (byte)'b' && observer.After == 0 &&
                observer.LastLogical && observer.Active == 0 && !InstanceIo.MemoryFilePageAvailable(second.Lease),
                "truncate encloses leased zeroing and EOF availability change");
            observer.WatchAddress = 0;
            begins = observer.Begins;
            Check(io.TruncateFile(fd, 1L << 40).Succeeded && observer.Begins == begins + 1 &&
                observer.LastLogical && InstanceIo.MemoryFilePageAvailable(second.Lease),
                "sparse EOF growth with no byte copy still revokes authority");
            Check(io.TruncateFile(fd, 8192).Succeeded, "restore bounded EOF");

            // Isolate hook overhead from WriteMemoryFile's existing List allocation.
            // Alternating sparse EOF needs no storage, temporary collections or tasks.
            for (int i = 0; i < 256; ++i) io.TruncateFile(fd, 8192 + (i & 1));
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            bool succeeded = true;
            for (int i = 0; i < 2048; ++i) succeeded &= io.TruncateFile(fd, 8192 + (i & 1)).Succeeded;
            allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
            Check(succeeded && allocated == 0 && observer.Begins == observer.Ends && observer.Active == 0,
                "warmed sparse EOF mutation scopes allocate no managed objects");

            Check(io.Close(fd).Succeeded, "close keeps active backing leases alive");
            fd = -1;
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
            begins = observer.Begins;
            Check(InstanceIo.ReleaseMemoryFilePage(second.Lease).Succeeded, "release zero backing after I/O disposal");
            second = default;
            Check(InstanceIo.ReleaseMemoryFilePage(first.Lease).Succeeded, "release last backing and reclaim closed file");
            first = default;
            Check(observer.Begins > begins && observer.Peak >= 2 && observer.Active == 0 && observer.Begins == observer.Ends &&
                io.MemoryFilesUsage().Bytes == 0 && io.MemoryFilesUsage().Files == 0 && io.MemoryFilesUsage().Leases == 0,
                "lease removal, free, and final reclaim balance nested admission after I/O disposal");
        }
        finally
        {
            observer.Reject = false; observer.WatchAddress = 0;
            if (fd >= 0) io.Close(fd);
            if (second.Lease != 0) InstanceIo.ReleaseMemoryFilePage(second.Lease);
            if (first.Lease != 0) InstanceIo.ReleaseMemoryFilePage(first.Lease);
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
            io.UnbindMemoryFileMutation(observer);
        }
    }

    static void CheckHostRegularSharedMutation()
    {
        using var files = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>
            { ["/immutable"] = "image"u8.ToArray() });
        var io = new InstanceIo(files);
        var observer = new HostMemoryFileMutationProbe();
        io.BindMemoryFileMutation(observer);
        int fd = io.OpenFile("/regular", FileAccessMode.Read | FileAccessMode.Write, create: true).Value;
        int replacementFd = -1, immutableFd = -1;
        MemoryFilePage first = default, second = default, alias = default, replacement = default;
        try
        {
            Check(io.WriteAt(fd, "abcd"u8, 0).Succeeded && io.TruncateFile(fd, 8192).Succeeded,
                "prepare regular backing before publication");
            first = io.AcquireMemoryFilePage(fd, 0).Value;
            second = io.AcquireMemoryFilePage(fd, 4096).Value;
            Check(first.Lease != 0 && second.Lease != 0 && observer.Begins == 0,
                "regular initial loads are unpublished and cause no false mutation");
            Check(!io.MemoryFileMutationOwned(first.Lease) && !io.MemoryFileMutationOwned(second.Lease),
                "regular shared leases never gain multi-instruction eligibility from host observer hooks");
            observer.WatchAddress = first.Address;
            Check(io.WriteAt(fd, "r"u8, 0).Value == 1 && observer.Before == (byte)'a' && observer.After == (byte)'r' &&
                observer.LastLogical && observer.Active == 0, "regular writer encloses resident bytes and EOF publication");
            int ends = observer.Ends;
            observer.Reject = true;
            bool rejected = false;
            try { io.WriteAt(fd, "z"u8, 0); }
            catch (InvalidOperationException) { rejected = true; }
            Span<byte> backend = stackalloc byte[1];
            Check(rejected && io.ReadAt(fd, backend, 0).Value == 1 && backend[0] == (byte)'r' &&
                ((byte*)first.Address)[0] == (byte)'r' && observer.Ends == ends && observer.Active == 0,
                "regular admission failure precedes backend commit as well as resident copy");

            observer.WatchAddress = first.Address + 4095;
            int begins = observer.Begins;
            Check(io.WriteAt(fd, "xy"u8, 4095).Value == 2 && observer.Before == 0 && observer.After == (byte)'x' &&
                ((byte*)second.Address)[0] == (byte)'y' && observer.Begins == begins + 1 && observer.Active == 0,
                "one regular writer scope covers copies to both resident pages");
            observer.WatchAddress = 0;
            begins = observer.Begins;
            Check(io.WriteAt(fd, ReadOnlySpan<byte>.Empty, 0).Value == 0 && !io.WriteAt(fd, "z"u8, -1).Succeeded &&
                observer.Begins == begins, "empty and invalid regular writes admit no writer");

            Check(io.TruncateFile(fd, 4096).Succeeded && !InstanceIo.MemoryFilePageAvailable(second.Lease) &&
                io.TruncateFile("/regular", 8192).Succeeded && InstanceIo.MemoryFilePageAvailable(second.Lease),
                "descriptor and path truncation guard regular cache and EOF");
            // Controlled out-of-band updates exercise refresh only. No concurrent
            // external-filesystem coherence is promised by the host observer.
            begins = observer.Begins;
            Check(files.Truncate("/regular", 1).Succeeded && observer.Begins == begins,
                "external filesystem operations are outside InstanceIo enrollment");
            observer.WatchAddress = first.Address + 1;
            Check(!InstanceIo.MemoryFilePageAvailable(second.Lease) && observer.Begins == begins + 1 &&
                observer.Before == (byte)'b' && observer.After == 0 && observer.LastLogical && observer.Active == 0,
                "availability refresh guards local EOF and leased zeroing without a write entry");
            observer.WatchAddress = 0;
            Check(files.Truncate("/regular", 8192).Succeeded, "prepare acquisition EOF refresh");
            begins = observer.Begins;
            alias = io.AcquireMemoryFilePage(fd, 4096).Value;
            Check(alias.Lease != 0 && alias.Address == second.Address && observer.Begins == begins + 1 &&
                observer.LastLogical && InstanceIo.MemoryFilePageAvailable(second.Lease),
                "page acquisition guards EOF refresh before reusing an exposed page");

            Check(io.UnlinkAt(-100, "/regular").Succeeded, "unlink preserves leased open-file identity");
            replacementFd = io.OpenFile("/regular", FileAccessMode.Read | FileAccessMode.Write, create: true).Value;
            Check(io.WriteAt(replacementFd, "new"u8, 0).Value == 3, "prepare replacement inode");
            replacement = io.AcquireMemoryFilePage(replacementFd, 0).Value;
            Check(replacement.Lease != 0 && replacement.Address != first.Address &&
                io.WriteAt(fd, "o"u8, 0).Value == 1 && ((byte*)first.Address)[0] == (byte)'o' &&
                ((byte*)replacement.Address)[0] == (byte)'n' && InstanceIo.MemoryFilePageAvailable(first.Lease),
                "replacement inode cannot repoint retained backing of an unlinked file");

            immutableFd = io.OpenFile("/immutable", FileAccessMode.Read).Value;
            begins = observer.Begins;
            Check(io.SharedFileReadOnly(immutableFd).Value == 1 && !io.AcquireMemoryFilePage(immutableFd, 0).Succeeded &&
                !io.WriteAt(immutableFd, "z"u8, 0).Succeeded && !io.TruncateFile(immutableFd, 0).Succeeded &&
                observer.Begins == begins, "immutable VFS images have no mutable shared backing");
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
            fd = replacementFd = immutableFd = -1;
            Check(InstanceIo.MemoryFilePageAvailable(first.Lease) && InstanceIo.MemoryFilePageAvailable(replacement.Lease) &&
                observer.Active == 0 && observer.Begins == observer.Ends,
                "regular reader detachment is guarded and resident leases survive I/O disposal");
        }
        finally
        {
            observer.Reject = false; observer.WatchAddress = 0;
            if (immutableFd >= 0) io.Close(immutableFd);
            if (replacementFd >= 0) io.Close(replacementFd);
            if (fd >= 0) io.Close(fd);
            if (replacement.Lease != 0) InstanceIo.ReleaseMemoryFilePage(replacement.Lease);
            if (alias.Lease != 0) InstanceIo.ReleaseMemoryFilePage(alias.Lease);
            if (second.Lease != 0) InstanceIo.ReleaseMemoryFilePage(second.Lease);
            if (first.Lease != 0) InstanceIo.ReleaseMemoryFilePage(first.Lease);
            io.DisposeAsync().AsTask().GetAwaiter().GetResult();
            io.UnbindMemoryFileMutation(observer);
        }
    }

    static void CheckHostMemoryFileMutationOverlap()
    {
        var a = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        var b = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var overlap = new Barrier(2);
        var observer = new HostMemoryFileMutationProbe();
        int af = a.CreateMemoryFile("writer-a", 0).Value, bf = b.CreateMemoryFile("writer-b", 0).Value;
        a.TruncateFile(af, 4096); b.TruncateFile(bf, 4096);
        var ap = a.AcquireMemoryFilePage(af, 0).Value;
        var bp = b.AcquireMemoryFilePage(bf, 0).Value;
        a.BindMemoryFileMutation(observer); b.BindMemoryFileMutation(observer);
        try
        {
            Check(a.MemoryFileMutationOwned(ap.Lease) && b.MemoryFileMutationOwned(bp.Lease) &&
                !a.MemoryFileMutationOwned(bp.Lease) && !b.MemoryFileMutationOwned(ap.Lease),
                "observer enrollment checks exact per-instance lease ownership");
            observer.Overlap = overlap;
            var aw = Task.Run(() => a.WriteAt(af, "a"u8, 0));
            var bw = Task.Run(() => b.WriteAt(bf, "b"u8, 0));
            Task.WaitAll(aw, bw);
            Check(aw.Result.Value == 1 && bw.Result.Value == 1 && observer.Peak == 2 && observer.Active == 0 &&
                observer.Begins == 2 && observer.Ends == 2,
                "shared observer balances overlapping writers from independent I/O instances");
        }
        finally
        {
            observer.Overlap = null;
            a.Close(af); b.Close(bf);
            InstanceIo.ReleaseMemoryFilePage(ap.Lease); InstanceIo.ReleaseMemoryFilePage(bp.Lease);
            a.DisposeAsync().AsTask().GetAwaiter().GetResult(); b.DisposeAsync().AsTask().GetAwaiter().GetResult();
            a.UnbindMemoryFileMutation(observer); b.UnbindMemoryFileMutation(observer);
        }
    }
}
