using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using Admission185Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // ONE parent runner entry. AUTHORED_UNRUN: implementation agent did not
    // build, emit, execute or measure. Reuses the existing actual fixture.
    static void CheckFullRetainedAdmissionR185(byte* machine)
    {
        Admission185WarmTable();
        Admission185PendingTable();
        Admission185ControllerTable();
        Admission185RetryTable();
        Admission185InvalidationTable();
        Admission185BackingTable();
        Admission185MemoryAndHelper();
        Admission185CleanupAndBounds();
        Admission185WarmAllocation();
    }

    static TranslationToken Admission185Warm(Admission185Fixture f, ulong pc)
        => RetainedWarm(f, pc); // existing checked source/provenance + AuthorizeLinks

    static void Admission185Clean(Admission185Fixture f)
    {
        RetainedClean(f);
        Check(f.Jit.Authority.FullTestR185NativeFrames == 0 && f.Statistics.InterpreterAttempts == 0,
            "R185 native enrollment and interpreter tripwire clean");
    }

    static void Admission185WarmTable()
    {
        foreach (string kind in new[] { "key", "resume", "interior", "outgoing", "stale-token", "runtime-pulse" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base;
            f.Map(pc, 8192); f.Fill(pc, 8192);
            var token = Admission185Warm(f, pc);
            ulong target = kind == "outgoing" ? pc + 16 : kind == "interior" ? pc + 5 : pc;
            var expected = kind == "outgoing" ? Admission185Warm(f, target) : token;
            if (kind == "runtime-pulse") lock (f.Jit.Authority.Gate) f.Jit.Authority.Advance();
            var before = f.Statistics; var queries = f.Jit.Authority.FullTestEligibilityQueries;
            long walks = JitPageIdentity.FullTestIdentityReads;
            TranslationToken resume = kind == "key" ? default : kind == "stale-token" ? new(token.Slot, ulong.MaxValue) : token;
            var hit = f.R185Acquire(target, resume, 127); // deliberately wrong index hint
            Check(hit.Acquired && hit.Error == 0 && hit.Token == expected && hit.Multiple && hit.Retained && !hit.FetchBorrow &&
                hit.Index == (kind == "interior" ? 1u : 0u), "R185 exact retained key/resume boundary: " + kind);
            var after = f.Statistics;
            Check(after.PageTableReadBorrows == before.PageTableReadBorrows &&
                AuthorityCount(after, "RetainedEligibilityHits") == AuthorityCount(before, "RetainedEligibilityHits") + 1 &&
                AuthorityCount(after, "ByteValidations") == AuthorityCount(before, "ByteValidations") &&
                after.ActualBlockCompilations == before.ActualBlockCompilations &&
                f.Jit.Authority.FullTestEligibilityQueries == queries && JitPageIdentity.FullTestIdentityReads == walks,
                "R185 trusted acquisition opens no physical read, walk, validation or compilation: " + kind);
            Check(f.Issued == 0 && f.Completed == 0, "R185 acquisition does not issue");
            Admission185Clean(f);
        }
    }

    static void Admission185PendingTable()
    {
        foreach (string kind in new[] { "mapping", "host", "copy", "reservation", "code-borrow", "semantic", "signal", "pending-memory", "TF", "nonflat" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base, data = pc + 4096;
            f.Map(pc, 8192); f.Fill(pc, 8192);
            var token = Admission185Warm(f, pc); var a = f.Jit.Authority;
            nint physical = f.Physical(data);
            uint flags = f.Machine()->flags; ulong cs = f.Machine()->cs.@base;
            var copies = (IHostGuestMemoryCopies)f.Owner;
            if (kind == "nonflat") { f.Map(pc + 0x10000); f.Fill(pc + 0x10000); }
            void Inspect()
            {
                var before = f.Statistics;
                var result = f.R185Acquire(pc, token, forceChecked: kind is "signal" or "pending-memory",
                    requestedCount: kind == "signal" ? 1u : 0u);
                Check(result.Acquired && result.Error == 0 && !result.Retained && result.FetchBorrow &&
                    f.Statistics.PageTableReadBorrows > before.PageTableReadBorrows,
                    "R185 interaction keeps actual checked physical read: " + kind);
            }
            if (kind == "code-borrow") f.DataScope(0, () => { f.Expose(pc, 0); Inspect(); });
            else if (kind == "semantic") f.DataScope(0, Inspect);
            else
            {
                if (kind == "mapping") a.BeginMappingMutation();
                else if (kind == "host") f.Jit.BeginMemoryFileMutation(physical, 4096, false);
                else if (kind == "copy") { f.At(pc); copies.BeginGuestWrite(physical, 1); }
                else if (kind == "reservation") lock (a.Gate)
                    Check(a.TryReserveNativeTicket(f.WorkerId(0)), "R185 real reservation");
                else if (kind == "TF") f.Machine()->flags ^= 1u << Blink.FLAGS_TF;
                else if (kind == "nonflat") f.Machine()->cs.@base = 0x10000;
                try { Inspect(); }
                finally
                {
                    if (kind == "mapping") a.EndMappingMutation();
                    else if (kind == "host") f.Jit.EndMemoryFileMutation();
                    else if (kind == "copy") { f.At(pc); copies.EndGuestWrite(); }
                    else if (kind == "reservation") a.AbortNativeReservation(f.WorkerId(0));
                    f.Machine()->flags = flags; f.Machine()->cs.@base = cs;
                }
            }
            Admission185Clean(f);
        }
    }

    static void Admission185ControllerTable()
    {
        foreach (string kind in new[] { "key", "resume", "outgoing", "attention", "signal", "budget", "stop", "native-budget", "cancel-at-entry" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base;
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            f.Map(pc); f.Fill(pc); var token = Admission185Warm(f, pc);
            ulong target = kind == "outgoing" ? pc + 16 : pc;
            if (target != pc) Admission185Warm(f, target);
            f.At(target);
            if (kind is "resume" or "outgoing") f.R185Resume(token, 0);
            var before = f.Statistics;
            int prepared = 0, returned = 0;
            f.NativePrepared = pointer =>
            {
                ++prepared;
                var frame = (FullBlockFrame*)pointer;
                bool checkedPath = kind == "signal";
                Check((f.Statistics.ActiveDataBorrows != 0) == checkedPath,
                    "R185 actual controller opens physical read only for checked signal path");
                if (kind == "cancel-at-entry") f.Stop.RequestStop();
                else if (kind != "native-budget") frame->NativeLimit = 1; // test bound; real enrollment remains mandatory
            };
            f.NativeReturned = _ =>
            {
                ++returned;
                Check(f.Jit.Authority.FullTestR185NativeFrames > 0 &&
                    f.Jit.Store.GetStatistics().ActiveNativeEpochs > 0,
                    "R185 native enrollment and store epoch remain held THROUGH RET");
            };
            f.Counts(kind == "budget" ? 0UL : kind == "native-budget" ? 1UL : 8UL);
            if (kind == "attention") f.Preissued = (_, _) => f.Machine()->attention = true;
            if (kind == "stop") f.Preissued = (_, _) => f.Stop.RequestStop();
            bool stepped = f.Continue(signalActor: kind == "signal");
            if (kind is "budget" or "stop")
                Check(f.Issued == 1 && f.Completed == 0 && prepared == 0 && returned == 0 &&
                    f.Machine()->ip == target, "R185 rejected budget/late stop has no native effect");
            else if (kind == "cancel-at-entry")
                Check(stepped && f.Issued == 1 && f.Completed == 0 && prepared == 1 && returned == 1 &&
                    f.Machine()->ip == target, "R185 actual native stop poll prevents the first effect");
            else if (kind == "native-budget")
                Check(stepped && f.Issued == 2 && f.Completed == 1 && prepared == 1 && returned == 1 &&
                    f.Machine()->ip == pc + 5 && f.Stop.Reason == HostExecutionStopReason.Budget,
                    "R185 actual native issue poll rejects exactly the first over-budget issue");
            else
                Check(stepped && f.Issued == 1 && f.Completed == 1 && prepared == 1 && returned == 1 &&
                    f.Machine()->ip == target + 5 && FullRax(f) == (target == pc ? 1UL : 17UL),
                    "R185 actual controller preserves exact first issue/result: " + kind);
            if (kind is not "signal")
                Check(f.Statistics.PageTableReadBorrows == before.PageTableReadBorrows,
                    "R185 actual trusted controller has zero outer physical reads: " + kind);
            Admission185Clean(f);
        }
    }

    static void Admission185RetryTable()
    {
        foreach (string kind in new[] { "enrollment", "publication" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base;
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            f.Map(pc); f.Fill(pc); Admission185Warm(f, pc);
            var before = f.Statistics;
            bool paused = false;
            int returned = 0, preparedBefore = f.NativePreparedCalls;
            f.NativePrepared = _ =>
            {
                Check(f.Statistics.ActiveDataBorrows == 0, "R185 first retry preparation omitted physical read");
                if (kind == "enrollment") f.Jit.Authority.Advance();
                else { f.Jit.Store.PauseNativePublication(); paused = true; }
                // The first attempt MUST unwind without entering native code.
                // Its replacement hook observes that no epoch/enrollment is
                // retained while the same preissued instruction is retried.
                f.NativePrepared = p =>
                {
                    Check(returned == 0 && f.Issued == 1 && f.Completed == 0 &&
                        f.Jit.Authority.FullTestR185NativeFrames == 0 &&
                        f.Jit.Store.GetStatistics().ActiveNativeEpochs == 0 &&
                        f.Jit.Store.GetStatistics().ActiveManagedLeases == 1,
                        "R185 retry has only its new root lease, with no old native lifetime or duplicated issue");
                    if (paused) { f.Jit.Store.ResumeNativePublication(); paused = false; }
                    ((FullBlockFrame*)p)->NativeLimit = 1;
                };
            };
            f.NativeReturned = _ =>
            {
                ++returned;
                Check(f.Jit.Authority.FullTestR185NativeFrames > 0,
                    "R185 retry never executes an unenrolled cap-one shortcut");
            };
            f.Counts(1);
            try
            {
                Check(f.Step(pc) && f.Issued == 1 && f.Completed == 1 && returned == 1 &&
                    f.NativePreparedCalls == preparedBefore + 2 &&
                    f.Statistics.NativeEntries == before.NativeEntries + 1 &&
                    f.Statistics.PageTableReadBorrows == before.PageTableReadBorrows,
                    "R185 actual " + kind + " denial retries without native effects or passive read");
            }
            finally { if (paused) f.Jit.Store.ResumeNativePublication(); }
            Admission185Clean(f);
        }
    }

    static void Admission185InvalidationTable()
    {
        foreach (string kind in new[] { "same-remap", "changed-remap", "code-write", "mprotect", "munmap", "flush" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base;
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            f.Map(pc); f.Fill(pc); var token = Admission185Warm(f, pc);
            if (kind is "same-remap" or "changed-remap")
            { Check(f.Mutate("MAP_FIXED", pc) == 0, "R185 actual fixed remap"); f.Fill(pc); }
            if (kind is "changed-remap" or "code-write") f.WriteMov(pc, 9);
            if (kind is "mprotect" or "munmap") Check(f.Mutate(kind, pc) == 0, "R185 actual revoke");
            if (kind == "flush") Check(f.Jit.Store.TryFlush(out _), "R185 quiescent flush");
            var before = f.Statistics;
            var result = f.R185Acquire(pc, token);
            Check(!result.Retained && result.FetchBorrow && f.Statistics.PageTableReadBorrows > before.PageTableReadBorrows,
                "R185 invalidation cannot use an old pointer or skip checked read: " + kind);
            if (kind is "mprotect" or "munmap") Check(!result.Acquired && result.Error != 0, "R185 precise fetch failure");
            else Check(result.Acquired && result.Error == 0 && result.Immediate == (kind is "changed-remap" or "code-write" ? 9UL : 1UL) &&
                (result.Token != token) == (kind is "changed-remap" or "code-write" or "flush"),
                "R185 invalidation reuses/replaces only the correct body: " + kind);
            if (kind is "mprotect" or "munmap")
            {
                var fault = f.StepSynchronousFault(pc);
                Check(fault.Signal != 0 && f.Completed == 0 && f.Issued == 1, "R185 actual controller delivers fetch fault precisely");
            }
            else
            {
                int returned = 0;
                f.NativePrepared = p => ((FullBlockFrame*)p)->NativeLimit = 1;
                f.NativeReturned = _ => { ++returned; Check(f.Jit.Authority.FullTestR185NativeFrames > 0, "R185 revalidated native enrollment"); };
                f.Counts(1); Check(f.Step(pc) && returned == 1 && f.Issued == 1 && f.Completed == 1,
                    "R185 actual controller executes revalidated source");
            }
            Admission185Clean(f);
        }
    }

    static void Admission185BackingTable()
    {
        foreach (string kind in new[] { "shared-mutable", "host-code", "EOF", "alias" })
        {
            using var f = new Admission185Fixture(); ulong pc = Admission185Fixture.Base, alias = pc + 0x10000;
            var opened = kind == "shared-mutable" ?
                f.Io.OpenFile("/r185-mutable", FileAccessMode.Read | FileAccessMode.Write, create: true) :
                f.Io.CreateMemoryFile("r185-owned", 0);
            Check(opened.Succeeded && f.Io.WriteAt(opened.Value, RetainedPage(), 0).Value == 4096, "R185 source backing");
            int fd = opened.Value;
            if (kind == "shared-mutable")
            {
                f.Map(pc); f.Fill(pc); Admission185Warm(f, pc);
                f.Owner.FullRetainedTestMapReadOnlyShared(fd, pc);
                var mutable = f.R185Acquire(pc);
                Check(mutable.Acquired && mutable.FetchBorrow && !mutable.Retained && !mutable.Multiple,
                    "R185 foreign mutable shared backing never trusted");
            }
            else
            {
                f.MapShared(fd, pc);
                if (kind == "alias") { f.MapShared(fd, alias); f.DataScope(0, () => f.Expose(alias, 0)); }
                var token = Admission185Warm(f, pc);
                Check(f.R185Acquire(pc, token).Retained, "R185 positive observer-owned backing");
                if (kind == "EOF") Check(f.Io.TruncateFile(fd, 0).Succeeded, "R185 actual EOF change");
                else if (kind == "host-code") Check(f.Io.WriteAt(fd, new byte[] { 9, 0, 0, 0 }, 1).Value == 4, "R185 actual host code change");
                else f.WriteMov(alias, 9);
                var result = f.R185Acquire(pc, token);
                Check(!result.Retained && result.FetchBorrow &&
                    (kind == "EOF" ? !result.Acquired && result.Error != 0 : result.Acquired && result.Token != token && result.Immediate == 9),
                    "R185 checked host/EOF/alias invalidation: " + kind);
            }
            Admission185Clean(f);
        }
    }

    static void Admission185MemoryAndHelper()
    {
        foreach (bool helper in new[] { false, true })
        {
            using var f = new Admission185Fixture(helper); ulong pc = Admission185Fixture.Base, data = pc + 4096;
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            f.Map(pc, 8192); f.Fill(pc, 8192);
            if (!helper) WriteControllerCode(f, pc, [0x8b, 0x06, 0xeb, 0xfc]); // mov eax,[rsi]; jmp back
            var token = Admission185Warm(f, pc);
            if (!helper) ((ulong*)((byte*)f.Machine() + 72))[0] = data;
            f.Counts(1);
            if (helper) f.Preissued = (_, _) => f.Machine()->attention = true;
            int prepared = 0;
            // Real NeedMemory retry must open a physical scope; the first
            // trusted entry must not. A helper returns only after ALL scopes.
            Action<nint> cap = p =>
            {
                ++prepared;
                Check((f.Statistics.ActiveDataBorrows != 0) == (prepared > 1),
                    "R185 actual pending-memory retry restores physical read before mapping assist");
                ((FullBlockFrame*)p)->NativeLimit = 1;
            };
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
            Check(f.Step(pc) && f.Completed == 1 && f.Issued == 1 &&
                (helper ? f.Statistics.SemanticHelperInstructions == 1 : f.Statistics.NativeMemoryInstructions == 1 && prepared >= 2),
                "R185 real helper/memory interaction consumes one approved issue");
            Admission185Clean(f);
            if (!helper)
            {
                long reads = f.Statistics.PageTableReadBorrows;
                long assists = f.Statistics.NativeMemoryAssists;
                int returned = 0;
                f.NativePrepared = p =>
                {
                    Check(f.Statistics.ActiveDataBorrows == 0, "R185 native cached-data hit has no passive backing pin");
                    ((FullBlockFrame*)p)->NativeLimit = 1;
                };
                f.NativeReturned = _ => { ++returned; Check(f.Jit.Authority.FullTestR185NativeFrames > 0,
                    "R185 cached native data remains writer-excluding through RET"); };
                f.Counts(1);
                Check(f.Step(pc) && f.Issued == 1 && f.Completed == 1 && returned == 1 &&
                    f.Statistics.PageTableReadBorrows == reads && f.Statistics.NativeMemoryAssists == assists,
                    "R185 actual warmed native data hit eliminates the outer read without another assist");
                Admission185Clean(f);
            }
        }
        using (var f = new Admission185Fixture(helpersOnly: true))
        {
            using var deadline = new Timer(_ => f.Stop.RequestStop(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            ulong pc = Admission185Fixture.Base; f.Map(pc); f.Fill(pc); Admission185Warm(f, pc);
            f.Counts(1);
            f.DataScope(0, () =>
            {
                int depth = f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0));
                f.SignalActor(pc);
                Check(f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0)) == depth,
                    "R185 recursive signal actor preserves older semantic enrollment");
            });
            Check(f.Completed == 1 && f.Issued == 2, "R185 recursive actor preserves precise budget charge");
            Admission185Clean(f);
        }
    }

    static void Admission185WarmAllocation()
    {
        using var store = new RetainedTranslationStore(RetainedOptions());
        Check(store.TryCreateWorkerCache(out var worker), "R185 allocation witness worker");
        try
        {
            var token = RetainedPublish(store, 0x1000);
            for (int warmup = 0; warmup < 5; ++warmup) Admission185CountViews(store, ref worker, token);
            for (int sample = 0; sample < 3; ++sample)
                Check(Admission185CountViews(store, ref worker, token) == 0,
                    "R185 warmed checked view lookup/release allocates no heap objects");
        }
        finally { worker.Dispose(); }
    }

    static long Admission185CountViews(RetainedTranslationStore store, ref TranslationWorkerCache worker, TranslationToken token)
    {
        long start = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1024; ++i)
        {
            if (!store.TryAcquireEntryView(ref worker, token, new(0x1000, 7), out var lease,
                out var view, out _, out _, out _)) throw new InvalidOperationException("R185 view warm lookup");
            try
            {
                if (view.Header->BodyGeneration != token.Generation) throw new InvalidOperationException("R185 view generation");
            }
            finally { lease.Dispose(); }
        }
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    static void Admission185CleanupAndBounds()
    {
        using (var f = new Admission185Fixture())
        {
            ulong pc = Admission185Fixture.Base; f.Map(pc); f.Fill(pc); Admission185Warm(f, pc);
            Check(!f.Jit.Store.TryCreateWorkerCache(out var extra), "R185 configured worker bound unchanged"); extra.Dispose();
            f.NativePrepared = _ => throw new InvalidOperationException("R185 injected preparation unwind");
            f.Counts(4);
            bool threw = false;
            try { f.Step(pc); }
            catch (InvalidOperationException e) when (e.Message == "R185 injected preparation unwind") { threw = true; }
            Check(threw && f.Completed == 0 && f.Issued == 1, "R185 exception unwinds before native effect");
            Admission185Clean(f);
            Check(f.Jit.Store.TryFlush(out _), "R185 no retained lease prevents quiescent flush after unwind");
        }
        using var store = new RetainedTranslationStore(RetainedOptions());
        Check(store.TryCreateWorkerCache(out var worker), "R185 bounded store worker");
        var token = RetainedPublish(store, 0x1000);
        Check(store.TryAcquireEntryView(ref worker, token, new(0x1000, 7), out var lease,
            out var view, out bool resumed, out _, out _) && resumed, "R185 real checked view capability");
        ulong generation = view.Generation;
        Check(view.Header->BodyGeneration == token.Generation && *view.GenerationPointer == generation,
            "R185 view points into actively leased owner generation");
        var held = new TranslationLease[RetainedOptions().MaximumWorkers * 4 + 15];
        try
        {
            for (int i = 0; i < held.Length; ++i)
                Check(store.TryAcquireEntryView(ref worker, token, new(0x1000, 7), out held[i],
                    out _, out _, out _, out _), "R185 bounded view lease ticket");
            Check(!store.TryAcquireEntryView(ref worker, token, new(0x1000, 7), out var denied,
                out var deniedView, out _, out _, out var capacity) && capacity == TranslationStoreFailure.LeaseCapacity &&
                deniedView.Header == null, "R185 ticket exhaustion returns no speculative pointer");
            denied.Dispose();
            bool blocked = false;
            try { store.Dispose(); } catch (InvalidOperationException) { blocked = true; }
            Check(blocked && view.Header->BodyGeneration == token.Generation,
                "R185 active owner capability prevents disposal/unmap");
            Check(!store.TryFlush(out var denial) && denial == TranslationStoreFailure.LeasedCapacity,
                "R185 view lease excludes reset/unmap");
        }
        finally { foreach (var item in held) item.Dispose(); }
        lease.Dispose(); // last release services pending flush; never read view again
        Check(store.Generation > generation && !store.TryAcquireToken(token, out var stale),
            "R185 release allows generation advance and stale token rejection");
        var staleWorker = worker; worker.Dispose();
        bool rejected = false;
        try { store.TryAcquireEntryView(ref staleWorker, token, new(0x1000, 7), out var invalid,
            out _, out _, out _, out _); invalid.Dispose(); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R185 unenrolled copied worker cannot obtain an address");
        Check(store.GetStatistics().ActiveManagedLeases == 0 && store.GetStatistics().ActiveNativeEpochs == 0,
            "R185 all bounded capabilities released before owner disposal");
    }
}
