using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using Mapping203Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE ONLY / AUTHORED_UNRUN. One serial parent entry; no benchmark.
    static void CheckFullPreciseMappingsR203(byte* machine)
    {
        Mapping203RetainedTable();
        Mapping203PendingTable();
        Mapping203CompletedTable();
        Mapping203DraftTable();
        Mapping203PublicationTable();
        Mapping203PhysicalTable();
        Mapping203FallbackTable();
        Mapping203ScopeTable();
        Mapping203ActualScopeTable();
        Mapping203TicketTable();
        // Existing real host/EOF/raw-alias, native-ticket/same-issue, ENV, REP
        // and semantic-reader contracts remain part of this single family.
        CheckFullCodeSnapshotsR201(machine);
        CheckFullControllerSharedAliases();
        CheckFullCodeEpochEncoding(); // actual emitted guards on both ABI layouts
    }

    static void Mapping203RetainedTable()
    {
        foreach (bool syscall in new[] { false, true })
        foreach (string operation in new[] { "mmap", "mprotect", "munmap", "MAP_FIXED", "madvise" })
        {
            using var f = new Mapping203Fixture();
            ulong pc = Mapping203Fixture.Base, data = pc + 0x10000, call = pc + 64;
            f.Map(pc); f.Fill(pc); WriteControllerCode(f, call, [0x0f, 0x05, 0xeb, 0xfe]);
            if (operation != "mmap") { f.Map(data); f.Fill(data); }
            var token = RetainedWarm(f, pc);
            if (syscall) RetainedWarm(f, call, requestedCount: 1);
            var a = f.Jit.Authority; ulong code = a.CodeEpoch, mapping = a.MappingAuthorityEpoch;
            long precise = a.PreciseMappingScopes;
            if (syscall)
            {
                f.Counts(1);
                switch (operation)
                {
                    case "mmap": case "MAP_FIXED": f.AtomicSyscall(call, 0, 9, data, 4096, 3, 0x32, ulong.MaxValue); break;
                    case "mprotect": f.AtomicSyscall(call, 0, 10, data, 4096, 3); break;
                    case "munmap": f.AtomicSyscall(call, 0, 11, data, 4096); break;
                    case "madvise": f.AtomicSyscall(call, 0, 28, data, 4096, 4); break;
                }
                Check(FullRax(f) == (operation is "mmap" or "MAP_FIXED" ? data : 0UL) &&
                    f.Issued == 1 && f.Completed == 1, "R203 actual syscall helper has one issue and successful mapping result");
            }
            else if (operation == "mmap") f.Map(data);
            else if (operation == "madvise") f.DataScope(0, () =>
                Check(f.Program.BlinkGuestMadvise(f.Machine(), (long)data, 4096, 4) == 0, "R203 actual madvise"));
            else Check(f.Mutate(operation, data) == 0, "R203 actual producer mutation");
            Check(a.CodeEpoch == code && a.MappingAuthorityEpoch > mapping && a.PreciseMappingScopes > precise &&
                a.MappingMutations == 0 && a.MappingWriters == 0, "R203 finite noncode mutation preserves code stamp and invalidates data authority");
            var before = f.Statistics; long walks = JitPageIdentity.FullTestIdentityReads;
            var retained = f.R185Acquire(pc, token);
            Check(retained.Acquired && retained.Retained && retained.Token == token && retained.Multiple && !retained.FetchBorrow &&
                JitPageIdentity.FullTestIdentityReads == walks, "R203 unaffected actual retained acquisition has no PTE walk");
            RetainedNoValidation(before, f.Statistics, passiveScopes: 0);
            int returned = 0;
            f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativeReturned = _ => { ++returned; Check(a.FullTestR185NativeFrames > 0, "R203 real enrollment held through native return"); };
            f.Counts(1);
            before = f.Statistics; walks = JitPageIdentity.FullTestIdentityReads;
            Check(f.Step(pc) && returned == 1 && f.Issued == 1 && f.Completed == 1 && FullRax(f) == 1 &&
                JitPageIdentity.FullTestIdentityReads == walks && f.Statistics.PageTableReadBorrows == before.PageTableReadBorrows &&
                AuthorityCount(f.Statistics, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
                AuthorityCount(f.Statistics, "Restamps") == AuthorityCount(before, "Restamps"),
                "R203 actual controller retains body without PTE/byte/restamp work after noncode mutation");
            RetainedClean(f);
        }
    }

    static void Mapping203PendingTable()
    {
        foreach (bool firstCode in new[] { false, true })
        foreach (string operation in new[] { "MAP_FIXED", "mprotect", "munmap" })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base;
            f.Map(pc); f.Fill(pc); if (!firstCode) RetainedWarm(f, pc);
            var a = f.Jit.Authority; ulong code = a.CodeEpoch;
            TranslationLease pending = default; int hooks = 0;
            f.MappingRevoked = (_, _) => f.DataScope(1, () =>
            {
                ++hooks;
                Check(a.MappingMutations > 0 && f.MappingBorrow(0), "R203 explicit owned worker mapping is pending before PTE effects");
                Check(f.Acquire(pc, 1, out pending, out bool multiple, out _) && !multiple &&
                    f.Jit.Store.GetHeaderCopy(in pending).ValidatedGlobalEpoch == 0,
                    "R203 affected old-byte/first-code snapshot after revocation cannot acquire a code stamp");
                Snapshot201EnrollmentDenied(a, "R203 mapping writer excludes native enrollment");
            });
            Check(f.Mutate(operation, pc) == 0 && hooks == 1, "R203 real mapping callback ran before mutation");
            using (pending)
            {
                Check(a.CodeEpoch == code && f.Jit.Store.GetHeaderCopy(in pending).ValidatedGlobalEpoch == 0,
                    "R203 completion does not promote old bytes or first-code registration");
                f.At(pc, 1); f.Counts(1, issued: 1);
                var frame = f.InvokeRetained(in pending, a.CodeEpoch);
                Check(frame.Exit == FullBlockExit.Invalidated && frame.Executed == 0 && f.Completed == 0 && f.Issued == 1,
                    "R203 saved pointer rejects stale header with a current runtime/code observation");
            }
            f.Map(pc); f.Fill(pc); RetainedWarm(f, pc); RetainedClean(f);
        }
    }

    static void Mapping203CompletedTable()
    {
        foreach (bool compiling in new[] { false, true })
        foreach (bool afterBytes in new[] { false, true })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base, data = pc + 0x10000;
            f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data);
            if (!compiling) { RetainedWarm(f, pc); f.Jit.RevokeMapping((long)pc, 4096); }
            int hooks = 0;
            f.Jit.TestCodeSnapshot201 = (compile, after) =>
            {
                if (compile != compiling || after != afterBytes) return;
                ++hooks;
                f.DataScope(1, () => Check(f.Program.ReserveVirtual(f.Machine(1)->blink_host_system,
                    (long)data, 4096, f.Program.SetProtection(7), -1, 0, false, true) == (long)data,
                    "R203 complete unrelated real PTE cycle between snapshot phases"));
            };
            try
            {
                Check(f.Acquire(pc, 0, out var lease, out bool multiple, out _) && !multiple,
                    "R203 completed mapping cycle rejects coherence even with pending checks both clear");
                using (lease) Check(f.Jit.Store.GetHeaderCopy(in lease).ValidatedGlobalEpoch == 0,
                    "R203 compile/revalidation never stamps an overlapping mapping cycle");
            }
            finally { f.Jit.TestCodeSnapshot201 = null; }
            Check(hooks == 1, "R203 exact snapshot phase ran"); RetainedWarm(f, pc); RetainedClean(f);
        }
    }

    static void Mapping203DraftTable()
    {
        foreach (bool known in new[] { false, true })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base;
            f.Map(pc); f.Fill(pc); var store = f.Jit.Store;
            Check(store.TryReserve(new(pc, 7), 0, 64, 8, out var draft, out _), "R203 real draft reservation");
            try
            {
                if (known) Check(store.SetProvenance(in draft, pc, (ulong)(nuint)f.Physical(pc)), "R203 real draft provenance");
                f.DataScope(0, () => f.NotifyMapping(pc, 0));
                Check(!store.Publish(ref draft, 0, 64, 1, f.Jit.Authority.CodeEpoch, out var failure) &&
                    failure == TranslationStoreFailure.AuthorityChanged, "R203 exact virtual revoke covers known and unknown drafts");
            }
            finally { store.Abort(ref draft); }
            RetainedClean(f);
        }
    }

    static void Mapping203PublicationTable()
    {
        foreach (string phase in new[] { "before-snapshot", "after-snapshot", "pending", "after-publication" })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base;
            f.Map(pc); f.Fill(pc); var a = f.Jit.Authority; var store = f.Jit.Store;
            ulong physical = (ulong)(nuint)f.Physical(pc);
            // Synthetic payload draft, real store/authority publication seam.
            lock (a.Gate)
            {
                if (phase == "before-snapshot")
                { a.BeginPhysicalRetirement(true); a.EndPhysicalRetirement(true); }
                var snapshot = a.CaptureCodeSnapshot(independentContent: true);
                Check(store.TryReserve(new(pc, 7), 0, 64, 8, out var draft, out _), "R203 snapshot publication draft");
                var token = draft.Token;
                try
                {
                    Check(store.SetProvenance(in draft, pc, physical), "R203 publication physical provenance");
                    if (phase is "after-snapshot" or "pending")
                    {
                        a.BeginPhysicalRetirement(true);
                        store.MarkPhysicalAuthorityChanged(physical, 4096, includeDrafts: false);
                        if (phase != "pending") a.EndPhysicalRetirement(true);
                    }
                    try
                    {
                        Check(store.PublishCodeSnapshot(ref draft, 0, 64, a, in snapshot, out _), "R203 atomic snapshot and store publication");
                    }
                    finally { if (phase == "pending") a.EndPhysicalRetirement(true); }
                    if (phase == "after-publication")
                    {
                        a.BeginPhysicalRetirement(true);
                        try { store.MarkPhysicalAuthorityChanged(physical, 4096, includeDrafts: false); }
                        finally { a.EndPhysicalRetirement(true); }
                    }
                    Check(store.TryAcquireToken(token, out var lease), "R203 snapshot publication resident lease");
                    using (lease)
                    {
                        var h = store.GetHeaderCopy(in lease);
                        Check(phase == "after-publication" ? h.Live == 0 :
                            h.ValidatedGlobalEpoch == (phase == "before-snapshot" ? a.CodeEpoch : 0UL),
                            "R203 retirement either excludes draft stamping or revokes the published resident");
                    }
                }
                finally { store.Abort(ref draft); }
            }
            RetainedClean(f);
        }
    }

    static void Mapping203PhysicalTable()
    {
        // The PTE/retirement/allocator paths are real. Extra store records below
        // are labelled synthetic provenance witnesses; no anonymous alias PTE
        // or ownership is invented and no unowned page is freed by the suite.
        foreach (bool deferred in new[] { false, true })
        foreach (bool known in new[] { false, true })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base, data = pc + 0x10000;
            f.Map(pc); f.Fill(pc); f.Map(data); f.Fill(data); RetainedWarm(f, pc);
            ulong physical = (ulong)(nuint)f.Physical(data); var store = f.Jit.Store;
            TranslationToken Witness() => known ? RetainedPublishWithProvenance(store, data + 0x10000, data + 0x10000, physical) :
                RetainedPublish(store, data + 0x10000);
            var witness = Witness(); ulong code = f.Jit.Authority.CodeEpoch;
            if (deferred) f.Jit.BeginRead(f.Program, false);
            try
            {
                Check(f.Mutate("munmap", data) == 0, "R203 actual anonymous detach/recycle");
                Check(store.TryAcquireToken(witness, out var revoked), "R203 physical witness still resident");
                using (revoked) Check(store.GetHeaderCopy(in revoked).Live == 0, "R203 recycle revokes physical aliases and unknown ownership records");
                if (deferred)
                {
                    // Simulates FIRST code registration after detach, before
                    // actual clear. This must be revoked at deferred completion.
                    // Revalidate existing witness, preserving its physical links.
                    Check(store.TryAcquireToken(witness, out var held), "R203 deferred first-registration witness lease");
                    using (held)
                    {
                        var h = store.GetHeaderCopy(in held);
                        Check(store.TryRevalidate(witness, h.CodeEpoch, h.CodeEpoch, code), "R203 synthetic newly trusted physical witness");
                    }
                }
            }
            finally { if (deferred) f.Jit.EndRead(f.Program); }
            Check(f.Jit.Authority.CodeEpoch == code, "R203 authenticated physical retirement does not dirty unrelated global code");
            Check(store.TryAcquireToken(witness, out var completed), "R203 physical completion retains witness identity");
            using (completed) Check(store.GetHeaderCopy(in completed).Live == 0, "R203 deferred/ordinary physical completion revokes saved alias authority");
            RetainedClean(f);
        }
    }

    static void Mapping203FallbackTable()
    {
        foreach (string kind in new[] { "generic", "logical", "negative-clipped", "overflow", "metal", "nonflat", "legacy", "saturation", "walk-budget" })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base;
            f.Map(pc); f.Fill(pc); var token = RetainedWarm(f, pc); var a = f.Jit.Authority;
            ulong code = a.CodeEpoch, cs = f.Machine()->cs.@base; var mode = f.Machine()->mode; var metal = f.Machine()->metal;
            if (kind == "metal") f.Machine()->metal = true;
            if (kind == "nonflat") f.Machine()->cs.@base = 0x1000;
            if (kind == "legacy") f.Machine()->mode.omode = 1;
            if (kind == "saturation") a.TestMappingSaturate200();
            if (kind == "walk-budget") f.Jit.TestMappingBudget203 = 0;
            try
            {
                if (kind == "generic") f.Jit.RevokeMapping((long)pc + 0x10000, 4096);
                else f.DataScope(0, () =>
                {
                    if (kind == "logical") f.Program.blink_host_guest_mapping_revoke(0, 0x800000000000L);
                    else if (kind == "negative-clipped") f.Program.blink_host_guest_mapping_revoke(-4096, 8192);
                    else if (kind == "overflow") ((IHostGuestCodeRevocation)f.Owner).RevokeMapping(long.MaxValue - 1, 4096);
                    else f.NotifyMapping(pc + 0x10000, 0);
                });
            }
            finally { f.Machine()->cs.@base = cs; f.Machine()->mode = mode; f.Machine()->metal = metal; }
            Check(a.CodeEpoch > code && a.MappingMutations == 0 && a.MappingWriters == 0,
                "R203 generic/logical/unsupported/unknown/saturated mutation retains global code fallback: " + kind);
            var hit = f.R185Acquire(pc, token);
            Check(!hit.Retained, "R203 global fallback cannot reuse old retained authority"); RetainedClean(f);
        }
    }

    static void Mapping203TicketTable()
    {
        foreach (bool writes in new[] { false, true })
        {
            using var io = new SemanticTestIo(); using var a = new FullCodeAuthority(io.Value, 4096, 2, 16384);
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(0) && a.TryPromoteNativeTicket(0, 0x1000, writesContent: writes),
                    "R203 real read/write native ticket promotion");
                bool precise = a.BeginPhysicalRetirement(true);
                try { Check(!a.TryEnterNativeTicket(0, a.Epoch), "R203 foreign physical mapping count denies ticket enrollment without identity exception"); }
                finally { a.EndPhysicalRetirement(precise); }
                a.EndNativeTicket(0);
                Check(a.MappingWriters == 0 && a.MappingMutations == 0 && a.PendingAtomicWorker == -1,
                    "R203 foreign retirement and own ticket release only their respective counts");
            }
        }
    }

    static void Mapping203ActualScopeTable()
    {
        foreach (string kind in new[] { "nested", "failed-producer", "exception" })
        {
            using var f = new Mapping203Fixture(); ulong pc = Mapping203Fixture.Base, data = pc + 0x10000;
            f.Map(pc); f.Fill(pc); var token = RetainedWarm(f, pc); var a = f.Jit.Authority;
            ulong code = a.CodeEpoch, sequence = a.MappingSequence; bool threw = false;
            try
            {
                f.DataScope(0, () =>
                {
                    f.NotifyMapping(data, 0);
                    f.DataScope(0, () => f.NotifyMapping(data + 4096, 0));
                    Check(f.MappingBorrow(0) && a.MappingMutations == 1,
                        "R203 nested actual controller data scope preserves outer mapping ownership");
                    if (kind == "failed-producer")
                        Check(f.Program.ReserveVirtual(f.Machine()->blink_host_system, (long)data, 4096,
                            f.Program.SetProtection(7), 0, -1, false, true) == -1,
                            "R203 real producer rejects bad file offset after revocation without PTE effects");
                    if (kind == "exception") throw new InvalidOperationException("R203 scope unwind");
                });
            }
            catch (InvalidOperationException error) when (error.Message == "R203 scope unwind") { threw = true; }
            Check(threw == (kind == "exception") && a.CodeEpoch == code && a.MappingSequence == sequence + 2 &&
                a.MappingMutations == 0 && !f.MappingBorrow(0), "R203 actual outer finally alone completes nested/failed mapping scope");
            Check(f.R185Acquire(pc, token).Retained, "R203 failed unrelated mapping preserves actual retained entry");
            RetainedClean(f);
        }
    }

    static void Mapping203ScopeTable()
    {
        foreach (string kind in new[] { "nested", "escalated", "failed", "exhausted-begin", "exhausted-end", "runtime-end", "mapping-end", "global-code-end", "physical-end" })
        {
            using var io = new SemanticTestIo(); using var a = new FullCodeAuthority(io.Value, 4096, 2, 16384);
            ulong code = a.CodeEpoch; bool threw = false;
            if (kind == "exhausted-begin") a.TestSetMappingSequence203(ulong.MaxValue);
            try
            {
                lock (a.Gate) a.BeginOrExtendOwnedMapping(0, precise: true);
                if (kind == "nested") lock (a.Gate) a.BeginOrExtendOwnedMapping(1, precise: true);
                if (kind is "escalated" or "global-code-end") lock (a.Gate) a.BeginOrExtendOwnedMapping(0, precise: false);
                lock (a.Gate)
                {
                    var pending = a.CaptureCodeSnapshot(independentContent: true);
                    Check(!a.CodeSnapshotCurrent(in pending), "R203 snapshots beginning inside mapping exclusion never gain trust");
                }
                if (kind == "exhausted-end") a.TestSetMappingSequence203(ulong.MaxValue);
                if (kind == "runtime-end") a.EpochPointer[0] = ulong.MaxValue;
                if (kind == "mapping-end") a.TestSetMappingEpoch203(ulong.MaxValue);
                if (kind == "global-code-end") a.CodeEpochPointer[0] = ulong.MaxValue;
                if (kind == "physical-end")
                {
                    a.BeginPhysicalRetirement(true); a.TestSetMappingSequence203(ulong.MaxValue); a.EndPhysicalRetirement(true);
                }
                if (kind == "failed") throw new InvalidOperationException("R203 injected failed mutation");
            }
            catch (InvalidOperationException error) when (
                (kind == "failed" && error.Message == "R203 injected failed mutation") ||
                (kind == "exhausted-begin" && error.Message == "Full-JIT mapping sequence exhausted.")) { threw = true; }
            finally
            {
                if (kind == "nested") a.EndOwnedMappingMutation(1);
                if (a.OwnsPreciseMapping(0) || kind is "escalated" or "global-code-end") a.EndOwnedMappingMutation(0);
            }
            Check(threw == (kind is "failed" or "exhausted-begin") && a.MappingWriters == 0 && a.MappingMutations == 0,
                "R203 owned/nested/failed/exhausted scope release is balanced and nonthrowing: " + kind);
            Check(kind is "escalated" or "global-code-end" ? a.CodeEpoch > code : a.CodeEpoch == code,
                "R203 only permanent fallback escalation pulses global code in owned scope table");
            if (kind.Contains("exhausted") || kind is "runtime-end" or "mapping-end" or "global-code-end" or "physical-end")
                Check(a.SemanticQuarantined && !a.TryEnterNative(a.Epoch), "R203 exhaustion never wraps or reopens admission");
            Check(a.Charge == FullCodeAuthority.ChargeFor(4096, 2, 16384), "R203 managed scope array/sequence/counters are charged");
        }
        using (var io = new SemanticTestIo())
        using (var a = new FullCodeAuthority(io.Value, 4096, 1, 16384))
        {
            a.QuarantineSemanticAdmission();
            ulong code = a.CodeEpoch;
            lock (a.Gate) a.BeginOrExtendOwnedMapping(0, precise: true);
            Check(!a.OwnsPreciseMapping(0) && !a.TryEnterNative(a.Epoch),
                "R203 quarantined cleanup uses only global exclusion and never reopens native admission");
            a.EndOwnedMappingMutation(0);
            Check(a.SemanticQuarantined && a.MappingWriters == 0 && a.MappingMutations == 0 && a.CodeEpoch > code,
                "R203 quarantine alone cannot block existing mapping cleanup; owned counts balance");
        }
        foreach (var row in new (long Address, long Size, bool Precise)[]
        { (1, 1, true), (4095, 2, true), (0, 0x800000000000L, false), (-4096, 8192, false), (long.MaxValue, 4096, false), (0, 0, false) })
            Check(FullCodeAuthority.TryPreciseMappingRange(row.Address, row.Size, out ulong start, out ulong length) == row.Precise &&
                (!row.Precise || ((start | length) & 4095) == 0), "R203 finite range authentication rounds full PTEs without narrowing logical invalidations");
    }
}
