using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Owner-wide retained mixed TBs. A worker hint is a nonowning token, not an
// admission policy. Helpers are chosen here, once per actual translation; no
// runtime opcode dispatch or old interpreter entry is reachable from execution.
internal sealed unsafe partial class FullTranslationJit : IDisposable, IHostGuestCodeRevocation, IHostMemoryFileMutation
{
    partial void FullTranslationSnapshotTaken(ulong pc);
    partial void FullTranslationPublished(TranslationKey key, TranslationToken token,
        int instructionCount, int inlineCount, int codeBytes, int payloadBytes);
    internal readonly struct Plan
    {
        internal readonly Blink.XedDecodedInst Decoded;
        internal readonly delegate*<Blink, Blink.Machine*, ulong, long, ulong, void> Helper;
        internal readonly ulong Ip;
        internal int Operation => (int)((Decoded.op.rde >> 40) & 2047);
        internal Plan(ulong ip, Blink.XedDecodedInst decoded,
            delegate*<Blink, Blink.Machine*, ulong, long, ulong, void> helper)
        { Ip = ip; Decoded = decoded; Helper = helper; }
    }
    internal readonly RetainedTranslationStore Store;
    internal readonly FullRepKernelStore RepKernels;
    internal readonly FullCodeAuthority Authority;
    internal readonly FullMemoryLifetime Memory;
    internal readonly FullAllocationMeter Allocations;
    private readonly long[] helperCalls, helperCompleted;
    private long compiles, compileAttempts, compileAllocated, emitted, nativeCalls, nativeInstructions, tailJumps;
    private long helperCount, helperInstructions, validationMisses, lookupMisses, blockInstructions;
    private long nativeMemoryComputations, memoryAssists;
    private readonly long[] memoryAssistOutcomes = new long[(int)FullMemoryAssistResult.UnsupportedBacking + 1];
    private readonly long[] memoryRetryDenials = new long[(int)FullDataPermissionDenial.PhysicalWriter + 1];
    private readonly long[] memoryMissObservations = new long[FullMemoryMissDiagnostics.ReasonCount];
    private ulong memoryLastAssistPc, memoryLastAssistAddress;
    private long diagnosticAllocated, setupAllocated;
    private long linkTargetLookups, linkReuseHits, deferredLinkPreparations;
    private int publicationWorker = -1;
    private long publicationRequests, publicationAdmissionDenials;
    private long virtualWriteNotifications, virtualWriteFallbacks;
    private long trustedAcquisitions, byteValidations, authorityRestamps;
    // 24 bytes reserved within the existing 4096-byte fixed owner allowance.
    private long retainedEligibilityHits, retainedEligibilityMisses, resumeRangeRejects;
    private int peakResident;
    private double compileMs;
    private readonly long overhead;
    private long peakCharge;
    private bool inlineEnabled;
    private readonly FullCompilerScratch compilerScratch;
    private readonly long[] blockLengths = new long[FullCompilerScratch.MaximumInstructions + 1];
    private readonly long[] blockStops = new long[Enum.GetValues<BlockStop>().Length];
    private long compiledInlineSites, compiledHelperSites, oversizedRetries;
    private readonly long[] nativeExits = new long[(int)FullBlockExit.NeedEnvironment + 1];
    private enum BlockStop { Count, Requested, Page, ControlFlow, State, Decode, CodeLimit }
    private readonly NativeCodeAllocation? sharedCode;
    private readonly FullSharedScaffold sharedScaffold;
    private readonly int sharedEmittedBytes;
    private readonly long sharedReservedBytes;
    private bool disposed;

    internal static FullTranslationJit Create(InstanceIo io, GuestJitOptions options, int workers, ulong memoryLimitBytes)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        var owner = new FullTranslationJit(io, options, workers, memoryLimitBytes);
        owner.setupAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        return owner;
    }
    internal FullTranslationJit(InstanceIo io, GuestJitOptions options, int workers, ulong memoryLimitBytes)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        // Integer status capture uses baseline SETcc. Optional host features
        // must be checked by the individual lowering that actually needs them.
        inlineEnabled = NativeCodePlatform.UnavailableReason(options.AllowUnverifiedWindows, requireNativeFlags: false) == null;
        long scratch = FullCompilerScratch.ChargeFor(workers) + (long)FullCompilerScratch.SlotCount * FullBlockEmitter.ScratchBytes +
            (FullCompilerScratch.MaximumInstructions + 1L + 7 + (int)FullBlockExit.NeedEnvironment + 1) * sizeof(long);
        // WorkerCharge includes aligned native storage (x87 exactly once) and
        // an independent128-byte managed binding/new Worker allowance. Reserve
        // all configured slots before constructing worker storage; arena/store
        // flushes never release this fixed owner reservation.
        // Reserve the ordinary invocation's private completion and captured
        // ticket decision (8 bytes each); REP/ENV charge their own scope locals.
        overhead = FullMemoryLifetime.ChargeFor(memoryLimitBytes) +
            FullCodeAuthority.ChargeFor(memoryLimitBytes, workers, options.CacheLimitBytes) +
            scratch + workers * (16L + 2 * sizeof(long) + FullNativeMemory.WorkerCharge + sizeof(LastExitLink) + ThreadedGuestExecution.AtomicScopeCharge) + 2 * 2048 * sizeof(long) + 4096 + RetainedCostCharge +
            144 + 2 * sizeof(long) + (memoryAssistOutcomes.LongLength + memoryRetryDenials.LongLength + memoryMissObservations.LongLength) * sizeof(long) +
            FullBlockEmitter.ScaffoldCapacity + TranslationCodeRegions.RoundPage(FullBlockEmitter.ScaffoldCapacity) + 128 +
            FullRepKernelStore.FixedChargeFor(workers) + workers * ThreadedGuestExecution.RepScopeCharge +
            workers * ThreadedGuestExecution.ProcessorEnvironmentScopeCharge;
        if (options.CacheLimitBytes - overhead < 1 << 20)
            throw new InvalidOperationException($"Full-JIT configured retention quota {options.CacheLimitBytes} bytes cannot cover {overhead} bytes of fixed owner/worker/compiler resources plus the minimum retained store; no guest instruction executed.");
        helperCalls = new long[2048]; helperCompleted = new long[2048];
        Allocations = new(workers);
        Memory = new(memoryLimitBytes); Authority = new(io, memoryLimitBytes, workers, options.CacheLimitBytes);
        try { compilerScratch = new(workers); }
        catch { Authority.Dispose(); Memory.Dispose(); throw; }
        try
        {
            string? unavailable = NativeCodePlatform.UnavailableReason(options.AllowUnverifiedWindows, requireNativeFlags: false);
            if (unavailable != null) throw new PlatformNotSupportedException("Full-JIT: " + unavailable);
            // One immutable common native slab belongs to this owner, not to
            // any body or flush generation. Its RX publication also proves OS
            // publication policy before creating/binding a guest Machine.
            Span<byte> common = stackalloc byte[FullBlockEmitter.ScaffoldCapacity];
            sharedEmittedBytes = FullBlockEmitter.EmitOwnerScaffold(common, out var sharedLayout);
            if (!NativeCodeAllocation.TryCreate(common[..sharedEmittedBytes], out var allocation,
                    out string publicationReason, requireNativeFlags: false))
                throw new PlatformNotSupportedException("Full-JIT native publication: " + publicationReason);
            sharedCode = allocation!;
            sharedReservedBytes = sharedCode.AllocatedBytes;
            if (sharedReservedBytes > TranslationCodeRegions.RoundPage(FullBlockEmitter.ScaffoldCapacity))
                throw new InvalidOperationException("Full-JIT common native slab exceeded its precharged reservation.");
            sharedScaffold = sharedLayout.Resolve(sharedCode.Entry);
            Store = new(new TranslationStoreOptions { BudgetBytes = options.CacheLimitBytes - overhead,
                RegionBytes = options.CacheLimitBytes <= 8L << 20 ? 65536 : 1 << 20,
                MaximumWorkers = workers, AllowUnverifiedWindows = options.AllowUnverifiedWindows,
                RequireNativeFlags = false });
            RepKernels = new(Authority, Store, workers, options.AllowUnverifiedWindows);
            peakCharge = Store.ChargedBytes + overhead;
            setupAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        }
        catch { RepKernels?.Dispose(); Store?.Dispose(); sharedCode?.Dispose(); compilerScratch.Dispose(); Authority.Dispose(); Memory.Dispose(); throw; }
    }
    // Authority.Gate owns this designation. Store admission remains closed
    // until this compiler publishes or its controller unwinds/cancels.
    private void RequestPublication(int worker)
    {
        if (publicationWorker == worker) return;
        if (publicationWorker >= 0) throw new InvalidOperationException("Full-JIT publication phase has another owner.");
        Store.PauseNativePublication(); publicationWorker = worker; ++publicationRequests;
        // The paused store owns host code-arena W^X publication only. Guest
        // first-code roles are independently revoked by PrepareCodeSnapshot.
        try { Authority.AdvanceCodePublicationRuntime(); }
        catch { Store.ResumeNativePublication(); publicationWorker = -1; throw; }
    }
    internal void CancelPublication(int worker)
    {
        // Only this worker's synchronous compiler can designate its id. Its
        // unwind cannot race another compiler for the same worker; foreign
        // publication ownership needs no cancellation here. Avoid contending
        // on the global gate after every ordinary retained native return.
        if (Volatile.Read(ref publicationWorker) != worker) return;
        lock (Authority.Gate)
            if (publicationWorker == worker)
            {
                Store.ResumeNativePublication(); publicationWorker = -1;
            }
    }
    internal void CheckProducer(Blink program)
    {
        if (program.BlinkGuestPreciseFaultProtocol() != 1 || program.BlinkGuestCopyProtocol() != 1 || program.BlinkGuestPhysicalCodeProtocol() != 2 ||
            program.BlinkGuestCodeRevocationProtocol() != 1 || program.BlinkGuestMappingRevocationProtocol() != 1)
            throw new InvalidOperationException("Full-JIT requires actual-copy and physical-alias producer protocols.");
    }
    private const ulong FlatLongContext = 1UL << 63;
    internal static ulong Context(Blink program, Blink.Machine* m, uint requestedCount = 0) => (uint)m->mode.omode | ((ulong)(requestedCount & 511) << 48) |
        (ulong)(uint)m->mode.genmode << 8 | FullCanonicalX87ExecutionProfile.ContextBits(m) | (ulong)(m->flags & ((1u << Blink.FLAGS_TF) | (1u << Blink.FLAGS_RF) | (1u << Blink.FLAGS_AC))) << 16 |
        (m->mode.omode == Blink.XED_MODE_LONG && m->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
            program.GetPc(m) == (long)m->ip ? FlatLongContext : 0);
    internal void BeginRead(Blink p, bool semantic) => Memory.BeginRead(p, semantic);
    internal void EndRead(Blink p) => Memory.EndRead(p, this);
    internal void RecycleAnonymousPage(Blink p, Blink.System* s, byte* page, bool clear, bool precise = false)
    {
        bool bounded = BeginAnonymousRetirement(page, precise);
        try { Memory.RecycleAnonymousPage(p, s, page, clear, bounded); }
        finally { Authority.EndPhysicalRetirement(bounded); }
    }
    // Deferred clear/freelist insertion needs a NEW physical proof. A code
    // alias can be compiled after detachment but before the last passive reader
    // drops; detachment's earlier revocation cannot cover that later effect.
    internal void RecycleDeferredAnonymousPage(Blink p, byte* page, bool clear, bool precise)
    {
        bool bounded = BeginAnonymousRetirement(page, precise);
        try { p.BlinkGuestRecycleAnonymousPage(null, page, clear ? 1 : 0); }
        finally { Authority.EndPhysicalRetirement(bounded); }
    }
    private bool BeginAnonymousRetirement(byte* page, bool precise)
    {
        if (page == null || ((nuint)page & 4095) != 0)
            throw new InvalidOperationException("Full-JIT anonymous retirement address invariant.");
        precise = Authority.BeginPhysicalRetirement(precise);
        try
        {
            Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page, 4096, includeDrafts: false);
            Authority.WaitNativeFrames();
            return precise;
        }
        catch { Authority.EndPhysicalRetirement(precise); throw; }
    }
    internal void PhysicalExposed(int worker, nint page)
    {
        lock (Authority.Gate)
            if (!disposed && Authority.Expose(worker, page))
            {
                if (Authority.Saturated) Store.MarkAuthorityChanged(0, 0x800000000000UL);
                else Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page, 4096);
                Authority.WaitNativeFrames();
            }
    }
    // Full native bodies touch CPU registers only. For these per-machine
    // paged routes the producer resolves/checks the page and calls physical
    // exposure BEFORE a writable pointer escapes. That callback revokes code
    // aliases and drains native execution when the actual backing is code.
    // Ordinary data therefore need not invalidate every retained body's epoch.
    internal void NotifyVirtualWrite(Blink.Machine* machine, long address, long size)
    {
        lock (Authority.Gate)
        {
            if (disposed) return;
            ++virtualWriteNotifications;
            // These predicates unconditionally choose the physical-callback
            // branch of LookupAddress2Access, independently of shared CR0.
            // Other metal/legacy contexts keep the conservative early drain.
            if (machine != null && ((int)machine->metal == 0 || machine->mode.omode == Blink.XED_MODE_LONG)) return;
            ++virtualWriteFallbacks;
            Authority.AdvanceCode(); Authority.Advance();
            Store.MarkAuthorityChanged(0, 0x800000000000UL);
            Authority.WaitNativeFrames();
        }
    }
    public void Revoke(long address, long size) => NotifyVirtualWrite(null, address, size);
    internal void RevokeOwnedMapping(Blink program, Blink.Machine* machine, int worker, long address, long size, bool authenticatedContext, ref bool ownsScope)
    {
        lock (Authority.Gate)
        {
            if (disposed) return;
            bool precise = authenticatedContext && FullCodeAuthority.TryPreciseMappingRange(address, size, out _, out _);
            Authority.BeginOrExtendOwnedMapping(worker, precise);
            ownsScope = true; // Begin succeeded; every later failure belongs to the outer finally.
            precise &= Authority.OwnsPreciseMapping(worker);
            if (!precise) Authority.AdvanceCode(); // preserve generic begin/callback/end global pulses
            Authority.Advance();
            if (precise)
            {
                FullCodeAuthority.TryPreciseMappingRange(address, size, out ulong start, out ulong length);
                Store.MarkAuthorityChanged(start, length);
                if (!RevokeMappingPhysicalSources(program, machine, start, length))
                {
                    Authority.BeginOrExtendOwnedMapping(worker, precise: false);
                    Authority.AdvanceCode();
                    Store.MarkAuthorityChanged(0, 0x800000000000UL);
                }
            }
            else Store.MarkAuthorityChanged(0, 0x800000000000UL);
            Authority.WaitNativeFrames();
        }
    }
    public void RevokeMapping(long address, long size)
    {
        lock (Authority.Gate)
        {
            if (disposed) return;
            Authority.AdvanceCode(); Authority.Advance();
            Store.MarkAuthorityChanged((ulong)address, (ulong)size);
            Authority.WaitNativeFrames();
        }
    }
    public void BeginMemoryFileMutation(nint page, long length, bool logical)
    {
        Authority.BeginHostWriter();
        try
        {
            // No Authority.Gate/IO calls in this observer. Drafts cannot run;
            // their caller checks the validation epoch before native admission.
            if (!logical && page != 0 && length > 0)
                Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page, (ulong)length, includeDrafts: false);
            else Store.MarkAuthorityChanged(0, 0x800000000000UL, includeDrafts: false);
            Authority.WaitNativeFrames();
        }
        catch { Authority.EndHostWriter(); throw; }
    }
    public void EndMemoryFileMutation() => Authority.EndHostWriter();
    internal static ReadOnlySpan<Plan> Plans(RetainedTranslationStore store, in TranslationLease lease)
        => MemoryMarshal.Cast<byte, Plan>(store.GetPayloadCopySpan(in lease));

    // The caller already approved the first issue before any demand fetch.
    // Own Authority.Gate + passive physical lease; no helper runs in this scope.
    internal bool Acquire(Blink program, Blink.Machine* machine, ref TranslationWorkerCache worker,
        TranslationToken resumeToken, uint resumeIndex, out TranslationLease lease,
        out uint index, out bool multiple, out int error, uint requestedCount = 0)
    {
        error = 0; index = 0; multiple = false; lease = default;
        if (publicationWorker >= 0 && publicationWorker != worker.Worker)
        { ++publicationAdmissionDenials; return false; }
        ulong context = Context(program, machine, requestedCount);
        if (resumeToken.Generation != 0 && Store.TryAcquireToken(resumeToken, out lease))
        {
            var header = Store.GetHeaderCopy(in lease);
            int match = -1;
            // Native Continue commonly names the predecessor while IP already
            // names its successor. Reject that payload before its store lock.
            // Unsigned relative range also handles a body ending at MaxValue.
            if (unchecked(machine->ip - header.GuestPc) >= header.GuestLength)
                ++resumeRangeRejects;
            else if (header.Context == context)
            {
                var plans = Plans(Store, in lease);
                if (resumeIndex < (uint)plans.Length && plans[(int)resumeIndex].Ip == machine->ip)
                    match = (int)resumeIndex;
                else
                    for (int i = 0; i < plans.Length; ++i)
                        if (plans[i].Ip == machine->ip) { match = i; break; }
            }
            if (match >= 0) index = (uint)match;
            else lease.Dispose();
        }
        if (lease.Owner == null && !Store.TryAcquireKey(ref worker, new(machine->ip, context), out lease, out var lookupFailure))
        {
            if (lookupFailure != TranslationStoreFailure.NotFound)
                throw new InvalidOperationException("Full-JIT resident lookup denied: " + lookupFailure);
            ++lookupMisses;
        }
        if (lease.Owner != null)
        {
            var header = Store.GetHeaderCopy(in lease);
            bool stableAuthority = header.Live == 1 && header.CodeEpoch == header.ValidatedCodeEpoch &&
                header.ValidatedGlobalEpoch == Authority.CodeEpoch;
            // Generic callers already own their passive read. Release just
            // this root lease on a foreign denial; the controller's earlier
            // AcquireNativeEntry retry also avoids opening that read at all.
            // Own ticket source reinspection MUST take the checked path below.
            if (requestedCount == 0 && stableAuthority && header.CodeEpoch != 0 && header.Entry != 0 &&
                header.Context == context && (context & FlatLongContext) != 0 && header.Reserved136 == 1 &&
                machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
                program.GetPc(machine) == (long)machine->ip && Authority.ShouldDeferTrustedSource(worker.Worker))
            {
                lease.Dispose(); lease = default; index = 0;
                RecordRetainedValidationDefer(); return false;
            }
            // A trusted retained body has a complete writer protocol. Every
            // physical borrow/mapping/host writer revokes before effects and
            // the native enrollment handshake repeats the epoch check. Reuse
            // that authority instead of comparing all retained sites at every helper
            // resume. Mutable external-file bodies never take this fast path.
            if (stableAuthority && header.Context == context && (header.Context & FlatLongContext) != 0 &&
                header.Reserved136 == 1 && Authority.CanReuseRetainedEligibility &&
                machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
                program.GetPc(machine) == (long)machine->ip)
            {
                multiple = true; ++retainedEligibilityHits; ++trustedAcquisitions;
                CancelPublication(worker.Worker); return true;
            }
            ++retainedEligibilityMisses;
            // Clear old eligibility BEFORE a slow check can publish Live and a
            // current code stamp. Acquire is also used outside the controller;
            // its later AuthorizeLinks(false) cannot be relied on to clear it.
            // Only AuthorizeLinks may mint trust from the validated key again.
            Volatile.Write(ref ((TranslationBlockHeader*)Store.GetHeader(in lease))->Reserved136, 0UL);
            var plans = Plans(Store, in lease);
            // Keep the checked path's first-code preparation BEFORE sampling
            // validation epochs, as before. Registering a remapped page may
            // advance runtime authority even when its bytes are identical.
            multiple = Authority.CanRunMultiple(program, machine, (ulong)program.GetPc(machine) -
                (machine->ip - header.GuestPc), checked((int)header.GuestLength));
            // Capture BEFORE reading bytes. A writer may run outside Gate;
            // stamping its completion version after an older snapshot would
            // authorize old bytes. The emitted guard rejects a stale stamp.
            var snapshot = Authority.CaptureCodeSnapshot(
                independentContent: (context & FlatLongContext) != 0 && (int)machine->metal == 0);
            CodeSnapshotStep(compiling: false, afterBytes: false);
            var validation = Matches(program, machine, plans);
            if (validation == Validation.NoAccess)
            {
                // Mapping/lazy-commit changes are not evidence of changed
                // instruction bytes. Demand-fetch only the approved current
                // instruction, preserving the retained body on a guest fault.
                program.ResetInstructionCache(machine);
                error = program.LoadInstruction2(machine, (ulong)program.GetPc(machine));
                if (error != 0) { lease.Dispose(); return false; }
                validation = Matches(program, machine, plans);
            }
            if (validation == Validation.Valid)
            {
                // A mapping can replace the physical backing with identical
                // bytes. Reusing emission must also move its invalidation
                // links to the CURRENT checked backing, including nonflat CS.
                if (!RefreshProvenance(program, machine, header, lease.Token))
                { lease.Dispose(); return false; }
                multiple = Authority.CanRunMultiple(program, machine, (ulong)program.GetPc(machine) -
                    (machine->ip - header.GuestPc), checked((int)header.GuestLength));
                CodeSnapshotStep(compiling: false, afterBytes: true);
                ulong codeStamp = SnapshotCodeStamp(in snapshot);
                multiple &= Authority.CanReuseRetainedEligibility && codeStamp != 0;
                if (header.Live == 1 && header.CodeEpoch == header.ValidatedCodeEpoch &&
                    header.ValidatedGlobalEpoch == codeStamp && Authority.CodeEpoch == codeStamp)
                { CancelPublication(worker.Worker); return true; }
                if (Store.TryRevalidate(lease.Token, header.CodeEpoch, header.CodeEpoch, codeStamp))
                {
                    ++authorityRestamps;
                    if (codeStamp == 0) ++zeroStampRestamps;
                    if (header.Live != 1 || header.CodeEpoch != header.ValidatedCodeEpoch) ++dirtyHeaderRestamps;
                    if (header.ValidatedGlobalEpoch != codeStamp) ++globalStampRestamps;
                    CancelPublication(worker.Worker); return true;
                }
                // A host writer may revoke without taking Authority.Gate.
                // Preserve this resident body and the caller's approved issue;
                // retry checked fetch/revalidation after its publication edge.
                lease.Dispose(); return false;
            }
            ++validationMisses;
            if (validation != Validation.BytesChanged)
            { lease.Dispose(); throw new InvalidOperationException("Full-JIT demand fetch did not establish readable code."); }
            Store.RetireContentChanged(lease.Token); lease.Dispose(); index = 0;
        }
        if (Store.GetStatistics().ActiveNativeEpochs != 0)
        {
            // Single-mapping W^X needs quiescence even for a new region. Ask
            // existing chains to leave before doing translation work again.
            RequestPublication(worker.Worker);
            return false;
        }
        bool compiled = Compile(program, machine, context, ref worker, out lease, out multiple, out error);
        if (compiled || error != 0) CancelPublication(worker.Worker);
        return compiled;
    }
    private enum Validation { Valid, NoAccess, BytesChanged }
    // Test-only partial implementation is erased in production.
    partial void CodeSnapshotStep(bool compiling, bool afterBytes);
    // Runtime rendezvous is still mandatory at actual native enrollment. Byte
    // validity instead binds the full code/mapping/content snapshot and pending
    // copy double check. Zero never authorizes execution, even at cap one.
    private ulong SnapshotCodeStamp(in FullCodeSnapshot snapshot)
        => Authority.CodeSnapshotCurrent(in snapshot) ? snapshot.CodeEpoch : 0;
    // All consumers use the same original-first compile-time IR classification.
    // Callers retain authoritative original XED/plan/byte identity guards.
    internal static bool TryClassifyNative(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction) =>
        TryClassifyNativeCore(ip, bytes, out instruction) ||
        TryClassifyCanonicalEncoding(ip, bytes, out instruction);

    private static bool TryClassifyNativeCore(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
    {
        // A rejected near width/prefix must not fall through to a reserved
        // alias in an older classifier. Existing canonical alias projection
        // still re-enters this exact owner with original next-IP semantics.
        // R192 owns only LEA/63/MOVZX/MOVSX; rejected family forms cannot
        // be reinterpreted by older ordinary classifiers. XED guards remain.
        if (FullAddressExtensionLowering.IsEncoding(bytes))
            return FullAddressExtensionLowering.TryClassify(ip, bytes, out instruction, out _);
        if (FullNearControlLowering.IsEncoding(bytes))
            return FullNearControlLowering.TryClassify(ip, bytes, out instruction, out _);
        bool found = TryClassifyCanonicalX87(ip, bytes, out instruction) ||
            FullEnterLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullChecksumLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullFlagStackLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullProcessorQueryLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullSystemStateLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullBitCountLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullScalarMiscLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullStringLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullControlFlagLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullProcessorEnvironmentLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullResidualScalarLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullHintLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullStackMemoryLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullAtomicLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullMemoryIntegerLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullFloatMmxLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullFloatExtendedLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullFloatLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullMaskedStoreLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullMmxLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullVectorExtendedLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullVectorMoveLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullVectorLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullMemoryLowering.TryClassify(ip, bytes, out instruction, out _) ||
            FullIntegerLowering.TryClassify(ip, bytes, out instruction, out _) ||
            GuestBlockJit.TryClassify(ip, bytes, out instruction, out _);
        // Operand width has no effect for a genuine legacy NOP. Keep only its
        // canonical no-operand metadata; never normalize arbitrary malformed IR.
        if (found && instruction.Operation == JitOperation.Nop)
        {
            if (instruction.Width is not (16 or 32 or 64) || instruction.Destination != default ||
                instruction.Source != default || instruction.Extra != default || instruction.Address != default ||
                instruction.Target != 0 || instruction.Condition != 0 || instruction.SourceWidth != 0 || instruction.Options != 0)
                return false;
            instruction = instruction with { Width = 64 };
        }
        return found;
    }
    private bool RefreshProvenance(Blink p, Blink.Machine* m,
        TranslationBlockHeader header, TranslationToken token)
    {
        ulong pc = unchecked((ulong)p.GetPc(m) - (m->ip - header.GuestPc));
        byte* first = p.LookupAddress2(m, unchecked((long)pc), (ulong)Blink.PAGE_XD, 0);
        if (first == null) return false;
        ulong secondPage = ulong.MaxValue;
        int firstLength = 4096 - (int)(pc & 4095);
        if (header.GuestLength > firstLength)
        {
            byte* second = p.LookupAddress2(m, unchecked((long)(pc + (uint)firstLength)), (ulong)Blink.PAGE_XD, 0);
            if (second == null) return false;
            secondPage = (ulong)(nuint)second;
        }
        return Store.SetProvenance(token, pc, (ulong)(nuint)(first - (int)(pc & 4095)), secondPage);
    }
    private Validation Matches(Blink p, Blink.Machine* m, ReadOnlySpan<Plan> plans)
    {
        ++byteValidations;
        var nofault = m->nofault; var segv = m->segvcode;
        m->nofault = true; p.ResetTlb(m);
        try
        {
            ulong pc = (ulong)p.GetPc(m);
            foreach (ref readonly var plan in plans)
            {
                ulong address = unchecked(pc + plan.Ip - m->ip);
                var decoded = plan.Decoded;
                int first = Math.Min(decoded.length, 4096 - (int)(address & 4095));
                byte* source = p.LookupAddress2(m, (long)address, (ulong)Blink.PAGE_XD, 0);
                if (source == null) return Validation.NoAccess;
                Authority.PrepareCodeSnapshot((nint)(source - (int)(address & 4095)));
                if (!new ReadOnlySpan<byte>(source, first).SequenceEqual(new(decoded.bytes, first))) return Validation.BytesChanged;
                int second = decoded.length - first;
                if (second != 0)
                {
                    source = p.LookupAddress2(m, unchecked((long)(address + (uint)first)), (ulong)Blink.PAGE_XD, 0);
                    if (source == null) return Validation.NoAccess;
                    Authority.PrepareCodeSnapshot((nint)source);
                    if (!new ReadOnlySpan<byte>(source, second).SequenceEqual(new(decoded.bytes + first, second))) return Validation.BytesChanged;
                }
            }
            return Validation.Valid;
        }
        finally { m->nofault = nofault; m->segvcode = segv; }
    }
    private bool Compile(Blink p, Blink.Machine* m, ulong context, ref TranslationWorkerCache worker,
        out TranslationLease lease, out bool multiple, out int error)
    {
        lease = default; multiple = false;
        ++compileAttempts;
        using var allocationScope = Allocations.Enter(worker.Worker, FullAllocationMeter.Category.Compilation);
        long allocated = GC.GetAllocatedBytesForCurrentThread(), started = Stopwatch.GetTimestamp();
        int scratchSlot = compilerScratch.Acquire();
        try
        {
        Span<Plan> plans = compilerScratch.Plans(scratchSlot);
        Span<FullBlockInstruction> ir = compilerScratch.Instructions(scratchSlot);
        p.ResetTlb(m); p.ResetInstructionCache(m);
        ulong pc = (ulong)p.GetPc(m), ip = m->ip;
        error = p.LoadInstruction2(m, pc);
        if (error != 0) return false;
        var decoded = *m->xedd;
        int count = 0, length = 0, inlineCount = 0;
        int requested = (int)((context >> 48) & 511);
        int maximum = requested == 0 ? FullCompilerScratch.MaximumInstructions : requested;
        BlockStop stopReason = BlockStop.Count;
        int pageRemaining = 4096 - (int)(pc & 4095);
        byte* snapshot = p.LookupAddress2(m, unchecked((long)pc), (ulong)Blink.PAGE_XD, 0);
        ulong physicalPage0 = snapshot == null ? ulong.MaxValue : (ulong)(nuint)(snapshot - (int)(pc & 4095));
        ulong physicalPage1 = ulong.MaxValue;
        // The first demand fetch establishes lazy mappings only. Register all
        // backing before retaining any instruction bytes, then decode afresh:
        // an older cached native data write must drain before the snapshot.
        if (physicalPage0 != ulong.MaxValue) Authority.PrepareCodeSnapshot((nint)(nuint)physicalPage0);
        ulong preparedSecond = ulong.MaxValue;
        if (pageRemaining < 15)
        {
            // Prepare an already committed possible second page independently
            // of the preliminary instruction length. Never demand-fetch a
            // speculative next page solely to populate this metadata.
            var oldNoFault = m->nofault; var oldCode = m->segvcode;
            m->nofault = true;
            try
            {
                byte* second = p.LookupAddress2(m, unchecked((long)(pc + (uint)pageRemaining)), (ulong)Blink.PAGE_XD, 0);
                if (second != null)
                {
                    preparedSecond = (ulong)(nuint)second;
                    Authority.PrepareCodeSnapshot((nint)second);
                }
            }
            finally { m->nofault = oldNoFault; m->segvcode = oldCode; }
        }
        p.ResetInstructionCache(m);
        var snapshotVersion = Authority.CaptureCodeSnapshot(
            independentContent: (context & FlatLongContext) != 0 && (int)m->metal == 0);
        CodeSnapshotStep(compiling: true, afterBytes: false);
        error = p.LoadInstruction2(m, pc);
        if (error != 0) return false;
        decoded = *m->xedd;
        if (decoded.length > pageRemaining)
        {
            byte* second = p.LookupAddress2(m, unchecked((long)(pc + (uint)pageRemaining)), (ulong)Blink.PAGE_XD, 0);
            if (second == null) throw new InvalidOperationException("Full-JIT checked crossing fetch lost its mapping.");
            if ((ulong)(nuint)second != preparedSecond)
            {
                // The demand decoder may have committed PAGE_RSRV. No prior
                // native data pointer can address that newly committed page.
                // Still mark/drain it and discard that decode before use.
                Authority.PrepareCodeSnapshot((nint)second);
                p.ResetInstructionCache(m);
                error = p.LoadInstruction2(m, pc);
                if (error != 0) return false;
                decoded = *m->xedd;
            }
            if (decoded.length > pageRemaining) physicalPage1 = (ulong)(nuint)second;
        }
        bool flatSequential = m->mode.omode == Blink.XED_MODE_LONG && m->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
            p.GetPc(m) == (long)m->ip && m->stashaddr == 0 && (int)m->metal == 0;
        bool sequential = flatSequential && FullCanonicalX87ExecutionProfile.AllowsFlags(m, canonicalX87Enabled);
        bool inline = inlineEnabled && sequential;
        bool repSite = false;
        FullRepInstruction repInstruction = default;
        FullRepKernelBinding repBinding = default;
        while (true)
        {
            if (decoded.length is < 1 or > 15) throw new InvalidOperationException("Invalid predecoded Full instruction length.");
            if (inline && FullCanonicalEncoding.TryClassifyRep(ip + (uint)length,
                new ReadOnlySpan<byte>(decoded.bytes, decoded.length), out var rep, out _))
            {
                // The issued REP owns a continuation independent of TB
                // retirement. End a preceding ordinary prefix BEFORE it;
                // its own guarded site latches once, then hands off natively.
                stopReason = BlockStop.State;
                if (count != 0) break;
                if (!RepKernels.TryBind(in rep, compilerScratch.Code(scratchSlot), out repBinding, out var repFailure))
                {
                    if (repFailure is TranslationStoreFailure.NativeEpochActive or TranslationStoreFailure.LeasedCapacity or
                        TranslationStoreFailure.NotPublished)
                    {
                        if (repFailure == TranslationStoreFailure.NativeEpochActive) RequestPublication(worker.Worker);
                        return false;
                    }
                    throw new InvalidOperationException("Full-JIT REP code quota exhausted: " + repFailure);
                }
                repSite = true; repInstruction = rep;
                plans[0] = new(ip, decoded, null);
                // This IR is scaffold metadata only. EmitRepSite emits the
                // actual latch (no NOP computation or ordinary completion).
                ir[0] = new(new(ip, decoded.length, JitOperation.Nop, 64, default, default));
                count = inlineCount = 1; length = decoded.length;
                break;
            }
            var helper = p.BlinkFullJitSelect(decoded.op.rde, decoded.op.disp, decoded.op.uimm0);
            if (helper == null) throw new InvalidOperationException("Missing operation-specific Full helper.");
            JitInstruction native = default;
            bool nativeSite = inline && TryClassifyNative(ip + (uint)length,
                new ReadOnlySpan<byte>(decoded.bytes, decoded.length), out native) && FullBlockEmitter.CanInline(native) &&
                (native.Operation != JitOperation.X87 || canonicalX87Enabled);
            // Reserved/LOCK-UD has no data access and precedes #AC eligibility.
            // Never use this exception for a missing ordinary computation form.
            if (!nativeSite && canonicalX87Enabled && inlineEnabled && flatSequential &&
                FullCanonicalX87InvalidLowering.TryClassify(ip + (uint)length,
                    new ReadOnlySpan<byte>(decoded.bytes, decoded.length), out var invalidX87))
            { native = invalidX87; nativeSite = true; }
            // Once raw80 is authoritative, an ordinary missing lowering/context
            // must never silently select managed OpFpu/FX semantics. A reserved
            // encoding has its own native UD IR, distinct from a missing form.
            if (canonicalX87Enabled && FullCanonicalX87InvalidLowering.IsStateOperation(decoded.op.rde) && !nativeSite)
            {
                if (count != 0) { stopReason = BlockStop.State; break; }
                throw new NotSupportedException("Active canonical x87 has no admitted native form/context; managed computation forbidden.");
            }
            if (nativeSite) ++inlineCount;
            plans[count] = new(ip + (uint)length, decoded, nativeSite ? null : helper);
            ir[count] = nativeSite ? new(native, X87Kernel: BindCanonicalX87Instruction(native)) :
                FullBlockInstruction.Helper(ip + (uint)length, decoded.length);
            ++count; length += decoded.length;
            if (Boundary(decoded.op.rde, out stopReason)) break;
            if (!sequential) { stopReason = BlockStop.State; break; }
            if (length >= pageRemaining) { stopReason = BlockStop.Page; break; }
            if (count == maximum) { stopReason = requested == 0 ? BlockStop.Count : BlockStop.Requested; break; }
            if (snapshot == null) { stopReason = BlockStop.Decode; break; }
            // Decode only already-committed bytes of this SAME checked page.
            // Speculation cannot commit/fault a later guest page or deliver #UD.
            Blink.XedDecodedInst next = default;
            int available = Math.Min(15, pageRemaining - length);
            if (p.DecodeInstruction(&next, snapshot + length, (ulong)available, m->mode.omode) != 0 ||
                next.length is < 1 or > 15 || next.length > available)
            { stopReason = BlockStop.Decode; break; }
            decoded = next;
        }
        FullTranslationSnapshotTaken(pc);
        // The 64KiB backend envelope is a charged scratch/region policy, not
        // QEMU's unwind limit. Retry a smaller prefix without publishing bytes.
        Span<byte> code = compilerScratch.Code(scratchSlot);
        int codeBytes;
        if (repSite)
        {
            var repDecoded = plans[0].Decoded;
            codeBytes = FullBlockEmitter.EmitRepSite(repInstruction,
                new ReadOnlySpan<byte>(repDecoded.bytes, repDecoded.length),
                Store.OwnerCookie, repBinding.Cookie, repBinding.Resume, code, in sharedScaffold);
        }
        // Full/ENV controllers bind a fresh stack Completed delta for every actual
        // invocation; internal/tail entries retain that pointer through native RET.
        else while (!FullBlockEmitter.TryEmitOwnerPrivateCompletion(ir[..count], code, in sharedScaffold, out codeBytes))
        {
            if (count == 1) throw new InvalidOperationException("Full-JIT one-instruction emission exceeds the backend envelope.");
            count /= 2; ++oversizedRetries; stopReason = BlockStop.CodeLimit;
            length = checked((int)(plans[count - 1].Ip - ip)) + plans[count - 1].Decoded.length;
            inlineCount = 0;
            for (int i = 0; i < count; ++i) if (plans[i].Helper == null) ++inlineCount;
        }
        int payloadBytes = checked(count * sizeof(Plan));
        var key = new TranslationKey(ip, context);
        if (!Store.TryReserve(key, codeBytes, payloadBytes, (uint)length, out var draft, out var failure))
        {
            // Arena reset/publication cannot overlap native execution or
            // retained managed readers. Leave this scope before retrying the
            // SAME approved issue; no helper or interpreter has run.
            if (failure is TranslationStoreFailure.NativeEpochActive or TranslationStoreFailure.LeasedCapacity or
                TranslationStoreFailure.NotPublished)
            {
                // Quiesce unbounded native chains for a packed RW->RX writer.
                // No lock is held by native execution; its epoch poll exits.
                if (failure == TranslationStoreFailure.NativeEpochActive) RequestPublication(worker.Worker);
                return false;
            }
            throw new InvalidOperationException("Full-JIT retention exhausted: " + failure);
        }
        TranslationToken token = draft.Token;
        try
        {
            if (physicalPage0 != ulong.MaxValue &&
                !Store.SetProvenance(in draft, pc, physicalPage0, physicalPage1))
                throw new InvalidOperationException("Full-JIT checked fetch provenance was rejected.");
            // Helper presence is represented by immutable per-site typed
            // pointers (null for native sites), never a 32-bit instruction mask.
            code[..codeBytes].CopyTo(draft.Code);
            MemoryMarshal.AsBytes(plans[..count]).CopyTo(draft.Payload);
            StampStaticExit(draft.Header, plans[..count], ir[..count], terminalReturns: repSite);
            using var publicationScope = Allocations.Enter(worker.Worker, FullAllocationMeter.Category.Publication);
            CodeSnapshotStep(compiling: true, afterBytes: true);
            if (!Store.PublishCodeSnapshot(ref draft, codeBytes, payloadBytes,
                Authority, in snapshotVersion, out failure))
            {
                if (failure == TranslationStoreFailure.AuthorityChanged) return false;
                throw new InvalidOperationException("Full-JIT publication failed: " + failure);
            }
        }
        finally { Store.Abort(ref draft); }
        ++compiles; blockInstructions += count; emitted += codeBytes;
        ++blockLengths[count]; ++blockStops[(int)stopReason];
        compiledInlineSites += inlineCount; compiledHelperSites += count - inlineCount;
        FullTranslationPublished(key, token, count, inlineCount, codeBytes, payloadBytes);
        peakResident = Math.Max(peakResident, Store.GetStatistics().ResidentBodies);
        long delta = GC.GetAllocatedBytesForCurrentThread() - allocated;
        compileAllocated += delta; Store.RecordCompilationAllocation(delta);
        compileMs += Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        multiple = Authority.CanRunMultiple(p, m, pc, length);
        multiple &= Authority.CodeSnapshotCurrent(in snapshotVersion);
        if (!Store.TryAcquireKey(ref worker, key, out lease, out failure))
            throw new InvalidOperationException("Full-JIT published body acquisition failed: " + failure);
        return true;
        }
        finally { compilerScratch.Release(scratchSlot); }
    }
    // Links never confer authority. A target must already have been byte-
    // validated in this global mapping epoch and qualify for bounded multiple
    // instruction execution. The emitter repeats all guards at every boundary.
    internal void AuthorizeLinks(in TranslationLease lease, bool multiple, uint index = 0, uint nativeLimit = 0)
        => AuthorizeLinksCore(in lease, (TranslationBlockHeader*)Store.GetHeader(in lease), multiple, index, nativeLimit);

    // The controller already acquired this exact view and still holds its
    // TranslationLease. Do not reacquire Store.gate just to recover its header.
    internal void AuthorizeEntryLinks(in TranslationLease lease, in TranslationLeaseView view,
        bool multiple, uint index = 0, uint nativeLimit = 0)
        => AuthorizeLinksCore(in lease, view.Header, multiple, index, nativeLimit);

    private void AuthorizeLinksCore(in TranslationLease lease, TranslationBlockHeader* header,
        bool multiple, uint index, uint nativeLimit)
    {
        // Mint provenance from the VALIDATED key context, not merely the
        // current machine's PC relationship. Flat/nonflat long contexts have
        // distinct keys; nonflat/legacy plans never acquire trusted links.
        bool trusted = multiple && (header->Context & FlatLongContext) != 0;
        header->Reserved136 = trusted ? 1UL : 0UL;
        if (!trusted) { Store.Unlink(lease.Token, 0); Store.Unlink(lease.Token, 1); return; }
        if (!TryReadStaticExit(in header[0], out var exit))
        { Store.Unlink(lease.Token, 0); Store.Unlink(lease.Token, 1); return; }
        // No plan scan or opcode classification on a resident execution path.
        // The final published prefix recorded its last helper at translation.
        if (index >= exit.Count || (nativeLimit != 0 && nativeLimit <= (uint)exit.Count - index) ||
            exit.LastHelperPlusOne > index)
        { ++deferredLinkPreparations; return; }
        bool edge0 = exit.Edge0, edge1 = exit.Edge1;
        ulong fallthrough = exit.Fallthrough, taken = exit.Taken;
        if (edge0) LinkTarget(in lease, header, 0, fallthrough, header->Context);
        else Store.Unlink(lease.Token, 0);
        if (edge1) LinkTarget(in lease, header, 1, taken, header->Context);
        else Store.Unlink(lease.Token, 1);
    }
    private void LinkTarget(in TranslationLease source, TranslationBlockHeader* sourceHeader, int edge, ulong pc, ulong context)
    {
        nint existing = edge == 0 ? Volatile.Read(ref sourceHeader->Link0TargetHeader) :
            Volatile.Read(ref sourceHeader->Link1TargetHeader);
        // Reverse adjacency clears this cell before target retirement. Do not
        // dereference a target without a native epoch: native traversal still
        // repeats complete owner/context/PC/generation/code/global guards.
        if (existing != 0) { ++linkReuseHits; return; }
        ++linkTargetLookups;
        if (Store.TryAcquireKey(new(pc, context), out var target, requireLive: true))
        {
            using (target)
            {
                var h = Store.GetHeaderCopy(in target);
                if (h.Reserved136 == 1 && h.ValidatedGlobalEpoch == Authority.CodeEpoch &&
                    h.CodeEpoch == h.ValidatedCodeEpoch && Store.TryLink(source.Token, edge, target.Token)) return;
            }
        }
        Store.Unlink(source.Token, edge);
    }
    private static bool Boundary(ulong rde, out BlockStop reason)
    {
        int opcode = (int)((rde >> 40) & 2047), group = (int)(rde & 7);
        reason = BlockStop.ControlFlow;
        if (opcode is >= 0x70 and <= 0x7f or >= 0x180 and <= 0x18f or >= 0xe0 and <= 0xe3 or
            0xe8 or 0xe9 or 0xeb or 0x9a or 0xea or 0xc2 or 0xc3 or 0xca or 0xcb or 0xcf or
            0xcc or 0xcd or 0xce or 0xf1 or 0x105 or 0x107 or 0x134 or 0x135 ||
            (opcode == 0xff && group is >= 2 and <= 5)) return true;
        // Mode/flags/interrupt inhibition/segment/translation authority changes
        // end the TB even when their typed helper could return normally.
        reason = BlockStop.State;
        return opcode is 0x07 or 0x17 or 0x1f or 0x8e or 0x9d or 0xc4 or 0xc5 or 0xf4 or 0xfa or 0xfb or
            0x100 or 0x101 or 0x106 or 0x108 or 0x109 or 0x120 or 0x121 or 0x122 or 0x123 or
            0x130 or 0x1a1 or 0x1a9 or 0x1b2 or 0x1b4 or 0x1b5 or 0x1ae;
    }
    internal void NativeCompleted(in FullBlockFrame frame)
    {
        Interlocked.Increment(ref nativeCalls);
        Interlocked.Add(ref nativeInstructions, checked((long)frame.Executed));
        Interlocked.Add(ref tailJumps, checked((long)frame.TailJumps));
        Interlocked.Add(ref nativeMemoryComputations, checked((long)frame.NativeMemoryComputations));
        if ((uint)frame.Exit < nativeExits.Length) Interlocked.Increment(ref nativeExits[(int)frame.Exit]);
        GC.KeepAlive(sharedCode); // Root common code through every complete native return.
    }
    internal void BeginHelper(int operation) { Interlocked.Increment(ref helperCount); Interlocked.Increment(ref helperCalls[operation]); }
    internal void MemoryRequested(in FullBlockFrame retainedExitFrame)
    {
        if (!Monitor.IsEntered(Authority.Gate)) throw new InvalidOperationException("Native miss observation requires authority gate.");
        var reason = FullMemoryMissDiagnostics.Observe(in retainedExitFrame, Authority.Epoch);
        Interlocked.Increment(ref memoryMissObservations[(int)reason]);
    }
    internal void MemoryAssisted(FullMemoryAssistResult result, in FullBlockFrame frame)
    {
        // Called under Gate after the mapping-only assist. Fixed counters do
        // not allocate or log per miss; formatting belongs to Snapshot.
        if (!Monitor.IsEntered(Authority.Gate)) throw new InvalidOperationException("Native assist statistics require authority gate.");
        Interlocked.Increment(ref memoryAssists);
        Interlocked.Increment(ref memoryAssistOutcomes[(int)result]);
        if (result == FullMemoryAssistResult.Retry)
            Interlocked.Increment(ref memoryRetryDenials[(int)Authority.LastDataPermissionDenial]);
        memoryLastAssistPc = frame.Machine == null ? 0 : ((Blink.Machine*)frame.Machine)->ip;
        memoryLastAssistAddress = frame.MemoryAddress;
    }
    internal void CompleteHelper(int operation) { Interlocked.Increment(ref helperInstructions); Interlocked.Increment(ref helperCompleted[operation]); }
    internal GuestJitStatistics Snapshot(GuestJitStatistics value)
    {
        lock (Authority.Gate)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            var s = Store.GetStatistics();
            peakCharge = Math.Max(peakCharge, s.PeakChargedBytes + overhead);
            var helpers = new Dictionary<string, long>(StringComparer.Ordinal);
            for (int i = 0; i < helperCalls.Length; ++i)
                if (Volatile.Read(ref helperCalls[i]) != 0)
                { helpers[$"{i:x3}/calls"] = Volatile.Read(ref helperCalls[i]); helpers[$"{i:x3}/completed"] = Volatile.Read(ref helperCompleted[i]); }
            var lengths = blockLengths.ToArray();
            var stops = Enum.GetValues<BlockStop>().ToDictionary(x => x.ToString(), x => blockStops[(int)x]);
            var exits = nativeExits.ToArray();
            var assists = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var outcome in Enum.GetValues<FullMemoryAssistResult>())
                assists[outcome.ToString()] = Volatile.Read(ref memoryAssistOutcomes[(int)outcome]);
            foreach (var denial in Enum.GetValues<FullDataPermissionDenial>())
                assists["Retry/" + denial] = Volatile.Read(ref memoryRetryDenials[(int)denial]);
            foreach (var reason in Enum.GetValues<FullMemoryMissReason>())
                assists["LookupObservation/" + reason] = Volatile.Read(ref memoryMissObservations[(int)reason]);
            var authorityCounts = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["RuntimeEpochAdvances"] = Authority.EpochAdvances,
                ["CodeEpochAdvances"] = Authority.CodeEpochAdvances,
                ["TrustedAcquisitions"] = trustedAcquisitions,
                ["RetainedEligibilityHits"] = retainedEligibilityHits,
                ["RetainedEligibilityMisses"] = retainedEligibilityMisses,
                ["ResumeRangeRejects"] = resumeRangeRejects,
                ["ByteValidations"] = byteValidations,
                ["Restamps"] = authorityRestamps,
                ["RetainedValidationDefers"] = retainedValidationDefers,
                ["ZeroStampRestamps"] = zeroStampRestamps,
                ["DirtyHeaderRestamps"] = dirtyHeaderRestamps,
                ["GlobalStampRestamps"] = globalStampRestamps,
                ["TicketReadSourcePreSkips"] = ticketReadPreSkips,
                ["TicketReadSourcePostSkips"] = ticketReadPostSkips,
                ["TicketWriteSourcePreRevocations"] = ticketWritePreRevocations,
                ["TicketWriteSourcePostRevocations"] = ticketWritePostRevocations,
                ["TicketWriteSourceRevokedBodies"] = ticketWriteRevokedBodies,
                ["KnownCodeWriteScopes"] = Authority.KnownCodeWriteScopes,
                ["UnknownCodeWriteScopes"] = Authority.UnknownCodeWriteScopes,
                ["PreciseMappingScopes"] = Authority.PreciseMappingScopes,
                ["GlobalMappingScopes"] = Authority.GlobalMappingScopes,
                ["MappingFallbackEscalations"] = Authority.MappingEscalations,
                ["PhysicalRetirementScopes"] = Authority.PhysicalRetirementScopes,
                ["MappingTopologyAdvances"] = Authority.MappingTopologyAdvances,
                ["CodeRoleRegistrations"] = Authority.CodeRoleRegistrations,
                ["CodeRoleCacheFilters"] = Authority.CodeRoleCacheFilters,
                ["CodeRoleCacheKept"] = Authority.CodeRoleCacheKept,
                ["CodeRoleCacheProvenanceDrops"] = Authority.CodeRoleCacheProvenanceDrops,
                ["CodeRoleCachePhysicalDrops"] = Authority.CodeRoleCachePhysicalDrops,
                ["CodeRoleCacheWriteDrops"] = Authority.CodeRoleCacheWriteDrops,
                ["MappingCacheTopologyDrops"] = Authority.MappingCacheTopologyDrops,
                ["MappingCacheFallbackDrops"] = Authority.MappingCacheFallbackDrops,
                ["MappingCacheRaceDrops"] = Authority.MappingCacheRaceDrops
            };
            diagnosticAllocated += GC.GetAllocatedBytesForCurrentThread() - before;
            return Memory.Snapshot(value with { Mode = "Full", Capability = "Available", InterpreterInstructions = 0,
                InlineNativeInstructions = nativeInstructions, GuestInstructions = nativeInstructions,
                SemanticHelperInstructions = helperInstructions, SemanticHelperCalls = helperCount, SemanticHelpers = helpers,
                PhysicalAuthorityPages = Authority.PhysicalPages, ActivePhysicalCodeBorrows = Authority.ActiveBorrows,
                PeakPhysicalCodeBorrows = Authority.PeakBorrows, AuthorityRegistrySaturated = Authority.Saturated,
                HostBackingMutationScopes = Authority.HostMutations, NativeAuthorityEnrollmentMisses = Authority.EnrollmentMisses,
                AuthorityEpochAdvances = Authority.EpochAdvances, VirtualWriteNotifications = virtualWriteNotifications,
                ConservativeVirtualWriteBarriers = virtualWriteFallbacks,
                CompiledBlocks = compiles, ActualBlockCompilations = compiles, UniqueBlockKeys = s.ResidentBodies,
                RepKernelCount = RepKernels.Count, RepKernelEmittedBytes = RepKernels.EmittedBytes,
                RepKernelReservedBytes = RepKernels.ReservedBytes,
                NativeRepIterations = Volatile.Read(ref repIterations), NativeRepCompletions = Volatile.Read(ref repCompletions),
                RepIterationCounterSaturated = Volatile.Read(ref repIterationCounterSaturated) != 0,
                RetainedBlockExecutions = nativeCalls, NativeEntries = nativeCalls, NativeDispatches = nativeCalls,
                NativeMemoryInstructions = nativeMemoryComputations, NativeMemoryAssists = memoryAssists,
                NativeMemoryAssistOutcomes = assists, NativeMemoryLastAssistPc = memoryLastAssistPc,
                FullAuthorityCounts = authorityCounts,
                NativeMemoryLastAssistAddress = memoryLastAssistAddress,
                NativeTailJumps = tailJumps, CompileAttempts = compileAttempts, TranslationMisses = lookupMisses,
                LookupTierMisses = s.WorkerHintMisses, CacheHits = s.HintHits,
                WorkerKeyLookups = s.WorkerLookups, SharedIndexLookups = s.DirectKeyLookups, ResumeTokenLookups = s.TokenLookups, CodeValidationMisses = validationMisses,
                LinkTargetLookups = linkTargetLookups, LinkReuseHits = linkReuseHits,
                LastExitLinkAttempts = lastExitLinkAttempts, LastExitLinksCompleted = lastExitLinksCompleted,
                DeferredLinkPreparations = deferredLinkPreparations,
                CompiledInlineSites = compiledInlineSites, CompiledHelperSites = compiledHelperSites,
                EmissionSizeRetries = oversizedRetries, CompiledInstructionSites = blockInstructions,
                PublicationQuiescenceRequests = publicationRequests, NativePublicationAdmissionDenials = publicationAdmissionDenials,
                BlockLengthCounts = lengths,
                BlockStopCounts = stops,
                NativeExitCounts = exits,
                ContentInvalidations = s.ContentRetirements, AuthorityRevalidations = s.Revalidations,
                BudgetEvictions = s.CapacityRetirements, Evictions = s.CapacityRetirements,
                ResidentTranslationBlocks = s.ResidentBodies, PeakResidentTranslationBlocks = peakResident,
                RetainedNativeBytes = disposed ? 0 : s.NativeReservedBytes + sharedReservedBytes,
                SharedScaffoldEmittedBytes = sharedEmittedBytes,
                SharedScaffoldReservedBytes = disposed ? 0 : sharedReservedBytes,
                PeakSharedScaffoldReservedBytes = sharedReservedBytes,
                CacheBytes = disposed ? 0 : s.ChargedBytes + overhead, PeakCacheBytes = peakCharge,
                UsefulPublishedCodeBytes = s.UsefulPublishedCodeBytes,
                PeakUsefulPublishedCodeBytes = s.PeakUsefulPublishedCodeBytes,
                AlignedOccupiedCodeBytes = s.AlignedOccupiedCodeBytes,
                PeakAlignedOccupiedCodeBytes = s.PeakAlignedOccupiedCodeBytes,
                PayloadUsedBytes = s.PayloadUsedBytes,
                PeakPayloadUsedBytes = s.PeakPayloadUsedBytes,
                DescriptorUsedBytes = s.DescriptorUsedBytes,
                PeakDescriptorUsedBytes = s.PeakDescriptorUsedBytes,
                IndexUsedBytes = s.IndexUsedBytes,
                PeakIndexUsedBytes = s.PeakIndexUsedBytes,
                FixedChargedBytes = disposed ? 0 : s.FixedChargedBytes + overhead,
                PeakFixedChargedBytes = s.PeakFixedChargedBytes + overhead,
                ArenaReservedBytes = s.ArenaReservedBytes,
                PeakArenaReservedBytes = s.PeakArenaReservedBytes,
                ArenaUsedBytes = s.ArenaUsedBytes,
                DataArenaUsedBytes = s.DataArenaUsedBytes,
                DataArenaReservedBytes = s.DataArenaReservedBytes,
                DataArenaSlackBytes = s.DataArenaSlackBytes,
                DataArenaOverheadBytes = s.DataArenaOverheadBytes,
                CodeReservedBytes = s.CodeReservedBytes,
                RegionSlackBytes = s.RegionSlackBytes,
                PageSlackBytes = s.PageSlackBytes,
                ProtectionCalls = s.ProtectionCalls,
                TotalFlushes = s.TotalFlushes,
                FlushRetirements = s.FlushRetirements,
                FlushRequests = s.FlushRequests,
                FlushDenials = s.FlushDenials,
                CompileAllocatedBytes = Allocations.Total(FullAllocationMeter.Category.Compilation),
                HelperManagedAllocatedBytes = Allocations.Total(FullAllocationMeter.Category.Helper), EmittedCodeBytes = emitted, CompileMilliseconds = compileMs,
                SetupAllocatedBytes = setupAllocated, PublicationAllocatedBytes = Allocations.Total(FullAllocationMeter.Category.Publication),
                DiagnosticsAllocatedBytes = diagnosticAllocated });
        }
    }
    public void Dispose()
    {
        if (x87Kernels != null) { DisposeWithX87Kernels(); return; }
        lock (Authority.Gate)
        {
            if (disposed) return;
            // Store disposal rejects outstanding native/managed/draft
            // capabilities and releases every body first. Common code can
            // only be released after that quiescent boundary succeeds.
            RepKernels.AssertQuiescent();
            Store.Dispose(); RepKernels.Dispose(); sharedCode?.Dispose(); compilerScratch.Dispose(); Memory.Dispose(); Authority.Dispose(); disposed = true;
        }
    }
}
