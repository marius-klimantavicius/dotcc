using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Entry195Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE ONLY / AUTHORED_UNRUN. Each group is independent and failures are
    // accumulated. The positive rows call the real controller and emitted code;
    // lookup-only API denials are labelled separately and cannot count as RET.
    static int CheckFullRetainedNativeEntryR195(byte* machine)
    {
        (string Name, Action Run)[] groups =
        [
            ("actual fast key/resume/interior and Gate contention", Entry195Native),
            ("production ACTIVE x87 and literal kernel", Entry195Active),
            ("checked fallback / hooks / repair / stale provenance", Entry195Fallback),
            ("SC writer/reader enrollment races and guarded RET", Entry195Races),
            ("catalog close and original-store lifetime", Entry195Closure),
            ("actual cached native data and pending-memory retry", Entry195Memory),
            ("retained backing revocation", () => { Admission185InvalidationTable(); Admission185BackingTable(); }),
            ("ENV and REP completion/fault cleanup", () => { Completion189Environment(); Completion189Rep(); }),
            ("R193 atomic discard and R183 reader closure", () => { Mapping193AtomicDiscard(); ScalarAtomicReaderClosure(); }),
            ("shared native/site guards", () => CheckFullSharedMemoryGuardsR188(machine)),
            ("actual last-exit repair and linked tails", () => { CheckFullLastExitTailPredecessor(); CheckFullLastExitAuthentication(); })
        ];
        var failures = new List<Exception>();
        foreach (var group in groups)
        {
            try { group.Run(); Console.WriteLine("PASS R195 " + group.Name); }
            catch (Exception error)
            { failures.Add(new InvalidOperationException(group.Name, error)); Console.Error.WriteLine(error); }
        }
        if (failures.Count != 0) throw new AggregateException("R195 retained native entry family", failures);
        Console.WriteLine($"PASS retained native entry family checks={checks}");
        return 0;
    }

    static Entry195Fixture Entry195Create(bool production = false)
    {
        var f = new Entry195Fixture();
        try
        {
            if (production)
            {
                Check(f.Jit.PrepareCanonicalX87Production(true) == FullCanonicalX87ActivationBlocker.None,
                    "R195 actual qualified production startup required; no test activation substitution");
                var state = f.CanonicalFile();
                FullX87StateStorage.ActivateFresh(state, (nint)f.Machine(), state->OwnerGeneration, state->OwnerToken);
            }
            else f.Jit.InitializeX87Kernels(true); // owner builds publish nonnull X87 even when inactive
            f.Map(Entry195Fixture.Base, 8192); f.Fill(Entry195Fixture.Base, 8192);
            return f;
        }
        catch { f.Dispose(); throw; }
    }

    static void Entry195AssertFast(Entry195Fixture f, int entries = 1)
    {
        Check(f.Entry195Counts == (entries, entries, entries, entries) && f.NativePreparedCalls == 0,
            "R195 actual fast path/enrollment/RET, zero hidden Gate preparation");
        Admission185Clean(f);
    }

    static void Entry195Native()
    {
        foreach (string kind in new[] { "key", "resume", "interior", "stale-token", "gate-held", "stop", "attention", "budget" })
        {
            using var f = Entry195Create(); using var deadline = f.AtomicDeadline();
            ulong pc = Entry195Fixture.Base;
            var token = RetainedWarm(f, pc); f.At(kind == "interior" ? pc + 5 : pc);
            if (kind != "key") f.R185Resume(kind == "stale-token" ? new(token.Slot, ulong.MaxValue) : token, 99);
            f.Entry195Ready(); f.Counts(kind == "budget" ? 0UL : 1UL, completed: 17);
            if (kind == "stop") f.Entry195Accepted = p => Volatile.Write(ref *((FullBlockFrame*)p)->Stop, 1);
            if (kind == "attention") f.Preissued = (_, _) => f.Machine()->attention = true;
            using var held = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var before = f.Statistics; long walks = JitPageIdentity.FullTestIdentityReads;
            var queries = f.Jit.Authority.FullTestEligibilityQueries;
            Task? holder = null;
            if (kind == "gate-held")
            {
                holder = Task.Run(() => { lock (f.Jit.Authority.Gate) { held.Set(); release.Wait(TimeSpan.FromSeconds(5)); } });
                Check(held.Wait(TimeSpan.FromSeconds(5)), "R195 independent writer holds Gate");
                f.Entry195Returned = _ => Check(!release.IsSet && !holder!.IsCompleted,
                    "R195 real emitted native RET completes while foreign thread owns Gate");
            }
            try { f.Continue(); }
            finally { release.Set(); holder?.GetAwaiter().GetResult(); }
            if (kind == "budget")
                Check(f.Entry195Counts == (0, 0, 0, 0) && f.Issued == 1 && f.Completed == 17,
                    "R195 managed budget rejection never borrows or executes");
            else
            {
                Entry195AssertFast(f);
                bool commits = kind != "stop";
                Check(f.Completed == (commits ? 18UL : 17UL) && f.Issued == (commits && kind != "attention" ? 2UL : 1UL),
                    "R195 first preissue, private completion publication, cancellation/attention/native budget");
            }
            var after = f.Statistics;
            Check(after.PageTableReadBorrows == before.PageTableReadBorrows &&
                after.ActualBlockCompilations == before.ActualBlockCompilations &&
                AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
                JitPageIdentity.FullTestIdentityReads == walks && f.Jit.Authority.FullTestEligibilityQueries == queries,
                "R195 positive entry does no fetch, walk, byte validation, backing eligibility or compile");
            Admission185Clean(f);
        }
    }

    static void Entry195Active()
    {
#if BLINK_FULL_X87_STATE_OWNER
        foreach (bool kernel in new[] { false, true })
        {
            using var f = Entry195Create(production: true); using var deadline = f.AtomicDeadline();
            ulong pc = Entry195Fixture.Base;
            if (kernel)
            {
                // FADD ST0,ST1 uses the actual immutable numeric kernel.
                WriteControllerCode(f, pc, [0xd8, 0xc1, 0xeb, 0xfc]);
                f.Machine()->fpu.tw = 0xfff0; f.Machine()->fpu.sw = 0; f.Machine()->fpu.cw = 0x37f;
                *FullX87RegisterFile.Physical(f.CanonicalFile(), 0) = new(0x8000000000000000, 0x3fff);
                *FullX87RegisterFile.Physical(f.CanonicalFile(), 1) = new(0x8000000000000000, 0x4000);
            }
            RetainedWarm(f, pc); f.At(pc); f.Entry195Ready(); f.Counts(1);
            f.Entry195Returned = p =>
            {
                var frame = (FullBlockFrame*)p;
                Check(frame->X87 == f.CanonicalFile() && frame->X87->ProfileActive == 1 && f.Completed == 1,
                    "R195 actual ACTIVE production registered576 frame survived exact RET guard/publication");
            };
            Check(f.Continue() && f.Completed == 1 && f.Issued == 2, "R195 production ACTIVE native result");
            Entry195AssertFast(f);
            if (kernel) Check(FullX87RegisterFile.Physical(f.CanonicalFile(), 0)->Significand == 0xc000000000000000 &&
                FullX87RegisterFile.Physical(f.CanonicalFile(), 0)->SignExponent == 0x4000,
                "R195 actual native catalog FADD computes independent literal 1+2=3");
        }
#else
        throw new InvalidOperationException("R195 ACTIVE proof requires parent build with BlinkX87StateOwner=true.");
#endif
    }

    static void Entry195Fallback()
    {
        foreach (string kind in new[] { "stale-runtime", "mapping", "prepared-hook", "returned-hook", "repair", "signal", "context", "revoked" })
        {
            using var f = Entry195Create(); using var deadline = f.AtomicDeadline(); ulong pc = Entry195Fixture.Base;
            var token = RetainedWarm(f, pc); f.At(pc); f.Entry195Ready();
            if (kind == "stale-runtime") Mapping193Pulse(f, "cpu");
            if (kind == "mapping") lock (f.Jit.Authority.Gate) f.Jit.Authority.Advance();
            if (kind is "stale-runtime" or "mapping")
            {
                var stamps = f.MappingState193.TestMappingStamp;
                Check(!f.MappingState193.RetainedMappingCacheCurrent(f.Jit.Authority, f.Jit.Authority.Epoch) &&
                    f.MappingState193.TestMappingStamp == stamps,
                    "R195 stale predicate cannot retime, discard or adopt hints");
            }
            if (kind == "revoked") f.WriteMov(pc, 9);
            if (kind == "context") f.Machine()->flags ^= 1u << Managed.Emulation.BlinkCore.FLAGS_TF;
            if (kind == "prepared-hook") f.NativePrepared = _ => Check(Monitor.IsEntered(f.Jit.Authority.Gate),
                "R195 installed legacy prepare hook keeps Gate contract");
            if (kind == "returned-hook") f.NativeReturned = _ => { };
            if (kind == "repair") f.Entry195Predecessor(token, pc);
            f.Counts(1); Check(f.Continue(signalActor: kind == "signal"), "R195 checked interaction runs");
            Check(f.Entry195Counts.Entered == 0 && f.NativePreparedCalls > 0 && f.Completed == 1,
                "R195 named fallback takes actual old preparation and computes once: " + kind);
            Admission185Clean(f);
        }
    }

    static void Entry195Races()
    {
        foreach (string kind in new[] { "writer-before", "writer-enrolled", "reader-before", "publication-before", "writer-after-accepted" })
        {
            using var f = Entry195Create(); using var deadline = f.AtomicDeadline(); ulong pc = Entry195Fixture.Base;
            RetainedWarm(f, pc); f.At(pc); f.Entry195Ready(); f.Counts(1, completed: 17);
            bool injected = false;
            void Pulse()
            {
                if (kind == "reader-before") Mapping193Pulse(f, "backing");
                else if (kind == "publication-before")
                {
                    f.Jit.Store.PauseNativePublication();
                    f.NativePrepared = _ => f.Jit.Store.ResumeNativePublication();
                }
                else { f.Jit.Authority.BeginHostWriter(); f.Jit.Authority.EndHostWriter(); }
            }
            f.Entry195Phase = (_, enrolled) =>
            {
                if (injected || kind == "writer-after-accepted" || enrolled != (kind == "writer-enrolled")) return;
                injected = true; Pulse(); // enrolled pulse uses only SC/volatile, no Gate/store/wait
            };
            if (kind == "writer-after-accepted")
            {
                f.Entry195Accepted = _ => { injected = true; Pulse(); };
                f.Entry195Returned = p => Check(((FullBlockFrame*)p)->Executed == 0 && f.Completed == 17 &&
                    ((FullBlockFrame*)p)->Exit == FullBlockExit.Invalidated,
                    "R195 emitted entry guard rejects late writer with zero native computation/publication");
            }
            Check(f.Continue() && injected && f.Completed == 18 && f.Issued >= 1 && f.NativePreparedCalls > 0,
                "R195 raced enrollment unwinds and uses checked retry exactly once: " + kind);
            Check(f.Entry195Counts.Entered == (kind == "writer-after-accepted" ? 1 : 0) &&
                f.Entry195Counts.Returned == (kind == "writer-after-accepted" ? 1 : 0),
                "R195 only accepted late-guard race reaches native RET");
            Admission185Clean(f);
        }
    }

    static void Entry195Closure()
    {
        foreach (bool closeAfterBorrow in new[] { false, true })
        {
            using var f = Entry195Create(); ulong pc = Entry195Fixture.Base;
            RetainedWarm(f, pc); f.At(pc); f.Entry195Ready();
            if (closeAfterBorrow) f.Entry195Phase = (_, enrolled) => { if (!enrolled) f.Jit.TestCloseCatalog195(); };
            else f.Jit.TestCloseCatalog195();
            // Direct denial oracle, deliberately no controller retry against a
            // terminal closed owner. It must never be mistaken for a native RET.
            Check(!f.Entry195DirectAttempt() && f.Entry195Counts.Entered == 0 && f.Completed == 0,
                "R195 closed literal catalog never grants cached native admission");
            bool blocked = false;
            try { f.Jit.TestDisposeCatalog195(); } catch (InvalidOperationException) { blocked = true; }
            Check(blocked && !f.Jit.Store.NativeLifetimeClosed && !f.Jit.TestCatalogOpen195,
                "R195 catalog unmap still requires original store lifetime closure");
            Admission185Clean(f);
        }
    }

    static void Entry195Memory()
    {
        using var f = Entry195Create(); using var deadline = f.AtomicDeadline();
        ulong pc = Entry195Fixture.Base, data = pc + 4096;
        WriteControllerCode(f, pc, [0x8b, 0x06, 0xeb, 0xfc]);
        Write64((byte*)f.Machine(), 72, data + 64);
        RetainedWarm(f, pc); f.At(pc); f.Entry195Ready(); f.Counts(1);
        f.Entry195Phase = (_, enrolled) =>
        {
            if (!enrolled) f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 1;
        };
        Check(f.Continue() && f.Completed == 1 && f.Entry195Counts.Entered == 1 &&
            f.Entry195Counts.Returned == 1 && f.NativePreparedCalls > 0 && f.Statistics.NativeMemoryAssists > 0,
            "R195 actual fast NeedMemory returns before checked pending-memory assist");
        // The pending retry's cap1 returned before another issue/budget poll.
        // No stop reset or forged hint is needed for the next ordinary entry.
        f.Entry195Phase = null;
        Check(f.Stop.Reason == HostExecutionStopReason.None, "R195 cap1 pending retry preserves live stop owner");
        f.At(pc); f.Entry195Ready(); f.Counts(1);
        var before = f.Entry195Counts; long assists = f.Statistics.NativeMemoryAssists;
        int prepared = f.NativePreparedCalls;
        Check(f.Continue() && f.Completed == 1 && f.Entry195Counts.Entered == before.Entered + 1 &&
            f.Entry195Counts.Returned == before.Returned + 1 && f.NativePreparedCalls == prepared &&
            f.Statistics.NativeMemoryAssists == assists,
            "R195 real warmed data executes through fast epoch without physical assist");
        Admission185Clean(f);
    }
}
