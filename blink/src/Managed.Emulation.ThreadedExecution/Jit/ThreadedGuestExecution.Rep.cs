using System.Runtime.CompilerServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    partial void FullControllerRepStateInitialized();
    // REP-only stack/value delta; ordinary baseline controller frames are not
    // charged again. Count both initial/inner states, issued/pending value
    // copies, kernel frame/fault/copied outcome, mapper descriptors/operands and
    // Plan/IR temporaries. The extra KiB covers scalar/pointer locals, alignment
    // and bounded leaf temporaries. No active REP capability survives Actor.
    internal static long RepScopeCharge { get; } = checked((
        2L * sizeof(FullRepState) + 2L * Unsafe.SizeOf<FullIssuedRep>() +
        3L * sizeof(FullBlockFrame) + 2L * Unsafe.SizeOf<FullRepRequest>() +
        3L * sizeof(FullStackMemoryDescriptor) + 4L * sizeof(FullStackMemoryRequest) +
        2L * Unsafe.SizeOf<FullTranslationJit.Plan>() + 4L * sizeof(FullRepInstruction) +
        2L * sizeof(long) + 1024L + 63L) & ~63L); // completion + captured ticket decision

    private sealed partial class Worker
    {
        // The registration serial is the worker namespace. Neither this counter
        // nor that serial is recycled by a TB flush or a synchronous Actor call.
        internal ulong FullRepAttempt;
    }

    // Managed VALUE capability, never a header/payload pointer or a main-store
    // lease. The exact decoded bytes live inside the copied Plan/Identity values.
    private struct FullIssuedRep
    {
        internal FullTranslationJit.Plan Plan;
        internal FullRepInstruction Instruction;
        internal FullRepState Identity;
        internal TranslationToken Origin;
        internal FullRepKernelLease Kernel;
    }

    // Only native-produced EAs/shape survive RET. Ready pointers, lengths,
    // permits and tickets are deliberately absent from the retained request.
    private struct FullRepRequest
    {
        internal ulong Attempt, Generation, Address, ObservedEpoch;
        internal uint Site, Width;
        internal FullMemoryAccess Access;
        internal FullStackMemoryDescriptor Shape;
        internal bool Present;
    }

    private static bool NativeRep(in FullTranslationJit.Plan plan, out FullRepInstruction instruction)
    {
        var decoded = plan.Decoded;
        instruction = default;
        return plan.Helper == null && FullCanonicalEncoding.TryClassifyRep(plan.Ip,
            new(decoded.bytes, decoded.length), out instruction, out _);
    }

    private void PrepareRepLatch(Worker worker, FullRepState* state)
    {
        if (worker.FullCache.Serial == 0 || worker.FullRepAttempt == ulong.MaxValue)
            throw new InvalidOperationException("Full-JIT REP attempt namespace exhausted or unregistered.");
        // This invocation's local starts zeroed. Ordinary native/mapping
        // returns may reuse it while Idle; do not clear 256 bytes on every
        // frame. A helper can recursively consume another attempt, so refresh
        // the sequence seed on EVERY preparation even when already initialized.
        if (state->Version == 0)
        {
            state->Version = FullRepState.CurrentVersion;
            state->Size = FullRepState.ByteSize;
            state->OwnerCookie = fullJit!.Store.OwnerCookie;
            state->WorkerCookie = worker.FullCache.Serial;
            FullControllerRepStateInitialized();
        }
        if (state->Version != FullRepState.CurrentVersion || state->Size != FullRepState.ByteSize ||
            state->Phase != FullRepPhase.Idle || state->WorkerCookie != worker.FullCache.Serial ||
            state->OwnerCookie != fullJit!.Store.OwnerCookie)
            throw new InvalidOperationException("Full-JIT REP preparation cannot replace an active or foreign state.");
        state->AttemptGeneration = worker.FullRepAttempt;
    }

    private static void CommitRepAttempt(Worker worker, FullRepState* state, in FullBlockFrame frame)
    {
        if (state->Phase == FullRepPhase.Idle) return;
        if (frame.Exit != FullBlockExit.RepYield || state->Phase != FullRepPhase.Active ||
            state->Reason != FullRepReason.Latched || worker.FullRepAttempt == ulong.MaxValue ||
            state->AttemptGeneration != worker.FullRepAttempt + 1 ||
            state->WorkerCookie != worker.FullCache.Serial || state->AdmissionEpoch != 0)
            throw new InvalidOperationException("Full-JIT REP latch generation authentication failed.");
        // Burn the attempt BEFORE any test callback, stop or exceptional handoff.
        worker.FullRepAttempt = state->AttemptGeneration;
    }

    private void CaptureIssuedRep(Worker worker, in NativeEpochLease epoch, in FullBlockFrame frame,
        FullRepState* state, ref FullIssuedRep issuedRep, bool signalActor)
    {
        // A concurrent writer may already have retired this body. Its old native
        // epoch still protects the returned header and payload. Do not require
        // Live/residency or Acquire a replacement opcode to capture an issued REP.
        if (epoch.Owner != fullJit!.Store || epoch.Serial == 0 || frame.Rep != state ||
            frame.Header == null || frame.Machine != worker.Machine ||
            frame.Version != FullBlockFrame.AbiVersion || frame.Size != FullBlockFrame.ByteSize ||
            frame.FirstPreissued != 0 || frame.HelperIndex != FullBlockFrame.NoHelperIndex ||
            state->Phase != FullRepPhase.Active || state->Reason != FullRepReason.Latched ||
            state->AttemptGeneration != worker.FullRepAttempt || state->CompletedIterations != 0 ||
            state->RequestGeneration != 1 || state->AdmissionEpoch != 0 || issuedRep.Kernel.Owner != null)
            throw new InvalidOperationException("Full-JIT initial REP handoff authentication failed.");
        var header = *frame.Header;
        if (header.OwnerCookie != fullJit.Store.OwnerCookie || header.OwnerCookie != frame.OwnerCookie ||
            header.BodyGeneration != frame.BodyGeneration || header.BodyGeneration != state->OriginBodyGeneration ||
            header.Context != frame.Context || state->Context != frame.Context ||
            unchecked((ulong)frame.OwnerHandle) != worker.FullCache.Serial || header.Payload == 0 || header.PayloadBytes == 0 ||
            header.PayloadBytes % sizeof(FullTranslationJit.Plan) != 0 ||
            frame.ResumeIndex >= header.PayloadBytes / sizeof(FullTranslationJit.Plan) ||
            frame.ResumeIndex != state->OriginSite)
            throw new InvalidOperationException("Full-JIT issued REP origin authentication failed.");
        var plan = ((FullTranslationJit.Plan*)header.Payload)[frame.ResumeIndex];
        if (!NativeRep(in plan, out var instruction))
            throw new InvalidOperationException("Full-JIT REP handoff has no immutable native Plan.");
        ulong sourceBase = instruction.SourceSegment == 4 ? worker.Machine->fs.@base :
            instruction.SourceSegment == 5 ? worker.Machine->gs.@base : 0;
        if (state->OriginalFlags != state->LastFlags || state->SourceBase != sourceBase ||
            frame.Temporary3 != state->AttemptGeneration || frame.NextIp != state->OriginPc)
            throw new InvalidOperationException("Full-JIT REP initial snapshot differs from its issued operands.");
        lock (fullJit.Authority.Gate)
        {
            var binding = fullJit.RepKernels.Find(in instruction);
            if (state->KernelCookie != binding.Cookie)
                throw new InvalidOperationException("Full-JIT REP handoff kernel identity differs.");
            issuedRep = new() { Plan = plan, Instruction = instruction, Identity = *state,
                Origin = new(checked((int)header.Slot), header.BodyGeneration) };
            ValidateIssuedRep(worker, in issuedRep, state, signalActor, requireLease: false);
            issuedRep.Kernel = fullJit.RepKernels.Acquire(in binding);
        }
    }

    private void ValidateIssuedRep(Worker worker, in FullIssuedRep issuedRep, FullRepState* state,
        bool signalActor, bool requireLease = true)
    {
        ref readonly var id = ref issuedRep.Identity;
        var instruction = issuedRep.Instruction;
        var decoded = issuedRep.Plan.Decoded;
        bool valid = state->Version == FullRepState.CurrentVersion && state->Size == FullRepState.ByteSize &&
            state->OwnerCookie == fullJit!.Store.OwnerCookie && state->OwnerCookie == id.OwnerCookie &&
            state->WorkerCookie != 0 && state->WorkerCookie == worker.FullCache.Serial &&
            state->WorkerCookie == id.WorkerCookie && state->AttemptGeneration != 0 &&
            state->AttemptGeneration == id.AttemptGeneration && state->AttemptGeneration == worker.FullRepAttempt &&
            state->OriginBodyGeneration == issuedRep.Origin.Generation && state->OriginBodyGeneration == id.OriginBodyGeneration &&
            state->OriginSite == id.OriginSite && state->OriginPc == instruction.Ip && state->OriginPc == issuedRep.Plan.Ip &&
            state->FallthroughPc == unchecked(instruction.Ip + (uint)instruction.Length) &&
            state->Context == id.Context && state->Context == FullTranslationJit.Context(program!, worker.Machine, signalActor ? 1u : 0u) &&
            state->KernelCookie == id.KernelCookie && state->OriginalFlags == id.OriginalFlags &&
            state->Operation == instruction.Operation && state->Width == (uint)instruction.Width &&
            state->AddressWidth == (uint)instruction.AddressWidth && state->RepeatMode == instruction.Repeat &&
            state->SourceSegment == instruction.SourceSegment && state->SourceBase == id.SourceBase &&
            state->InstructionLength == (uint)instruction.Length && decoded.length == instruction.Length &&
            state->RemainingCount == Register(worker.Machine, 1) && state->SourceIndex == Register(worker.Machine, 6) &&
            state->DestinationIndex == Register(worker.Machine, 7) && state->Accumulator == Register(worker.Machine, 0) &&
            state->LastFlags == worker.Machine->flags;
        if (state->Phase is FullRepPhase.Active or FullRepPhase.FaultPending or FullRepPhase.FaultFinal)
            valid &= worker.Machine->ip == state->OriginPc && worker.Machine->oplen == 0 &&
                state->CompletedIterations != ulong.MaxValue && state->RequestGeneration == state->CompletedIterations + 1;
        else if (state->Phase == FullRepPhase.Done)
            valid &= worker.Machine->ip == state->FallthroughPc && worker.Machine->oplen == 0 &&
                state->RequestGeneration == (state->CompletedIterations == 0 ? 1 : state->CompletedIterations);
        else valid = false;
        for (int i = 0; i < 16; ++i)
            valid &= state->DecodedBytes[i] == (i < decoded.length ? decoded.bytes[i] : 0);
        for (int i = 0; i < 52; ++i) valid &= state->Reserved[i] == 0;
        if (requireLease)
            valid &= fullJit!.RepKernels.IsValid(in issuedRep.Kernel) &&
                issuedRep.Kernel.Binding.Cookie == state->KernelCookie &&
                issuedRep.Kernel.Binding == fullJit.RepKernels.Find(in instruction);
        if (!valid) throw new InvalidOperationException("Full-JIT issued REP identity/progress authentication failed.");
    }

    private static void AbandonIssuedRep(ref FullIssuedRep issuedRep, FullRepState* state)
    {
        // Ordinary instructions never acquired an issued capability. Their
        // scope-local value has no live pointer/resource to clear or abandon.
        if (issuedRep.Kernel.Owner == null && (state == null || state->Phase == FullRepPhase.Idle)) return;
        // Cleanup remains unconditional if reservation/ticket/mapping cleanup
        // earlier in the stack throws. No old state survives handler recursion.
        try { if (state != null) { state->AdmissionEpoch = 0; state->Phase = FullRepPhase.Abandoned; } }
        finally
        {
            try { issuedRep.Kernel.Dispose(); }
            finally { issuedRep = default; if (state != null) *state = default; }
        }
    }

    private static FullStackMemoryRequest RepRequestShape(in FullStackMemoryRequest request)
        => new() { Address = request.Address, Access = request.Access, Width = request.Width };

    private static FullStackMemoryDescriptor RepDescriptorShape(in FullStackMemoryDescriptor descriptor)
        => new() { Version = FullStackMemoryDescriptor.CurrentVersion, Count = descriptor.Count,
            Site = descriptor.Site, State = descriptor.Count == 0 ? FullStackMemoryState.Empty : FullStackMemoryState.Request,
            Request0 = descriptor.Count == 0 ? default : RepRequestShape(in descriptor.Request0),
            Request1 = descriptor.Count == 0 ? default : RepRequestShape(in descriptor.Request1) };

    private static void ValidateRepMapping(in FullIssuedRep issuedRep, FullRepState* state,
        in FullRepRequest request, FullBlockFrame* frame)
    {
        var instruction = issuedRep.Instruction;
        bool valid = request.Present && request.Attempt == issuedRep.Identity.AttemptGeneration &&
            request.Attempt == state->AttemptGeneration && request.Generation == state->RequestGeneration &&
            request.Site == issuedRep.Identity.OriginSite && frame->ResumeIndex == request.Site &&
            frame->MemoryResume == 1 && frame->MemorySite == request.Site && frame->FirstPreissued == 0 &&
            request.Width == (uint)instruction.Width && request.ObservedEpoch != 0;
        if (instruction.IsPair)
        {
            var d = frame->StackMemory;
            valid &= d != null && d->Version == FullStackMemoryDescriptor.CurrentVersion && d->Count == 2 &&
                d->Site == request.Site && d->State == FullStackMemoryState.Request &&
                d->Request0.Access == FullMemoryAccess.Read && d->Request1.Access == FullRepLowering.DestinationAccess(in instruction) &&
                d->Request0.Width == request.Width && d->Request1.Width == request.Width &&
                d->Request0.Address == request.Shape.Request0.Address && d->Request1.Address == request.Shape.Request1.Address;
            valid &= request.Access is FullMemoryAccess.Read or FullMemoryAccess.Write;
        }
        else
            valid &= frame->StackMemory != null && frame->StackMemory->Count == 0 &&
                frame->MemoryAddress == request.Address && frame->MemoryAccess == request.Access && frame->MemoryWidth == request.Width &&
                request.Access == (instruction.WritesMemory ? FullMemoryAccess.Write : FullMemoryAccess.Read);
        if (!valid) throw new InvalidOperationException("Full-JIT REP mapping request authentication failed.");
    }

    private static FullRepRequest CaptureRepRequest(in FullIssuedRep issuedRep, FullRepState* state, FullBlockFrame* frame)
    {
        var descriptor = frame->StackMemory == null ? default : *frame->StackMemory;
        var result = new FullRepRequest { Present = true, Attempt = state->AttemptGeneration,
            Generation = state->RequestGeneration, Site = frame->MemorySite, Width = frame->MemoryWidth,
            Access = frame->MemoryAccess, Address = frame->MemoryAddress, ObservedEpoch = frame->ObservedGlobalEpoch,
            Shape = RepDescriptorShape(in descriptor) };
        // Native pair summaries may identify either ordered operand. The full
        // descriptor, not a managed EA recipe, authenticates the two requests.
        if (issuedRep.Instruction.IsPair)
        {
            var a = result.Shape.Request0; var b = result.Shape.Request1;
            if (!((result.Address == a.Address && result.Access == a.Access && result.Width == a.Width) ||
                (result.Address == b.Address && result.Access == b.Access && result.Width == b.Width)))
                throw new InvalidOperationException("Full-JIT REP pair summary is not an issued operand.");
            *frame->StackMemory = result.Shape;
        }
        ValidateRepMapping(in issuedRep, state, in result, frame);
        return result;
    }

    private FullBlockFrame ExecuteIssuedRep(Worker worker, bool signalActor, ref FullRepState latch, ref FullIssuedRep issuedRep)
    {
        FullRepState state = latch; latch = default;
        FullRepRequest pending = default;
        bool reservation = false, missObserved = false;
        FullRepReason paused = FullRepReason.Latched;
        FullBlockFrame fault = default;
        try
        {
            while (true)
            {
                // This seam is after ALL old capabilities, including the passive
                // pin and pending reservation, have ended. Only values and the
                // independent immutable kernel code lease survive here.
                if (paused != FullRepReason.None)
                { FullControllerRepPaused(&state, paused); paused = FullRepReason.None; }
                if (worker.Stop.Reason != HostExecutionStopReason.None && state.Phase != FullRepPhase.FaultPending)
                    return new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize, Exit = FullBlockExit.Stop };
                if ((int)worker.Machine->attention != 0 && state.Phase != FullRepPhase.FaultPending)
                    return new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize, Exit = FullBlockExit.Attention };
                if (fullJit!.Authority.SemanticQuarantined)
                    throw new InvalidOperationException("Full-JIT REP admission is quarantined.");
                // Gate-free reader drain, with no store/fetch/passive capability.
                if (reservation && !fullJit.Authority.NativeReservationDrained(worker.FullCache.Worker))
                { Thread.Yield(); continue; }
                FullBlockFrame frame = default;
                long nativeCompleted = 0; // fresh per actual invocation, never frame.Executed
                FullStackMemoryDescriptor descriptor = default;
                bool enrolled = false, ticket = false;
                bool ticketWritesContent = true; // captured from immutable issued IR before promotion
                nint page0 = 0, page1 = 0, entry = 0;
                ulong completedBefore = state.CompletedIterations;
                bool previousFetch = worker.FullFetchActive;
                fullJit.BeginRead(program!, semantic: false);
                try
                {
                    worker.FullFetchActive = true;
                    lock (fullJit.Authority.Gate)
                    {
                        fullJit.Authority.DrainNativeForFiniteGateRead();
                        ValidateIssuedRep(worker, in issuedRep, &state, signalActor);
                        if (state.AdmissionEpoch != 0) throw new InvalidOperationException("REP permit survived RET.");
                        worker.NativePoll ??= worker.Stop.AcquireNativePoll();
                        frame = new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize,
                            Machine = worker.Machine, Stop = worker.NativePoll.Pointer, Budget = instructionBudget,
                            OwnerHandle = unchecked((nint)worker.FullCache.Serial), OwnerCookie = state.OwnerCookie,
                            BodyGeneration = state.OriginBodyGeneration, Context = state.Context, NextIp = state.OriginPc,
                            ResumeIndex = state.OriginSite, HelperIndex = FullBlockFrame.NoHelperIndex,
                            Temporary3 = state.AttemptGeneration, Rep = &state, StackMemory = &descriptor,
                            X87 = GetX87NativePointer(worker),
                            GlobalEpochPointer = fullJit.Authority.EpochPointer, ObservedGlobalEpoch = fullJit.Authority.Epoch,
                            CodeAuthorityEpochPointer = fullJit.Authority.CodeEpochPointer,
                            ObservedCodeAuthorityEpoch = fullJit.Authority.CodeEpoch,
                            MemoryTlb = worker.FullNativeState!.Tlb, MemoryTlbMask = FullNativeMemory.TlbCount - 1,
                            FsBase = worker.Machine->fs.@base, GsBase = worker.Machine->gs.@base };
                        PrepareNativeMappingCache(worker, &frame, reservation, ticket, out _);
                        descriptor = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
                        bool finalize = state.Phase == FullRepPhase.FaultPending;
                        if (finalize)
                        { frame.FaultTrap = fault.FaultTrap; frame.FaultCode = fault.FaultCode; frame.FaultAddress = fault.FaultAddress; }
                        else if (pending.Present)
                        {
                            descriptor = pending.Shape;
                            frame.MemoryResume = 1; frame.MemorySite = pending.Site;
                            frame.MemoryAddress = pending.Address; frame.MemoryAccess = pending.Access; frame.MemoryWidth = pending.Width;
                            ValidateRepMapping(in issuedRep, &state, in pending, &frame);
                            if (!missObserved) { fullJit.MemoryRequested(in frame); missObserved = true; }
                            program!.ResetTlb(worker.Machine);
                            fullJit.Authority.ResetDataPermissionDenial();
                            var instruction = issuedRep.Instruction;
                            var assist = reservation ? FullMemoryAssistResult.WriteBarrier : instruction.IsPair ?
                                FullNativeMemory.TryFillStringPair(program, worker.Machine, &frame, fullJit.Authority, instruction.Compares, instruction.SourceSegment) :
                                FullNativeMemory.TryFill(program, worker.Machine, &frame, fullJit.Authority);
                            fullJit.MemoryAssisted(assist, in frame);
                            if (assist == FullMemoryAssistResult.DemandPage)
                            {
                                assist = MaterializeNativeMemory(worker, &frame);
                                if (assist != FullMemoryAssistResult.GuestFault) continue;
                            }
                            if (assist == FullMemoryAssistResult.Retry) continue;
                            if (assist is FullMemoryAssistResult.WriteBarrier or FullMemoryAssistResult.UnsupportedBacking)
                            {
                                assist = InspectRepTicket(worker, in issuedRep, &state, in pending, &frame,
                                    out byte* first, out byte* second, out uint length, out page0, out page1);
                                if (assist == FullMemoryAssistResult.DemandPage)
                                {
                                    assist = MaterializeNativeMemory(worker, &frame);
                                    if (assist != FullMemoryAssistResult.GuestFault) continue;
                                }
                                if (assist == FullMemoryAssistResult.UnsupportedBacking)
                                    throw new InvalidOperationException("Full-JIT REP memory backing is unsupported.");
                                if (assist == FullMemoryAssistResult.Resolved)
                                {
                                    if (!reservation)
                                    {
                                        if (fullJit.Authority.TryReserveNativeTicket(worker.FullCache.Worker)) reservation = true;
                                        continue; // drop every capability BEFORE reader drain
                                    }
                                    ticketWritesContent = instruction.WritesMemory;
                                    if (!fullJit.Authority.TryPromoteNativeTicket(worker.FullCache.Worker, page0, page1,
                                        writesContent: ticketWritesContent)) continue;
                                    ticket = true;
                                    RevokeNativeTicketPages(page0, page1, ticketWritesContent, post: false);
                                    frame.ObservedGlobalEpoch = fullJit.Authority.Epoch;
                                    frame.ObservedCodeAuthorityEpoch = fullJit.Authority.CodeEpoch;
                                    // Rebuild pointer-free requests, then re-resolve BOTH
                                    // operands at the final promoted authority identity.
                                    descriptor = pending.Shape;
                                    assist = InspectRepTicket(worker, in issuedRep, &state, in pending, &frame,
                                        out byte* verifiedFirst, out byte* verifiedSecond, out uint verifiedLength,
                                        out nint verified0, out nint verified1);
                                    if (assist == FullMemoryAssistResult.UnsupportedBacking)
                                        throw new InvalidOperationException("Full-JIT promoted REP memory backing is unsupported.");
                                    if (assist != FullMemoryAssistResult.GuestFault)
                                    {
                                        if (assist != FullMemoryAssistResult.Resolved || verifiedLength != length ||
                                            verified0 != page0 || verified1 != page1 || fullJit.Authority.Epoch != frame.ObservedGlobalEpoch)
                                            continue;
                                        ValidateIssuedRep(worker, in issuedRep, &state, signalActor);
                                        if (instruction.IsPair)
                                            FullNativeMemory.PublishStringPairTicket(&frame, frame.ObservedGlobalEpoch,
                                                frame.ObservedGlobalEpoch, instruction.Compares);
                                        else FullNativeMemory.PublishBarrierTicket(&frame, verifiedFirst, verifiedSecond, verifiedLength,
                                            frame.ObservedGlobalEpoch, frame.ObservedGlobalEpoch);
                                    }
                                }
                            }
                            if (assist == FullMemoryAssistResult.GuestFault)
                            {
                                // FaultFinalize runs in a FRESH call after ticket,
                                // mapping and passive cleanup, never in this scope.
                                fault = CopyRepOutcome(in frame, FullBlockExit.GuestFault);
                                state.Phase = FullRepPhase.FaultPending;
                                state.Reason = FullRepReason.Fault;
                                pending = default;
                                paused = FullRepReason.Fault;
                                continue;
                            }
                        }
                        PrepareNativeMappingCache(worker, &frame, reservation, ticket, out _);
                        FullControllerNativePrepared(&frame);
                        ValidateOwnedX87KernelFrame(worker, in frame);
                        fullJit.RequireX87KernelAdmission(in frame);
                        ValidateIssuedRep(worker, in issuedRep, &state, signalActor);
                        enrolled = ticket ? fullJit.Authority.TryEnterNativeTicket(worker.FullCache.Worker, frame.ObservedGlobalEpoch) :
                            fullJit.Authority.TryEnterNative(frame.ObservedGlobalEpoch);
                        if (!enrolled) continue; // NO register-only fallback for an issued REP
                        state.AdmissionEpoch = frame.ObservedGlobalEpoch; // only successful enrollment mints this permit
                        entry = finalize ? issuedRep.Kernel.Binding.FaultFinalize : issuedRep.Kernel.Binding.Resume;
                    }
                    try
                    {
                        fixed (long* issuedPointer = &issued)
                        {
                            frame.Issued = issuedPointer; frame.Completed = &nativeCompleted;
                            EnterX87Native(worker);
                            ((delegate* unmanaged<FullBlockFrame*, void>)entry)(&frame);
                            // Only this straight-line path proves RET. Keep the x87 guard first;
                            // publish even if it rejects, before any other managed consumer.
                            try { X87NativeReturned(worker); }
                            finally
                            {
                                if (nativeCompleted != 0) Interlocked.Add(ref completed, nativeCompleted);
                            }
                            ValidateX87KernelReturn(worker, in frame);
                            FullControllerNativeReturned(&frame);
                        }
                    }
                    finally { if (enrolled) { fullJit.Authority.ExitNative(); enrolled = false; } }
                    fullJit.NativeCompleted(in frame);
                    worker.Instructions += frame.Executed;
                    lock (fullJit.Authority.Gate)
                    {
                        ValidateIssuedRep(worker, in issuedRep, &state, signalActor);
                        ValidateIssuedRepReturn(in issuedRep, &state, in frame);
                        if (state.CompletedIterations < completedBefore)
                            throw new InvalidOperationException("Full-JIT REP progress counter regressed.");
                        fullJit.RecordRepProgress(state.CompletedIterations - completedBefore,
                            frame.Exit == FullBlockExit.Continue);
                    }
                    if (frame.Exit == FullBlockExit.NeedMemory)
                    { pending = CaptureRepRequest(in issuedRep, &state, &frame); missObserved = false; }
                    else pending = default;
                    paused = state.Reason;
                }
                finally
                {
                    try
                    {
                        if (enrolled) fullJit.Authority.ExitNative();
                    }
                    finally
                    {
                        state.AdmissionEpoch = 0;
                        try
                        {
                            if (ticket)
                            {
                                lock (fullJit.Authority.Gate)
                                    try { RevokeNativeTicketPages(page0, page1, ticketWritesContent, post: true); }
                                    finally
                                    {
                                        try { fullJit.Authority.EndNativeTicket(worker.FullCache.Worker); }
                                        finally { reservation = false; fullJit.Authority.AbortNativeReservation(worker.FullCache.Worker); }
                                    }
                            }
                        }
                        finally
                        {
                            if (state.Phase == FullRepPhase.FaultPending && reservation && !ticket)
                            { fullJit.Authority.AbortNativeReservation(worker.FullCache.Worker); reservation = false; }
                            // No Ready/Host/ticket can escape the physical pin.
                            descriptor = default; frame = CopyRepOutcome(in frame, frame.Exit);
                            worker.FullFetchActive = previousFetch;
                            fullJit.EndRead(program!);
                        }
                    }
                }
                GC.KeepAlive(issuedRep.Kernel.Owner);
                if (frame.Exit is FullBlockExit.Continue or FullBlockExit.GuestFault)
                    return frame;
                if (state.Reason == FullRepReason.Stop)
                    return new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize, Exit = FullBlockExit.Stop };
                if (state.Reason == FullRepReason.Attention)
                    return new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize, Exit = FullBlockExit.Attention };
                // NeedMemory, authority/ticket boundary, explicit diagnostic
                // quantum all retain the same issue and immutable instruction.
            }
        }
        finally
        {
            try { fullJit!.Authority.AbortNativeReservation(worker.FullCache.Worker); }
            finally { pending = default; AbandonIssuedRep(ref issuedRep, &state); }
        }
    }

    private static void ValidateIssuedRepReturn(in FullIssuedRep issuedRep, FullRepState* state, in FullBlockFrame frame)
    {
        bool done = frame.Exit == FullBlockExit.Continue && state->Phase == FullRepPhase.Done && state->Reason == FullRepReason.Complete;
        bool fault = frame.Exit == FullBlockExit.GuestFault && state->Phase == FullRepPhase.FaultFinal && state->Reason == FullRepReason.Fault;
        bool memory = frame.Exit == FullBlockExit.NeedMemory && state->Phase == FullRepPhase.Active && state->Reason == FullRepReason.NeedMemory;
        bool paused = frame.Exit == FullBlockExit.RepYield && state->Phase == FullRepPhase.Active &&
            state->Reason is FullRepReason.Authority or FullRepReason.TicketBoundary or FullRepReason.Attention or FullRepReason.Stop or FullRepReason.Quantum;
        if (!(done || fault || memory || paused) || frame.Header != null || frame.Rep != state ||
            frame.OwnerCookie != state->OwnerCookie || unchecked((ulong)frame.OwnerHandle) != state->WorkerCookie ||
            frame.BodyGeneration != issuedRep.Origin.Generation || frame.Context != state->Context ||
            frame.Temporary3 != state->AttemptGeneration || frame.ResumeIndex != state->OriginSite ||
            frame.NextIp != (done ? state->FallthroughPc : state->OriginPc) || frame.FirstPreissued != 0 ||
            frame.HelperIndex != FullBlockFrame.NoHelperIndex || frame.Executed != (done ? 1UL : 0UL) ||
            frame.TailJumps != 0 || state->AdmissionEpoch != 0 ||
            fault && (!IsRepStringFault(issuedRep.Instruction, in frame) ||
                issuedRep.Instruction.Compares && state->LastFlags != state->OriginalFlags))
            throw new InvalidOperationException("Full-JIT REP kernel returned an unauthenticated result.");
    }

    // Typed transport follows the immutable issued source identity. Native12
    // consumes CPU12 as SIGBUS7/SI_KERNEL128/address0; CPU13 remains SIGSEGV11.
    private static bool IsRepStringFault(FullRepInstruction instruction, in FullBlockFrame frame)
    {
        if (frame.FaultTrap == 14) return true;
        if (frame.FaultCode != 128 || frame.FaultAddress != 0) return false;
        return frame.FaultTrap == 12 ? instruction.HasSource && instruction.SourceSegment == 2 :
            frame.FaultTrap == 13 && (instruction.SourceSegment != 2 || instruction.HasDestination);
    }

    private static FullBlockFrame CopyRepOutcome(in FullBlockFrame frame, FullBlockExit exit)
        => new() { Version = FullBlockFrame.AbiVersion, Size = FullBlockFrame.ByteSize,
            Exit = exit, Executed = frame.Executed, FaultTrap = frame.FaultTrap,
            FaultCode = frame.FaultCode, FaultAddress = frame.FaultAddress };
}
