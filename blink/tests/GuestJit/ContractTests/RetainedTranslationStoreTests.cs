using System.Globalization;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;

unsafe partial class Program
{
    // Root wires this into the contract runner. These tests map/protect code
    // pages but NEVER invoke a native instruction from them. They discriminate
    // retained reuse/lifetime/W^X independently of emitter correctness.
    static void CheckRetainedTranslationStore()
    {
        Check(sizeof(TranslationBlockHeader) == 160, "retained TB header size");
        foreach (var field in new (string Name, int Offset)[] {
            (nameof(TranslationBlockHeader.OwnerCookie), 0), (nameof(TranslationBlockHeader.BodyGeneration), 8),
            (nameof(TranslationBlockHeader.GuestPc), 16), (nameof(TranslationBlockHeader.Entry), 24),
            (nameof(TranslationBlockHeader.Context), 32), (nameof(TranslationBlockHeader.Live), 40),
            (nameof(TranslationBlockHeader.Slot), 44), (nameof(TranslationBlockHeader.Payload), 48),
            (nameof(TranslationBlockHeader.PayloadBytes), 56), (nameof(TranslationBlockHeader.GuestLength), 60),
            (nameof(TranslationBlockHeader.Link0TargetHeader), 64), (nameof(TranslationBlockHeader.Link1TargetHeader), 72),
            (nameof(TranslationBlockHeader.Link0TargetGeneration), 80), (nameof(TranslationBlockHeader.Link1TargetGeneration), 88),
            (nameof(TranslationBlockHeader.Link0Pc), 96), (nameof(TranslationBlockHeader.Link1Pc), 104),
            (nameof(TranslationBlockHeader.CodeEpoch), 112), (nameof(TranslationBlockHeader.ValidatedCodeEpoch), 120),
            (nameof(TranslationBlockHeader.ValidatedGlobalEpoch), 128), (nameof(TranslationBlockHeader.Reserved144), 144),
            (nameof(TranslationBlockHeader.Reserved152), 152) })
            Check(Marshal.OffsetOf<TranslationBlockHeader>(field.Name).ToInt32() == field.Offset, "retained TB header field offset");
        CheckRetainedCollisionsAndPhases();
        CheckRetainedRevocationAndLinks();
        CheckRetainedEpochRetirement();
        CheckRetainedPoolReuseAndQuota();
        CheckRetainedWholeBufferFlush();
        CheckRetainedPhysicalProvenance();
        CheckRetainedDraftAndCapabilities();
        CheckRetainedMetadataCapacity();
    }

    static TranslationStoreOptions RetainedOptions(int bodies = 128) => new()
    {
        BudgetBytes = 4L << 20, RegionBytes = 65536, BodyCapacity = bodies,
        PayloadBytes = 1 << 18, MaximumWorkers = 2, HintsPerWorker = 1,
        AllowUnverifiedWindows = true, RequireNativeFlags = false, PhaseAdmissionInterval = 8
    };

    static TranslationToken RetainedPublish(RetainedTranslationStore store, ulong pc,
        int codeCapacity = 1, int payloadCapacity = 64, ulong context = 7, uint guestLength = 8)
    {
        Check(store.TryReserve(new(pc, context), codeCapacity, payloadCapacity, guestLength,
            out var draft, out var failure), "retained TB reserve: " + failure);
        try
        {
            TranslationToken token = draft.Token;
            if (codeCapacity != 0) { draft.Code.Fill(0x90); draft.Code[0] = 0xc3; }
            draft.Payload.Fill(0x5a);
            Check(store.Publish(ref draft, codeCapacity, payloadCapacity, 1, 11, out failure), "retained TB RX publication");
            return token;
        }
        finally { store.Abort(ref draft); }
    }

    static void CheckRetainedCollisionsAndPhases()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        Check(store.TryCreateWorkerCache(out var worker), "retained worker hint reservation");
        try
        {
            // All twelve keys collide in the ONE worker hint. The historical
            // four-way witness is also included in the first five PCs.
            TranslationToken[] tokens = new TranslationToken[12];
            for (int i = 0; i < tokens.Length; ++i) tokens[i] = RetainedPublish(store, 0x1000UL + (uint)i * 0x100);
            long publications = store.Publications;
            for (int phase = 0; phase < 12; ++phase)
            for (int round = 0; round < 10; ++round)
            for (int i = 0; i < tokens.Length; ++i)
            {
                int index = (i + phase) % tokens.Length;
                Check(store.TryAcquireKey(ref worker, new(0x1000UL + (uint)index * 0x100, 7), out var lease), "owner-wide lookup survives >4 worker colliders");
                Check(lease.Token == tokens[index], "phase-fitting set retains unchanged body generation");
                lease.Dispose();
            }
            Check(store.Publications == publications && store.GetStatistics().CapacityRetirements == 0,
                "worker collisions and fitting phases cause zero publications/evictions");
            // Warm relevant code paths before counting successful warm lookup.
            store.TryAcquireKey(ref worker, new(0x1000, 7), out var warm); warm.Dispose();
            // CoreCLR can allocate a one-time runtime object while OSR/tiering
            // the first long lookup loop (24 B observed in tb1c1/c2). Retain
            // those setup windows; require three following windows to be zero.
            for (int warmup = 0; warmup < 5; ++warmup)
            {
                long bytes = CountRetainedLookups(store, ref worker, out bool warmAll);
                Console.WriteLine($"retained-lookup-warmup/{warmup}: allocated={bytes}");
                Check(warmAll, "retained lookup warmup succeeded");
            }
            for (int sample = 0; sample < 3; ++sample)
            {
                long allocated = CountRetainedLookups(store, ref worker, out bool all);
                Console.WriteLine($"retained-lookup-steady/{sample}: allocated={allocated}");
                Check(all && allocated == 0, "allocation-counted warmed retained lookup has no heap translation objects: all=" + all + " allocated=" + allocated);
            }
            Check(store.Publications == publications, "allocation control reused resident publications");
            var before = store.GetStatistics();
            Check(store.TryAcquireKey(ref worker, new(0x1300, 7), out var hinted), "counted final warm hint"); hinted.Dispose();
            Check(store.TryAcquireToken(tokens[0], out var resumed), "counted token resume"); resumed.Dispose();
            Check(store.TryAcquireKey(new(0x1000, 7), out var shared), "counted explicit shared-index query"); shared.Dispose();
            Check(store.TryAcquireKey(ref worker, new(0x1000, 7), out var missed), "counted colliding worker hint"); missed.Dispose();
            var after = store.GetStatistics();
            Check(after.Lookups - before.Lookups == 4 && after.WorkerLookups - before.WorkerLookups == 2 &&
                after.HintHits - before.HintHits == 1 && after.WorkerHintMisses - before.WorkerHintMisses == 1 &&
                after.DirectKeyLookups - before.DirectKeyLookups == 1 && after.TokenLookups - before.TokenLookups == 1,
                "jump-hint misses exclude direct index queries and resident-token resumes");
        }
        finally { worker.Dispose(); }
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static long CountRetainedLookups(RetainedTranslationStore store, ref TranslationWorkerCache worker, out bool all)
    {
        all = true;
        long start = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; ++i)
        {
            all &= store.TryAcquireKey(ref worker, new(0x1000UL + (uint)(i % 12) * 0x100, 7), out var lease);
            lease.Dispose();
        }
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    static void CheckRetainedRevocationAndLinks()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        var a = RetainedPublish(store, 0x2000); var b = RetainedPublish(store, 0x3000);
        var c = RetainedPublish(store, 0x4000);
        Check(store.TryLink(a, 0, b) && store.TryLink(a, 1, c) && store.TryLink(c, 0, b), "retained links two outgoing/multiple incoming");
        Check(store.TryAcquireToken(a, out var al), "retained source inspection lease");
        Check(store.TryAcquireToken(b, out var bl), "retained target inspection lease");
        try
        {
            nint oldEntry = store.GetHeaderCopy(in bl).Entry;
            long publications = store.Publications;
            Check(store.MarkAuthorityChanged(0x3002, 1) == 1, "revocation overlaps instruction interior");
            var ah = store.GetHeaderCopy(in al); var bh = store.GetHeaderCopy(in bl);
            Check(ah.Link0TargetHeader == 0 && ah.Link1TargetHeader != 0, "reverse adjacency removes only incoming edge to revoked target");
            Check(bh.Live == 0 && bh.Entry == oldEntry && bh.BodyGeneration == b.Generation && bh.CodeEpoch == 2,
                "authority revocation keeps code/body and changes separate code epoch");
            Check(store.TryAcquireKey(new(0x3000, 7), out var revoked), "revoked retained key can be leased for byte validation");
            Check(store.GetPayloadCopySpan(in revoked)[0] == 0x5a, "revoked payload survives for byte validation");
            revoked.Dispose();
            Check(!store.TryAcquireToken(b, out var denied, requireLive: true), "native-authority lookup rejects revoked body");
            denied.Dispose();
            Check(!store.TryRevalidate(b, 1, 1, 12), "stale authority stamp cannot revalidate");
            Check(store.TryRevalidate(b, bh.CodeEpoch, bh.CodeEpoch, 12), "identical-byte rebind revalidates retained body");
            Check(store.GetHeaderCopy(in bl).Entry == oldEntry && store.Publications == publications,
                "same-PC unchanged mapping/protection rebind emits no code");
            Check(store.TryLink(a, 0, b), "revalidated target can be linked again");
            store.MarkAuthorityChanged(0x2000, 8);
            Check(store.GetHeaderCopy(in al).Link0TargetHeader == 0 && store.GetHeaderCopy(in al).Link1TargetHeader == 0,
                "source revocation removes all outgoing links");
            Check(store.RetireContentChanged(b), "true code-byte change retires body");
            Check(!store.TryRevalidate(b, 2, 2, 13), "retired body cannot regain authority");
            Check(store.GetPayloadCopySpan(in bl)[0] == 0x5a, "retired body payload remains readable under prior explicit lease");
        }
        finally { al.Dispose(); bl.Dispose(); }
        var replacement = RetainedPublish(store, 0x3000);
        Check(replacement.Generation != b.Generation, "changed bytes receive new generation");
        Check(!store.TryAcquireToken(b, out var stale), "old token cannot alias");
        stale.Dispose();
    }

    static void CheckRetainedEpochRetirement()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        TranslationToken[] tokens = new TranslationToken[8];
        for (int i = 0; i < tokens.Length; ++i) tokens[i] = RetainedPublish(store, 0x5000UL + (uint)i * 0x100);
        Check(store.TryAcquireToken(tokens[0], out var lease), "managed lease before retirement");
        nint headerPointer = store.GetHeader(in lease);
        Check(store.TryBeginNativeEpoch(out var first), "first global native epoch");
        Check(store.TryBeginNativeEpoch(out var second), "second global native epoch");
        try
        {
            Check(store.GetHeader(in first, tokens[1]) != 0, "epoch authenticates another chained target");
            Check(store.RetireContentChanged(tokens[0]) && store.RetireContentChanged(tokens[1]), "invalidate without waiting for epoch/helper lease");
            Check(store.GetStatistics().RetiredBodies == 2 && store.CollectRetired() == 0,
                "global native epoch protects all retired headers/code/payload");
            Check(((TranslationBlockHeader*)headerPointer)->BodyGeneration == tokens[0].Generation &&
                store.GetPayloadCopySpan(in lease)[0] == 0x5a, "active lease sees stable retired storage");
            Check(!store.TryReserve(new(0x9000, 7), 1, 64, 8, out var failed, out var failure) &&
                failure == TranslationStoreFailure.NativeEpochActive, "any code staging excludes active native epochs");
            store.Abort(ref failed);
            int retired = store.GetStatistics().RetiredBodies;
            first.Dispose();
            Check(store.GetStatistics().RetiredBodies == retired, "remaining epoch protects every chained target");
            bool refused = false; try { store.Dispose(); } catch (InvalidOperationException) { refused = true; }
            Check(refused, "disposal refuses active epochs/leases");
            second.Dispose();
            Check(store.GetStatistics().RetiredBodies == 1, "epoch drain still respects managed helper lease");
            long occupied = store.GetStatistics().AlignedOccupiedCodeBytes;
            lease.Dispose();
            Check(store.GetStatistics().RetiredBodies == 0 && store.GetStatistics().AlignedOccupiedCodeBytes == occupied,
                "drained retired lifetimes do not reclaim arena bytes");
            CheckRetainedPageProtection(((TranslationBlockHeader*)headerPointer)->Entry, writable: false, executable: true);
            var replacement = RetainedPublish(store, 0x5000);
            Check(replacement != tokens[0], "append replacement has a new token");
            Check(!store.TryAcquireToken(tokens[0], out var stale), "append replacement rejects old token"); stale.Dispose();
            Check(store.GetStatistics().CodePageReuses == 0, "individual invalidation never recycles a code page");
        }
        finally { first.Dispose(); second.Dispose(); lease.Dispose(); }
    }

    static void CheckRetainedPoolReuseAndQuota()
    {
        using var store = new RetainedTranslationStore(RetainedOptions(1)); // No artificial one-body slot cap.
        var first = RetainedPublish(store, 0xa000, 1, 101);
        Check(store.TryAcquireToken(first, out var lease), "packed-page first body inspection");
        nint entry = store.GetHeaderCopy(in lease).Entry;
        try
        {
            CheckRetainedPageProtection(entry, writable: false, executable: true);
            Check(store.TryReserve(new(0xb000, 7), 17, 0, 1, out var adjacent, out _), "packed append on a live tail page");
            Check(adjacent.CodePointer == entry + 16, "16-byte pointer advancement packs tiny translations");
            CheckRetainedPageProtection(entry, writable: true, executable: false);
            CheckRetainedPageProtection(adjacent.CodePointer, writable: true, executable: false);
            Check(!store.TryBeginNativeEpoch(out var blocked), "draft closes native admission outside the store lock"); blocked.Dispose();
            bool otherThreadAdmitted = System.Threading.Tasks.Task.Run(() =>
            {
                bool admitted = store.TryBeginNativeEpoch(out var epoch); epoch.Dispose(); return admitted;
            }).GetAwaiter().GetResult();
            Check(!otherThreadAdmitted, "escaped draft spans keep admission closed on another thread");
            Check(!store.TryReserve(new(0xc000, 7), 1, 0, 1, out var secondDraft, out var busy) &&
                busy == TranslationStoreFailure.NotPublished, "one shared translation writer admission"); store.Abort(ref secondDraft);
            adjacent.Code.Fill(0x90);
            store.Abort(ref adjacent);
            CheckRetainedPageProtection(entry, writable: false, executable: true);
            Check(store.TryBeginNativeEpoch(out var restored), "abort restores prior body RX and reopens native admission"); restored.Dispose();
            Check(store.GetHeaderCopy(in lease).Live == 1 && store.GetHeaderCopy(in lease).BodyGeneration == first.Generation,
                "abort preserves prior shared-page translation and generation");
        }
        finally { lease.Dispose(); }
        for (int i = 0; i < 99; ++i) RetainedPublish(store, 0xc000UL + (uint)i * 32, 1, 73 + i % 400);
        var stats = store.GetStatistics();
        Check(stats.ResidentBodies == 100 && stats.TotalFlushes == 0 && stats.UsefulPublishedCodeBytes == 100,
            "fitting tiny translations ignore historical BodyCapacity hint");
        Check(stats.AlignedOccupiedCodeBytes == 100 * 16 + 32 && stats.AlignedOccupiedCodeBytes < Environment.SystemPageSize,
            "aligned code occupancy includes abandoned bytes and packs below a page");
        Check(stats.PageSlackBytes + stats.AlignedOccupiedCodeBytes == Environment.SystemPageSize &&
            stats.RegionSlackBytes + stats.PageSlackBytes + stats.AlignedOccupiedCodeBytes == stats.CodeReservedBytes,
            "page/region slack is separate from useful and aligned bytes");
        Check(stats.FixedChargedBytes + stats.ArenaReservedBytes == stats.ChargedBytes &&
            stats.DataArenaUsedBytes == stats.DescriptorUsedBytes + stats.IndexUsedBytes + stats.PayloadUsedBytes + stats.DataArenaOverheadBytes &&
            stats.DataArenaUsedBytes + stats.DataArenaSlackBytes == stats.DataArenaReservedBytes,
            "all descriptors, grow-in-arena indices, payload, headers and reservation slack are charged");
        Check(stats.NativeReservedBytes <= stats.ChargedBytes && stats.ChargedBytes <= 4L << 20 && stats.PeakChargedBytes == stats.ChargedBytes,
            "dynamic native reservation and peak remain inside the shared configured quota");
        Check(stats.GrowAllocatedBytes == 0 && stats.RetireAllocatedBytes == 0 && stats.PublicationAllocatedBytes == 0 && stats.ProtectionCalls > 0,
            "native arena growth has no managed translation allocation and counts protection calls");
        long occupied = stats.AlignedOccupiedCodeBytes, payload = stats.PayloadUsedBytes;
        store.RetireContentChanged(first);
        Check(store.GetStatistics().AlignedOccupiedCodeBytes == occupied && store.GetStatistics().PayloadUsedBytes == payload,
            "content invalidation does not individually reuse code/payload");
    }

    static void CheckRetainedWholeBufferFlush()
    {
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        Check(store.TryCreateWorkerCache(out var worker), "flush preserves registered worker capability");
        try
        {
            var first = RetainedPublish(store, 0x10000, 4096);
            Check(store.TryAcquireKey(ref worker, new(0x10000, 7), out var held), "whole-buffer pressure held lease");
            nint oldEntry = store.GetHeaderCopy(in held).Entry;
            TranslationToken last = first;
            int admitted = 1;
            for (; admitted < 4096; ++admitted)
            {
                if (!store.TryReserve(new(0x20000UL + (uint)admitted * 32, 7), 4096, 64, 8, out var draft, out var failure))
                {
                    Check(failure == TranslationStoreFailure.LeasedCapacity, "exhaustion refuses reset under any managed lease");
                    Check(store.FlushPending && store.GetStatistics().TotalFlushes == 0, "deferred flush is observable without per-body eviction");
                    break;
                }
                last = draft.Token; draft.Code.Fill(0xc3); draft.Payload.Clear();
                Check(store.Publish(ref draft, 4096, 64, 1, 1, out _), "fill bounded code arena");
            }
            Check(admitted < 4096 && admitted > 1, "byte exhaustion is bounded and not a tiny fixed slot cap");
            Check(store.TryAcquireToken(first, out var stillHeld) && stillHeld.Token == first, "failed flush leaves all prior tokens present"); stillHeld.Dispose();
            Check(!store.TryFlush(out var blocked) && blocked == TranslationStoreFailure.LeasedCapacity, "explicit flush admission rejects managed reader");
            held.Dispose(); // Last capability automatically services pending work.
            Check(store.GetStatistics().TotalFlushes == 1 && !store.FlushPending, "last lease services pending exclusive flush");
            bool firstAbsent = !store.TryAcquireToken(first, out var staleFirst);
            bool lastAbsent = !store.TryAcquireToken(last, out var staleLast);
            Check(firstAbsent && lastAbsent, "all tokens from reset generation are stale");
            staleFirst.Dispose(); staleLast.Dispose();
            Check(!store.TryAcquireKey(ref worker, new(0x10000, 7), out var staleHint), "bulk flush clears nonowning worker hints and exact index"); staleHint.Dispose();
            CheckRetainedPageProtection(oldEntry, writable: false, executable: false);
            var replacement = RetainedPublish(store, 0x10000, 4096);
            Check(store.TryAcquireToken(replacement, out var current), "recycled arena new generation");
            Check(store.GetHeaderCopy(in current).Entry == oldEntry && replacement.Generation != first.Generation,
                "whole-buffer rewind reuses mapping while tokens never alias"); current.Dispose();
            Check(store.GetStatistics().CodePageReuses > 0 && store.GetStatistics().CapacityRetirements == admitted &&
                store.GetStatistics().FlushRetirements == admitted && store.GetStatistics().ContentRetirements == 0,
                "compatibility capacity retirements count whole-flush bodies, separately from actual content changes");

            Check(store.TryBeginNativeEpoch(out var epoch), "native reader before explicit flush");
            Check(!store.TryFlush(out blocked) && blocked == TranslationStoreFailure.NativeEpochActive, "explicit whole reset rejects native reader");
            Check(store.GetHeader(in epoch, replacement) != 0, "pending request preserves admitted native reader storage");
            epoch.Dispose();
            Check(store.GetStatistics().TotalFlushes == 2 && !store.FlushPending, "last native epoch services pending flush");
            Check(store.TryReserve(new(0x30000, 7), 1, 0, 1, out var writer, out _), "draft before explicit flush");
            Check(!store.TryFlush(out blocked) && blocked == TranslationStoreFailure.NotPublished, "explicit whole reset rejects writable draft");
            writer.Code[0] = 0xc3;
            Check(store.GetStatistics().ActiveDrafts == 1, "flush denial preserves draft lifetime");
            store.Abort(ref writer);
            Check(store.GetStatistics().TotalFlushes == 3 && !store.FlushPending, "last draft abort restores protection then services pending flush");

            long flushes = store.GetStatistics().TotalFlushes;
            for (int i = 0; i < 2048; ++i) RetainedPublish(store, 0x40000UL + (uint)i * 32, 4096);
            var stats = store.GetStatistics();
            Check(stats.TotalFlushes > flushes && stats.ChargedBytes <= 4L << 20 &&
                stats.PeakArenaReservedBytes >= stats.ArenaReservedBytes && stats.CumulativeEmittedCodeBytes > stats.UsefulPublishedCodeBytes,
                "unleased byte exhaustion automatically flushes and reuses bounded arenas");
            Check(stats.PhaseDemotions == 0 && stats.FlushDenials >= 3, "no per-body policy remains and busy admission is observable");
            Check(store.TryFlush(out _), "empty reset before changing code/data mix");
            var largePayload = RetainedPublish(store, 0x900000, 0, 2 << 20);
            Check(store.TryAcquireToken(largePayload, out var big), "shared quota rebalances empty code reservations for a payload-heavy phase");
            Check(store.GetPayloadCopySpan(in big).Length == 2 << 20 && store.ChargedBytes <= 4L << 20,
                "arena mix changes retain strict total reservation charge"); big.Dispose();
        }
        finally { worker.Dispose(); }
    }

    static TranslationToken RetainedPublishWithProvenance(RetainedTranslationStore store, ulong keyPc,
        ulong fetchPc, ulong page0, ulong page1 = ulong.MaxValue, uint bytes = 8)
    {
        Check(store.TryReserve(new(keyPc, 7), 0, 64, bytes, out var draft, out _), "provenance draft reservation");
        try
        {
            var token = draft.Token; draft.Payload.Fill(0x5a);
            Check(store.SetProvenance(in draft, fetchPc, page0, page1), "checked actual fetch/physical provenance registration");
            Check(store.Publish(ref draft, 0, 64, 1, 1, out _), "payload-only provenance publication");
            return token;
        }
        finally { store.Abort(ref draft); }
    }

    static void CheckRetainedPhysicalProvenance()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        var a = RetainedPublishWithProvenance(store, 0x1000, 0x800002, 0xa000);
        var alias = RetainedPublishWithProvenance(store, 0x2000, 0x900002, 0xa000);
        var unrelated = RetainedPublishWithProvenance(store, 0x3000, 0x900102, 0xb000);
        var unknown = RetainedPublish(store, 0x4000, 0);
        var crossing = RetainedPublishWithProvenance(store, 0x5000, 0x1000fff, 0x1a000, 0x1b000);
        for (int i = 0; i < 512; ++i)
            RetainedPublishWithProvenance(store, 0x500000UL + (uint)i * 32, 0x2000000UL + (uint)i * 4096, 0x200000UL + (uint)i * 4096);
        Check(store.TryAcquireToken(a, out var al), "physical alias first inspection lease");
        Check(store.TryAcquireToken(alias, out var bl), "physical alias second inspection lease");
        Check(store.TryAcquireToken(unrelated, out var ul), "physical unrelated inspection lease");
        Check(store.TryAcquireToken(unknown, out var kl), "unknown provenance inspection lease");
        try
        {
            Check(store.MarkAuthorityChanged(0x1000, 1) == 0, "semantic key is not mistaken for actual checked fetch address");
            long probes = store.GetStatistics().PhysicalIndexProbes;
            Check(store.MarkPhysicalAuthorityChanged(0xa002, 1) == 3, "physical writer revokes both aliases plus unknown provenance");
            Check(store.GetHeaderCopy(in al).Live == 0 && store.GetHeaderCopy(in bl).Live == 0 && store.GetHeaderCopy(in kl).Live == 0,
                "aliases and generic unknown body lose authority");
            var unaffected = store.GetHeaderCopy(in ul);
            Check(unaffected.Live == 1 && unaffected.CodeEpoch == 1, "physical range index preserves unrelated body authority");
            Check(store.GetStatistics().PhysicalIndexProbes - probes < 64, "targeted physical invalidation probes buckets rather than every retained body");
            Check(store.GetPayloadCopySpan(in al)[0] == 0x5a && store.TryRevalidate(a, 2, 2, 2), "unchanged revoked alias retains emission/payload for revalidation");
            Check(store.SetProvenance(a, 0x800002, 0xc000), "revalidated mapping can register newly checked backing provenance");
            Check(store.MarkPhysicalAuthorityChanged(0xa002, 1) == 2 && store.GetHeaderCopy(in al).Live == 1,
                "rebound provenance unlinks old physical page membership");
            Check(store.MarkAuthorityChanged(0x800003, 1) == 1, "virtual mapping writer uses actual fetch range index");
            Check(store.MarkPhysicalAuthorityChanged(0x1b000, 1) == 2, "second fetched backing page participates in targeted invalidation");
            Check(store.MarkPhysicalAuthorityChanged(0x1b010, 1) == 1, "physical byte ranges spare nonoverlapping bytes in a known page");
            Check(!store.SetProvenance(crossing, 0x1000fff, 0x1a001, 0x1b000), "unaligned backing page provenance is rejected");
        }
        finally { al.Dispose(); bl.Dispose(); ul.Dispose(); kl.Dispose(); }
    }

    static void CheckRetainedDraftAndCapabilities()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        using var other = new RetainedTranslationStore(RetainedOptions());
        var own = RetainedPublish(store, 0x80000); var foreign = RetainedPublish(other, 0x80000);
        Check(!store.TryAcquireToken(foreign, out var wrong), "tokens from another owner cannot alias local generation"); wrong.Dispose();
        Check(store.TryAcquireToken(own, out var lease), "managed lease capability acquisition");
        var copied = lease; copied.Dispose();
        bool refused = false;
        try { store.GetHeader(in lease); } catch (InvalidOperationException) { refused = true; }
        Check(refused, "disposed copy invalidates shared lease ticket"); lease.Dispose();
        Check(store.TryReserve(new(0x81000, 7), 1, 64, 8, out var draft, out _), "draft reservation before emission");
        var stale = draft; TranslationToken old = draft.Token;
        Check(!store.TryAcquireToken(old, out var unpublished), "unpublished draft unavailable to lookup"); unpublished.Dispose();
        Check(!store.TryAcquireKey(new(0x81000, 7), out unpublished, out var notPublished) &&
            notPublished == TranslationStoreFailure.NotPublished, "reserved key distinguishes unpublished from absent"); unpublished.Dispose();
        Check(!store.TryReserve(new(0x81000, 7), 1, 64, 8, out var duplicate, out var failure) &&
            failure == TranslationStoreFailure.AlreadyPresent, "same-key compiler reservation prevents duplicate emission"); store.Abort(ref duplicate);
        store.MarkAuthorityChanged(0x81000, 1);
        Check(!store.Publish(ref draft, 1, 64, 1, 1, out failure) && failure == TranslationStoreFailure.AuthorityChanged,
            "authority changed during compilation prevents publication");
        store.Abort(ref draft);
        var replacement = RetainedPublish(store, 0x81000);
        store.Abort(ref stale);
        Check(replacement.Generation != old.Generation, "replacement generation differs from stale draft");
        Check(store.TryAcquireToken(replacement, out var current), "stale draft abort cannot retire replacement"); current.Dispose();
        Check(store.TryCreateWorkerCache(out var worker), "worker cache for disposal guard");
        refused = false; try { store.Dispose(); } catch (InvalidOperationException) { refused = true; }
        Check(refused, "dispose refuses registered worker until joined/unregistered"); worker.Dispose();
        TranslationLease[] leases = new TranslationLease[24]; // MaximumWorkers*4+16.
        try
        {
            for (int i = 0; i < leases.Length; ++i)
                Check(store.TryAcquireToken(own, out leases[i]), "bounded managed lease ticket admission");
            Check(!store.TryAcquireToken(own, out var exhausted, out var leaseFailure) &&
                leaseFailure == TranslationStoreFailure.LeaseCapacity, "lease exhaustion is explicit rather than a translation miss"); exhausted.Dispose();
        }
        finally { for (int i = 0; i < leases.Length; ++i) leases[i].Dispose(); }
    }

    static void CheckRetainedMetadataCapacity()
    {
        // Payload-only entries are valid store contracts; the root owns the
        // execution policy for each plan. This checks metadata is budgeted,
        // independent of the historical tiny positive-cache slot limit.
        using var store = new RetainedTranslationStore(RetainedOptions(4096));
        for (int i = 0; i < 2049; ++i) RetainedPublish(store, 0x100000UL + (uint)i * 32, 0, 64);
        Check(store.GetStatistics().ResidentBodies == 2049 && store.GetStatistics().CapacityRetirements == 0,
            "byte-funded retained metadata has no 1024-body hard cap");
        for (int i = 0; i < 2049; ++i)
        {
            Check(store.TryAcquireKey(new(0x100000UL + (uint)i * 32, 7), out var lease), "large fitting retained key set");
            lease.Dispose();
        }
        Check(store.Publications == 2049, "large fitting retained key set requires no republication");
    }

    static void CheckRetainedPageProtection(nint address, bool writable, bool executable)
    {
        if (OperatingSystem.IsLinux())
        {
            foreach (string line in File.ReadLines("/proc/self/maps"))
            {
                string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] range = fields[0].Split('-');
                ulong lo = ulong.Parse(range[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                ulong hi = ulong.Parse(range[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if ((ulong)address < lo || (ulong)address >= hi) continue;
                Check((fields[1][1] == 'w') == writable && (fields[1][2] == 'x') == executable, "actual Linux page W^X protection"); return;
            }
            throw new InvalidOperationException("Retained code mapping absent from /proc/self/maps");
        }
        Check(RetainedVirtualQuery(address, out var info, (nuint)sizeof(RetainedMemoryInfo)) != 0, "Windows page protection query");
        uint protection = info.Protect & 0xff;
        bool actualWrite = protection is 0x04 or 0x08 or 0x40 or 0x80;
        bool actualExecute = protection is 0x10 or 0x20 or 0x40 or 0x80;
        Check(actualWrite == writable && actualExecute == executable, "actual Windows page W^X protection");
    }
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    struct RetainedMemoryInfo
    {
        [FieldOffset(0)] public nint BaseAddress;
        [FieldOffset(8)] public nint AllocationBase;
        [FieldOffset(16)] public uint AllocationProtect;
        [FieldOffset(24)] public nuint RegionSize;
        [FieldOffset(32)] public uint State;
        [FieldOffset(36)] public uint Protect;
        [FieldOffset(40)] public uint Type;
    }
    [DllImport("kernel32", EntryPoint = "VirtualQuery", SetLastError = true)]
    static extern nuint RetainedVirtualQuery(nint address, out RetainedMemoryInfo information, nuint length);
}
