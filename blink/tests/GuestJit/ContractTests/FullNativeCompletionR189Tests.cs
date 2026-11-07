using Managed.Emulation.Execution;
using Completion189Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class RetainedTranslationStore
    {
        // Test-only bounded view. The caller retains the exact managed lease
        // until it has finished reading this immutable published code span.
        internal ReadOnlySpan<byte> Completion206Code(scoped in TranslationLease lease)
        {
            lock (gate)
            {
                ValidateLease(in lease);
                var body = records[lease.Token.Slot];
                return new((void*)body->Header.Entry, body->UsefulCode);
            }
        }
    }

    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal sealed partial class FullControllerFixture
        {
            internal bool Completion189Private(FullBlockFrame* frame)
            {
                fixed (long* global = &Owner.completed)
                    return frame->Completed != null && frame->Completed != global;
            }
        }
    }
}

unsafe partial class Program
{
    // One family entry; AUTHORED_UNRUN. Uses actual current controllers and the
    // existing return/prepare/REP-pause/ENV-Ready hooks, never a fake executor.
    static void CheckFullNativeCompletionR189()
    {
        (string Name, Action Run)[] rows =
        [
            ("R206 both ABI encodings / explicit capability / validation", Completion206Encoding),
            ("R206 actual compiler bodies / independent kernel selection", Completion206ProductionCode),
            ("R206 actual dynamic commit / pre-sample traps", Completion206Boundaries),
            ("multi-site / inherited Executed / native budget", () => Completion189Chain(false)),
            ("linked tails", () => Completion189Chain(true)),
            ("preissue / stop / pre-call throw / post-RET throw / managed helper", Completion189Boundaries),
            ("pending memory", Completion189Memory),
            ("REP intermediate yields / completion / FaultFinalize / hook throw", Completion189Rep),
            ("ENV Ready / deferred stop / pre-call throw / post-RET throw", Completion189Environment),
            ("two workers / concurrent aggregate / exact shared issue budget", Completion189Concurrent),
            ("warm actual controller / zero managed allocation", Completion189Warm)
        ];
        foreach (var row in rows)
        {
            try { row.Run(); }
            catch (Exception error) { throw new InvalidOperationException("R189 " + row.Name, error); }
        }
    }

    // R206 is ONE extension of this existing family. The R189 runtime rows
    // below continue to cover actual Full/ENV/REP private counters, concurrent
    // publication, issue/budget boundaries, stop and throwing-hook cleanup.
    // Byte rows are source-authored / UNRUN on both ABIs; Windows is UNVERIFIED.
    static void Completion206Bytes(ReadOnlySpan<byte> code, int commits, bool owner, int issues = 0)
    {
        Check(CountFullPattern(code, [0x49, 0x8b, 0x42, 24, 0x48, 0xff, 0x00]) == (owner ? commits : 0) &&
            CountFullPattern(code, [0x49, 0x8b, 0x42, 24, 0xf0, 0x48, 0xff, 0x00]) == (owner ? 0 : commits),
            "R206 only explicit owner-private emission removes LOCK from Completed");
        Check(CountFullPattern(code, [0xf0, 0x48, 0x0f, 0xc1, 0x10]) == issues,
            "R206 exact shared Issue/Budget LOCK XADD stays in its original scaffold");
    }

    static void Completion206Encoding()
    {
        byte[] common = new byte[FullBlockEmitter.ScaffoldCapacity];
        byte[] general = new byte[FullBlockEmitter.CodeCapacity], owned = new byte[FullBlockEmitter.CodeCapacity];
        foreach (bool windows in new[] { false, true })
        foreach (bool retained in new[] { false, true })
        {
            // Neither IsValid NOR EmitOwnerScaffold/retainedIdentity grants the
            // completion capability: the general body API remains locked for BOTH.
            FullSharedScaffoldLayout layout;
            int commonLength = retained ? FullBlockEmitter.EmitOwnerScaffold(common, out layout, windows) :
                FullBlockEmitter.EmitScaffold(common, out layout, windows);
            var scaffold = layout.Resolve((nint)0x0000000100000000);
            Completion206Bytes(common.AsSpan(0, commonLength), 0, false, issues: 1);
            FullBlockInstruction[][] shapes =
            [
                FullShape(1, 0), FullShape(8, 0), FullShape(8, 1), FullShape(8, 2),
                [new(new(Ip, 2, JitOperation.IndirectJump, 64, default, new(0, 0)))],
                [new(EnvironmentClassify(new([0x0f, 0x31], FullProcessorEnvironmentOperation.Rdtsc, 64, 0)))],
                [new(EnvironmentClassify(new([0x48, 0x0f, 0xc7, 0xf0], FullProcessorEnvironmentOperation.Rdrand, 64, 0)))]
            ];
            foreach (var sites in shapes)
            {
                int commits = sites.Count(site => !site.RequiresHelper);
                // ENV has a Ready-entry commit plus the ordinary site's unreachable
                // after-request commit; both must obey the explicit writer capability.
                if (FullBlockEmitter.CanInlineProcessorEnvironment(sites[0].Instruction)) ++commits;
                int n = FullBlockEmitter.Emit(sites, general, windows);
                Completion206Bytes(general.AsSpan(0, n), commits, false, issues: 1);
                int shared = FullBlockEmitter.Emit(sites, general, in scaffold, windows);
                Completion206Bytes(general.AsSpan(0, shared), commits, false);
                Check(FullBlockEmitter.TryEmitOwnerPrivateCompletion(sites, owned, in scaffold, out int length, windows) &&
                    length == shared - commits && owned[6] == (windows ? 0xca : 0xfa),
                    "R206 explicit owner body removes exactly one byte per commit on either ABI");
                Completion206Bytes(owned.AsSpan(0, length), commits, true);
            }
            // Shared owner and general APIs run the SAME validation and oversize
            // core, including a destination too short to publish any code.
            foreach (bool owner in new[] { false, true })
            foreach (string kind in new[] { "invalid-ir", "missing-scaffold", "abi-mismatch", "short" })
            {
                FullBlockInstruction[] sites = kind == "invalid-ir" ? [new(new(Ip, 0, JitOperation.Nop, 64, default, default))] : FullShape(1, 0);
                var binding = kind == "missing-scaffold" ? default : scaffold;
                bool abi = kind == "abi-mismatch" ? !windows : windows;
                Span<byte> destination = kind == "short" ? owned.AsSpan(0, 1) : owned;
                bool rejected = false;
                try
                {
                    int length;
                    bool ok = owner ? FullBlockEmitter.TryEmitOwnerPrivateCompletion(sites, destination, in binding, out length, abi) :
                        FullBlockEmitter.TryEmit(sites, destination, in binding, out length, abi);
                    rejected = !ok && length == 0;
                }
                catch (ArgumentException) { rejected = kind != "short"; }
                Check(rejected, "R206 owner/general strict validation or zero-length oversize: " + kind);
            }
        }
        foreach (bool windows in new[] { false, true })
        foreach (var operation in new[] { FullRepOperation.Load, FullRepOperation.Compare })
        {
            var rep = new FullRepInstruction(Ip, 2, operation, 8, 64, -1,
                operation == FullRepOperation.Compare ? FullRepRepeat.WhileEqual : FullRepRepeat.Count);
            int generalLength = FullBlockEmitter.EmitRepKernel(rep, 73, 91, general, out var generalLayout, windows);
            int length = FullBlockEmitter.EmitOwnerPrivateCompletionRepKernel(rep, 73, 91, owned, out var ownerLayout, windows);
            Completion206Bytes(general.AsSpan(0, generalLength), 1, false);
            Completion206Bytes(owned.AsSpan(0, length), 1, true);
            Completion206Bytes(owned.AsSpan(ownerLayout.FaultFinalizeOffset, length - ownerLayout.FaultFinalizeOffset), 0, true);
            Check(length == generalLength - 1 && ownerLayout.FaultFinalizeOffset == generalLayout.FaultFinalizeOffset - 1 &&
                owned[6] == (windows ? 0xca : 0xfa) &&
                owned[ownerLayout.FaultFinalizeOffset + 6] == (windows ? 0xca : 0xfa),
                "R206 independent Resume/FaultFinalize preserve both ABI entries and no fault completion");
            foreach (bool owner in new[] { false, true })
            foreach (string kind in new[] { "invalid-ir", "owner-cookie", "kernel-cookie", "short" })
            {
                var instruction = kind == "invalid-ir" ? rep with { Width = 24 } : rep;
                ulong ownerCookie = kind == "owner-cookie" ? 0UL : 73UL, kernelCookie = kind == "kernel-cookie" ? 0UL : 91UL;
                Span<byte> destination = kind == "short" ? owned.AsSpan(0, 1) : owned;
                bool rejected = false;
                try
                {
                    if (owner) FullBlockEmitter.EmitOwnerPrivateCompletionRepKernel(instruction, ownerCookie, kernelCookie, destination, out _, windows);
                    else FullBlockEmitter.EmitRepKernel(instruction, ownerCookie, kernelCookie, destination, out _, windows);
                }
                catch (ArgumentException) { rejected = true; }
                Check(rejected, "R206 independent owner/general kernel strict rejection: " + kind);
            }
        }
    }

    static void Completion206ProductionCode()
    {
        // Inspect exact published bytes while their real store lease is live;
        // never guess a code span length or read an unprotected raw body pointer.
        foreach (var row in new (byte[] Bytes, int Commits, bool Rep)[]
        {
            ([0x90], 1, false), ([0xff, 0xe0], 1, false),
            ([0x0f, 0x31], 2, false), ([0x48, 0x0f, 0xc7, 0xf0], 2, false),
            ([0xf3, 0xac], 0, true), ([0xf3, 0xa6], 0, true)
        })
        {
            using var f = new Completion189Fixture();
            ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, row.Bytes);
            Check(f.Acquire(pc, 0, out var lease, out _, out int error, requestedCount: 1) && error == 0,
                "R206 actual compiler/scratch publishes the requested owner body");
            using (lease)
            {
                Completion206Bytes(f.Jit.Store.Completion206Code(in lease), row.Commits, true);
                if (row.Rep)
                {
                    Check(FullCanonicalEncoding.TryClassifyRep(pc, row.Bytes, out var rep, out _), "R206 real REP specialization");
                    lock (f.Jit.Authority.Gate)
                    {
                        var binding = f.Jit.RepKernels.Find(in rep);
                        using var kernel = f.Jit.RepKernels.Acquire(in binding);
                        byte[] expected = new byte[FullBlockEmitter.RepKernelCapacity];
                        var key = rep with { Ip = 0, Length = 2 };
                        _ = FullBlockEmitter.EmitOwnerPrivateCompletionRepKernel(key, f.Jit.Store.OwnerCookie,
                            binding.Cookie, expected, out var layout);
                        // Resume-to-FaultFinalize is an exact published boundary,
                        // independent of any guessed function length or RX padding.
                        int resumeLength = checked((int)(binding.FaultFinalize - binding.Resume));
                        Check(resumeLength == layout.FaultFinalizeOffset, "R206 actual owner kernel boundary");
                        var actual = new ReadOnlySpan<byte>((void*)binding.Resume, resumeLength);
                        Check(actual.SequenceEqual(expected.AsSpan(0, resumeLength)),
                            "R206 actual independent production kernel uses the explicit owner capability");
                        Completion206Bytes(actual, 1, true);
                    }
                }
            }
            RetainedClean(f);
        }
    }

    static void Completion206Boundaries()
    {
        foreach (bool trap in new[] { false, true })
        {
            using var f = new Completion189Fixture();
            using var deadline = f.AtomicDeadline();
            ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc);
            WriteControllerCode(f, pc, trap ? [0x0f, 0x31] : [0xff, 0xe0]);
            if (trap) { f.Machine()->traprdtsc = true; f.Machine()->segvcode = 1; }
            else Write64((byte*)f.Machine(), 24, pc + 64); // JMP RAX dynamic commit
            long delta = 0; int returns = 0, samples = 0;
            FullProcessorEnvironmentProtocol.TestAfterSample = _ => ++samples;
            f.NativeReturned = pointer => { ++returns; Completion189Observe(f, (FullBlockFrame*)pointer, ref delta); };
            f.Counts(1, completed: 17);
            try
            {
                if (trap) Check(f.StepSynchronousFault(pc).Signal == 11, "R206 native traprdtsc before any data sample");
                else Check(f.Step(pc, signalActor: true) && f.Machine()->ip == pc + 64, "R206 actual dynamic commit reaches exact target");
                Check(f.Completed == (trap ? 17UL : 18UL) && f.Issued == 1 && delta == (trap ? 0 : 1) && returns == 1 && samples == 0,
                    "R206 native dynamic commit versus trapping pre-sample denial publishes the exact delta once");
                RetainedClean(f);
            }
            finally { FullProcessorEnvironmentProtocol.TestAfterSample = null; f.NativeReturned = null; }
        }
    }

    static void Completion189Observe(Completion189Fixture f, FullBlockFrame* frame, ref long total)
    {
        Check(f.Completion189Private(frame), "R189 native completion pointer is private and live at RET callback");
        long delta = *frame->Completed;
        Check(delta >= 0, "R189 native invocation delta is nonnegative");
        total += delta;
        Check(f.Completed == 17UL + (ulong)total, "R189 global completion is published before callback");
    }

    static void Completion189Chain(bool tail)
    {
        using var f = new Completion189Fixture();
        using var deadline = f.AtomicDeadline();
        ulong pc = Completion189Fixture.Base, target = tail ? pc + 64 : pc;
        f.Map(pc); f.Fill(pc);
        WriteControllerCode(f, target, [0x90, 0x90, 0xeb, 0xfc]); // two NOPs; JMP target
        if (tail) WriteControllerCode(f, pc, [0x90, 0xeb, 61]);
        RetainedWarm(f, target); RetainedWarm(f, pc);
        // Executed is a TOTAL cap/index accumulator, not this invocation's delta.
        // The shared scaffold deliberately preserves an inherited nonzero value.
        f.NativePrepared = pointer => ((FullBlockFrame*)pointer)->Executed = 5;
        long delta = 0; int returns = 0;
        f.NativeReturned = pointer => { ++returns; Completion189Observe(f, (FullBlockFrame*)pointer, ref delta); };
        f.Counts(9, completed: 17);
        Check(f.Step(pc) && f.Completed == 26 && f.Issued == 10 && delta == 9 && returns == 1,
            "R189 one real chained call aggregates nine commits, excludes inherited Executed and rejected Issue");
        Check(!tail || f.Statistics.NativeTailJumps > 0, "R189 actual authenticated linked tail executes");
        RetainedClean(f);
    }

    static void Completion189Boundaries()
    {
        foreach (string kind in new[] { "preissued", "budget", "stop", "prepare-throw", "return-throw", "helper" })
        {
            using var f = new Completion189Fixture(helpersOnly: kind == "helper");
            ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc);
            var injected = new InvalidOperationException("R189 injected managed failure");
            if (kind == "prepare-throw") f.NativePrepared = _ => throw injected;
            if (kind == "stop") f.Stop.RequestStop();
            long delta = 0; int returns = 0;
            f.NativeReturned = pointer =>
            {
                ++returns; Completion189Observe(f, (FullBlockFrame*)pointer, ref delta);
                if (kind == "return-throw") throw injected;
            };
            f.Counts(kind == "budget" ? 0UL : 1UL, completed: 17);
            bool threw = false, result = false;
            try { result = f.Step(pc, signalActor: true); }
            catch (InvalidOperationException error) when (ReferenceEquals(error, injected)) { threw = true; }
            bool commits = kind is "preissued" or "return-throw" or "helper";
            Check(f.Completed == (commits ? 18UL : 17UL) && f.Issued == (kind == "stop" ? 0UL : 1UL),
                "R189 exact issue/completion at managed and native boundaries");
            Check(threw == (kind is "prepare-throw" or "return-throw") &&
                returns == (kind is "preissued" or "return-throw" or "helper" ? 1 : 0) &&
                delta == (kind is "preissued" or "return-throw" ? 1 : 0) &&
                result == (kind is "preissued" or "helper"), "R189 zero before call, once after RET including throwing hook");
            RetainedClean(f);
        }
    }

    static void Completion189Memory()
    {
        using var f = new Completion189Fixture();
        using var deadline = f.AtomicDeadline();
        ulong pc = Completion189Fixture.Base, data = pc + 0x10000;
        f.Map(pc); f.Fill(pc); f.Map(data); // deliberately lazy backing
        WriteControllerCode(f, pc, [0x90, 0x48, 0x8b, 0x03, 0x90, 0xeb, 0xfe]);
        Write64((byte*)f.Machine(), 48, data); // RBX
        long delta = 0; int misses = 0;
        f.NativeReturned = pointer =>
        {
            var frame = (FullBlockFrame*)pointer;
            Completion189Observe(f, frame, ref delta);
            if (frame->Exit == FullBlockExit.NeedMemory) ++misses;
        };
        f.Counts(3, completed: 17);
        for (int attempt = 0; f.Completed < 20 && attempt < 8; ++attempt)
            Check(attempt == 0 ? f.Step(pc) : f.Continue(), "R189 actual pending-memory continuation");
        Check(misses > 0 && delta == 3 && f.Completed == 20 && f.Issued == 4 &&
            f.Machine()->ip == pc + 5 && f.Statistics.NativeMemoryInstructions == 1,
            "R189 each memory call has fresh delta; the retained preissue is consumed once");
        RetainedClean(f);
    }

    static void Completion189Rep()
    {
        foreach (string kind in new[] { "complete", "fault", "return-throw" })
        {
            using var f = new Completion189Fixture();
            using var deadline = f.AtomicDeadline();
            ulong pc = Completion189Fixture.Base, source = pc + 0x10000;
            f.Map(pc); f.Fill(pc); f.Map(source, kind == "fault" ? 4096 : 8192);
            f.Fill(source, kind == "fault" ? 4096 : 8192);
            WriteControllerCode(f, pc, [0x90, 0xf3, 0xac, 0xeb, 0xfe]); // NOP; REP LODSB
            RepSeed(f, source + 4095, 0, 0, 3, f.Machine()->flags & ~0x10500u);
            long delta = 0; int yields = 0, finals = 0, quanta = 0;
            var injected = new InvalidOperationException("R189 REP return failure");
            f.Owner.RepPausedForTests = (pointer, reason) =>
            {
                // Diagnostic quantum is already supported; do not add a stop
                // check, permit or alternate native path to production code.
                var state = (FullRepState*)pointer;
                if (reason == FullRepReason.Latched) state->QuantumLimit = 1;
                if (reason == FullRepReason.Quantum) ++quanta;
            };
            f.NativeReturned = pointer =>
            {
                var frame = (FullBlockFrame*)pointer;
                Completion189Observe(f, frame, ref delta);
                if (frame->Rep != null && frame->Rep->Phase == FullRepPhase.FaultFinal)
                {
                    ++finals;
                    Check(*frame->Completed == 0 && frame->Executed == 0 && f.Completed == 18,
                        "R206 fresh FaultFinalize has a private zero delta and never completes the faulting REP");
                }
                if (frame->Exit is FullBlockExit.RepYield or FullBlockExit.NeedMemory)
                {
                    ++yields;
                    Check(f.Completed == 18 && *frame->Completed == (frame->Executed == 0 ? 0 : 1),
                        "R189 partial REP iterations never become whole completions");
                }
                if (kind == "return-throw" && frame->Rep != null && frame->Rep->Phase == FullRepPhase.Done)
                    throw injected;
            };
            f.Counts(2, completed: 17);
            bool failed = false;
            try
            {
                if (kind == "fault")
                    Check(f.StepSynchronousFault(pc).Signal == 11, "R189 REP fault uses real guest delivery");
                else
                {
                    for (int attempt = 0; f.Completed < 19 && attempt < 8; ++attempt)
                        Check(attempt == 0 ? f.Step(pc) : f.Continue(), "R189 REP controller continuation");
                }
            }
            catch (InvalidOperationException error) when (ReferenceEquals(error, injected)) { failed = true; }
            finally { f.Owner.RepPausedForTests = null; f.NativeReturned = null; }
            Check(f.Completed == (kind == "fault" ? 18UL : 19UL) && f.Issued == 2 &&
                delta == (kind == "fault" ? 1 : 2) && yields > 0 && quanta > 0 &&
                finals == (kind == "fault" ? 1 : 0) && failed == (kind == "return-throw"),
                "R189 Resume and fresh FaultFinalize publish only whole commits once, including throwing completion hook");
            RepClean(f);
        }
    }

    static void Completion189Environment()
    {
        foreach (var row in new[]
        {
            new EnvironmentCase([0x0f, 0x31], FullProcessorEnvironmentOperation.Rdtsc, 64, 0),
            new EnvironmentCase([0x0f, 0x01, 0xf9], FullProcessorEnvironmentOperation.Rdtscp, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf0], FullProcessorEnvironmentOperation.Rdrand, 64, 0),
            new EnvironmentCase([0x48, 0x0f, 0xc7, 0xf8], FullProcessorEnvironmentOperation.Rdseed, 64, 0)
        })
        foreach (string kind in new[] { "complete", "ready-throw", "return-throw" })
        {
            using var f = new Completion189Fixture();
            var clock = new EnvironmentClock(); var entropy = new EnvironmentEntropy();
            EnvironmentBind(clock, entropy);
            var injected = new InvalidOperationException("R189 ENV injected failure");
            long delta = 0; int samples = 0, consumed = 0; nint sampledCounter = 0, sampledUnion = 0;
            try
            {
                ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc);
                WriteControllerCode(f, pc, [..row.Bytes, 0x90]);
                EnvironmentSeed((byte*)f.Machine(), realWorker: true);
                FullProcessorEnvironmentProtocol.TestAfterSample = pointer =>
                {
                    var frame = (FullBlockFrame*)pointer; ++samples;
                    Check(f.Completion189Private(frame) && *frame->Completed == 0 && f.Completed == 17,
                        "R189 ENV Ready already owns the exact fresh private counter before native call");
                    sampledCounter = (nint)frame->Completed; sampledUnion = (nint)frame->StackMemory;
                    if (kind == "ready-throw") throw injected;
                    f.Stop.RequestStop(); // R177: no post-sample guard may discard the approved commit
                };
                f.NativeReturned = pointer =>
                {
                    var frame = (FullBlockFrame*)pointer;
                    Completion189Observe(f, frame, ref delta);
                    if (!FullProcessorEnvironmentProtocol.IsEnvironment(frame)) return;
                    var descriptor = (FullEnvironmentDataDescriptor*)frame->StackMemory;
                    if (descriptor->State != FullEnvironmentDataState.Consumed) return;
                    ++consumed;
                    Check((nint)frame->Completed == sampledCounter && *frame->Completed == 1 && f.Completed == 18,
                        "R189 ENV exact pointer survives Ready, RET, RequireConsumed and return hook");
                    if (kind == "return-throw") throw injected;
                };
                f.Counts(8, completed: 17);
                bool failed = false;
                try { Check(f.Step(pc), "R189 ENV actual dedicated controller"); }
                catch (InvalidOperationException error) when (ReferenceEquals(error.InnerException, injected)) { failed = true; }
                Check(samples == 1 && f.Issued == 1 && f.Completed == (kind == "ready-throw" ? 17UL : 18UL) &&
                    delta == (kind == "ready-throw" ? 0 : 1) && consumed == (kind == "ready-throw" ? 0 : 1) &&
                    failed == (kind != "complete") && f.Jit.Authority.SemanticQuarantined == failed,
                    "R189 ENV zero without call, once after RET even when sampled terminal hook throws");
                if (kind == "complete")
                    Check(((FullStackMemoryDescriptor*)sampledUnion)->Version == FullStackMemoryDescriptor.CurrentVersion &&
                        ((FullStackMemoryDescriptor*)sampledUnion)->Count == 0,
                        "R189 normal ENV acknowledgement follows publication");
                RetainedClean(f);
            }
            finally { f.NativeReturned = null; FullProcessorEnvironmentProtocol.TestAfterSample = null; f.RestoreEnvironment(); }
        }
    }

    static void Completion189Concurrent()
    {
        using var f = new Completion189Fixture();
        using var deadline = f.AtomicDeadline();
        ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc); RetainedWarm(f, pc, 1);
        const int iterations = 256;
        using var rendezvous = new Barrier(2);
        nint[] counters = new nint[2]; int returns = 0;
        f.NativeReturned = pointer =>
        {
            var frame = (FullBlockFrame*)pointer;
            if (!f.Completion189Private(frame) || *frame->Completed != 1)
                throw new InvalidOperationException("R189 concurrent call has no private delta of one");
            if (Interlocked.Increment(ref returns) > 2) return;
            int worker = frame->Machine == f.Machine() ? 0 : 1;
            counters[worker] = (nint)frame->Completed;
            if (!rendezvous.SignalAndWait(TimeSpan.FromSeconds(5)) || counters[0] == counters[1] ||
                counters[0] == 0 || counters[1] == 0 || f.Completed < 19)
                throw new InvalidOperationException("R189 simultaneous returned workers must already publish distinct private counters");
        };
        f.Counts(2 * iterations, completed: 17);
        using (var child = new AtomicFamilyThread(f, 1, () =>
        {
            for (int i = 0; i < iterations; ++i)
                if (!f.Step(pc, 1, signalActor: true)) throw new InvalidOperationException("R189 child stopped");
        }))
        {
            for (int i = 0; i < iterations; ++i)
                if (!f.Step(pc, signalActor: true)) throw new InvalidOperationException("R189 main stopped");
        }
        f.NativeReturned = null;
        Check(returns == 2 * iterations && f.Completed == 17 + 2 * iterations && f.Issued == 2 * iterations,
            "R189 concurrent atomic publication loses no delta and consumes the exact shared issue budget");
        Check(!f.Step(pc, signalActor: true) && f.Issued == 2 * iterations + 1 &&
            f.Completed == 17 + 2 * iterations, "R189 next shared issue is rejected with no completion");
        RetainedClean(f);
    }

    static void Completion189Warm()
    {
        using var f = new Completion189Fixture();
        using var deadline = f.AtomicDeadline();
        ulong pc = Completion189Fixture.Base; f.Map(pc); f.Fill(pc); RetainedWarm(f, pc);
        // Reuse R185's bounded real retained-controller pattern. The one-shot
        // prepared hook is rearmed with the same delegate, outside allocation.
        Action<nint> cap = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
        f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
        f.Counts(10000, completed: 17);
        for (int i = 0; i < 32; ++i) Check(f.Step(pc), "R189 warm actual controller");
        var compilations = f.Statistics.ActualBlockCompilations;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 512; ++i)
            if (!f.Step(pc)) throw new InvalidOperationException("R189 warm controller stopped");
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0 && f.Completed == 17 + 32 + 512 && f.Issued == 32 + 512 &&
            f.Statistics.ActualBlockCompilations == compilations,
            "R189 warmed actual controller adds zero managed allocations and preserves exact accounting");
        RetainedClean(f);
    }
}
