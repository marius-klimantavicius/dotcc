using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Role204Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE ONLY, AUTHORED_UNRUN. One grouped serial parent gate for both
    // producer runtimes. Native guards/ABI encodings are independent oracles.
    static void CheckFullCodeRoleMappingsR204(byte* machine)
    {
        Mapping204Roles();
        Mapping204Provenance();
        Mapping204Topology();
        Mapping204Eof();
        Mapping204NativeWriter();
        Mapping204Races();
        Mapping204Bounds();
        Mapping204Atomic();
        foreach (bool store in new[] { false, true })
        foreach (bool samePage in new[] { false, true }) Mapping204Controller(store, samePage);
        CheckFullMemoryRegistrationDrain(); // real native enrollment blocks first Code
        CheckFullMappingCacheEpochR193(machine); // OWN dormant tickets, same issue, REP/EOF/MMU
        CheckFullCodeEpochEncoding(); // actual emitted SysV + Windows scaffolds
        Cache181Encoding(); // unchanged actual memory lookup encoding in both ABIs
    }

    static bool Mapping204Prepare(Role204Fixture f, out bool discarded, ulong context = 0x181)
    {
        var a = f.Jit.Authority;
        lock (a.Gate) return f.MappingState193.PrepareMappingCache(a, a.Epoch,
            FullMappingCachePhase.Ordinary, -1, out discarded, context);
    }
    static void Mapping204Register(Role204Fixture f, ulong pc)
    {
        // Real acquire, not a fabricated role/version. The code page was
        // mapped/materialized before data hints were seeded.
        Check(f.Acquire(pc + 256, 1, out var lease, out _, out int error) && error == 0,
            "R204 actual first guest code acquisition");
        lease.Dispose();
    }
    static void Mapping204Reasons(Role204Fixture f, string scenario)
    {
        var a = f.Jit.Authority;
        Console.WriteLine($"R204 observed {scenario}: roles={a.CodeRoleRegistrations} topology={a.MappingTopologyAdvances} " +
            $"filters={a.CodeRoleCacheFilters} kept={a.CodeRoleCacheKept} provenance={a.CodeRoleCacheProvenanceDrops} " +
            $"physical={a.CodeRoleCachePhysicalDrops} codeWrite={a.CodeRoleCacheWriteDrops} " +
            $"topologyDrop={a.MappingCacheTopologyDrops} fallback={a.MappingCacheFallbackDrops} race={a.MappingCacheRaceDrops}");
    }
    static void Mapping204Roles()
    {
        foreach (var dataAccess in new[] { FullMemoryAccess.Read, FullMemoryAccess.Write })
        foreach (var codeAccess in new[] { FullMemoryAccess.Read, FullMemoryAccess.Write })
        {
            using var f = new Role204Fixture(); ulong data = Role204Fixture.Base, code = data + 8192;
            f.Map(data); f.Fill(data); f.Map(code); f.Fill(code);
            Mapping193Seed(f, data, dataAccess); Mapping193Seed(f, code, codeAccess);
            var dataBefore = Mapping193Seed(f, data, dataAccess);
            var codeBefore = Mapping193Seed(f, code, codeAccess);
            var a = f.Jit.Authority;
            ulong topology = a.MappingTopologyEpoch, mapping = a.MappingAuthorityEpoch, runtime = a.Epoch;
            long registrations = a.CodeRoleRegistrations;
            Mapping204Register(f, code);
            Check(a.MappingTopologyEpoch == topology && a.MappingAuthorityEpoch > mapping && a.Epoch > runtime &&
                a.CodeRoleRegistrations == registrations + 1, "R204 actual first Code is the sole topology-neutral mapping delta");
            long walks = JitPageIdentity.FullTestIdentityReads;
            Check(Mapping204Prepare(f, out bool discarded) && discarded == (codeAccess == FullMemoryAccess.Write) &&
                JitPageIdentity.FullTestIdentityReads == walks, "R204 filtering has no hidden PTE walks");
            var kept = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)];
            Check(kept.Host == dataBefore.Host && kept.Page == dataBefore.Page && kept.Context == dataBefore.Context &&
                kept.Identity == dataBefore.Identity && kept.Permissions == dataBefore.Permissions &&
                kept.Flags == dataBefore.Flags && kept.ValidStart == dataBefore.ValidStart && kept.ValidEnd == dataBefore.ValidEnd &&
                kept.AuthorityEpoch == a.Epoch, "R204 untouched checked data keeps exact provenance and rights");
            var target = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(code, 63)];
            Check(codeAccess == FullMemoryAccess.Write ? target.Host == null :
                target.Host == codeBefore.Host && target.Permissions == FullMemoryAccess.Read && target.AuthorityEpoch == a.Epoch,
                "R204 newly code page loses Write while exact Read remains admissible");
            lock (a.Gate)
            {
                Check(!a.TryGetNativeDataPermission((nint)codeBefore.Host, true, out _) &&
                    a.TryGetNativeDataPermission((nint)codeBefore.Host, false, out _), "R204 independent current physical authority agrees");
                Check(a.TryEnterNative(a.Epoch), "R204 retained hints still require actual current native enrollment"); a.ExitNative();
            }
            Check(a.CodeRoleCacheFilters == 1 && a.CodeRoleCacheKept == (codeAccess == FullMemoryAccess.Read ? 2 : 1) &&
                a.CodeRoleCacheWriteDrops == (codeAccess == FullMemoryAccess.Write ? 1 : 0), "R204 observed reset reasons count actual kept/refused hints");
            Mapping204Reasons(f, $"roles/{dataAccess}/{codeAccess}"); RetainedClean(f);
        }
    }
    static void Mapping204Provenance()
    {
        foreach (string kind in new[] { "context", "epoch", "identity", "null", "unaligned", "page", "canonical", "write-only",
            "atomic", "flags", "start", "end", "unknown", "borrow" })
        {
            using var f = new Role204Fixture(); ulong data = Role204Fixture.Base, code = data + 8192;
            f.Map(data); f.Fill(data); f.Map(code); f.Fill(code);
            if (kind == "borrow") lock (f.Jit.Authority.Gate) f.Jit.Authority.Expose(f.WorkerId(1), f.Physical(data));
            Mapping193Seed(f, data); int slot = FullNativeMemory.TlbIndex(data, 63);
            ref var entry = ref f.MappingState193.Tlb[slot];
            byte* unknown = (byte*)NativeMemory.AlignedAlloc(4096, 4096);
            try
            {
                switch (kind)
                {
                    case "context": entry.Context ^= 1; break;
                    case "epoch": ++entry.AuthorityEpoch; break;
                    case "identity": entry.Identity ^= 4096; break;
                    case "null": entry.Host = null; break;
                    case "unaligned": ++entry.Host; entry.Identity = (ulong)entry.Host; break;
                    case "page": ++entry.Page; break;
                    case "canonical": entry.Page = 0x800000000000UL; break;
                    case "write-only": entry.Permissions = FullMemoryAccess.Write; break;
                    case "atomic": entry.Permissions = FullNativeMemory.AtomicReadWrite; break;
                    case "flags": entry.Flags = FullMemoryFlags.BarrierTicket; break;
                    case "start": entry.ValidStart = 1; break;
                    case "end": entry.ValidEnd = 4095; break;
                    case "unknown": entry.Host = unknown; entry.Identity = (ulong)unknown; break;
                }
                Mapping204Register(f, code);
                Check(Mapping204Prepare(f, out bool discarded) && discarded && f.MappingState193.Tlb[slot].Host == null &&
                    f.Jit.Authority.CodeRoleCacheKept == 0, "R204 malformed/foreign/borrowed hint never promoted: " + kind);
                Check((kind is "unknown" or "borrow" ? f.Jit.Authority.CodeRoleCachePhysicalDrops :
                    f.Jit.Authority.CodeRoleCacheProvenanceDrops) == 1, "R204 denial reports exact cause: " + kind);
            }
            finally { NativeMemory.AlignedFree(unknown); if (kind == "borrow") f.Jit.Authority.EndBorrow(f.WorkerId(1)); }
            RetainedClean(f);
        }
    }
    static void Mapping204Topology()
    {
        foreach (string mutation in new[] { "mprotect", "munmap", "MAP_FIXED", "host", "retire", "borrow", "unknown", "unknown-read", "unknown-copy", "bounded-unknown", "global" })
        {
            using var f = new Role204Fixture(); ulong data = Role204Fixture.Base, code = data + 8192;
            f.Map(data); f.Fill(data); f.Map(code); f.Fill(code);
            var before = Mapping193Seed(f, data, FullMemoryAccess.Write); var a = f.Jit.Authority;
            ulong topology = a.MappingTopologyEpoch;
            Mapping204Register(f, code);
            switch (mutation)
            {
                case "mprotect": f.MappingReadOnly193(data); break;
                case "munmap": case "MAP_FIXED": Check(f.Mutate(mutation, data) == 0, "R204 actual PTE mutation"); break;
                case "host": a.BeginHostWriter(); a.EndHostWriter(); break;
                case "retire": bool precise = a.BeginPhysicalRetirement(true); a.EndPhysicalRetirement(precise); break;
                case "borrow": lock (a.Gate) a.Expose(f.WorkerId(1), (nint)before.Host); a.EndBorrow(f.WorkerId(1)); break;
                case "unknown-read":
                    lock (a.Gate) Check(a.TryGetNativeDataPermission((nint)0x1000, false, out _), "R204 unknown read registration"); break;
                case "unknown-copy": case "bounded-unknown":
                    a.BeginGuestWrite(f.WorkerId(1));
                    try
                    {
                        lock (a.Gate)
                            if (mutation == "unknown-copy") a.NoteGuestWritePages(f.WorkerId(1), (nint)0x1000);
                            else f.Jit.RevokeBoundedCopy(f.WorkerId(1), (nint)0x1000, 0);
                    }
                    finally { a.EndGuestWrite(f.WorkerId(1)); }
                    break;
                case "global": a.BeginMappingMutation(); a.EndMappingMutation(); break;
                default: a.Advance(); break;
            }
            if (mutation == "unknown-read")
            {
                Check(a.MappingTopologyEpoch == topology && Mapping204Prepare(f, out bool readDiscarded) && !readDiscarded &&
                    f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == before.Host &&
                    a.CodeRoleCacheFilters > 0 && a.MappingCacheTopologyDrops == 0,
                    "R204 metadata-only first Read preserves existing checked unrelated data; no topology mutation");
                Mapping204Reasons(f, "metadata/unknown-read"); RetainedClean(f); continue;
            }
            Check(a.MappingTopologyEpoch > topology && Mapping204Prepare(f, out bool discarded) && discarded &&
                f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null && a.CodeRoleCacheFilters == 0 &&
                a.MappingCacheTopologyDrops > 0, "R204 role delta cannot launder real topology: " + mutation);
            if (mutation is "mprotect" or "munmap")
            {
                var request = Cache181Request(f.MappingState193.Tlb, data, FullMemoryAccess.Write);
                Check(FillFullMemoryMmu(f, &request) == FullMemoryAssistResult.GuestFault && request.FaultTrap == 14 &&
                    request.FaultCode == (mutation == "mprotect" ? 2u : 1u), "R204 exact ACCERR/MAPERR after topology denial");
            }
            Mapping204Reasons(f, "topology/" + mutation); RetainedClean(f);
        }
        // Actual truncate/EOF/remap/recycle and owned dormant-cache lifecycle
        // are also covered by Mapping200RealChanges in the grouped R193 suite.
    }
    static void Mapping204Eof()
    {
        using var f = new Role204Fixture(); ulong data = Role204Fixture.Base, code = data + 8192, shared = data + 0x10000;
        f.Map(data); f.Fill(data); f.Map(code); f.Fill(code);
        var file = f.Io.CreateMemoryFile("r204-eof", 0); Check(file.Succeeded, "R204 actual EOF file");
        int fd = file.Value;
        try
        {
            Check(f.Io.TruncateFile(fd, 4096).Succeeded, "R204 EOF initial extent");
            f.MapShared(fd, shared); f.Fill(shared); Mapping193Seed(f, data, FullMemoryAccess.Write);
            Mapping204Register(f, code); var a = f.Jit.Authority; ulong topology = a.MappingTopologyEpoch;
            Check(f.Io.TruncateFile(fd, 0).Succeeded && a.MappingTopologyEpoch > topology,
                "R204 actual EOF remains a topology change after first Code");
            Check(Mapping204Prepare(f, out bool discarded) && discarded &&
                f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null && a.CodeRoleCacheFilters == 0,
                "R204 EOF cannot be mistaken for role-only data preservation");
            var frame = Cache181Request(f.MappingState193.Tlb, shared, FullMemoryAccess.Write);
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.GuestFault && frame.FaultTrap == 14 &&
                frame.FaultCode == unchecked((uint)-2) && frame.FaultAddress == shared,
                "R204 current shared backing reports exact BUS_ADRERR sentinel after EOF");
            Mapping204Reasons(f, "EOF"); RetainedClean(f);
        }
        finally { Check(f.Io.Close(fd).Succeeded, "R204 EOF descriptor cleanup"); }
    }
    static void Mapping204NativeWriter()
    {
        using var f = new Role204Fixture(); using var deadline = f.AtomicDeadline();
        ulong pc = Role204Fixture.Base, data = pc + 0x20000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0x48,0x89,0x06,0xeb,0xfe]);
        nint physical = f.Physical(data); var a = f.Jit.Authority;
        Write64((byte*)f.Machine(), 72, data + 64); Write64((byte*)f.Machine(), 24, 0x8877665544332211); f.Counts(1);
        using var start = new ManualResetEventSlim(); int finished = 0; Exception? error = null;
        var registrar = new Thread(() =>
        {
            try
            {
                AtomicFamilyWait(start, "R204 native store returns while still enrolled");
                lock (a.Gate) Check(a.PrepareCodeSnapshot(physical), "R204 native writer first Code completes after drain");
            }
            catch (Exception e) { error = e; }
            finally { Volatile.Write(ref finished, 1); }
        });
        int returns = 0; ulong topology = 0;
        Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        f.NativePrepared = cap;
        f.NativeReturned = p =>
        {
            f.NativePrepared = cap;
            var frame = (FullBlockFrame*)p;
            if (frame->Executed != 1) return;
            ++returns; topology = a.MappingTopologyEpoch; ulong oldRuntime = a.Epoch;
            Check(a.FullTestR185NativeFrames > 0 && *(ulong*)((byte*)physical + 64) == 0x8877665544332211,
                "R204 actual native writer has stored exact bytes and still owns enrollment");
            start.Set();
            Check(SpinWait.SpinUntil(() => a.Epoch != oldRuntime, TimeSpan.FromSeconds(5)) &&
                Volatile.Read(ref finished) == 0 && a.MappingWriters > 0 && !a.TryEnterNative(a.Epoch),
                "R204 first Code revokes and waits for the actual enrolled native writer before any snapshot");
        };
        registrar.Start();
        try
        {
            Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1 && returns == 1,
                "R204 native writer commits once during first-code drain");
            Check(registrar.Join(TimeSpan.FromSeconds(5)) && error == null && a.MappingTopologyEpoch == topology,
                "R204 actual first-Code drain retains topology and completes after native release");
            int slot = FullNativeMemory.TlbIndex(data, FullNativeMemory.TlbCount - 1);
            ulong context = f.MappingState193.Tlb[slot].Context;
            Check(Mapping204Prepare(f, out bool discarded, context) && discarded && f.MappingState193.Tlb[slot].Host == null &&
                a.CodeRoleCacheWriteDrops > 0, "R204 old actual native Write is refused after first-Code registration");
            Mapping204Reasons(f, "native-writer"); RetainedClean(f);
        }
        finally
        {
            f.NativePrepared = null; f.NativeReturned = null; start.Set();
            Check(registrar.Join(TimeSpan.FromSeconds(5)), "R204 bounded native-writer registrar joins");
        }
    }
    static void Mapping204Races()
    {
        foreach (string transition in new[] { "retire", "host", "unknown", "runtime" })
        foreach (bool late in new[] { false, true })
        {
            using var f = new Role204Fixture(); ulong data = Role204Fixture.Base, code = data + 8192;
            f.Map(data); f.Fill(data); f.Map(code); f.Fill(code); Mapping193Seed(f, data, FullMemoryAccess.Write);
            Mapping204Register(f, code); var a = f.Jit.Authority; ulong observed = a.Epoch;
            using var start = new ManualResetEventSlim(); using var done = new ManualResetEventSlim(); Exception? error = null;
            var mutator = new Thread(() =>
            {
                try
                {
                    AtomicFamilyWait(start, "R204 off-Gate mutator starts");
                    if (transition == "retire") { bool precise = a.BeginPhysicalRetirement(true); a.EndPhysicalRetirement(precise); }
                    else if (transition == "host") { a.BeginHostWriter(); a.EndHostWriter(); }
                    else if (transition == "runtime") { Check(a.TryBeginHostBackingRead(), "R204 racing host read"); a.EndHostBackingRead(); }
                    else a.Advance();
                }
                catch (Exception e) { error = e; }
                finally { done.Set(); }
            });
            mutator.Start();
            try
            {
                if (!late) f.MappingState193.TestMappingRetagged = _ => { start.Set(); AtomicFamilyWait(done, "R204 overlap completes under Gate"); };
                bool prepared = Mapping204Prepare(f, out bool discarded);
                if (late) { start.Set(); AtomicFamilyWait(done, "R204 late mutation completes"); }
                Check(error == null && (late ? prepared : !prepared && discarded && f.MappingState193.TestTopologyStamp204 == 0 &&
                    f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null && a.MappingCacheRaceDrops == 1),
                    "R204 final SC version check discards every overlapping mutation: " + transition);
                bool entered = a.TryEnterNative(observed); if (entered) a.ExitNative();
                Check(!entered, "R204 unchanged final native enrollment rejects late mutation: " + transition);
            }
            finally { f.MappingState193.TestMappingRetagged = null; start.Set(); Check(mutator.Join(TimeSpan.FromSeconds(5)), "R204 bounded mutator joins"); }
            Mapping204Reasons(f, $"race/{transition}/{late}"); RetainedClean(f);
        }
    }
    static void Mapping204Bounds()
    {
        // Standalone authority: exhausted cleanup cannot invoke producer
        // FreeMachine mapping begins. No fake mapping is executed natively.
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        byte* pages = (byte*)NativeMemory.AlignedAlloc(8192, 4096);
        try
        {
            foreach (string denial in new[] { "topology", "topology-last", "mapping", "runtime", "saturation", "quarantine", "contextless" })
            {
                using var a = new FullCodeAuthority(io, 4096 * 16, 1, 1 << 20);
                using var state = new FullWorkerNativeState();
                lock (a.Gate)
                {
                    Check(a.TryGetNativeDataPermission((nint)pages, false, out _) && state.PrepareMappingCache(a, a.Epoch),
                        "R204 bounds establishes real physical registry and worker provenance");
                    state.Tlb[0] = new() { Page = Page, Host = pages, Identity = (ulong)pages, Context = 0x181,
                        AuthorityEpoch = a.Epoch, Permissions = FullMemoryAccess.Read, ValidEnd = 4096 };
                    Check(a.PrepareCodeSnapshot((nint)(pages + 4096)), "R204 bounds real first Code");
                    switch (denial)
                    {
                        case "topology": a.TestTopologyExhaustion204(); break;
                        case "topology-last":
                            a.TestTopologyExhaustion204(last: true);
                            bool stopped = false; try { a.Advance(); } catch (InvalidOperationException) { stopped = true; }
                            Check(stopped && a.SemanticQuarantined && a.MappingTopologyEpoch == ulong.MaxValue &&
                                !a.TryEnterNative(a.Epoch), "R204 first reach of exhausted topology is unusable");
                            break;
                        case "mapping": a.TestMappingExhaustion(); break;
                        case "runtime": *a.EpochPointer = ulong.MaxValue; break;
                        case "saturation": a.TestMappingSaturate200(); break;
                        case "quarantine": a.QuarantineSemanticAdmission(); break;
                    }
                    bool ready = denial == "contextless" ? state.PrepareMappingCache(a, a.Epoch) :
                        state.PrepareMappingCache(a, a.Epoch, FullMappingCachePhase.Ordinary, -1, out _, 0x181);
                    Check(ready == (denial == "contextless") && state.Tlb[0].Host == null &&
                        (denial == "contextless" || state.TestTopologyStamp204 == 0), "R204 unsupported/exhausted state clears: " + denial);
                    if (denial == "topology")
                    {
                        bool exhausted = false; try { a.Advance(); } catch (InvalidOperationException) { exhausted = true; }
                        Check(exhausted && a.SemanticQuarantined && !a.TryEnterNative(a.Epoch), "R204 topology cannot wrap or reopen native admission");
                    }
                }
            }
        }
        finally { NativeMemory.AlignedFree(pages); io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        Check(FullCodeAuthority.MappingCacheCharge == 96 && FullWorkerNativeState.MappingCacheCharge == 24 &&
            FullMemoryTlbEntry.ByteSize == 64 && FullNativeMemory.TlbCount == 256 && FullBlockFrame.ByteSize == 416 &&
            FullBlockFrame.AbiVersion == 0x46420007, "R204 charges managed witnesses/counters with no native ABI changes");
    }
    static void Mapping204Atomic()
    {
        using var f = new Role204Fixture(); using var deadline = f.AtomicDeadline();
        ulong pc = Role204Fixture.Base, data = pc + 8192, code = pc + 0x10000;
        AtomicFamilySetup(f, pc, data); f.Map(code); f.Fill(code);
        WriteControllerCode(f, pc, [0xf0,0x48,0x0f,0xc1,0x0b]); AtomicFamilySeed(f, data + 64);
        byte* host = (byte*)f.Physical(data) + 64; *(ulong*)host = 40; f.Counts(1); int inspections = 0;
        FullNativeMemory.TestAtomicTicketMapped = _ =>
        {
            if (++inspections != 1) return;
            Check(f.Issued == 1 && f.Completed == 0 && *(ulong*)host == 40, "R204 first-Code race precedes atomic effects");
            // No recursive acquire during a live mapper; use its exact audited
            // authority seam on an actual materialized physical page instead.
            lock (f.Jit.Authority.Gate) Check(f.Jit.Authority.PrepareCodeSnapshot(f.Physical(code)), "R204 first Code inside atomic mapping");
        };
        try
        {
            Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1 && *(ulong*)host == 45 &&
                RepRegister(f, 32) == 40 && inspections >= 2 && f.Statistics.SemanticHelperInstructions == 0,
                "R204 first-Code discard retries the same issued atomic with one effect/commit"); AtomicFamilyAssertClean(f);
        }
        finally { FullNativeMemory.TestAtomicTicketMapped = null; }
    }
    static void Mapping204Controller(bool store, bool samePage)
    {
        using var f = new Role204Fixture(); using var deadline = f.AtomicDeadline();
        ulong pc = Role204Fixture.Base, data = pc + 0x20000, code = samePage ? data : pc + 0x30000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data); if (!samePage) { f.Map(code); f.Fill(code); }
        byte[] op = store ? [0x48,0x89,0x06] : [0x48,0x8b,0x06];
        WriteControllerCode(f, pc, [..op, ..op, 0xeb,0xfe]);
        byte* host = (byte*)f.Physical(data); *(ulong*)(host + 64) = 0x1122334455667788;
        Write64((byte*)f.Machine(), 72, data + 64); Write64((byte*)f.Machine(), 24, 0x8877665544332211);
        Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap; f.Counts(2);
        try
        {
            Check(f.Step(pc, signalActor: true) && f.Completed == 1, "R204 actual controller seeds ordinary data");
            long assists = f.Statistics.NativeMemoryAssists; var a = f.Jit.Authority; ulong topology = a.MappingTopologyEpoch;
            Mapping204Register(f, code);
            Check(a.MappingTopologyEpoch == topology, "R204 actual compiler publication leaves topology intact");
            int reservations = 0; a.TestReservationPublished = _ => ++reservations;
            Check(f.Continue(signalActor: true) && f.Issued == 2 && f.Completed == 2 &&
                f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                "R204 actual native continuation commits the next issue exactly once");
            if (store && samePage)
                Check(a.CodeRoleCacheWriteDrops > 0 && f.Statistics.NativeMemoryAssists > assists && reservations > 0,
                    "R204 newly code Write returns through independent actual native ticket");
            else Check(a.CodeRoleCacheKept > 0 && f.Statistics.NativeMemoryAssists == assists && reservations == 0,
                "R204 actual untouched data/new-code Read uses retained mapping without refill");
            Check(store ? *(ulong*)(host + 64) == 0x8877665544332211 : RepRegister(f, 24) == 0x1122334455667788,
                "R204 independent actual backing/register oracle");
            Mapping204Reasons(f, $"controller/store={store}/samePage={samePage}"); RetainedClean(f);
        }
        finally { f.Jit.Authority.TestReservationPublished = null; f.NativePrepared = null; f.NativeReturned = null; }
    }
}
