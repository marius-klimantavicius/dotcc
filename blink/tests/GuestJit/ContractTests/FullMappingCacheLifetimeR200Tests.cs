using Managed.Emulation.Execution;
using Lifetime200Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE AUTHORED ONLY. Integrated through the existing R193 group so the
    // parent runs one combined serial gate, both producers and both scaffolds.
    static void CheckFullMappingCacheLifetimeR200()
    {
        Check(FullNativeMemory.TlbCount == 256 && FullBlockFrame.ByteSize == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007 && FullWorkerNativeState.MappingCacheCharge == 24 &&
            FullCodeAuthority.MappingCacheCharge == 96, "R204 charged managed metadata; R200 default capacity/frame unchanged");
        Mapping200Lifecycle();
        Mapping200Denials();
        Mapping200Exhaustion();
        Mapping200Publication();
        Mapping200PublicationNative();
        Mapping200PublicationFailure();
        Mapping200RealChanges();
        foreach (string kind in new[] { "store", "scalar-fallback", "pair", "strings", "REP" })
        foreach (int offset in new[] { 64, 4095 }) Mapping200Controller(kind, offset);
        // Independent real MMU, reader/provenance, cancellation and exact fault
        // oracles remain in R193's surrounding group. These add native ticket
        // failure/unwind and backing/permission cases without a new executor.
        ScalarAtomicMapperDenials(); ScalarAtomicAdmissionForgery();
        ScalarAtomicFileDenials(); ScalarAtomicSnapshotInputs();
        CheckFullPublicationCleanupFastPath();
        CheckFullRepSelfModifyingAlias(); CheckRepR104DeniedInitialAdmission();
        CheckRepKernelFailurePaths(); AtomicFamilyPendingFaultAndStop();
    }

    static void Mapping200Lifecycle()
    {
        foreach (bool promote in new[] { false, true })
        {
            using var f = new Lifetime200Fixture();
            ulong data = Lifetime200Fixture.Base; f.Map(data); f.Fill(data);
            var before = Mapping193Seed(f, data, FullMemoryAccess.Write);
            int slot = FullNativeMemory.TlbIndex(data, 63), worker = f.WorkerId(0);
            var a = f.Jit.Authority; var stamp = f.MappingState193.TestMappingStamp;
            ulong runtime = a.Epoch, mapping = a.MappingAuthorityEpoch;
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(worker), "R200 own reservation publishes and drains");
                var frame = new FullBlockFrame { ObservedGlobalEpoch = a.Epoch, MemoryTlb = f.MappingState193.Tlb };
                Check(!f.PrepareMapping200(&frame, true, false, out bool discarded) && !discarded &&
                    frame.MemoryTlb == null && f.MappingState193.TestMappingStamp == stamp &&
                    f.MappingState193.Tlb[slot].AuthorityEpoch == before.AuthorityEpoch,
                    "R200 initial own pending phase retains stale storage with NO ordinary hint capability");
                // Pre-promotion with an old finite reader still admitted. This
                // retention cannot promote, enroll, fill or authorize an atomic.
                using (a.EnterCpuSemanticRead(f.WorkerId(1)))
                {
                    frame.ObservedGlobalEpoch = a.Epoch;
                    Check(!f.PrepareMapping200(&frame, true, false, out discarded) && !discarded &&
                        !a.TryPromoteNativeTicket(worker, (nint)before.Host) &&
                        !a.TryEnterNative(a.Epoch) && f.MappingState193.TestMappingStamp == stamp,
                        "R200 own pending preservation is inert while old semantic reader blocks promotion");
                }
                if (promote)
                {
                    Check(a.TryPromoteNativeTicket(worker, (nint)before.Host), "R200 real promotion");
                    frame.ObservedGlobalEpoch = a.Epoch;
                    frame.MemoryFlags = FullMemoryFlags.BarrierTicket; frame.MemoryTicket = a.Epoch;
                    for (int phase = 0; phase < 2; ++phase)
                        Check(!f.PrepareMapping200(&frame, true, true, out discarded) && !discarded &&
                            frame.MemoryTlb == null && frame.MemoryFlags == FullMemoryFlags.BarrierTicket &&
                            frame.MemoryTicket == a.Epoch && f.MappingState193.TestMappingStamp == stamp &&
                            f.MappingState193.Tlb[slot].AuthorityEpoch == before.AuthorityEpoch,
                            "R200 promoted prefill/final phases neither retag hints nor lose existing barrier ticket");
                    Check(a.TryEnterNativeTicket(worker, a.Epoch), "R200 real owned native enrollment");
                    a.ExitNative(); // RET before relinquishing ticket exclusion
                    a.EndNativeTicket(worker);
                }
                else a.AbortNativeReservation(worker);
                Check(a.MappingAuthorityEpoch == mapping && a.Epoch > runtime &&
                    f.MappingState193.TestMappingStamp == stamp,
                    "R200 cancellation/release changes no mapping version or cached stamps");
                Check(f.MappingState193.PrepareMappingCache(a, a.Epoch) &&
                    f.MappingState193.Tlb[slot].Host == before.Host &&
                    f.MappingState193.Tlb[slot].AuthorityEpoch == a.Epoch &&
                    f.MappingState193.Tlb[slot].Permissions == before.Permissions,
                    "R200 ONLY actual ordinary-open Gate revalidation retags the same checked rights");
            }
        }
    }

    static void Mapping200Denials()
    {
        foreach (bool promoted in new[] { false, true })
        foreach (string denial in new[] { "foreign", "phase", "unowned", "malformed", "no-provenance", "stale",
            "mapping", "first-code", "borrow", "host", "copy", "saturation", "ordinary", "quarantine", "race" })
        {
            using var f = new Lifetime200Fixture(); ulong data = Lifetime200Fixture.Base;
            f.Map(data); f.Fill(data); var old = Mapping193Seed(f, data, FullMemoryAccess.Write);
            var a = f.Jit.Authority; int worker = f.WorkerId(0), slot = FullNativeMemory.TlbIndex(data, 63);
            lock (a.Gate)
            {
                if (denial == "no-provenance")
                    Check(!f.MappingState193.PrepareMappingCache(a, 0), "R200 erase provenance before own exclusion");
                Check(a.TryReserveNativeTicket(worker), "R200 denial owns real reservation");
                if (promoted) Check(a.TryPromoteNativeTicket(worker, (nint)old.Host), "R200 denial owns real ticket");
                var phase = promoted ? FullMappingCachePhase.OwnTicket : FullMappingCachePhase.OwnReservation;
                int claimed = worker; ulong observed = a.Epoch;
                switch (denial)
                {
                    case "foreign": claimed = f.WorkerId(1); break;
                    case "phase": phase = promoted ? FullMappingCachePhase.OwnReservation : FullMappingCachePhase.OwnTicket; break;
                    case "unowned": a.AbortNativeReservation(worker); break;
                    case "malformed": phase = (FullMappingCachePhase)999; break;
                    case "ordinary": phase = FullMappingCachePhase.Ordinary; claimed = -1; break;
                    case "stale": --observed; break;
                    case "mapping": a.BeginMappingMutation(); a.EndMappingMutation(); break;
                    case "first-code": Check(a.PrepareCodeSnapshot((nint)old.Host), "R200 first Code still invalidates mapping"); break;
                    case "borrow": a.Expose(f.WorkerId(1), (nint)old.Host); break;
                    case "host": a.BeginHostWriter(); break;
                    case "copy": a.BeginGuestWrite(f.WorkerId(1)); break;
                    case "saturation": a.TestMappingSaturate200(); break;
                    case "quarantine": a.QuarantineSemanticAdmission(); break;
                    case "race":
                        f.MappingState193.TestMappingPreserving200 = authority =>
                        { authority.BeginHostWriter(); authority.EndHostWriter(); }; break;
                }
                if (denial is not ("stale" or "no-provenance")) observed = a.Epoch;
                try
                {
                    Check(!f.MappingState193.PrepareMappingCache(a, observed, phase, claimed, out bool discarded) &&
                        discarded && f.MappingState193.TestMappingStamp == (0UL, 0UL) &&
                        f.MappingState193.Tlb[slot].Host == null,
                        "R200 exclusion preservation fails closed: " + denial + "/" + promoted);
                }
                finally
                {
                    f.MappingState193.TestMappingPreserving200 = null;
                    if (denial == "host") a.EndHostWriter();
                    if (denial == "copy") a.EndGuestWrite(f.WorkerId(1));
                    if (denial == "borrow") a.EndBorrow(f.WorkerId(1));
                    if (promoted) a.EndNativeTicket(worker); else a.AbortNativeReservation(worker);
                }
            }
        }
        // Exercise the actual controller's Worker -> state/id binding with a
        // real FOREIGN reservation, rather than merely a wrong claimed id.
        using (var f = new Lifetime200Fixture())
        {
            ulong data = Lifetime200Fixture.Base; f.Map(data); f.Fill(data); Mapping193Seed(f, data);
            var a = f.Jit.Authority; int foreign = f.WorkerId(1);
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(foreign), "R200 real foreign reservation");
                try
                {
                    var frame = new FullBlockFrame { ObservedGlobalEpoch = a.Epoch, MemoryTlb = f.MappingState193.Tlb };
                    Check(!f.PrepareMapping200(&frame, true, false, out bool discarded) && discarded &&
                        frame.MemoryTlb == null && f.MappingState193.TestMappingStamp == (0UL, 0UL),
                        "R200 controller cannot preserve under foreign reservation even with matching mapping version");
                }
                finally { a.AbortNativeReservation(foreign); }
            }
        }
        // Ordinary context and invalid worker must not masquerade as OWN.
        foreach (int worker in new[] { -1, 123456 })
        {
            using var f = new Lifetime200Fixture(); ulong data = Lifetime200Fixture.Base;
            f.Map(data); f.Fill(data); Mapping193Seed(f, data);
            lock (f.Jit.Authority.Gate)
                Check(!f.MappingState193.PrepareMappingCache(f.Jit.Authority, f.Jit.Authority.Epoch,
                    FullMappingCachePhase.OwnReservation, worker, out bool discarded) && discarded,
                    "R200 missing/out-of-range owner never preserves");
        }
    }

    static void Mapping200Publication()
    {
        using var f = new Lifetime200Fixture(); ulong data = Lifetime200Fixture.Base;
        f.Map(data); f.Fill(data); var old = Mapping193Seed(f, data, FullMemoryAccess.Write);
        ulong mapping = f.Jit.Authority.MappingAuthorityEpoch, runtime = f.Jit.Authority.Epoch;
        f.Jit.TestCleanupPublicationOwner(f.WorkerId(0));
        try
        {
            Check(f.Jit.Authority.Epoch > runtime && f.Jit.Authority.MappingAuthorityEpoch == mapping &&
                !f.Jit.Store.TryBeginNativeEpoch(out _), "R200 pure host-code publication revokes runtime and closes store enrollment");
            f.Jit.CancelPublication(f.WorkerId(1));
            Check(!f.Jit.Store.TryBeginNativeEpoch(out _), "R200 foreign cancellation cannot reopen publication");
        }
        finally { f.Jit.CancelPublication(f.WorkerId(0)); }
        Check(Mapping193Prepare(f) && f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == old.Host &&
            f.Jit.Authority.MappingAuthorityEpoch == mapping,
            "R200 exact publication cancellation retains checked data mapping");
        Check(f.Jit.Store.TryBeginNativeEpoch(out var epoch), "R200 publication owner reopens store admission");
        epoch.Dispose();
    }

    static void Mapping200Exhaustion()
    {
        // Raw owner/state isolates theoretical exhausted words from guest
        // teardown, which must itself perform real mapping invalidations.
        foreach (bool promoted in new[] { false, true })
        foreach (bool runtime in new[] { false, true })
        {
            using var io = new SemanticTestIo();
            using var a = new FullCodeAuthority(io.Value, 4096, 1, 16384);
            using var state = new FullWorkerNativeState();
            lock (a.Gate)
            {
                Check(state.PrepareMappingCache(a, a.Epoch) && a.TryReserveNativeTicket(0), "R200 empty valid provenance and reservation");
                if (promoted) Check(a.TryPromoteNativeTicket(0, 0), "R200 bounds owned ticket");
                if (runtime) *a.EpochPointer = ulong.MaxValue; else a.TestMappingExhaustion();
                Check(!state.PrepareMappingCache(a, a.Epoch, promoted ? FullMappingCachePhase.OwnTicket :
                    FullMappingCachePhase.OwnReservation, 0, out bool discarded) && discarded &&
                    state.TestMappingStamp == (0UL, 0UL), "R200 exhausted own phase always drops provenance");
                if (promoted)
                {
                    a.EndNativeTicket(0);
                    Check(a.MappingWriters == 0 && a.PendingAtomicWorker == -1 &&
                        (!runtime || (a.SemanticQuarantined && !a.TryEnterNative(a.Epoch))),
                        "R200/R201 runtime exhaustion quarantines and releases exact ticket without throwing from cleanup");
                }
                else a.AbortNativeReservation(0);
            }
        }
    }

    static void Mapping200PublicationFailure()
    {
        using var io = new SemanticTestIo();
        using var jit = FullTranslationJit.Create(io.Value, new() { Mode = GuestJitMode.Full,
            CacheLimitBytes = Lifetime200Fixture.Budget, AllowUnverifiedWindows = true }, 2, Lifetime200Fixture.MemoryLimit);
        var a = jit.Authority;
        ulong mapping = a.MappingAuthorityEpoch; *a.EpochPointer = ulong.MaxValue;
        bool failed = false;
        try { jit.TestCleanupPublicationOwner(0); }
        catch (InvalidOperationException) { failed = true; }
        bool open = jit.Store.TryBeginNativeEpoch(out var reopened);
        Check(failed && a.SemanticQuarantined && a.MappingAuthorityEpoch == mapping && open,
            "R200 failed runtime pulse unwinds publication designation/store pause and closes authority");
        reopened.Dispose();
        jit.CancelPublication(0); // no designation remained to cancel
    }

    static void Mapping200PublicationNative()
    {
        using var f = new Lifetime200Fixture(); using var deadline = f.AtomicDeadline();
        ulong pc = Lifetime200Fixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0x48,0x8b,0x06,0x48,0x8b,0x06,0xeb,0xfe]);
        *(ulong*)((byte*)f.Physical(data) + 64) = 0x200;
        Write64((byte*)f.Machine(), 72, data + 64); f.Counts(2);
        Check(f.Step(pc, signalActor: true) && f.Completed == 1, "R200 publication native seed");
        long assists = f.Statistics.NativeMemoryAssists; ulong mapping = f.Jit.Authority.MappingAuthorityEpoch;
        f.Jit.TestCleanupPublicationOwner(f.WorkerId(0));
        f.Jit.CancelPublication(f.WorkerId(0));
        Check(f.Continue(signalActor: true) && f.Completed == 2 && f.Issued == 2 &&
            f.Statistics.NativeMemoryAssists == assists && RepRegister(f, 24) == 0x200 &&
            f.Jit.Authority.MappingAuthorityEpoch == mapping,
            "R200 actual native data access after publication/cancellation retains checked cache without assist");
        RetainedClean(f);
    }

    static void Mapping200RealChanges()
    {
        foreach (string change in new[] { "RO", "unmap", "remap", "recycle", "EOF", "code" })
        {
            using var f = new Lifetime200Fixture(); ulong data = Lifetime200Fixture.Base, requestAddress = data;
            int fd = -1;
            f.Map(data); f.Fill(data);
            if (change == "EOF")
            {
                // Shared backing deliberately has no ordinary TLB authority,
                // even for Read; real controller access uses an inspected ticket.
                // Observe its truncation invalidating a separate checked private
                // hint, then require the exact EOF fault at the shared address.
                requestAddress = data + 0x10000;
                var file = f.Io.CreateMemoryFile("r200-lifetime", 0); Check(file.Succeeded, "R200 EOF file"); fd = file.Value;
                Check(f.Io.TruncateFile(fd, 4096).Succeeded, "R200 EOF initial extent");
                f.MapShared(fd, requestAddress); f.Fill(requestAddress);
            }
            var before = Mapping193Seed(f, data, FullMemoryAccess.Write);
            var a = f.Jit.Authority; int slot = FullNativeMemory.TlbIndex(data, 63), worker = f.WorkerId(0);
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(worker) && a.TryPromoteNativeTicket(worker, (nint)before.Host),
                    "R200 real-change prelude has actual ticket lifecycle");
                Check(!f.MappingState193.PrepareMappingCache(a, a.Epoch, FullMappingCachePhase.OwnTicket, worker, out bool discarded) &&
                    !discarded, "R200 prelude preserves dormant mapping");
                a.EndNativeTicket(worker);
            }
            ulong mapping = a.MappingAuthorityEpoch;
            switch (change)
            {
                case "RO": f.MappingReadOnly193(data); break;
                case "unmap": Check(f.Mutate("munmap", data) == 0, "R200 actual unmap"); break;
                case "remap": Check(f.Mutate("MAP_FIXED", data) == 0, "R200 actual MAP_FIXED"); f.Fill(data); break;
                case "recycle":
                    Check(f.Mutate("munmap", data) == 0, "R200 producer detaches PTE before recycle");
                    f.Map(data); f.Fill(data); break;
                case "EOF": Check(f.Io.TruncateFile(fd, 0).Succeeded, "R200 actual shared EOF"); break;
                case "code": lock (a.Gate) Check(a.PrepareCodeSnapshot((nint)before.Host), "R200 actual first Code role"); break;
            }
            Check(a.MappingAuthorityEpoch != mapping && Mapping193Prepare(f) && f.MappingState193.Tlb[slot].Host == null,
                "R200 real change after neutral ticket still clears ordinary hint: " + change);
            var frame = Cache181Request(f.MappingState193.Tlb, requestAddress, FullMemoryAccess.Write);
            var result = FillFullMemoryMmu(f, &frame);
            if (change is "RO" or "unmap" or "EOF")
                Check(result == FullMemoryAssistResult.GuestFault && frame.FaultTrap == 14 &&
                    frame.FaultAddress == requestAddress && frame.FaultCode == (change == "RO" ? 2u : change == "unmap" ? 1u : unchecked((uint)-2)),
                    "R200 real permission/PTE/EOF fault metadata remains exact after neutral cycle");
            else if (change == "code")
                Check(result == FullMemoryAssistResult.WriteBarrier && f.MappingState193.Tlb[slot].Host == null,
                    "R200 first Code role still requires independent barrier ticket");
            else Check(result == FullMemoryAssistResult.Resolved && f.MappingState193.Tlb[slot].AuthorityEpoch == a.Epoch,
                "R200 actual remap/recycle requires fresh checked MMU fill");
            if (fd >= 0) Check(f.Io.Close(fd).Succeeded, "R200 EOF descriptor cleanup");
        }
    }

    static void Mapping200Controller(string kind, int offset)
    {
        using var f = new Lifetime200Fixture(); using var deadline = f.AtomicDeadline();
        ulong pc = Lifetime200Fixture.Base, normal = pc + 0x10000, target = pc + 0x20000, source = pc + 0x30000;
        foreach (ulong page in new[] { pc, normal, target, source }) { f.Map(page, 8192); f.Fill(page, 8192); }
        // One actual normal read, one independently issued ticket operation,
        // then another normal read. The checked normal entry must survive.
        byte[] op = kind switch
        {
            "store" => [0x48,0x89,0x03],
            "scalar-fallback" => [0xf0,0x48,0x01,0x0b],
            "pair" => [0x0f,0xc7,0x0b],
            "strings" => [0x48,0xa5],
            _ => [0xf3,0x48,0xa5]
        };
        WriteControllerCode(f, pc, [0x48,0x8b,0x06, ..op, 0x48,0x8b,0x06, 0xeb,0xfe]);
        byte* n = (byte*)f.Physical(normal), a = (byte*)f.Physical(target), b = (byte*)f.Physical(target + 4096);
        byte* src0 = (byte*)f.Physical(source), src1 = (byte*)f.Physical(source + 4096);
        *(ulong*)(n + 64) = 7;
        for (int k = 0; k < 32; ++k)
        {
            int at = offset + k;
            (at < 4096 ? a + at : b + at - 4096)[0] = 0;
            (at < 4096 ? src0 + at : src1 + at - 4096)[0] = (byte)(k + 1);
        }
        // Real watched physical code aliases, registered BEFORE the normal
        // seed. Ticket revocation is retained at entry and AFTER actual RET.
        foreach (ulong page in new[] { target, target + 4096 })
        {
            Check(f.Acquire(page + 256, 1, out var watched, out _, out int error) && error == 0,
                "R200 real watched code alias"); watched.Dispose();
        }
        Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
        Write64((byte*)f.Machine(), 72, normal + 64); f.Counts(3);
        Check(f.Step(pc, signalActor: true) && f.Completed == 1, "R200 actual native read seeds worker cache");
        int slot = FullNativeMemory.TlbIndex(normal, FullNativeMemory.TlbCount - 1);
        var before = f.MappingState193.Tlb[slot]; var stamp = f.MappingState193.TestMappingStamp;
        Check(before.Host == n && before.AuthorityEpoch != 0, "R200 checked native normal entry exists");
        ulong mapping = f.Jit.Authority.MappingAuthorityEpoch, oldRuntime = f.Jit.Authority.Epoch;
        ulong address = target + (uint)(kind == "scalar-fallback" && offset == 64 ? 65 : offset);
        Write64((byte*)f.Machine(), 48, address); Write64((byte*)f.Machine(), 32, kind == "REP" ? 3UL : 5UL);
        if (kind == "pair") { Write64((byte*)f.Machine(), 24, 0); Write64((byte*)f.Machine(), 40, 0); }
        if (kind is "strings" or "REP")
        { Write64((byte*)f.Machine(), 72, source + (uint)offset); Write64((byte*)f.Machine(), 80, address); }
        int reservations = 0, tickets = 0, preserved = 0;
        var cycleStamp = stamp; ulong cycleEpoch = before.AuthorityEpoch;
        byte* poison = stackalloc byte[4096]; new Span<byte>(poison, 4096).Fill(0xa7);
        nint poisoned = (nint)poison;
        f.MappingState193.TestMappingPreserving200 = _ =>
        {
            ++preserved;
            Check(f.MappingState193.TestMappingStamp == cycleStamp && f.MappingState193.Tlb[slot].AuthorityEpoch == cycleEpoch,
                "R200 actual controller initial/prefill/final OWN phase never retags dormant normal entry");
        };
        f.Jit.Authority.TestReservationPublished = _ =>
        {
            ++reservations;
            cycleStamp = f.MappingState193.TestMappingStamp; cycleEpoch = f.MappingState193.Tlb[slot].AuthorityEpoch;
            // Deliberately install an apparently current ordinary Write/RW
            // pointer to unrelated storage. Ticket paths must not consume it.
            int index = FullNativeMemory.TlbIndex(address, FullNativeMemory.TlbCount - 1);
            f.MappingState193.Tlb[index] = new FullMemoryTlbEntry { Page = address & ~4095UL,
                Host = (byte*)poisoned, Identity = (ulong)poisoned, AuthorityEpoch = f.Jit.Authority.Epoch,
                Context = FullTranslationJit.Context(f.Program, f.Machine(), 1),
                Permissions = FullMemoryAccess.Read | FullMemoryAccess.Write | FullMemoryAccess.Atomic, ValidEnd = 4096 };
            f.NativePrepared = p =>
            {
                var frame = (FullBlockFrame*)p; ++tickets;
                Check(frame->MemoryFlags == FullMemoryFlags.BarrierTicket && frame->MemoryTlb == null &&
                    (kind is "strings" or "REP" ? frame->StackMemory != null && frame->StackMemory->Ticket != 0 :
                        frame->MemoryTicket != 0) && f.Jit.Authority.MappingWriters == 1,
                    "R200 actual owned descriptor/ticket masks even poisoned ordinary TLB");
                frame->NativeLimit = 1;
            };
        };
        try
        {
            Check(f.Continue(signalActor: true) && f.Completed == 2 && f.Issued == 2 && reservations > 0 && tickets > 0 && preserved > 0,
                "R200 real ticket cycle completes SAME issue: " + kind);
            Check(f.Jit.Authority.MappingAuthorityEpoch == mapping && f.Jit.Authority.Epoch > oldRuntime &&
                f.MappingState193.Tlb[slot].Host == before.Host && f.MappingState193.TestMappingStamp == cycleStamp &&
                f.MappingState193.Tlb[slot].AuthorityEpoch == cycleEpoch,
                "R200 temporary cycle preserves exact stale normal entry without mapping invalidation: " + kind);
            for (int k = 0; k < 4096; ++k) Check(poison[k] == 0xa7, "R200 no poisoned ordinary pointer store");
            if (kind is "strings" or "REP")
            {
                int count = kind == "REP" ? 24 : 8;
                for (int k = 0; k < count; ++k)
                { int at = offset + k; Check((at < 4096 ? a + at : b + at - 4096)[0] == (byte)(k + 1), "R200 ticket string copies exact split bytes"); }
            }
            else
            {
                ulong expected = kind == "pair" ? (5UL << 32) | (uint)address : kind == "store" ? 7UL : 5UL;
                int at = (int)(address - target);
                for (int k = 0; k < 8; ++k)
                    Check((at + k < 4096 ? a + at + k : b + at + k - 4096)[0] == (byte)(expected >> (k * 8)),
                        "R200 native scalar/pair fallback changes exact independent backing bytes");
            }
            long assists = f.Statistics.NativeMemoryAssists;
            Write64((byte*)f.Machine(), 72, normal + 64); f.NativePrepared = cap;
            Check(f.Continue(signalActor: true) && f.Completed == 3 && f.Issued == 3 &&
                f.Statistics.NativeMemoryAssists == assists && RepRegister(f, 24) == 7 &&
                f.MappingState193.Tlb[slot].AuthorityEpoch == f.Jit.Authority.Epoch,
                "R200 actual native access AFTER ordinary-open revalidation uses preserved normal mapping without assist: " + kind);
        }
        finally
        {
            f.MappingState193.TestMappingPreserving200 = null; f.Jit.Authority.TestReservationPublished = null;
            f.NativePrepared = null; f.NativeReturned = null;
        }
        RetainedClean(f);
    }
}
