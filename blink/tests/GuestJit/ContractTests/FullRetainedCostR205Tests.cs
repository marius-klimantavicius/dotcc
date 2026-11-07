using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using Cost205Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // ONE grouped serial family. SOURCE ONLY / AUTHORED_UNRUN.
    static void CheckFullRetainedCostR205(byte* machine)
    {
        Cost205HeldWriter();
        Cost205ControllerRetry();
        Cost205AffectedAndUnknown();
        Cost205ReadWriteAlias();
        Cost205RepTickets();
        Cost205CompoundGrammar();
        // Retain actual SC native, Ready pointer, authority and immutable guard
        // gates. R203 includes R201 (cold reader/own ticket, host/EOF, bounded
        // copies, ENV, REP source retirement/nested cleanup and R185 retries).
        CheckFullImmutableGuardR202(machine);
        CheckFullPreciseMappingsR203(machine);
        CheckFullRetainedAuthority(machine);
        CheckFullStackMemoryAdmission();
        CheckFullNativeStackMemory(machine);
        CheckFullNativeNearControl(machine);
        CheckFullStringFormsR162(machine);
        CheckFullCanonicalX87Controls(machine);
        CheckFullEnterDemandMaterialization();
        CheckFullMaskedStoreControllerBoundaries();
    }

    static void Cost205SameTrust(TranslationBlockHeader a, TranslationBlockHeader b)
        => Check(a.Live == b.Live && a.CodeEpoch == b.CodeEpoch &&
            a.ValidatedCodeEpoch == b.ValidatedCodeEpoch && a.ValidatedGlobalEpoch == b.ValidatedGlobalEpoch &&
            a.Reserved136 == b.Reserved136 && a.BodyGeneration == b.BodyGeneration && a.Entry == b.Entry,
            "R205 exact previous live/source/trust/entry metadata preserved");

    static void Cost205NoChurn(GuestJitStatistics before, GuestJitStatistics after)
        => Check(after.PageTableReadBorrows == before.PageTableReadBorrows &&
            AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
            AuthorityCount(after, "Restamps") == AuthorityCount(before, "Restamps") &&
            AuthorityCount(after, "ZeroStampRestamps") == AuthorityCount(before, "ZeroStampRestamps") &&
            after.ActualBlockCompilations == before.ActualBlockCompilations,
            "R205 foreign denial has zero passive/PTE/byte/restamp/compile churn");

    static void Cost205HeldWriter()
    {
        foreach (string kind in new[] { "copy", "precise-mapping", "reservation" })
        {
            using var f = new Cost205Fixture(); ulong pc = Cost205Fixture.Base, data = pc + 4096;
            f.Map(pc, 8192); f.Fill(pc, 8192); nint physical = f.Physical(data);
            var a = f.Jit.Authority;
            lock (a.Gate) Check(a.TryGetNativeDataPermission(physical, false, out _), "R205 known unrelated data role");
            var token = RetainedWarm(f, pc); var header = f.R205Header(token);
            using var held = new ManualResetEventSlim(); using var finish = new ManualResetEventSlim();
            using var writer = new AtomicFamilyThread(f, 1, () =>
            {
                int worker = f.WorkerId(1); var copies = (IHostGuestMemoryCopies)f.Owner;
                if (kind == "copy") copies.BeginGuestWrite(physical, 1);
                else lock (a.Gate)
                {
                    if (kind == "precise-mapping") a.BeginOrExtendOwnedMapping(worker, precise: true);
                    else Check(a.TryReserveNativeTicket(worker), "R205 real foreign reservation");
                }
                try { held.Set(); AtomicFamilyWait(finish, "R205 release held foreign writer"); }
                finally
                {
                    // Completion needs Gate in the mapping case. The retry
                    // must release it and hold no lifetime/read/lease on return.
                    if (kind == "copy") copies.EndGuestWrite();
                    else if (kind == "precise-mapping") a.EndOwnedMappingMutation(worker);
                    else a.AbortNativeReservation(worker);
                }
            });
            try
            {
                AtomicFamilyWait(held, "R205 writer held after begin, before effects/end");
                var before = f.Statistics; long walks = JitPageIdentity.FullTestIdentityReads;
                var queries = a.FullTestEligibilityQueries;
                for (int n = 0; n < 8; ++n)
                {
                    var result = f.R185Acquire(pc, token);
                    Check(!result.Acquired && !result.FetchBorrow && !result.Retained && !result.Multiple && result.Error == 0,
                        "R205 retry returns no native capability or execution fallback");
                    Cost205SameTrust(header, f.R205Header(token));
                }
                var after = f.Statistics; Cost205NoChurn(before, after);
                Check(JitPageIdentity.FullTestIdentityReads == walks && a.FullTestEligibilityQueries == queries &&
                    AuthorityCount(after, "RetainedValidationDefers") == AuthorityCount(before, "RetainedValidationDefers") + 8,
                    "R205 held denial takes bounded store lookups, no PTE/backing eligibility probes");
                Snapshot201EnrollmentDenied(a, "R205 preserved source trust does not weaken writer exclusion");
                var s = f.Jit.Store.GetStatistics();
                Check(s.ActiveManagedLeases == 0 && s.ActiveNativeEpochs == 0 &&
                    f.Statistics.ActiveDataBorrows == 0 && a.FullTestR185NativeFrames == 0,
                    "R205 held foreign writer is the only surviving capability");
            }
            finally { finish.Set(); writer.Join(); }
            var hit = f.R185Acquire(pc, token);
            Check(hit.Acquired && hit.Retained && hit.Token == token && !hit.FetchBorrow,
                "R205 writer completion retries exact original trust without minting it");
            RetainedClean(f);
        }
    }

    static void Cost205ControllerRetry()
    {
        foreach (string kind in new[] { "retry", "attention", "cancel" })
        {
            bool cancel = kind == "cancel";
            using var f = new Cost205Fixture(); ulong pc = Cost205Fixture.Base, data = pc + 4096;
            using var deadline = f.AtomicDeadline();
            f.Map(pc, 8192); f.Fill(pc, 8192); nint physical = f.Physical(data);
            var a = f.Jit.Authority;
            lock (a.Gate) Check(a.TryGetNativeDataPermission(physical, false, out _), "R205 controller known data");
            var token = RetainedWarm(f, pc); var header = f.R205Header(token);
            using var held = new ManualResetEventSlim(); using var finish = new ManualResetEventSlim();
            using var writer = new AtomicFamilyThread(f, 1, () =>
            {
                var copies = (IHostGuestMemoryCopies)f.Owner;
                copies.BeginGuestWrite(physical, 1);
                try { held.Set(); AtomicFamilyWait(finish, "R205 controller releases copy"); }
                finally { copies.EndGuestWrite(); }
            });
            try
            {
                AtomicFamilyWait(held, "R205 controller foreign copy began");
                var before = f.Statistics; long walks = JitPageIdentity.FullTestIdentityReads;
                int defers = 0, returns = 0;
                f.Jit.TestRetainedDeferred205 = () =>
                {
                    ++defers;
                    Check(f.Issued == 1 && f.Completed == 0 && a.FullTestR185NativeFrames == 0 &&
                        f.Jit.Store.GetStatistics().ActiveManagedLeases == 0 &&
                        f.Jit.Store.GetStatistics().ActiveNativeEpochs == 0,
                        "R205 actual retry keeps same approved issue and releases every execution capability");
                    Cost205NoChurn(before, f.Statistics);
                    Cost205SameTrust(header, f.R205Header(token));
                    if (cancel) f.Stop.RequestStop();
                    finish.Set(); // writer may need Gate; never join inside it
                };
                f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 1;
                f.NativeReturned = _ => { ++returns; Check(a.FullTestR185NativeFrames > 0, "R205 successful retry really enrolled"); };
                if (kind == "attention") f.Preissued = (_, _) => f.Machine()->attention = true;
                f.Counts(1);
                Check(f.Step(pc) && defers > 0 && f.Issued == 1 &&
                    f.Completed == (cancel ? 0UL : 1UL) && returns == (cancel ? 0 : 1) &&
                    f.Machine()->ip == pc + (cancel ? 0UL : 5UL), "R205 same-approved retry/stop has exact effects");
                Check(JitPageIdentity.FullTestIdentityReads == walks, "R205 actual controller retry does not walk PTEs");
            }
            finally { f.Jit.TestRetainedDeferred205 = null; finish.Set(); writer.Join(); }
            RetainedClean(f);
        }
    }

    static void Cost205AffectedAndUnknown()
    {
        foreach (string kind in new[] { "affected", "untrusted" })
        {
            using var f = new Cost205Fixture(); ulong pc = Cost205Fixture.Base;
            f.Map(pc); f.Fill(pc); var token = RetainedWarm(f, pc);
            if (kind == "untrusted")
            {
                // A checked acquisition has no minted trust until AuthorizeLinks.
                f.Jit.RevokeMapping((long)pc, 4096);
                Check(f.Acquire(pc, 0, out var lease, out _, out _), "R205 checked source setup");
                using (lease) Check(f.Jit.Store.GetHeaderCopy(in lease).Reserved136 == 0, "R205 no fresh trust forged");
            }
            nint physical = f.Physical(pc);
            using var held = new ManualResetEventSlim(); using var finish = new ManualResetEventSlim();
            using var writer = new AtomicFamilyThread(f, 1, () =>
            {
                var copies = (IHostGuestMemoryCopies)f.Owner;
                copies.BeginGuestWrite(physical + 1, 1);
                try
                {
                    held.Set(); AtomicFamilyWait(finish, "R205 affected writer may complete actual store");
                    ((byte*)physical)[1] = 9;
                }
                finally { copies.EndGuestWrite(); }
            });
            try
            {
                AtomicFamilyWait(held, "R205 foreign source revoked before first store");
                var before = f.Statistics;
                var hit = f.R185Acquire(pc, token);
                Check(hit.Acquired && hit.FetchBorrow && !hit.Retained && !hit.Multiple &&
                    AuthorityCount(f.Statistics, "RetainedValidationDefers") == AuthorityCount(before, "RetainedValidationDefers"),
                    "R205 revoked/untrusted source cannot use preservation shortcut");
                Check(f.R205Header(token).ValidatedGlobalEpoch == 0, "R205 affected pending source has no executable stamp");
            }
            finally { finish.Set(); writer.Join(); }
            var fresh = f.R185Acquire(pc, token);
            Check(fresh.Acquired && fresh.Token != token && fresh.Immediate == 9 && !fresh.Retained,
                "R205 genuine source change retires old content and checks new bytes");
            RetainedClean(f);
        }
        // Real unknown/foreign backing, EOF and fetch permissions are retained
        // by the R185 backing/invalidation tables in the enclosing family.
    }

    // R205 specifically exercises the conservative ticket protocol. This real
    // forwarding observer retains the same mutation barrier but deliberately
    // does NOT authenticate the optional R207 retained-read capability. R207's
    // family separately verifies owned reads avoid tickets and foreign reads do not.
    sealed class Cost205UnretainedReads : IHostMemoryFileMutation, IDisposable
    {
        readonly InstanceIo io;
        readonly IHostMemoryFileMutation target;
        internal Cost205UnretainedReads(Cost205Fixture fixture)
        {
            io = fixture.Io; target = fixture.Jit;
            io.UnbindMemoryFileMutation(target);
            io.BindMemoryFileMutation(this);
        }
        public void BeginMemoryFileMutation(nint page, long length, bool logical)
            => target.BeginMemoryFileMutation(page, length, logical);
        public void EndMemoryFileMutation() => target.EndMemoryFileMutation();
        public void Dispose()
        {
            io.UnbindMemoryFileMutation(this);
            io.BindMemoryFileMutation(target);
        }
    }

    static void Cost205ReadWriteAlias()
    {
        foreach (bool write in new[] { false, true })
        {
            using var f = new Cost205Fixture(); using var deadline = f.AtomicDeadline();
            using var unretainedReads = new Cost205UnretainedReads(f);
            ulong pc = Cost205Fixture.Base, body = pc + 0x10000, alias = pc + 0x20000;
            f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, write ? new byte[] { 0xc6,0x03,0x09,0xeb,0xfe } : new byte[] { 0x48,0x8b,0x03,0xeb,0xfe });
            var file = f.Io.CreateMemoryFile("r205-owned-code", 0);
            Check(file.Succeeded && f.Io.WriteAt(file.Value, RetainedPage(), 0).Value == 4096, "R205 actual owned CODE alias file");
            f.MapShared(file.Value, body); f.MapShared(file.Value, alias);
            f.DataScope(0, () => f.Expose(alias, 0));
            nint physical = f.Physical(body);
            Check(physical == f.Physical(alias), "R205 shared aliases name actual same owned physical CODE");
            var token = RetainedWarm(f, body); RetainedWarm(f, pc);
            var old = f.R205Header(token); ulong expected = *(ulong*)(physical + 1);
            Write64((byte*)f.Machine(), 48, alias + 1);
            var before = f.Statistics; int ticketReturns = 0;
            Action<nint>? observe = null;
            observe = p =>
            {
                var frame = (FullBlockFrame*)p;
                if (f.Jit.Authority.MappingWriters != 0)
                {
                    ++ticketReturns;
                    Check(f.Jit.Authority.FullTestR185NativeFrames > 0, "R205 real ticket is enrolled through RET");
                    if (!write) Cost205SameTrust(old, f.R205Header(token));
                    frame->MemoryAccess = write ? FullMemoryAccess.Read : FullMemoryAccess.Write;
                    // Deliberately mutate returned summary: finally must use
                    // the pre-promotion immutable scope decision at both edges.
                }
                f.NativeReturned = observe;
            };
            f.NativeReturned = observe; f.Counts(1);
            Check(f.Step(pc) && f.Issued == 1 && f.Completed == 1 && ticketReturns == 1,
                "R205 actual native read/write ticket completes exactly once");
            var after = f.Statistics;
            if (!write)
            {
                Cost205SameTrust(old, f.R205Header(token));
                Check(FullRax(f) == expected &&
                    AuthorityCount(after, "TicketReadSourcePreSkips") == AuthorityCount(before, "TicketReadSourcePreSkips") + 1 &&
                    AuthorityCount(after, "TicketReadSourcePostSkips") == AuthorityCount(before, "TicketReadSourcePostSkips") + 1 &&
                    AuthorityCount(after, "TicketWriteSourceRevokedBodies") == AuthorityCount(before, "TicketWriteSourceRevokedBodies"),
                    "R205 read alias leaves CODE trust intact and skips both source invalidations");
            }
            else
            {
                Check(((byte*)physical)[1] == 9 && f.R205Header(token).Live == 0 &&
                    AuthorityCount(after, "TicketWriteSourcePreRevocations") == AuthorityCount(before, "TicketWriteSourcePreRevocations") + 1 &&
                    AuthorityCount(after, "TicketWriteSourcePostRevocations") == AuthorityCount(before, "TicketWriteSourcePostRevocations") + 1 &&
                    AuthorityCount(after, "TicketWriteSourceRevokedBodies") >= AuthorityCount(before, "TicketWriteSourceRevokedBodies") + 2,
                    "R205 matching real alias write retains both invalidation edges despite returned read summary");
                Check(f.R185Acquire(body, token).Immediate == 9, "R205 changed CODE is recompiled from actual new bytes");
            }
            RetainedClean(f);
        }
    }

    static void Cost205RepTickets()
    {
        foreach (byte opcode in new byte[] { 0xa4, 0xaa, 0xac, 0xa6, 0xae })
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (byte prefix in new byte[] { 0xf3, 0xf2 })
        {
            if (prefix == 0xf2 && opcode is not (0xa6 or 0xae)) continue;
            using var f = new Cost205Fixture(); using var deadline = f.AtomicDeadline();
            using var unretainedReads = new Cost205UnretainedReads(f);
            ulong pc = Cost205Fixture.Base, body = pc + 0x10000, alias = pc + 0x20000;
            f.Map(pc); f.Fill(pc);
            byte[] bytes = RepLoweringBytes(opcode, width, false, 0, prefix);
            WriteControllerCode(f, pc, [..bytes, 0xeb, 0xfe]);
            var file = f.Io.CreateMemoryFile("r205-rep-code", 0);
            Check(file.Succeeded && f.Io.WriteAt(file.Value, RetainedPage(), 0).Value == 4096, "R205 REP shared CODE setup");
            f.MapShared(file.Value, body); f.MapShared(file.Value, alias);
            f.DataScope(0, () => f.Expose(alias, 0));
            var token = RetainedWarm(f, body); var old = f.R205Header(token);
            bool writes = opcode is 0xa4 or 0xaa;
            RepSeed(f, alias + 256, alias + 512, 5, 1, 0x202); f.Counts(1);
            var before = f.Statistics;
            Check(f.Step(pc, signalActor: true) && f.Issued == 1 && f.Completed == 1,
                "R205 all REP read/write classes and widths keep one whole issue");
            var after = f.Statistics;
            string pre = writes ? "TicketWriteSourcePreRevocations" : "TicketReadSourcePreSkips";
            string post = writes ? "TicketWriteSourcePostRevocations" : "TicketReadSourcePostSkips";
            Check(AuthorityCount(after, pre) > AuthorityCount(before, pre) &&
                AuthorityCount(after, pre) - AuthorityCount(before, pre) == AuthorityCount(after, post) - AuthorityCount(before, post),
                "R205 REP immutable IR classifies and balances actual pre/post ticket edges");
            if (!writes) Cost205SameTrust(old, f.R205Header(token));
            else Check(f.R205Header(token).Live == 0, "R205 REP MOVS/STOS invalidate genuine CODE writes");
            RepClean(f);
        }
    }

    static void Cost205CompoundGrammar()
    {
        // Actual compound/native/EOF/fault execution is also retained above.
        // This narrow adversarial check catches the dangerous read-summary
        // interpretation directly at the production decision boundary.
        using var f = new Cost205Fixture(); ulong pc = Cost205Fixture.Base;
        f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, [0x48,0x8f,0x03,0xeb,0xfe]); // POP [rbx]
        Write64((byte*)f.Machine(), 48, pc + 512); Write64((byte*)f.Machine(), 56, pc + 256); f.At(pc);
        Check(f.Acquire(pc, 0, out var lease, out _, out _), "R205 exact compound plan");
        using (lease)
        {
            var plan = FullTranslationJit.Plans(f.Jit.Store, in lease)[0];
            var decoded = plan.Decoded;
            Check(FullTranslationJit.TryClassifyNative(pc, new(decoded.bytes, decoded.length), out var instruction), "R205 immutable POP classification");
            FullStackMemoryDescriptor d = new() { Version = FullStackMemoryDescriptor.CurrentVersion,
                Count = 2, State = FullStackMemoryState.Request, Site = 0,
                Request0 = new() { Address = pc + 256, Access = FullMemoryAccess.Read, Width = 64 },
                Request1 = new() { Address = pc + 512, Access = FullMemoryAccess.Write, Width = 64 } };
            FullBlockFrame frame = new() { Machine = f.Machine(), StackMemory = &d, MemoryResume = 1,
                MemorySite = 0, ResumeIndex = 0, MemoryAddress = pc + 256, MemoryAccess = FullMemoryAccess.Read, MemoryWidth = 64 };
            Check(FullNativeMemory.StackTicketWritesContent(&frame, in instruction, f.Machine()),
                "R205 POP pair with read summary still has immutable write decision");
            d.Request1.Access = FullMemoryAccess.Read;
            bool rejected = false;
            try { FullNativeMemory.StackTicketWritesContent(&frame, in instruction, f.Machine()); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "R205 forged pure-read compound is rejected before write decision");
        }
        RetainedClean(f);
    }
}
