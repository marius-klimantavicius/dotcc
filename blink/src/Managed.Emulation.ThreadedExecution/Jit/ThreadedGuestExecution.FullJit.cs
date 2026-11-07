using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    partial void FullControllerPreissued(Blink.Machine* machine, bool signalActor);
    partial void FullControllerMappingRevoked(long address, long size);
    partial void FullControllerNativePrepared(FullBlockFrame* frame);
    partial void FullControllerNativeReturned(FullBlockFrame* frame);
    partial void FullControllerHelperMappingEnded(Blink.Machine* machine);
    private long interpreterAttempts;
    private void AssertInterpreterAllowed(Blink.Machine* machine)
    {
        if (jitOptions.EffectiveMode != GuestJitMode.Full) return;
        Interlocked.Increment(ref interpreterAttempts);
        throw new InvalidOperationException($"Full-JIT interpreter tripwire at guest IP 0x{(machine == null ? 0 : machine->ip):x}.");
    }

    void IHostGuestThreads.InterpreterDispatch(nint machine) => AssertInterpreterAllowed((Blink.Machine*)machine);

    void IHostGuestInstructionMemory.RecycleAnonymousPage(nint system, nint page, bool clear)
    {
        if (fullJit != null) fullJit.RecycleAnonymousPage(program!, (Blink.System*)system, (byte*)page, clear,
            current.Value is { } worker && fullJit.Authority.OwnsPreciseMapping(worker.FullCache.Worker));
        else program!.BlinkGuestRecycleAnonymousPage((Blink.System*)system, (byte*)page, clear ? 1 : 0);
    }

    // Nonopcode consumers (signal-frame installation and child-TID cleanup)
    // retain lazy data borrows through their final memcpy/atomic as well.
    private readonly struct FullDataScope : IDisposable
    {
        private readonly ThreadedGuestExecution owner;
        private readonly Worker worker;
        private readonly bool previousActive, previousBorrow, previousAuthority, previousMapping;
        internal FullDataScope(ThreadedGuestExecution owner, Worker worker)
        {
            this.owner = owner; this.worker = worker;
            previousActive = worker.FullHelperActive; previousBorrow = worker.FullDataBorrow; previousAuthority = worker.FullAuthorityBorrow; previousMapping = worker.FullMappingBorrow;
            if (owner.fullJit != null) { worker.FullHelperActive = true; ++worker.FullMappingScopeDepth; }
        }
        public void Dispose()
        {
            worker.FullHelperActive = previousActive;
            if (owner.fullJit != null) --worker.FullMappingScopeDepth;
            try
            {
                if (!previousMapping && worker.FullMappingBorrow)
                { worker.FullMappingBorrow = false; owner.fullJit!.Authority.EndOwnedMappingMutation(worker.FullCache.Worker); }
            }
            finally
            {
                try
                {
                    if (!previousAuthority && worker.FullAuthorityBorrow)
                    { worker.FullAuthorityBorrow = false; owner.fullJit!.Authority.EndBorrow(worker.FullCache.Worker); }
                }
                finally
                {
                    if (!previousBorrow && worker.FullDataBorrow)
                    { worker.FullDataBorrow = false; owner.fullJit!.EndRead(owner.program!); }
                }
            }
        }
    }

    private void CheckSignals(Worker worker)
    {
        worker.FullLastExit = default;
        using var data = new FullDataScope(this, worker);
        program!.CheckForSignals(worker.Machine);
    }

    ulong IHostGuestInstructionMemory.FindPageTableEntry(nint pointer, ulong page)
    {
        var machine = (Blink.Machine*)pointer;
        var worker = current.Value;
        if (fullJit == null) return program!.BlinkGuestFindPageTableEntry(machine, page);
        if (worker == null) throw new InvalidOperationException("Full-JIT page walk has no owning worker.");
        if (worker.FullFetchActive || worker.FullDataBorrow)
            return program!.BlinkGuestFindPageTableEntry(machine, page);
        if (worker.FullHelperActive && (int)machine->insyscall == 0)
        {
            // Enroll before clearing the TLB: a new pin cannot resurrect a
            // raw cached backing pointer reclaimed before enrollment.
            fullJit.BeginRead(program!, semantic: true);
            worker.FullDataBorrow = true;
            program!.ResetTlb(machine);
            return program.BlinkGuestFindPageTableEntry(machine, page);
        }
        // Syscalls retain their ordinary PAGE_LOCK leaf borrows. Protect only
        // the walk until that lock is acquired; never retain this pin over I/O.
        // Clearing on every short scope also prevents nested syscall TLB hits
        // from bypassing PAGE_LOCK acquisition after a handler's code fetch.
        fullJit.BeginRead(program!, semantic: false);
        try { program!.ResetTlb(machine); return program.BlinkGuestFindPageTableEntry(machine, page); }
        finally { fullJit.EndRead(program!); }
    }

    void IHostGuestCodeRevocation.Revoke(long address, long size)
    {
        if (fullJit == null) { ((IHostGuestCodeRevocation?)guestJit)?.Revoke(address, size); return; }
        var worker = current.Value;
        fullJit.NotifyVirtualWrite(worker == null ? null : worker.Machine, address, size);
    }
    void IHostGuestCodeRevocation.RevokeMapping(long address, long size)
    {
        if (fullJit == null) { ((IHostGuestCodeRevocation?)guestJit)?.RevokeMapping(address, size); return; }
        var worker = current.Value ?? throw new InvalidOperationException("Full-JIT mapping mutation has no owning scope.");
        var machine = worker.Machine;
        bool precise = worker.FullMappingScopeDepth > 0 && !worker.FullFetchActive && machine != null &&
            machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
            (int)machine->metal == 0 && program!.GetPc(machine) == (long)machine->ip;
        fullJit.RevokeOwnedMapping(program!, machine, worker.FullCache.Worker, address, size, precise, ref worker.FullMappingBorrow);
        FullControllerMappingRevoked(address, size);
    }

    void IHostGuestInstructionMemory.PhysicalDataExposed(nint machine, nint page)
    {
        if (fullJit == null) return;
        var worker = current.Value ?? throw new InvalidOperationException("Full-JIT physical borrow has no owning worker.");
        worker.FullAuthorityBorrow = true;
        fullJit.PhysicalExposed(worker.FullCache.Worker, page);
    }

    // Erased in production; discriminates a real reader pulse across Acquire.
    partial void FullControllerEligibilitySnapshot();

    // Native blocks return before helpers/GC/guest unwinds. A retained payload
    // carries a copied decoded instruction and its specific selected function.
    private bool ExecuteFullInstruction(Worker worker, bool signalActor = false)
    {
        if (signalActor) worker.FullLastExit = default;
        try { return ExecuteFullInstructionCore(worker, signalActor); }
        catch { worker.FullLastExit = default; throw; }
        finally
        {
            try
            {
                try { fullJit?.CancelPublication(worker.FullCache.Worker); }
                finally { fullJit?.Authority.AbortNativeReservation(worker.FullCache.Worker); }
            }
            catch { worker.FullLastExit = default; throw; }
        }
    }
    private bool ExecuteFullInstructionCore(Worker worker, bool signalActor)
    {
        if (worker.Stop.Reason != HostExecutionStopReason.None) { worker.FullLastExit = default; return false; }
        if ((ulong)Interlocked.Increment(ref issued) > instructionBudget)
        { worker.FullLastExit = default; stop.RequestBudgetStop(); return false; }
        bool preissued = true;
        FullControllerPreissued(worker.Machine, signalActor);
        var predecessor = worker.FullLastExit;
        worker.FullLastExit = default;
        if (signalActor || (int)worker.Machine->attention != 0) predecessor = default;
        var machine = worker.Machine;
        bool pendingMemory = false;
        bool memoryMissObserved = false;
        bool ticketReservation = false;
        FullBlockFrame memoryRequest = default;
        FullStackMemoryDescriptor stackRequest = default;
        FullTranslationJit.Plan memoryPlan = default;
        AtomicRequestSnapshot atomicRequest = default;
        FullRepState repState = default;
        FullIssuedRep issuedRep = default;
        FullIssuedEnvironment issuedEnvironment = default;
        try
        {
        while (true)
        {
            if (worker.Stop.Reason != HostExecutionStopReason.None) return true;
            if (fullJit!.Authority.SemanticQuarantined)
                throw new InvalidOperationException("Full-JIT semantic/memory admission is quarantined.");
            // Reservation carries no store epoch, translation lease, fetch pin
            // or Gate while readers drain. Keep the copied request/plan and
            // its already approved issue; stop abandons it in the outer finally.
            if (ticketReservation && !fullJit!.Authority.NativeReservationDrained(worker.FullCache.Worker))
            { Thread.Yield(); continue; }
            if (!preissued)
            {
                if ((ulong)Interlocked.Increment(ref issued) > instructionBudget)
                { stop.RequestBudgetStop(); return true; }
                preissued = true;
            }
            FullTranslationJit.Plan plan = default;
            bool needsHelper = false, revokedNativeAuthority = false;
            bool guestFault = false;
            int error = 0;
            FullBlockFrame frame = default;
            long nativeCompleted = 0; // fresh per actual invocation, never frame.Executed
            FullTranslationJit.LastExitLink lastExit = default;
            TranslationLease lease = default;
            NativeEpochLease epoch = default;
            bool enrolled = false;
            bool nativeTicket = false;
            bool ticketWritesContent = true; // scope value; never reread a returned frame
            nint ticketPage0 = 0, ticketPage1 = 0;
            nint entry = 0;
            bool requestOnlyAtomic = false;
            bool retainedNativeEntry = false;
            bool previousFetch = worker.FullFetchActive;
            bool fetchBorrow = false;
            try
            {
                worker.FullFetchActive = true;
                if (!pendingMemory && !signalActor && !ticketReservation && predecessor.Source.Generation == 0 &&
                    TryPrepareRetainedNativeEntry(worker, preissued, &repState, out lease, out epoch, out frame, out entry))
                {
                    enrolled = retainedNativeEntry = true;
                }
                else
                {
                lock (fullJit.Authority.Gate)
                {
                    fullJit.Authority.DrainNativeForFiniteGateRead();
                    var validationSnapshot = fullJit.Authority.CaptureCodeSnapshot(independentContent:
                        machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
                        program!.GetPc(machine) == (long)machine->ip && (int)machine->metal == 0);
                    if (fullJit.AcquireNativeEntry(program!, machine, ref worker.FullCache,
                        worker.FullResume, worker.FullResumeIndex, pendingMemory || signalActor, ref fetchBorrow,
                        out lease, out var entryView, out uint index, out bool multiple,
                        out bool retainedAdmission, out error, requestedCount: signalActor ? 1u : 0u))
                    {
                        {
                            FullControllerEligibilitySnapshot();
                            multiple &= fullJit.Authority.CodeSnapshotCurrent(in validationSnapshot);
                            uint nativeLimit = multiple && !signalActor ? 0u : 1u;
                            fullJit.AuthorizeEntryLinks(in lease, in entryView, multiple, index, nativeLimit);
                            if (predecessor.Source.Generation != 0)
                            {
                                var previousExit = predecessor; predecessor = default;
                                fullJit.CompleteLastExitLink(in previousExit, in lease, index, multiple, nativeLimit,
                                    FullTranslationJit.Context(program!, machine, signalActor ? 1u : 0u), worker.Instructions);
                            }
                            var header = entryView.Header[0];
                            worker.NativePoll ??= worker.Stop.AcquireNativePoll();
                            frame = new() { Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame),
                                Machine = machine, Stop = worker.NativePoll.Pointer, Budget = instructionBudget,
                                NativeLimit = nativeLimit,
                                NextIp = machine->ip, Header = entryView.Header,
                                OwnerCookie = header.OwnerCookie, BodyGeneration = header.BodyGeneration,
                                Context = header.Context, GlobalEpochPointer = fullJit.Authority.EpochPointer,
                                ObservedGlobalEpoch = fullJit.Authority.Epoch,
                                CodeAuthorityEpochPointer = fullJit.Authority.CodeEpochPointer,
                                ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch, ResumeIndex = index,
                                HelperIndex = FullBlockFrame.NoHelperIndex, FirstPreissued = preissued ? 1u : 0,
                                MemoryTlb = worker.FullNativeState!.Tlb, MemoryTlbMask = FullNativeMemory.TlbCount - 1,
                                JumpCache = worker.FullNativeState.Jumps, JumpCacheMask = FullNativeMemory.JumpCount - 1,
                                StoreGenerationPointer = entryView.GenerationPointer,
                                ObservedStoreGeneration = entryView.Generation,
                                FsBase = machine->fs.@base, GsBase = machine->gs.@base,
                                StackMemory = worker.FullNativeState.StackMemory, Rep = &repState,
                                X87 = GetX87NativePointer(worker),
                                OwnerHandle = unchecked((nint)worker.FullCache.Serial) };
                            PrepareNativeMappingCache(worker, &frame, ticketReservation, nativeTicket, out _);
                            PrepareRepLatch(worker, &repState);
                            *frame.StackMemory = pendingMemory ? stackRequest :
                                new FullStackMemoryDescriptor { Version = FullStackMemoryDescriptor.CurrentVersion };
                            if (pendingMemory && frame.StackMemory->Count != 0) frame.StackMemory->Site = index;
                            if (pendingMemory)
                            {
                                plan = FullTranslationJit.Plans(fullJit.Store, in lease)[(int)index];
                                if (plan.Helper != null || !SameNativeInstruction(in plan, in memoryPlan))
                                {
                                    fullJit.Authority.AbortNativeReservation(worker.FullCache.Worker); ticketReservation = false;
                                    pendingMemory = false; frame = default; continue;
                                }
                                if (!memoryMissObserved)
                                {
                                    fullJit.MemoryRequested(in memoryRequest);
                                    memoryMissObserved = true;
                                }
                                CopyMemoryRequest(in memoryRequest, ref frame, index);
                                program!.ResetTlb(machine);
                                fullJit.Authority.ResetDataPermissionDenial();
                                AuthenticateNativeMemoryDescriptor(in plan, &frame, machine);
                                AuthenticateAtomicSnapshot(in plan, &frame, machine, in atomicRequest);
                                bool stringPair = NativeStringPair(in plan, out bool stringCompare, out int stringWidth, out int stringSourceSegment);
                                if (stringPair) AuthenticateStringPairWidth(&frame, stringWidth);
                                if (ticketReservation && FullNativeMemory.IsCanonicalX87MemoryProbe(&frame))
                                    throw new InvalidOperationException("Mapping-only x87 proof cannot inherit a write reservation.");
                                var assist = ticketReservation ? FullMemoryAssistResult.WriteBarrier :
                                    stringPair ? FullNativeMemory.TryFillStringPair(program, machine, &frame,
                                        fullJit.Authority, stringCompare, stringSourceSegment) :
                                    TryFillNativeMemoryForPlan(worker, in plan, &frame);
                                fullJit.MemoryAssisted(assist, in frame);
                                if (assist == FullMemoryAssistResult.DemandPage)
                                {
                                    assist = MaterializeNativeMemoryForPlan(worker, in plan, &frame);
                                    if (assist != FullMemoryAssistResult.GuestFault) { frame = default; continue; }
                                }
                                if (assist == FullMemoryAssistResult.Retry)
                                { frame = default; continue; }
                                if (assist is FullMemoryAssistResult.WriteBarrier or FullMemoryAssistResult.UnsupportedBacking)
                                {
                                    if (FullNativeMemory.IsCanonicalX87MemoryProbe(&frame))
                                        throw new InvalidOperationException("Mapping-only x87 proof cannot acquire a physical write ticket.");
                                    assist = InspectNativeTicketWithFxProof(worker, in plan, &frame, out byte* first, out byte* second,
                                        out uint firstLength, out ticketPage0, out ticketPage1, out _);
                                    if (assist == FullMemoryAssistResult.DemandPage)
                                    {
                                        assist = MaterializeNativeMemoryForPlan(worker, in plan, &frame);
                                        if (assist != FullMemoryAssistResult.GuestFault) { frame = default; continue; }
                                    }
                                    // The read-only inspection can race an epoch
                                    // advance independently of the preceding fill.
                                    // Retry the SAME approved request before any
                                    // ticket/native preparation; no unowned entry.
                                    if (assist == FullMemoryAssistResult.Retry)
                                    { frame = default; continue; }
                                    if (assist == FullMemoryAssistResult.UnsupportedBacking)
                                        throw new InvalidOperationException("Full-JIT native memory backing is not supported by the checked mapping assist.");
                                    if (assist == FullMemoryAssistResult.Resolved)
                                    {
                                        if (!ticketReservation)
                                        {
                                            if (!fullJit.Authority.TryReserveNativeTicket(worker.FullCache.Worker))
                                            { frame = default; continue; }
                                            ticketReservation = true;
                                        }
                                        ticketWritesContent = NativeTicketWritesContent(in plan, &frame, machine);
                                        if (!fullJit.Authority.TryPromoteNativeTicket(worker.FullCache.Worker, ticketPage0, ticketPage1,
                                            writesContent: ticketWritesContent))
                                        { frame = default; continue; }
                                        nativeTicket = true;
                                        RevokeNativeTicketPages(ticketPage0, ticketPage1, ticketWritesContent, post: false);
                                        var token = lease.Token;
                                        lease.Dispose();
                                        if (!fullJit.Acquire(program, machine, ref worker.FullCache, token, index, out lease,
                                            out index, out _, out error, requestedCount: signalActor ? 1u : 0u))
                                        { frame = default; continue; }
                                        plan = FullTranslationJit.Plans(fullJit.Store, in lease)[(int)index];
                                        if (plan.Helper != null || !SameNativeInstruction(in plan, in memoryPlan))
                                        { pendingMemory = false; frame = default; continue; }
                                        header = fullJit.Store.GetHeaderCopy(in lease);
                                        frame.Header = (TranslationBlockHeader*)fullJit.Store.GetHeader(in lease);
                                        frame.OwnerCookie = header.OwnerCookie; frame.BodyGeneration = header.BodyGeneration;
                                        frame.Context = header.Context; frame.ObservedGlobalEpoch = fullJit.Authority.Epoch;
                                        frame.ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch;
                                        frame.ResumeIndex = frame.MemorySite = index;
                                        if (FullNativeMemory.IsStackRequest(&frame) || FullNativeMemory.IsMaskedStoreRequest(&frame) || FullNativeMemory.IsEnterRequest(&frame))
                                            frame.StackMemory->Site = index;
                                        // Host EOF/backing observation can change while reservation
                                        // drains native frames, or while Acquire validates code.
                                        // Reinspect both endpoints at the FINAL header epoch; do
                                        // not stamp previously inspected pointers with a new epoch.
                                        assist = InspectNativeTicketWithFxProof(worker, in plan, &frame, out byte* verifiedFirst, out byte* verifiedSecond,
                                            out uint verifiedLength, out nint verifiedPage0, out nint verifiedPage1,
                                            out var verifiedFxProof);
                                        if (assist == FullMemoryAssistResult.UnsupportedBacking)
                                            throw new InvalidOperationException("Full-JIT promoted memory backing is unsupported.");
                                        if (assist != FullMemoryAssistResult.GuestFault)
                                        {
                                            if (assist != FullMemoryAssistResult.Resolved || verifiedLength != firstLength ||
                                                verifiedPage0 != ticketPage0 || verifiedPage1 != ticketPage1 ||
                                                fullJit.Authority.Epoch != frame.ObservedGlobalEpoch)
                                            { frame = default; continue; }
                                            if (NativeStringPair(in plan, out stringCompare, out stringWidth, out _))
                                            {
                                                AuthenticateStringPairWidth(&frame, stringWidth);
                                                FullNativeMemory.PublishStringPairTicket(&frame, frame.ObservedGlobalEpoch,
                                                    frame.ObservedGlobalEpoch, stringCompare);
                                            }
                                            else if (FullNativeMemory.IsMaskedStoreRequest(&frame))
                                                FullNativeMemory.PublishMaskedStoreTicket(&frame, frame.ObservedGlobalEpoch, frame.ObservedGlobalEpoch);
                                            else if (FullNativeMemory.IsEnterRequest(&frame))
                                                FullNativeMemory.PublishEnterTicket(&frame, frame.ObservedGlobalEpoch, frame.ObservedGlobalEpoch);
                                            else if (FullNativeMemory.IsStackRequest(&frame))
                                                FullNativeMemory.PublishStackTicket(&frame, frame.ObservedGlobalEpoch, frame.ObservedGlobalEpoch);
                                            else FullNativeMemory.PublishBarrierTicket(&frame, verifiedFirst, verifiedSecond, verifiedLength,
                                                frame.ObservedGlobalEpoch, frame.ObservedGlobalEpoch);
                                            if (verifiedFxProof.Version == FullCanonicalX87MemoryLowering.MemoryProbeTag)
                                                PublishCanonicalX87FxProof(&frame, in verifiedFxProof);
                                        }
                                    }
                                }
                                if (assist == FullMemoryAssistResult.GuestFault)
                                { frame.Exit = FullBlockExit.GuestFault; guestFault = true; }
                                else FullNativeMemory.ResumeSameIssue(&frame);
                            }
                            if (guestFault) { /* Deliver only after every capability below has unwound. */ }
                            else
                            {
                            // Pending fills may have registered a physical role or
                            // raced a host writer. Never adopt old hints at that epoch.
                            bool mappingReady = PrepareNativeMappingCache(worker, &frame, ticketReservation, nativeTicket, out bool mappingDiscarded);
                            FullControllerNativePrepared(&frame);
                            ValidateOwnedX87KernelFrame(worker, in frame);
                            fullJit.RequireX87KernelAdmission(in frame);
                            // A pending scalar CAS authenticates its freshly filled
                            // TLB entry after enrollment. If OUR final provenance
                            // phase discarded it, or kept it dormant under OWN reservation,
                            // retry the same approved request
                            // before entering. Still run the preparation boundary:
                            // an old reader may need it to finish/release admission.
                            // Foreign/tampered missing mappings retain strict auth;
                            // owned tickets and their independent pointers are intact.
                            if ((mappingDiscarded || !mappingReady) && pendingMemory && atomicRequest.Valid && !nativeTicket)
                            { frame = default; continue; }
                            if (!fullJit.Store.TryBeginNativeEpoch(out epoch))
                            {
                                if (fullJit.Store.GetStatistics().Quarantined)
                                    throw new InvalidOperationException("Full-JIT code arena protection failed; native admission is closed.");
                                // A packed-arena writer has closed admission.
                                // Unwind every fetch/managed lease before a
                                // retry, retaining the approved issue exactly.
                                frame = default;
                                continue;
                            }
                            enrolled = nativeTicket ? fullJit.Authority.TryEnterNativeTicket(worker.FullCache.Worker, frame.ObservedGlobalEpoch) :
                                fullJit.Authority.TryEnterNative(frame.ObservedGlobalEpoch);
                            if (!enrolled)
                            {
                                var entryPlan = FullTranslationJit.Plans(fullJit.Store, in lease)[(int)index];
                                // Retained admission omitted the physical read scope:
                                // it requires real enrollment even for cap1.
                                bool coldAtomic = NativeAtomic(in entryPlan, out _);
                                if (retainedAdmission || nativeTicket || atomicRequest.Valid || frame.X87 != null && !coldAtomic) { frame = default; continue; }
                                requestOnlyAtomic = coldAtomic;
                                // A COLD atomic may only stage its checked request in
                                // the pointer-free cap1 envelope below. It cannot
                                // load/CAS: both the worker TLB and host pointers are
                                // removed. Requiring enrollment even for this request
                                // deadlocks an old semantic reader awaiting reservation.
                                // Every pending atomic execution still requires true
                                // enrollment plus exact ticket or fast-map admission.
                                // A REP latch must only run under actual native
                                // admission, including a tail-reached REP body.
                                if (NativeRep(in entryPlan, out _)) { frame = default; continue; }
                                // Register-only checked cap1 remains usable in
                                // a pending mapping scope. Memory cannot touch
                                // a cached pointer without native enrollment.
                                frame.NativeLimit = 1; frame.MemoryTlb = null;
                                // A Ready two-access descriptor contains direct
                                // pointers independent of the TLB. They require
                                // the SAME native admission; discard that path
                                // when only register-only cap1 may execute.
                                if (frame.StackMemory != null)
                                {
                                    *frame.StackMemory = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
                                }
                            }
                            if (pendingMemory && !RequireAtomicAdmission(in plan, &frame, machine, in atomicRequest, nativeTicket, enrolled))
                            { frame = default; continue; }
                            if (enrolled && multiple && header.Reserved136 == 1)
                                FullNativeMemory.PublishJump(frame.JumpCache, frame.JumpCacheMask, frame.ObservedStoreGeneration,
                                    frame.Header, in header);
                            entry = header.Entry;
                            }
                        }
                    }
                }
                }
                // No authority gate spans native execution. Writers advance
                // the polled epoch and drain the enrollment counter before
                // effects. Store epoch + root lease protect ALL chain bodies.
                if (entry != 0)
                {
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
                                        CommitRepAttempt(worker, &repState, in frame);
                                        if (retainedNativeEntry) RetainedNativeEntryReturned(&frame);
                                        FullControllerNativeReturned(&frame);
                                    }
                                }
                                finally { if (enrolled) { fullJit.Authority.ExitNative(); enrolled = false; } }
                                if (requestOnlyAtomic && (frame.Executed != 0 || frame.NativeMemoryComputations != 0 ||
                                    frame.Exit is not (FullBlockExit.NeedMemory or FullBlockExit.GuestFault or FullBlockExit.Invalidated or
                                        FullBlockExit.Stop or FullBlockExit.Attention or FullBlockExit.Budget)))
                                    throw new InvalidOperationException("Cold pointer-free atomic request attempted a computation.");
                                fullJit.NativeCompleted(in frame);
                                worker.Instructions += frame.Executed;
                                preissued = frame.FirstPreissued != 0;
                                RequireProcessorEnvironmentDispatch(worker, in frame);
                                // A tailchain can exit in another body; the global
                                // epoch protects that header/payload until copied.
                                var currentHeader = frame.Header;
                                if (frame.Exit == FullBlockExit.Invalidated)
                                {
                                    // Host writer admission advances the global
                                    // epoch BEFORE revoking headers. A revalidation
                                    // can stamp that new global version just before
                                    // the writer changes Live/CodeEpoch. Capture
                                    // those reasons while the native epoch still
                                    // protects the header; the global version alone
                                    // does not establish that the entry stayed live.
                                    revokedNativeAuthority = currentHeader->Live != 1 ||
                                        currentHeader->CodeEpoch != currentHeader->ValidatedCodeEpoch ||
                                        currentHeader->ValidatedGlobalEpoch != frame.ObservedCodeAuthorityEpoch ||
                                        fullJit.Authority.CodeEpoch != frame.ObservedCodeAuthorityEpoch ||
                                        fullJit.Authority.Epoch != frame.ObservedGlobalEpoch;
                                }
                                worker.FullResume = new((int)currentHeader->Slot, currentHeader->BodyGeneration);
                                worker.FullResumeIndex = frame.ResumeIndex;
                                if (frame.Exit == FullBlockExit.RepYield)
                                    CaptureIssuedRep(worker, in epoch, in frame, &repState, ref issuedRep, signalActor);
                                if (!signalActor && frame.Exit == FullBlockExit.Continue)
                                    lastExit = fullJit.CaptureLastExitLink(program!, in epoch, in frame, worker.Instructions);
                                if (frame.Exit == FullBlockExit.NeedEnvironment)
                                {
                                    issuedEnvironment = CaptureIssuedEnvironment(worker, in epoch, &frame, signalActor);
                                    pendingMemory = false;
                                    preissued = true; // native Issue already approved this exact operation
                                }
                                else if (frame.Exit == FullBlockExit.NeedHelper)
                                {
                                    if (frame.HelperIndex >= currentHeader->PayloadBytes / sizeof(FullTranslationJit.Plan))
                                        throw new InvalidOperationException("Full-JIT helper index authentication failed.");
                                    plan = ((FullTranslationJit.Plan*)currentHeader->Payload)[frame.HelperIndex];
                                    if (plan.Helper == null) throw new InvalidOperationException("Full-JIT native site requested a typed helper.");
                                    needsHelper = true;
                                }
                                else if (frame.Exit == FullBlockExit.GuestFault)
                                {
                                    if (frame.ResumeIndex >= currentHeader->PayloadBytes / sizeof(FullTranslationJit.Plan))
                                        throw new InvalidOperationException("Full-JIT fault site authentication failed.");
                                    plan = ((FullTranslationJit.Plan*)currentHeader->Payload)[frame.ResumeIndex];
                                    if (plan.Helper != null || plan.Ip != machine->ip ||
                                        (frame.FaultTrap is 0 or 6 or 16 or 19 && frame.FaultAddress != plan.Ip) ||
                                        (frame.FaultTrap is 12 or 13 && (frame.FaultAddress != 0 || frame.FaultCode != 128)) ||
                                        (frame.FaultTrap == 6 && (frame.FaultCode != 1 ||
                                            !FullCanonicalX87InvalidLowering.TryClassify(plan.Ip,
                                                new(plan.Decoded.bytes, plan.Decoded.length), out _))) ||
                                        frame.FaultTrap is not (0 or 6 or 12 or 13 or 14 or 16 or 19))
                                        throw new InvalidOperationException("Full-JIT native fault metadata authentication failed.");
                                    guestFault = true;
                                }
                                else if (frame.Exit == FullBlockExit.NeedMemory)
                                {
                                    if (frame.MemoryResume != 1 || frame.MemorySite != frame.ResumeIndex ||
                                        frame.MemorySite >= currentHeader->PayloadBytes / sizeof(FullTranslationJit.Plan))
                                        throw new InvalidOperationException("Full-JIT native memory site authentication failed.");
                                    memoryPlan = ((FullTranslationJit.Plan*)currentHeader->Payload)[frame.MemorySite];
                                    if (memoryPlan.Helper != null || memoryPlan.Ip != machine->ip)
                                        throw new InvalidOperationException("Full-JIT native memory plan authentication failed.");
                                    if (NativeRep(in memoryPlan, out _))
                                        throw new InvalidOperationException("Issued REP cannot use ordinary memory reacquisition.");
                                    atomicRequest = CaptureAtomicRequest(in memoryPlan, &frame, machine);
                                    memoryRequest = frame;
                                    memoryRequest.Completed = null; // copied request cannot retain this stack counter
                                    memoryMissObserved = false;
                                    stackRequest = frame.StackMemory == null ? default : *frame.StackMemory;
                                    pendingMemory = true;
                                    preissued = true; // NeedMemory already reserved this issue.
                                }
                                else pendingMemory = false;
                }
            }
            finally
            {
                try
                {
                    if (enrolled) fullJit.Authority.ExitNative();
                    if (nativeTicket)
                    {
                        lock (fullJit.Authority.Gate)
                        {
                            try { RevokeNativeTicketPages(ticketPage0, ticketPage1, ticketWritesContent, post: true); }
                            finally
                            {
                                try { fullJit.Authority.EndNativeTicket(worker.FullCache.Worker); }
                                finally { ticketReservation = false; }
                            }
                        }
                    }
                }
                finally
                {
                    try { epoch.Dispose(); }
                    finally
                    {
                        try { lease.Dispose(); }
                        finally
                        {
                            worker.FullFetchActive = previousFetch;
                            if (fetchBorrow) fullJit.EndRead(program!);
                        }
                    }
                }
            }
            if (issuedEnvironment.Origin.Generation != 0)
            {
                // Original native enrollment, epoch, root lease, Gate and fetch
                // read have ALL ended. The pending request is only a value.
                plan = issuedEnvironment.Plan;
                memoryRequest = default; stackRequest = default; memoryPlan = default;
                frame = ExecuteIssuedEnvironment(worker, signalActor, in issuedEnvironment, out error);
                issuedEnvironment = default;
                preissued = frame.FirstPreissued != 0;
                pendingMemory = false;
                guestFault = frame.Exit == FullBlockExit.GuestFault;
                predecessor = default; // dedicated cap1 commit supplies no chain edge
            }
            if (issuedRep.Kernel.Owner != null)
            {
                // Every original-body capability has ended before this call.
                // The inner scope disposes state/code lifetime before returning
                // a copied result to normal fault/signal/Actor handling.
                plan = issuedRep.Plan;
                frame = default; memoryRequest = default; stackRequest = default; memoryPlan = default;
                *worker.FullNativeState!.StackMemory = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
                frame = ExecuteIssuedRep(worker, signalActor, ref repState, ref issuedRep);
                preissued = false;
                pendingMemory = false;
                guestFault = frame.Exit == FullBlockExit.GuestFault;
                worker.FullResume = default; worker.FullResumeIndex = 0;
            }
            if (error != 0)
            {
                fullJit.Authority.AbortNativeReservation(worker.FullCache.Worker); ticketReservation = false;
                using var data = new FullDataScope(this, worker);
                program!.HaltMachine(machine, error);
                throw new InvalidOperationException("Guest instruction-fetch fault returned.");
            }
            if (frame.Version == 0)
            { Thread.Yield(); continue; } // authority retry; approved issue retained
            if (guestFault)
            {
                fullJit.Authority.AbortNativeReservation(worker.FullCache.Worker); ticketReservation = false;
                // HaltMachine restores IP from the ordinary pre-op staging.
                // All generated frames, store capabilities and physical pins
                // have ended before signal handling can recurse into Full.
                using var data = new FullDataScope(this, worker);
                var decoded = plan.Decoded;
                var previousDecoded = machine->xedd;
                machine->xedd = &decoded;
                machine->oplen = decoded.length;
                machine->ip = unchecked(plan.Ip + decoded.length);
                machine->faultaddr = unchecked((long)frame.FaultAddress);
                machine->segvcode = unchecked((sbyte)frame.FaultCode);
                int haltReason = frame.FaultTrap switch { 0 => Blink.kMachineDivideError,
                    6 => Blink.kMachineUndefinedInstruction,
                    // The selected positive-vector path already delivers #GP
                    // as Linux SIGSEGV/SI_KERNEL, with a zero fault address.
                    13 => 13,
                    12 => 12,
                    16 => Blink.kMachineFpuException,
                    19 => Blink.kMachineSimdException, _ => Blink.kMachineSegmentationFault };
                try { program!.HaltMachine(machine, haltReason); }
                finally { machine->xedd = previousDecoded; }
                throw new InvalidOperationException("Guest native instruction fault returned.");
            }
            if (needsHelper)
            {
                var resume = worker.FullResume;
                uint resumeIndex = worker.FullResumeIndex;
                try { ExecuteFullHelper(worker, plan); }
                finally { worker.FullResume = resume; worker.FullResumeIndex = resumeIndex; }
                preissued = false;
                if (signalActor || worker.Stop.Reason != HostExecutionStopReason.None || (int)machine->attention != 0) return true;
                continue;
            }
            switch (frame.Exit)
            {
                case FullBlockExit.NeedMemory: continue;
                case FullBlockExit.Budget: stop.RequestBudgetStop(); return true;
                case FullBlockExit.Stop:
                case FullBlockExit.Attention: return true;
                case FullBlockExit.Invalidated:
                    // No opcode fallback. Retry with the same approved issue;
                    // authority validation can reuse a revoked resident body.
                    if (frame.Executed == 0 && preissued)
                    {
                        if (revokedNativeAuthority || frame.ObservedGlobalEpoch != fullJit.Authority.Epoch ||
                            frame.ObservedCodeAuthorityEpoch != fullJit.Authority.CodeEpoch)
                        { Thread.Yield(); continue; }
                        throw new InvalidOperationException("Full-JIT validated native entry rejected without a concurrent authority change.");
                    }
                    return true;
                case FullBlockExit.Continue:
                    if (frame.Executed == 0) throw new InvalidOperationException("Full-JIT native block made no progress.");
                    if (!signalActor && worker.Stop.Reason == HostExecutionStopReason.None && (int)machine->attention == 0)
                        worker.FullLastExit = lastExit;
                    return true;
                default: throw new InvalidOperationException("Unknown Full-JIT native exit.");
            }
        }
        }
        finally { AbandonIssuedRep(ref issuedRep, &repState); }
    }

    private void ExecuteFullHelper(Worker worker, FullTranslationJit.Plan plan)
    {
        // Check actual consumption, including cached payloads. Ordinary scalar,
        // fault and syscall helpers no longer trip a blanket legacy-state guard.
        // No x87/MMX or unclassified helper is authorized by that relaxation.
        if (worker.FullNativeState != null && worker.FullNativeState.X87Pointer->ProfileActive != 0)
        {
            var policy = FullCanonicalX87HelperPolicy.Classify(plan.Decoded.op.rde);
            if (policy == FullCanonicalX87HelperKind.NativeOnlyState)
                throw new InvalidOperationException("Active canonical x87/MMX cannot execute a managed computation helper.");
            if (policy == FullCanonicalX87HelperKind.Unknown &&
                plan.Helper != program!.BlinkFullJitSelect(0x10bUL << 40, 0, 0))
                throw new InvalidOperationException("Unknown helper has no canonical-state closure proof.");
            // The exact selected OpUd pointer is a named synchronous fault
            // boundary, not permission to compute an unknown ordinary opcode.
        }
        worker.FullLastExit = default;
        using var allocationScope = fullJit!.Allocations.Enter(worker.FullCache.Worker, FullAllocationMeter.Category.Helper);
        // CPU semantics, including raw reads and final CommitStash, participate
        // in atomic exclusion. Syscalls use bounded actual-copy scopes instead.
        using var semantic = plan.Operation == 0x105 ? default :
            fullJit.Authority.EnterCpuSemanticRead(worker.FullCache.Worker);
        var machine = worker.Machine;
        ulong ip = plan.Ip;
        bool returned = false;
        ThreadedSyscallObservation? observation = null;
        int firstPendingStart = worker.PendingStarts.Count;
        Blink.XedDecodedInst* previousDecoded = machine->xedd;
        bool previousHelper = worker.FullHelperActive, previousBorrow = worker.FullDataBorrow;
        bool previousAuthority = worker.FullAuthorityBorrow, previousMapping = worker.FullMappingBorrow;
        ++worker.FullMappingScopeDepth; // includes syscall 0x105; data-borrow policy is independent
        try
        {
            worker.FullHelperActive = plan.Operation != 0x105;
            var decoded = plan.Decoded;
            machine->xedd = &decoded;
            if (trace != null && plan.Operation == 0x105)
                observation = new(worker.GuestId, ip, Register(machine, 0), Register(machine, 7), Register(machine, 6),
                    Register(machine, 2), Register(machine, 10), Register(machine, 8), Register(machine, 9), null);
            fullJit!.BeginHelper(plan.Operation);
            plan.Helper(program!, machine, decoded.op.rde, decoded.op.disp, decoded.op.uimm0);
            if (machine->stashaddr != 0) program!.CommitStash(machine);
            machine->oplen = 0;
            returned = true;
            fullJit.CompleteHelper(plan.Operation);
            ++worker.Instructions; Interlocked.Increment(ref completed);
        }
        catch (Blink.Libc.JumpBufferException jump)
        {
            worker.FirstTrap ??= $"ip=0x{ip:x} address=0x{machine->faultaddr:x} code={jump.Value}";
            throw;
        }
        finally
        {
            worker.FullHelperActive = previousHelper;
            --worker.FullMappingScopeDepth;
            try
            {
                try
                {
                    if (!previousMapping && worker.FullMappingBorrow)
                    { worker.FullMappingBorrow = false; fullJit!.Authority.EndOwnedMappingMutation(worker.FullCache.Worker); }
                    FullControllerHelperMappingEnded(machine);
                }
                finally
                {
                    try
                    {
                        if (!previousAuthority && worker.FullAuthorityBorrow)
                        { worker.FullAuthorityBorrow = false; fullJit!.Authority.EndBorrow(worker.FullCache.Worker); }
                    }
                    finally
                    {
                        if (!previousBorrow && worker.FullDataBorrow)
                        { worker.FullDataBorrow = false; fullJit!.EndRead(program!); }
                    }
                }
            }
            finally
            {
                machine->xedd = previousDecoded;
                for (int index = worker.PendingStarts.Count - 1; index >= firstPendingStart; --index)
                {
                    Worker child = worker.PendingStarts[index];
                    worker.PendingStarts.RemoveAt(index); child.SpawnReady.Set();
                    lock (gate) child.StartPublished = true;
                }
                if (observation != null)
                {
                    var item = observation with { ReturnValue = returned ? Register(machine, 0) : null };
                    lock (gate) trace!(item);
                }
            }
        }
    }
}
