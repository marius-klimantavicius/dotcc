using Blink = Managed.Emulation.BlinkCore;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Snapshot201Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE ONLY / UNRUN. Schedules invoke real authority admission and copies;
    // synthetic version exhaustion is labelled explicitly. No timing benchmark.
    static void CheckFullCodeSnapshotsR201(byte* machine)
    {
        Snapshot201Readers();
        Snapshot201Controller();
        Snapshot201Copies();
        Snapshot201CompletedCopy();
        Snapshot201Ticket();
        Snapshot201Mutations();
        Snapshot201Bounds();
        // Existing emitted runtime/code guard encoding on BOTH ABI scaffolds.
        CheckFullCodeEpochEncoding();
        // Keep the existing real request, ticket, mapping, ENV and REP tests.
        CheckFullBoundedCopiesR190();
        CheckFullRetainedAdmissionR185(machine);
        CheckFullRetainedContexts();
        CheckFullRetainedCrossPage();
        CheckFullMappingCacheEpochR193(machine);
        CheckFullNativeProcessorEnvironment(machine);
        CheckFullRepKernelStore();
        Snapshot201ActiveRepFlags();
        CheckFullRepContracts(machine);
        CheckFullRepControllerR104();
        CheckSemanticControllerPendingStop();
        // Explicit original cold atomic + old-reader schedules, including
        // active canonical x87 state; retain their issue/cancellation checks.
        AtomicFamilyReaderSchedules();
    }


    static void Snapshot201ActiveRepFlags()
    {
        using var f = new Snapshot201Fixture();
        const ulong pc = Snapshot201Fixture.Base, src = pc + 0x10000, dst = pc + 0x20000;
        f.Map(pc); f.Fill(pc); f.Map(src); f.Fill(src); f.Map(dst); f.Fill(dst);
        byte[] opcodes = [0xa4, 0xaa, 0xac, 0xa6, 0xae];
        for (int row = 0; row < opcodes.Length; ++row)
            WriteControllerCode(f, pc + (uint)(row * 16), [0xf3, opcodes[row], 0xeb, 0xfe]);
        f.DataScope(0, () =>
        {
            for (int k = 0; k < 3; ++k)
            {
                f.Program.LookupAddress2(f.Machine(), (long)(src + (uint)k), 0, 0)[0] = 5;
                f.Program.LookupAddress2(f.Machine(), (long)(dst + (uint)k), 0, 0)[0] = 5;
            }
        });
        f.StartCanonicalFamily();
        try
        {
            foreach (int row in Enumerable.Range(0, opcodes.Length))
            foreach (uint special in new uint[] { 1u << Blink.FLAGS_TF, 1u << Blink.FLAGS_RF,
                1u << Blink.FLAGS_AC, (1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF) | (1u << Blink.FLAGS_AC) })
            {
                uint flags = 0x202u | special;
                RepSeed(f, src, dst, 5, 3, flags); f.AlignmentMask(false); f.Counts(1);
                Check(f.Step(pc + (uint)(row * 16), signalActor: true) && f.Issued == 1 && f.Completed == 1 &&
                    f.Machine()->ip == pc + (uint)(row * 16 + 2) && RepRegister(f, 32) == 0 &&
                    (f.Machine()->flags & special) == special,
                    "R201 active finite-profile REP preserves TF/RF/AC with AM0 and one issue/completion");
                RepClean(f);
            }
        }
        finally { f.EndCanonicalFamily(); }
    }

    static void Snapshot201ReadPulse(Snapshot201Fixture f, bool backing)
    {
        var a = f.Jit.Authority;
        // Populate an unpublished backing page before taking the witness. Its
        // actual Io.ReadAt below invokes the real backing read/copy observer.
        int fd = -1;
        if (backing)
        {
            var opened = f.Io.CreateMemoryFile("r201-read", 0);
            Check(opened.Succeeded && f.Io.WriteAt(opened.Value, new byte[] { 0x5a }, 0).Value == 1,
                "R201 readonly backing pulse setup");
            fd = opened.Value;
        }
        ulong runtime = a.Epoch, content = a.ContentSequence, mapping = a.MappingAuthorityEpoch, code = a.CodeEpoch;
        if (backing)
        {
            Check(a.TryBeginHostBackingRead(), "R201 real backing reader admission");
            try
            {
                Snapshot201EnrollmentDenied(a, "R201 backing reader still excludes native enrollment");
                Span<byte> bytes = stackalloc byte[1];
                Check(f.Io.ReadAt(fd, bytes, 0).Value == 1 && bytes[0] == 0x5a,
                    "R201 actual read-only backing copy between snapshot phases");
            }
            finally { a.EndHostBackingRead(); }
        }
        else
        {
            using var read = a.EnterCpuSemanticRead(f.WorkerId(1));
            Snapshot201EnrollmentDenied(a, "R201 CPU reader still excludes native enrollment");
        }
        Check(a.Epoch > runtime && a.ContentSequence == content && a.MappingAuthorityEpoch == mapping && a.CodeEpoch == code,
            "R201 real reader changes runtime alone");
    }

    static void Snapshot201EnrollmentDenied(FullCodeAuthority a, string message)
    {
        bool entered = a.TryEnterNative(a.Epoch);
        if (entered) a.ExitNative();
        Check(!entered, message);
    }

    static void Snapshot201Readers()
    {
        foreach (bool compile in new[] { false, true })
        foreach (bool afterBytes in new[] { false, true })
        foreach (bool backing in new[] { false, true })
        {
            using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base;
            f.Map(pc); f.Fill(pc);
            TranslationToken original = default;
            if (!compile)
            {
                original = RetainedWarm(f, pc);
                f.Jit.RevokeMapping((long)pc, 4096); // force genuine slow revalidation
            }
            int hooks = 0;
            f.Jit.TestCodeSnapshot201 = (compiling, after) =>
            {
                if (compiling != compile || after != afterBytes) return;
                ++hooks;
                if (!compile)
                {
                    Check(f.Jit.Store.TryAcquireToken(original, out var originalLease), "R201 slow validation retains original header");
                    using (originalLease) Check(f.Jit.Store.GetHeaderCopy(in originalLease).Reserved136 == 0,
                        "R201 original retained header trust is cleared before real slow validation/restamp");
                }
                Snapshot201ReadPulse(f, backing);
            };
            TranslationToken token;
            try
            {
                Check(f.Acquire(pc, 0, out var lease, out bool multiple, out int error) && error == 0 && multiple,
                    "R201 snapshot survives a real pure-reader pulse between phases");
                using (lease)
                {
                    token = lease.Token;
                    var h = f.Jit.Store.GetHeaderCopy(in lease);
                    Check(h.ValidatedGlobalEpoch == f.Jit.Authority.CodeEpoch && h.ValidatedGlobalEpoch != 0,
                        "R201 unchanged bytes have executable code stamp without a runtime restamp");
                    lock (f.Jit.Authority.Gate) f.Jit.AuthorizeLinks(in lease, multiple);
                }
            }
            finally { f.Jit.TestCodeSnapshot201 = null; }
            Check(hooks == 1, "R201 intended compile/revalidate phase actually ran");
            var before = f.Statistics; long walks = JitPageIdentity.FullTestIdentityReads;
            var hit = f.R185Acquire(pc, token);
            Check(hit.Acquired && hit.Retained && hit.Multiple && !hit.FetchBorrow &&
                JitPageIdentity.FullTestIdentityReads == walks, "R201 safe retained body adds no PTE walk");
            RetainedNoValidation(before, f.Statistics, passiveScopes: 0);
            int returned = 0;
            f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 2;
            f.NativeReturned = _ => { ++returned; Check(f.Jit.Authority.FullTestR185NativeFrames > 0,
                "R201 actual native enrollment remains held through RET"); };
            f.Counts(8);
            Check(f.Step(pc) && returned == 1 && f.Completed == 2 && f.Issued == 2,
                "R201 reader-independent stamp executes through actual controller enrollment");
            RetainedClean(f);
        }
    }

    static void Snapshot201Controller()
    {
        foreach (bool backing in new[] { false, true })
        {
            using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base;
            f.Map(pc); f.Fill(pc); RetainedWarm(f, pc);
            // Existing trusted fast entry is deliberately bypassed once so this
            // exercises the controller's pre-Acquire comparison itself.
            f.Jit.RevokeMapping((long)pc, 4096);
            int pulses = 0, returned = 0;
            f.Owner.TestEligibilitySnapshot201 = () => { ++pulses; Snapshot201ReadPulse(f, backing); };
            f.NativePrepared = p =>
            {
                Check(((FullBlockFrame*)p)->NativeLimit == 0,
                    "R201 pre-Acquire structural eligibility ignores completed reader pulse");
                ((FullBlockFrame*)p)->NativeLimit = 2;
            };
            f.NativeReturned = _ => { ++returned; Check(f.Jit.Authority.FullTestR185NativeFrames > 0,
                "R201 controller did not substitute a cap-one unenrolled entry"); };
            f.Counts(8);
            try { Check(f.Step(pc) && pulses == 1 && returned == 1 && f.Completed == 2 && f.Issued == 2,
                "R201 structural multiple eligibility remains available after actual reader rendezvous"); }
            finally { f.Owner.TestEligibilitySnapshot201 = null; }
            RetainedClean(f);
        }
        // Generic checked Acquire is also used for pointer-free cold staging
        // while a semantic reader blocks enrollment. It must still progress.
        using (var f = new Snapshot201Fixture())
        {
            ulong pc = Snapshot201Fixture.Base; f.Map(pc); f.Fill(pc);
            using (var reader = f.Jit.Authority.EnterCpuSemanticRead(f.WorkerId(1)))
            {
                Check(f.Acquire(pc, 0, out var lease, out bool multiple, out _) && multiple,
                    "R201 generic cold Acquire retains its structural contract under semantic reader");
                using (lease) Check(f.Jit.Store.GetHeaderCopy(in lease).ValidatedGlobalEpoch != 0,
                    "R201 pointer-free staging has a coherent code stamp");
                Snapshot201EnrollmentDenied(f.Jit.Authority,
                    "R201 pointer-free staging progresses while real native admission remains closed");
            }
            RetainedClean(f);
        }
    }

    static void Snapshot201Copies()
    {
        foreach (bool firstCode in new[] { false, true })
        foreach (bool nested in new[] { false, true })
        {
            using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base;
            f.Map(pc); f.Fill(pc); nint physical = f.Physical(pc);
            if (!firstCode) RetainedWarm(f, pc);
            var a = f.Jit.Authority; ulong code = a.CodeEpoch, sequence = a.ContentSequence;
            var copies = (IHostGuestMemoryCopies)f.Owner;
            f.At(pc); copies.BeginGuestWrite(physical + 1, 1);
            TranslationLease old = default;
            try
            {
                // Exact R190 interleaving: header revoked, store has NOT run.
                if (nested)
                {
                    copies.BeginGuestWrite(physical + 2, 1);
                    try { Check(a.CopyWriters == 2, "R201 nested actual write counted independently"); }
                    finally { copies.EndGuestWrite(); }
                    Check(a.CopyWriters == 1, "R201 inner completion cannot reopen outer writer");
                }
                Check(f.Acquire(pc, 1, out old, out bool multiple, out _) && !multiple,
                    "R201 first-code/revoked old-byte validation during actual writer remains checked");
                Check(f.Jit.Store.GetHeaderCopy(in old).ValidatedGlobalEpoch == 0,
                    "R201 revoke-before-store old snapshot is never blessed");
                ((byte*)physical)[1] = 7;
            }
            finally { f.At(pc); copies.EndGuestWrite(); }
            using (old) Check(f.Jit.Store.GetHeaderCopy(in old).ValidatedGlobalEpoch == 0,
                "R201 completion cannot promote a pending byte image");
            Check(a.CodeEpoch == code && a.ContentSequence == sequence + (nested ? 4UL : 2UL) && a.CopyWriters == 0,
                "R201 targeted writes use independent nested begin/end sequence without global code pulses");
            RetainedWarm(f, pc);
            f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.Counts(8);
            Check(f.Step(pc) && f.Completed == 1 && FullRax(f) == 7,
                "R201 actual enrolled native execution observes completed code bytes");
            RetainedClean(f);
        }
    }

    static void Snapshot201CompletedCopy()
    {
        using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base;
        f.Map(pc); f.Fill(pc); nint physical = f.Physical(pc);
        var a = f.Jit.Authority; ulong sequence = a.ContentSequence;
        // Copy starts and finishes between the two pending-writer checks, before
        // any draft exists. The content sequence must discriminate this case.
        f.Jit.TestCodeSnapshot201 = (compiling, after) =>
        {
            if (!compiling || after) return;
            var copies = (IHostGuestMemoryCopies)f.Owner;
            copies.BeginGuestWrite(physical + 1, 1);
            try { ((byte*)physical)[1] = 9; }
            finally { copies.EndGuestWrite(); }
        };
        try
        {
            Check(f.Acquire(pc, 0, out var lease, out bool multiple, out _) && !multiple,
                "R201 completed intervening copy denies coherence even with both pending checks clear");
            using (lease) Check(f.Jit.Store.GetHeaderCopy(in lease).ValidatedGlobalEpoch == 0 &&
                a.CopyWriters == 0 && a.ContentSequence == sequence + 2,
                "R201 completed-cycle proof does not collapse to CopyWriters or CodeEpoch alone");
        }
        finally { f.Jit.TestCodeSnapshot201 = null; }
        RetainedWarm(f, pc); RetainedClean(f);
    }

    static void Snapshot201Ticket()
    {
        using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base, target = pc + 256;
        f.Map(pc); f.Fill(pc);
        WriteControllerCode(f, pc, [0x48,0x89,0x03,0xeb,0xfe]); // mov [rbx],rax; jmp self
        Write64((byte*)f.Machine(), 48, target); FullRax(f, 0x12345678UL);
        RetainedWarm(f, pc);
        var a = f.Jit.Authority; ulong content = a.ContentSequence, mapping = a.MappingAuthorityEpoch;
        int ticketReturns = 0;
        Action<nint>? observe = null;
        observe = _ =>
        {
            Check(a.FullTestR185NativeFrames > 0, "R201 native ticket execution retains SC enrollment through RET");
            if (a.MappingWriters != 0)
            {
                ++ticketReturns;
                Check(a.ContentSequence == content + 1 && a.MappingAuthorityEpoch == mapping,
                    "R201 ticket begins content cycle without promoting mapping cache authority");
            }
            f.NativeReturned = observe;
        };
        f.NativeReturned = observe; f.Counts(1);
        Check(f.Step(pc) && f.Completed == 1 && f.Issued == 1 && ticketReturns == 1 &&
            a.ContentSequence == content + 2 && *(ulong*)(f.Physical(target) + 256) == 0x12345678,
            "R201 actual code-page native ticket stores between content begin/end and source pre/post revocation");
        RetainedClean(f);
    }

    static void Snapshot201Mutations()
    {
        foreach (string kind in new[] { "host", "EOF", "PTE", "raw-pointer", "failed-copy" })
        {
            using var f = new Snapshot201Fixture(); ulong pc = Snapshot201Fixture.Base;
            var file = f.Io.CreateMemoryFile("r201-source", 0);
            Check(file.Succeeded && f.Io.WriteAt(file.Value, RetainedPage(), 0).Value == 4096, "R201 owned backing setup");
            f.MapShared(file.Value, pc); RetainedWarm(f, pc); nint physical = f.Physical(pc);
            var a = f.Jit.Authority;
            lock (a.Gate)
            {
                var snapshot = a.CaptureCodeSnapshot();
                if (kind == "host") Check(f.Io.WriteAt(file.Value, new byte[] { 7 }, 1).Value == 1, "R201 actual backing write");
                if (kind == "EOF") Check(f.Io.TruncateFile(file.Value, 0).Succeeded, "R201 actual EOF mutation");
                if (kind == "PTE") Check(f.Mutate("MAP_FIXED", pc) == 0, "R201 actual PTE/backing replacement");
                if (kind == "raw-pointer") f.DataScope(0, () => f.Expose(pc, 0));
                if (kind == "failed-copy")
                {
                    var copy = (IHostGuestMemoryCopies)f.Owner; f.At(pc);
                    copy.BeginGuestWrite(physical, 1);
                    var marker = new InvalidOperationException("R201 failed bounded copy");
                    try
                    {
                        try { throw marker; }
                        finally { copy.EndGuestWrite(); }
                    }
                    catch (InvalidOperationException error) when (ReferenceEquals(error, marker)) { }
                }
                Check(!a.CodeSnapshotCurrent(in snapshot), "R201 true code/backing/PTE/pointer/failed-write transition rejects snapshot: " + kind);
            }
            RetainedClean(f);
        }
    }

    static void Snapshot201Bounds()
    {
        using var io = new SemanticTestIo();
        foreach (string kind in new[] { "begin", "copy-end", "host-end", "ticket-end" })
        {
            using var a = new FullCodeAuthority(io.Value, 4096, 1, 16384);
            if (kind == "copy-end") a.BeginGuestWrite(0);
            if (kind == "host-end") a.BeginHostWriter();
            if (kind == "ticket-end") lock (a.Gate)
                Check(a.TryReserveNativeTicket(0) && a.TryPromoteNativeTicket(0, 0x1000), "R201 exhaustion ticket setup");
            a.TestSetContentSequence201(ulong.MaxValue); // explicitly synthetic exhaustion
            if (kind == "begin")
            {
                bool rejected = false;
                try { a.BeginGuestWrite(0); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "R201 exhausted begin rejects before effects");
            }
            if (kind == "copy-end") a.EndGuestWrite(0);
            if (kind == "host-end") a.EndHostWriter();
            if (kind == "ticket-end") a.EndNativeTicket(0);
            Check(a.ContentSequence == ulong.MaxValue && a.SemanticQuarantined && a.CopyWriters == 0 &&
                a.HostWriters == 0 && a.MappingWriters == 0 && a.PendingAtomicWorker == -1 &&
                a.SemanticReadersDrained() && !a.TryEnterNative(a.Epoch),
                "R201 content exhaustion never wraps and valid release cleanup is nonthrowing: " + kind);
        }
        foreach (bool mapping in new[] { false, true })
        {
            using var a = new FullCodeAuthority(io.Value, 4096, 1, 16384);
            if (mapping) a.BeginMappingMutation();
            else lock (a.Gate)
            {
                a.Expose(0, 0x1000);
                Check(!a.PrepareCodeSnapshot(0x1000), "R201 old raw borrower blocks first-code multiple authority");
            }
            a.CodeEpochPointer[0] = ulong.MaxValue; // injected completion exhaustion
            if (mapping) a.EndMappingMutation(); else a.EndBorrow(0);
            Check(a.SemanticQuarantined && a.ActiveBorrows == 0 && a.MappingWriters == 0 &&
                a.CodeEpoch == ulong.MaxValue,
                "R201 genuine mapping/raw-pointer completion remains balanced on code exhaustion");
        }
        using (var a = new FullCodeAuthority(io.Value, 4096, 1, 16384))
        {
            ulong sequence = a.ContentSequence;
            lock (a.Gate)
            {
                Check(a.TryReserveNativeTicket(0) && a.ContentSequence == sequence &&
                    a.TryPromoteNativeTicket(0, 0x1000, writesContent: false), "R201 real pure-read ticket setup");
                a.EndNativeTicket(0);
                a.AdvanceCodePublicationRuntime();
                Check(a.ContentSequence == sequence, "R201 pure reservation/read-ticket/publication never dirties content sequence");
            }
            using (var reader = a.EnterCpuSemanticRead(0))
            {
                a.EpochPointer[0] = ulong.MaxValue; // injected failure AFTER copy count/sequence publication
                bool rejected = false;
                try { a.BeginGuestWrite(0); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected && a.CopyWriters == 0 && a.ContentSequence == sequence + 2 &&
                    a.ShortSemanticDepth(0) == 0 && a.CpuSemanticDepth(0) == 1,
                    "R201 failed begin publishes completion and preserves enclosing semantic depth");
            }
        }
        using (var a = new FullCodeAuthority(io.Value, 4096, 1, 16384))
        {
            lock (a.Gate)
            {
                var snapshot = a.CaptureCodeSnapshot();
                a.TestMappingSaturate200();
                Check(a.Saturated && !a.CodeSnapshotCurrent(in snapshot), "R201 registry saturation retains code/mapping invalidation");
                snapshot = a.CaptureCodeSnapshot(independentContent: false);
                using (var read = a.EnterCpuSemanticRead(0)) { }
                Check(!a.CodeSnapshotCurrent(in snapshot), "R201 unsupported-context runtime fallback is retained");
            }
            Check(a.Charge == FullCodeAuthority.ChargeFor(4096, 1, 16384), "R201 managed content word is charged in owner and quota estimate");
        }
    }
}
