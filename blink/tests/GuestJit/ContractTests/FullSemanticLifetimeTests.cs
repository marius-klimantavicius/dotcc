using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckSemanticPlanCache()
    {
        static FullSemanticJit.Prepared Plan(ulong tag)
        { Blink.XedDecodedInst decoded = default; decoded.length = 1; decoded.op.uimm0 = tag; return new(decoded, null); }
        using var cache = new FullSemanticJit.PlanCache(8);
        ulong[] keys = new ulong[8]; int found = 0;
        for (ulong pc = 0; found < keys.Length; ++pc)
            if ((FullSemanticJit.PlanCache.Hash(pc, 0) & 15) == 15) keys[found++] = pc;
        // Eight colliding keys, with the cluster wrapping past the last cell,
        // must stay resident; collisions alone must not trigger retranslation.
        for (int i = 0; i < keys.Length; ++i)
        {
            Check(!cache.TryFind(keys[i], 0, out int slot), "new plan key absent");
            cache.Bind(keys[i], 0, Plan((ulong)i), (ulong)i, slot);
        }
        for (int i = 0; i < keys.Length; ++i)
        {
            Check(cache.TryFind(keys[i], 0, out int slot) && cache.Get(slot).Decoded.op.uimm0 == (ulong)i,
                "colliding plan remains resident with exact operands");
            cache.Touch(slot, 10);
        }
        Check(cache.Resident == 8 && cache.CapacityEvictions == 0 && cache.LookupCollisions > 0,
            "directory probes do not evict plans");
        for (int i = 0; i < 100; ++i)
        {
            for (int hot = 0; hot < 2; ++hot)
            { Check(cache.TryFind(keys[hot], 0, out int slot), "hot plan retained across cold scan"); cache.Touch(slot, (ulong)i + 20); }
            ulong pc = 0x100000000UL + (uint)i * 4096UL;
            Check(!cache.TryFind(pc, 1, out int target), "cold scan key absent");
            cache.Bind(pc, 1, Plan(pc), (ulong)i + 20, target);
            Check(cache.TryFind(pc, 1, out int stored) && cache.Get(stored).Decoded.op.uimm0 == pc,
                "backward-shift directory remains searchable after replacement");
        }
        Check(cache.Resident == 8 && cache.CapacityEvictions == 100, "capacity-only bounded replacement");
        Check(cache.TryFind(keys[0], 0, out int rebound), "metadata rebind resident key");
        var previous = cache.Get(rebound);
        cache.Bind(keys[0], 0, Plan(0xabcdef), 200, rebound);
        Check(cache.Get(rebound).Decoded.op.uimm0 == 0xabcdef && previous.Decoded.op.uimm0 == 0,
            "metadata patch preserves an in-flight copied plan");
        for (int i = 0; i < 100; ++i) { cache.TryFind(keys[0], 0, out int slot); cache.Touch(slot, 201); }
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; ++i)
        { cache.TryFind(keys[0], 0, out int slot); cache.Touch(slot, 202); _ = cache.Get(slot); }
        Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "warm plan residency performs no managed allocation");
        using var modes = new FullSemanticJit.PlanCache(4);
        modes.Bind(0, 0, Plan(1), 1, -1); modes.Bind(0, 1, Plan(2), 2, -1);
        Check(modes.TryFind(0, 0, out int zero) && modes.TryFind(0, 1, out int other) && zero != other &&
            modes.Get(zero).Decoded.op.uimm0 == 1 && modes.Get(other).Decoded.op.uimm0 == 2,
            "zero IP and distinct guest modes retain independent plans");
        Check(modes.MetadataBytes == FullSemanticJit.PlanCache.Charge(4), "slab and directory storage explicitly charged");
        using var phases = new FullSemanticJit.PlanCache(8);
        ulong phaseClock = 0;
        for (ulong pc = 0; pc < 8; ++pc) phases.Bind(pc, 0, Plan(pc), ++phaseClock, -1);
        for (ulong pc = 0; pc < 6; ++pc)
        { phases.TryFind(pc, 0, out int slot); phases.Touch(slot, ++phaseClock); }
        // Six old protected entries and two probation slots used to make a
        // different six-key phase miss forever, although it fits in eight slots.
        for (int pass = 0; pass < 32; ++pass)
            for (ulong pc = 100; pc < 106; ++pc)
                if (phases.TryFind(pc, 0, out int slot)) phases.Touch(slot, ++phaseClock);
                else phases.Bind(pc, 0, Plan(pc), ++phaseClock, -1);
        long phaseEvictions = phases.CapacityEvictions;
        for (int pass = 0; pass < 8; ++pass)
            for (ulong pc = 100; pc < 106; ++pc)
            {
                Check(phases.TryFind(pc, 0, out int slot), "new fitting phase remains resident after bounded aging");
                phases.Touch(slot, ++phaseClock);
            }
        Check(phases.CapacityEvictions == phaseEvictions, "stable fitting phase stops retranslation");
        cache.Dispose(); Check(cache.Resident == 0, "value cache releases retained arrays on disposal");
    }

    static void CheckSemanticReadLifetime()
    {
        CheckSemanticPlanCache();
        byte* page = (byte*)NativeMemory.AlignedAlloc(4096, 4096);
        try
        {
            using var program = new Blink();
            using var binding = program.__DotCcEnter();
            ulong token = program.BlinkHostMemoryCreateShared(8UL << 20);
            Check(token != 0 && program.BlinkHostMemoryAttach(token) == 0, "semantic read owner attach");
            using (var cache = new FullSemanticJit(new() { CacheLimitBytes = 8L << 20, AllowUnverifiedWindows = true }, 2, 8UL << 20))
            {
                new Span<byte>(page, 4096).Fill(0x5a);
                cache.BeginRead(program, semantic: true);
                try
                {
                    cache.RecycleAnonymousPage(program, null, page, clear: true);
                    Check(new ReadOnlySpan<byte>(page, 4096).IndexOfAnyExcept((byte)0x5a) < 0,
                        "anonymous clear delayed during semantic data use");
                    cache.BeginRead(program, semantic: false);
                    try { throw new InvalidOperationException("walk unwind witness"); }
                    catch (InvalidOperationException e) when (e.Message == "walk unwind witness") { }
                    finally { cache.EndRead(program); }
                    var live = cache.Snapshot(new());
                    Check(live.ActiveDataBorrows == 1 && live.DeferredAnonymousPages == 1 && live.SemanticDataBorrows == 1 &&
                        live.PageTableReadBorrows == 1 && live.DeferredAnonymousRecycles == 1,
                        "nested short walk cannot reclaim outer data borrow");
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    Check(new ReadOnlySpan<byte>(page, 4096).IndexOfAnyExcept((byte)0x5a) < 0, "passive borrow across moving GC");
                }
                finally { cache.EndRead(program); }
                Check(new ReadOnlySpan<byte>(page, 4096).IndexOfAnyExcept((byte)0) < 0, "last data borrower clears retired anonymous page");
                // Exercise the translated physical-owner implementation as
                // well as the managed anonymous retirement queue. The logical
                // unmap must preserve a data reader's bytes and quota charge.
                ulong baseline = program.BlinkHostMemoryBytes();
                byte* mapped = (byte*)program.blink_host_mmap(null, 4096, 3, 0x22, -1, 0);
                Check(mapped != (byte*)(nint)(-1), "translated physical data mapping");
                new Span<byte>(mapped, 4096).Fill(0x6b);
                ulong liveBytes = program.BlinkHostMemoryBytes();
                cache.BeginRead(program, semantic: true);
                try
                {
                    Check(program.blink_host_munmap(mapped, 4096) == 0 && program.BlinkHostMemoryContains(mapped, 4096) == 0,
                        "logical unmap during passive data use");
                    Check(program.BlinkHostMemoryBytes() == liveBytes, "physical retirement stays quota charged");
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    Check(new ReadOnlySpan<byte>(mapped, 4096).IndexOfAnyExcept((byte)0x6b) < 0,
                        "translated physical storage survives logical unmap and GC");
                }
                finally { cache.EndRead(program); }
                Check(program.BlinkHostMemoryBytes() == baseline && program.BlinkHostMemoryMappings() == 0,
                    "last data reader releases physical storage and charge");
                var released = cache.Snapshot(new());
                Check(released.ActiveDataBorrows == 0 && released.DeferredAnonymousPages == 0 && released.PeakDeferredAnonymousPages == 1,
                    "passive data retirement quiescence");
            }
            Check(program.BlinkHostMemoryDetach() == 0 && program.BlinkHostMemoryDestroyShared(token) == 0,
                "semantic read owner teardown");
            // Raw recycle published an external page to this test program's
            // allocator list. No allocation consumes it; program disposal frees
            // its native metadata before the external page is released below.
        }
        finally { NativeMemory.AlignedFree(page); }
    }
}
