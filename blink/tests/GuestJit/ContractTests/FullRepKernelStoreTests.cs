using Managed.Emulation.Execution;

unsafe partial class Program
{
    // Publication/lifetime/quota controls. These emit and seal bytes but never
    // invoke them; actual REP computation is checked by the R100 native group.
    static void CheckFullRepKernelStore()
    {
        CheckExternalCodeQuota();
        CheckRepKernelFailurePaths();
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var pool = new FullRepKernelStore(authority, store, 2, allowUnverifiedWindows: true);
        byte[] scratch = new byte[FullBlockEmitter.RepKernelCapacity];
        FullRepKernelBinding first = default;
        lock (authority.Gate)
        {
            int expected = 0;
            foreach (var op in Enum.GetValues<FullRepOperation>())
            foreach (int width in new[] { 8, 16, 32, 64 })
            foreach (int address in new[] { 32, 64 })
            foreach (int segment in new[] { -1, 4, 5 })
            foreach (var repeat in new[] { FullRepRepeat.Count, FullRepRepeat.WhileEqual, FullRepRepeat.WhileNotEqual })
            {
                var i = new FullRepInstruction(0x400000, 2, op, width, address, segment, repeat);
                if (!FullRepLowering.IsCanonical(in i)) continue;
                Check(pool.TryBind(in i, scratch, out var binding, out var failure), "REP packed specialization publication: " + failure);
                if (expected++ == 0) first = binding;
                long bytes = pool.EmittedBytes;
                var same = i with { Ip = 0x700000, Length = 15 };
                Check(pool.TryBind(in same, scratch, out var reused, out _) && reused == binding && pool.EmittedBytes == bytes,
                    "REP specialization excludes guest PC/length, preserving unchanged native code");
            }
            Check(expected == 120 && pool.Count == 120 && pool.ReservedBytes == store.ExternalReservedBytes &&
                pool.EmittedBytes < pool.ReservedBytes && pool.ReservedBytes < 120L * 16384 &&
                store.ChargedBytes <= RetainedOptions().BudgetBytes,
                "REP finite specialization space packs useful bytes in shared charged regions");
            var lease = pool.Acquire(in first);
            var duplicate = lease;
            Check(store.TryFlush(out _) && pool.IsValid(in lease) && pool.Find(new(0, 2,
                FullRepOperation.Move, 8, 32, -1, FullRepRepeat.Count)) == first,
                "issued REP code capability survives main TB whole flush");
            bool rejected = false;
            try { pool.Dispose(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected && pool.IsValid(in lease), "active issued kernel prevents owner code disposal");
            lease.Dispose();
            Check(!pool.IsValid(in duplicate), "copied REP lease becomes stale when its original ticket is released");
            duplicate.Dispose();
            var warm = new FullRepInstruction(0x500000, 3, FullRepOperation.Move, 8, 32, -1, FullRepRepeat.Count);
            pool.TryBind(in warm, scratch, out _, out _);
            long emitted = pool.EmittedBytes;
            for (int window = 0; window < 8; ++window)
            {
                long allocated = CountRepKernelBindings(pool, in warm, scratch, first, out bool all);
                Console.WriteLine($"REP-resident-bind/{window}: allocated={allocated}");
                Check(all && pool.EmittedBytes == emitted && pool.Count == 120,
                    "warm REP lookup windows retain the exact code and finite manifest");
                if (window >= 5) Check(allocated == 0, "steady resident REP kernel binding allocates zero managed bytes");
            }
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static long CountRepKernelBindings(FullRepKernelStore pool, in FullRepInstruction instruction,
        byte[] staging, FullRepKernelBinding expected, out bool all)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        all = true;
        for (int j = 0; j < 10000; ++j)
            all &= pool.TryBind(in instruction, staging, out var reused, out _) && reused == expected;
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    static void CheckExternalCodeQuota()
    {
        using var store = new RetainedTranslationStore(RetainedOptions() with { BudgetBytes = 1 << 20, PayloadBytes = 0 });
        var token = RetainedPublish(store, 0x1000);
        Check(store.TryAcquireToken(token, out var lease), "external quota lease pressure prerequisite");
        long bytes = (1 << 20) - store.GetStatistics().FixedChargedBytes - 65536;
        Check(!store.TryReserveExternalBytes(bytes, out var failure) && failure == TranslationStoreFailure.LeasedCapacity &&
            store.ExternalReservedBytes == 0, "external code growth cannot reset leased body storage");
        lease.Dispose();
        Check(store.TryReserveExternalBytes(bytes, out _) && store.ExternalReservedBytes == bytes &&
            store.GetStatistics().ExternalReservedBytes == bytes && store.ChargedBytes <= 1 << 20,
            "external code growth uses quiescent bulk reset/empty trim within the original byte budget");
        Check(!store.TryReserve(new(0x2000, 7), 1, 64, 1, out var draft, out failure) && failure == TranslationStoreFailure.Capacity,
            "persistent owner code charge cannot be consumed by body compilation after flush");
        store.Abort(ref draft);
        store.ReleaseExternalBytes(bytes);
        Check(store.TryReserve(new(0x2000, 7), 1, 64, 1, out draft, out _), "actual owner-code release restores quota capacity");
        Check(!store.TryReserveExternalBytes(65536, out failure) && failure == TranslationStoreFailure.NotPublished,
            "external code writer cannot overlap an escaped body draft");
        store.Abort(ref draft);
        Check(store.TryBeginNativeEpoch(out var epoch), "external quota native epoch prerequisite");
        Check(!store.TryReserveExternalBytes(65536, out failure) && failure == TranslationStoreFailure.NativeEpochActive,
            "external code writer cannot overlap native body execution");
        epoch.Dispose();
    }
}
