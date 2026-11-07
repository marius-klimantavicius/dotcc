using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Mapping193Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // One source-authored family. Parent alone builds/runs both producers and
    // host ABIs. Synthetic/raw-frame fixtures remain bound to runtime epochs.
    static void CheckFullMappingCacheEpochR193(byte* machine)
    {
        Mapping193Snapshots();
        Mapping193Native(machine);
        Mapping193Roles();
        Mapping193Races();
        Mapping193Controller();
        Mapping193AtomicDiscard();
        Mapping193Rep();
        Mapping193Bounds();
        CheckFullMappingCacheLifetimeR200();
        // Existing independent MMU/guard/reader/lifetime oracles, unchanged.
        CheckFullMemoryMmu(); // lazy, RO, unmap/remap, shared EOF/truncate
        Cache181MmuBoundaries();
        Admission185BackingTable();
        Admission185MemoryAndHelper(); // pending memory retains its physical pin
        ScalarAtomicReaderClosure();
        ScalarAtomicActualBackingRead();
        CheckFullSharedMemoryGuardsR188(machine); // raw + owner scaffold, tickets/polls
        CheckFullBoundedCopiesR190();
    }

    static bool Mapping193Prepare(Mapping193Fixture f, ulong? observed = null)
    {
        lock (f.Jit.Authority.Gate)
            return f.MappingState193.PrepareMappingCache(f.Jit.Authority, observed ?? f.Jit.Authority.Epoch);
    }

    static FullMemoryTlbEntry Mapping193Seed(Mapping193Fixture f, ulong page,
        FullMemoryAccess access = FullMemoryAccess.Read)
    {
        // Use the REAL MMU to obtain rights and backing. A mapping transition
        // while filling may discard the first result; never forge its epoch.
        for (int attempt = 0; attempt < 4; ++attempt)
        {
            Check(Mapping193Prepare(f), "R193 pre-fill coherent worker provenance");
            var frame = Cache181Request(f.MappingState193.Tlb, page, access);
            Check(FillFullMemoryMmu(f, &frame) == FullMemoryAssistResult.Resolved, "R193 real checked MMU fill");
            if (!Mapping193Prepare(f, frame.ObservedGlobalEpoch)) continue;
            var entry = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(page, 63)];
            if (entry.Host != null) return entry;
        }
        throw new InvalidOperationException("R193 stable fill failed");
    }

    static void Mapping193Pulse(Mapping193Fixture f, string kind)
    {
        ulong mapping = f.Jit.Authority.MappingAuthorityEpoch, runtime = f.Jit.Authority.Epoch;
        int worker = f.WorkerId(1);
        if (kind == "cpu") { using var scope = f.Jit.Authority.EnterCpuSemanticRead(worker); }
        else if (kind == "copy-read") { f.Jit.Authority.BeginGuestRead(worker); f.Jit.Authority.EndGuestRead(worker); }
        else if (kind == "backing")
        {
            Check(f.Jit.Authority.TryBeginHostBackingRead(), "R193 host read publication");
            f.Jit.Authority.EndHostBackingRead();
        }
        else
        {
            f.Jit.Authority.BeginGuestWrite(worker);
            f.Jit.Authority.EndGuestWrite(worker);
        }
        Check(f.Jit.Authority.MappingAuthorityEpoch == mapping && f.Jit.Authority.Epoch > runtime,
            "R193 audited rendezvous changes runtime only: " + kind);
    }

    static void Mapping193Snapshots()
    {
        foreach (var access in new[] { FullMemoryAccess.Read, FullMemoryAccess.Write })
        foreach (string kind in new[] { "cpu", "copy-read", "backing", "copy-write" })
        {
            using var f = new Mapping193Fixture(); ulong data = Mapping193Fixture.Base;
            f.Map(data, 8192); f.Fill(data, 8192);
            var before = Mapping193Seed(f, data, access);
            int slot = FullNativeMemory.TlbIndex(data, 63);
            // Mixed epochs and contextual/partial/flagged entries must never
            // gain new rights or become a Ready descriptor via this helper.
            int other = FullNativeMemory.TlbIndex(data + 4096, 63);
            var mixed = before; mixed.Page += 4096; mixed.AuthorityEpoch = 0;
            mixed.Context ^= 1; mixed.ValidEnd = 17; mixed.Flags = FullMemoryFlags.BarrierConsumed;
            f.MappingState193.Tlb[other] = mixed;
            Mapping193Pulse(f, kind);
            Check(Mapping193Prepare(f), "R193 stable runtime retag admitted");
            var after = f.MappingState193.Tlb[slot];
            Check(after.AuthorityEpoch == f.Jit.Authority.Epoch && after.Host == before.Host &&
                after.Identity == before.Identity && after.Permissions == before.Permissions &&
                after.Context == before.Context && after.Page == before.Page && after.Flags == before.Flags &&
                after.ValidStart == before.ValidStart && after.ValidEnd == before.ValidEnd,
                "R193 retag preserves exact checked rights, context, backing and spans");
            Check(f.MappingState193.Tlb[other].AuthorityEpoch == 0 &&
                f.MappingState193.Tlb[other].Context == mixed.Context &&
                f.MappingState193.Tlb[other].Flags == mixed.Flags,
                "R193 mixed/empty epoch is never promoted");
            ulong stale = f.Jit.Authority.Epoch;
            Mapping193Pulse(f, "cpu");
            Check(!Mapping193Prepare(f, stale) && f.MappingState193.TestMappingStamp == (0UL, 0UL) &&
                f.MappingState193.Tlb[slot].Host == null, "R193 frame epoch must match exact final snapshot");
        }
        using (var f = new Mapping193Fixture())
        {
            ulong data = Mapping193Fixture.Base; f.Map(data); f.Fill(data);
            Mapping193Seed(f, data); int slot = FullNativeMemory.TlbIndex(data, 63);
            var old = f.MappingState193.Tlb[slot];
            Mapping193Seed(f, data, FullMemoryAccess.Write);
            Check(old.Permissions == FullMemoryAccess.Read &&
                f.MappingState193.Tlb[slot].Permissions == (FullMemoryAccess.Read | FullMemoryAccess.Write),
                "R193 read-to-write upgrade requires real MMU permission admission");
        }
        using (var f = new Mapping193Fixture())
        {
            ulong data = Mapping193Fixture.Base; f.Map(data, 8192); f.Fill(data, 8192);
            Mapping193Seed(f, data, FullMemoryAccess.Write);
            // Reproduce a pending fill sampled AFTER a mapping/role transition
            // but BEFORE the worker's final stamp update. The newly filled B
            // and old Write A must both drop, even though B has today's runtime.
            lock (f.Jit.Authority.Gate) f.Jit.Authority.Advance();
            var pending = Cache181Request(f.MappingState193.Tlb, data + 4096);
            Check(FillFullMemoryMmu(f, &pending) == FullMemoryAssistResult.Resolved,
                "R193 pending fill independently observes new runtime/mapping");
            Check(Mapping193Prepare(f, pending.ObservedGlobalEpoch) &&
                f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null &&
                f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data + 4096, 63)].Host == null,
                "R193 final stamp cannot adopt mixed provenance after a mapping change");
        }
    }

    static void Mapping193Roles()
    {
        foreach (string role in new[] { "code", "borrow", "host", "mapping", "ticket", "unknown", "RO", "unmap", "remap" })
        {
            using var f = new Mapping193Fixture(); ulong data = Mapping193Fixture.Base;
            f.Map(data); f.Fill(data); var before = Mapping193Seed(f, data, FullMemoryAccess.Write);
            var a = f.Jit.Authority; ulong mapping = a.MappingAuthorityEpoch;
            int slot = FullNativeMemory.TlbIndex(data, 63), worker = f.WorkerId(1);
            lock (a.Gate)
            {
                switch (role)
                {
                    case "code": Check(a.PrepareCodeSnapshot((nint)before.Host), "R193 first Code role"); break;
                    case "borrow": a.Expose(worker, (nint)before.Host); break;
                    case "host": a.BeginHostWriter(); a.EndHostWriter(); break;
                    case "mapping": a.BeginMappingMutation(); a.EndMappingMutation(); break;
                    case "ticket":
                        Check(a.TryReserveNativeTicket(worker) && a.TryPromoteNativeTicket(worker, (nint)before.Host),
                            "R193 checked ticket owns reservation");
                        a.EndNativeTicket(worker); break;
                    case "unknown": a.Advance(); break;
                }
            }
            if (role == "RO") f.MappingReadOnly193(data);
            else if (role == "unmap") Check(f.Mutate("munmap", data) == 0, "R193 actual unmap");
            else if (role == "remap") { Check(f.Mutate("MAP_FIXED", data) == 0, "R193 actual remap"); f.Fill(data); }
            if (role == "ticket")
                Check(a.MappingAuthorityEpoch == mapping && Mapping193Prepare(f) &&
                    f.MappingState193.Tlb[slot].Host == before.Host,
                    "R200 completed temporary ticket retains checked mapping; R193 role revocations remain separate");
            else
                Check(a.MappingAuthorityEpoch != mapping && Mapping193Prepare(f) && f.MappingState193.Tlb[slot].Host == null,
                    "R193 eligibility/mapping change discards old pointers: " + role);
            if (role == "borrow")
            {
                lock (a.Gate) Check(!a.TryGetNativeDataPermission((nint)before.Host, true, out _),
                    "R193 active foreign physical borrow cannot reuse Write");
                mapping = a.MappingAuthorityEpoch; ulong runtime = a.Epoch;
                a.EndBorrow(worker);
                Check(a.MappingAuthorityEpoch != mapping && a.Epoch == runtime,
                    "R193 borrow completion invalidates mapping without changing legacy runtime behavior");
            }
            else if (role == "code")
                lock (a.Gate) Check(!a.TryGetNativeDataPermission((nint)before.Host, true, out _),
                    "R193 retag cannot fabricate Write on first Code registration");
            if (role is "RO" or "unmap")
            {
                var request = Cache181Request(f.MappingState193.Tlb, data,
                    role == "RO" ? FullMemoryAccess.Write : FullMemoryAccess.Read);
                Check(FillFullMemoryMmu(f, &request) == FullMemoryAssistResult.GuestFault &&
                    request.FaultTrap == 14 && request.FaultCode == (role == "RO" ? 2u : 1u),
                    "R193 dropped hints return to real MMU ACCERR/MAPERR after permission/unmap changes");
            }
            // A pending fill can publish at this NEW mapping's runtime. Other
            // old entries were already dropped, so committing cannot adopt them.
            if (role is "mapping" or "unknown" or "host")
            {
                var fresh = Mapping193Seed(f, data);
                Mapping193Pulse(f, "cpu"); Check(Mapping193Prepare(f), "R193 post-mutation runtime snapshot");
                Check(f.MappingState193.Tlb[slot].Permissions == fresh.Permissions && fresh.Permissions == FullMemoryAccess.Read,
                    "R193 pending fresh Read never resurrects an old Write stamp");
            }
        }
    }

    static void Mapping193Native(byte* machine)
    {
        foreach (bool owner in new[] { false, true })
        foreach (bool store in new[] { false, true })
        foreach (bool crossing in new[] { false, true })
        foreach (bool wrongContext in new[] { false, true })
        {
            using var f = new Mapping193Fixture(); ulong data = Mapping193Fixture.Base;
            f.Map(data, 8192); f.Fill(data, 8192);
            var access = store ? FullMemoryAccess.Write : FullMemoryAccess.Read;
            Mapping193Seed(f, data, access); Mapping193Seed(f, data + 4096, access);
            var first = Mapping193Seed(f, data, access);
            var second = Mapping193Seed(f, data + 4096, access);
            int offset = crossing ? 4095 : 64;
            for (int i = 0; i < 8; ++i)
                (offset + i < 4096 ? first.Host + offset + i : second.Host + offset + i - 4096)[0] = (byte)(i + 1);
            Mapping193Pulse(f, "cpu"); Check(Mapping193Prepare(f), "R193 native mapping handoff");
            using var scaffold = CreateFullScaffold(out var addresses);
            fullOwnerScaffold = owner ? addresses : default;
            try
            {
                var instruction = new JitInstruction(Ip, 3, store ? JitOperation.StoreMov : JitOperation.LoadMov,
                    64, new(0, 0, IsMemory: store), new(1, 0, IsMemory: !store), Address: new(3, -1, 0, 0));
                using var code = CreateFullBlock([new(instruction)]);
                ResetFull(machine); Write64(machine, 48, data + (uint)offset); Write64(machine, 32, 0x8877665544332211);
                long issued = 0, completed = 0; int stop = 0;
                var header = FullHeader(Ip, code.Entry); header.Context = first.Context;
                header.ValidatedGlobalEpoch = f.Jit.Authority.CodeEpoch;
                var frame = FullFrame(machine, &header, f.Jit.Authority.EpochPointer, &issued, &completed, &stop,
                    limit: 1, codeEpoch: f.Jit.Authority.CodeEpochPointer);
                frame.MemoryTlb = f.MappingState193.Tlb; frame.MemoryTlbMask = 63;
                if (wrongContext) frame.MemoryTlb[FullNativeMemory.TlbIndex(data, 63)].Context ^= 1;
                f.Jit.BeginRead(f.Program, false);
                bool enrolled = false;
                try
                {
                    lock (f.Jit.Authority.Gate) enrolled = f.Jit.Authority.TryEnterNative(frame.ObservedGlobalEpoch);
                    Check(enrolled, "R193 native witness retains actual enrollment");
                    var exit = RunFull(code, &frame);
                    Check(exit == (wrongContext ? FullBlockExit.NeedMemory : FullBlockExit.Continue) &&
                        issued == 1 && completed == (wrongContext ? 0 : 1),
                        "R193 retagged native guard preserves context checks in both scaffolds");
                    if (!wrongContext)
                    {
                        if (!store) Check(*(ulong*)(machine + 24) == 0x0807060504030201,
                            "R193 checked independent backing pages load exact crossing bytes");
                        else for (int i = 0; i < 8; ++i)
                            Check((offset + i < 4096 ? first.Host + offset + i : second.Host + offset + i - 4096)[0] ==
                                (byte)(0x8877665544332211UL >> (8 * i)), "R193 checked native split-store bytes");
                    }
                }
                finally { if (enrolled) f.Jit.Authority.ExitNative(); f.Jit.EndRead(f.Program); }
            }
            finally { fullOwnerScaffold = default; }
        }
    }

    static void Mapping193Races()
    {
        foreach (string when in new[] { "early", "during", "late" })
        {
            using var f = new Mapping193Fixture(); ulong data = Mapping193Fixture.Base;
            f.Map(data); f.Fill(data); Mapping193Seed(f, data, FullMemoryAccess.Write);
            var a = f.Jit.Authority; ulong observed = a.Epoch;
            using var start = new ManualResetEventSlim(); using var done = new ManualResetEventSlim();
            Exception? failure = null;
            var writer = new Thread(() =>
            {
                try
                {
                    AtomicFamilyWait(start, "R193 host writer starts");
                    a.BeginHostWriter(); a.WaitNativeFrames(); a.EndHostWriter();
                }
                catch (Exception e) { failure = e; }
                finally { done.Set(); }
            }) { IsBackground = true };
            writer.Start();
            try
            {
                if (when == "early") { start.Set(); AtomicFamilyWait(done, "R193 early writer complete"); }
                else if (when == "during") f.MappingState193.TestMappingRetagged = _ =>
                    { start.Set(); AtomicFamilyWait(done, "R193 writer between epoch samples"); };
                bool prepared = Mapping193Prepare(f, observed);
                if (when == "late") { start.Set(); AtomicFamilyWait(done, "R193 late writer complete"); }
                Check(failure == null, "R193 host writer completed without fault");
                if (when != "late") Check(!prepared && f.MappingState193.TestMappingStamp == (0UL, 0UL),
                    "R193 incoherent snapshot drops all worker stamps");
                lock (a.Gate)
                {
                    bool enrolled = a.TryEnterNative(observed);
                    if (enrolled) a.ExitNative();
                    Check(!enrolled, "R193 unchanged native enrollment rejects every stale sampled frame");
                }
                Check(Mapping193Prepare(f) && f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)].Host == null,
                    "R193 next entry cannot promote a stale host pointer");
            }
            finally { start.Set(); Check(writer.Join(TimeSpan.FromSeconds(5)), "R193 bounded writer joins"); }
        }
    }

    static void Mapping193Controller()
    {
        foreach (bool store in new[] { false, true })
        {
            using var f = new Mapping193Fixture(); ulong pc = Mapping193Fixture.Base, data = pc + 0x20000;
            f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
            byte[] op = store ? [0x48, 0x89, 0x06] : [0x48, 0x8b, 0x06];
            WriteControllerCode(f, pc, [..op, ..op, 0xeb, 0xfe]);
            Write64((byte*)f.Machine(), 72, data + 64); Write64((byte*)f.Machine(), 24, 0x8877665544332211);
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap; f.Counts(2);
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            Check(f.Step(pc) && f.Completed == 1, "R193 actual controller seeds checked mapping");
            long assists = f.Statistics.NativeMemoryAssists;
            foreach (string pulse in new[] { "cpu", "copy-read", "backing", "copy-write" }) Mapping193Pulse(f, pulse);
            var entry = f.MappingState193.Tlb[FullNativeMemory.TlbIndex(data, 63)];
            ulong mapping = f.Jit.Authority.MappingAuthorityEpoch;
            var copies = (IHostGuestMemoryCopies)f.Owner;
            copies.BeginGuestWrite((nint)(entry.Host + 64), 1);
            try { entry.Host[64] = 0x5a; } finally { copies.EndGuestWrite(); }
            Check(f.Jit.Authority.MappingAuthorityEpoch == mapping,
                "R193 actual finite physical copy preserves mapping while keeping write exclusion");
            Check(f.Continue() && f.Completed == 2 && f.Issued == 2 && f.Statistics.NativeMemoryAssists == assists &&
                f.Statistics.NativeMemoryInstructions == 2 && f.Statistics.SemanticHelperInstructions == 0 &&
                f.Statistics.InterpreterAttempts == 0, "R193 actual retained Read/Write avoids rendezvous-only refills");
            f.NativePrepared = null; f.NativeReturned = null; RetainedClean(f);
        }
        Cache181ControllerRetainedWrite(); // real noncontiguous vector crossing preserves existing A Write
        Cache181ControllerRevocation(); // first Code and mapping replacement through native tickets
    }

    static void Mapping193AtomicDiscard()
    {
        using var f=new Mapping193Fixture(); using var deadline=f.AtomicDeadline();
        ulong pc=Mapping193Fixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
        WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc1,0x0b]);AtomicFamilySeed(f,data+64);
        byte* host=(byte*)f.Physical(data)+64;*(ulong*)host=40;f.Counts(1);
        int inspections=0;
        FullNativeMemory.TestAtomicTicketMapped=pointer=>
        {
            if(++inspections!=1)return;
            Check(f.Issued==1&&f.Completed==0&&*(ulong*)host==40,
                "R193 cache-discard witness precedes all atomic effects");
            // Force the exact owner provenance-loss state reachable when the
            // pre-fill snapshot was denied by a reader that finishes during
            // mapping. The mapper then publishes a fresh valid current entry;
            // the final cache phase must clear it and explicitly retry.
            Check(!f.MappingState193.PrepareMappingCache(f.Jit.Authority,0),
                "R193 owner rejects stale provenance before atomic fill");
        };
        try
        {
            bool returned=f.Step(pc,signalActor:true); // cap1 ends after the atomic, before any following Issue
            Check(returned&&f.Issued==1&&f.Completed==1&&*(ulong*)host==45&&
                RepRegister(f,32)==40&&inspections>=2&&f.Statistics.SemanticHelperInstructions==0,
                $"R193 owner-discarded atomic retry: returned={returned}, issued={f.Issued}, completed={f.Completed}, memory={*(ulong*)host}, rcx={RepRegister(f,32)}, inspections={inspections}");
            AtomicFamilyAssertClean(f);
        }
        finally { FullNativeMemory.TestAtomicTicketMapped=null; }
    }

    static void Mapping193Rep()
    {
        {
        using var f = new Mapping193Fixture(); ulong pc = Mapping193Fixture.Base, data = pc + 0x20000;
        f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
        WriteControllerCode(f, pc, [0xf3, 0xaa, 0xf3, 0xaa, 0xeb, 0xfe]);
        RepSeed(f, 0, data + 64, 0x5a, 1, f.Machine()->flags & ~0x10500u); f.Counts(2);
        using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        Check(f.Step(pc, signalActor: true) && f.Completed == 1, "R193 first actual REP seeds data TLB");
        long assists = f.Statistics.NativeMemoryAssists;
        Mapping193Pulse(f, "backing"); Write64((byte*)f.Machine(), 32, 1);
        Check(f.Continue(signalActor: true) && f.Completed == 2 && f.Issued == 2 &&
            f.Statistics.NativeMemoryAssists == assists && RepRegister(f, 80) == data + 66,
            "R193 separately issued REP reuses mapping through actual enrollment");
        RepClean(f);
        } // Release this owner/binding before the existing tests create theirs.
        CheckRepR104DeniedInitialAdmission(); CheckFullRepLazyDestination();
    }

    static void Mapping193Bounds()
    {
        using var f = new Mapping193Fixture(); ulong data = Mapping193Fixture.Base;
        f.Map(data); f.Fill(data); Mapping193Seed(f, data);
        Check(FullWorkerNativeState.MappingCacheCharge == 24 && FullCodeAuthority.MappingCacheCharge == 96 &&
            FullNativeMemory.WorkerCharge == FullNativeMemory.WorkerNativeStorageBytes +
                FullNativeMemory.X87BindingAndWorkerAllowance + 24 &&
            FullNativeMemory.WorkerChargeFor(FullNativeMemory.TlbCount) == FullNativeMemory.WorkerCharge &&
            FullNativeMemory.WorkerChargeFor(64) == FullNativeMemory.WorkerNativeStorageBytesFor(64) +
                FullNativeMemory.X87BindingAndWorkerAllowance + 24 &&
            FullNativeMemory.WorkerChargeFor(256) - FullNativeMemory.WorkerChargeFor(64) ==
                (256 - 64) * FullMemoryTlbEntry.ByteSize &&
            f.Jit.Authority.Charge == FullCodeAuthority.ChargeFor(Mapping193Fixture.MemoryLimit, 2, Mapping193Fixture.Budget),
            "R204 owner 96 / worker 24 are reserved with unchanged native storage");
        lock (f.Jit.Authority.Gate)
        {
            for (int i = 0; i < 32; ++i)
            {
                f.Jit.Authority.BeginGuestRead(f.WorkerId(0)); f.Jit.Authority.EndGuestRead(f.WorkerId(0));
                f.MappingState193.PrepareMappingCache(f.Jit.Authority, f.Jit.Authority.Epoch);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4096; ++i)
            {
                f.Jit.Authority.BeginGuestRead(f.WorkerId(0)); f.Jit.Authority.EndGuestRead(f.WorkerId(0));
                f.MappingState193.PrepareMappingCache(f.Jit.Authority, f.Jit.Authority.Epoch);
            }
            Check(GC.GetAllocatedBytesForCurrentThread() == allocated, "R193 warm mapping handoff allocates zero managed bytes");
        }
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        try
        {
            foreach (string edge in new[] { "mapping", "runtime", "saturation" })
            {
                using var authority = new FullCodeAuthority(io, 4096, 1, 16384);
                using var state = new FullWorkerNativeState();
                lock (authority.Gate)
                {
                    Check(state.PrepareMappingCache(authority, authority.Epoch) && state.Tlb[0].Host == null,
                        "R193 initially empty worker establishes no fabricated mapping");
                    if (edge == "mapping") authority.TestMappingExhaustion();
                    else if (edge == "runtime") *authority.EpochPointer = ulong.MaxValue;
                    else for (int i = 1; i < 18; ++i)
                        authority.TryGetNativeDataPermission((nint)(i * 4096), false, out _);
                    Check(!state.PrepareMappingCache(authority, authority.Epoch) && state.TestMappingStamp == (0UL, 0UL),
                        "R193 exhaustion/saturation never aliases an initial stamp: " + edge);
                    if (edge != "saturation")
                    {
                        bool exhausted = false; try { authority.Advance(); } catch (InvalidOperationException) { exhausted = true; }
                        Check(exhausted && authority.SemanticQuarantined,
                            "R193 exhausted counter permanently closes native admission");
                    }
                    else Check(authority.Saturated, "R193 physical registry saturates closed");
                }
            }
        }
        finally { io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
    }
}
