namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Erased in product builds. Legacy NativePrepared callbacks require Gate
    // and may block; installed legacy hooks must decline before borrowing.
    partial void RejectRetainedNativeEntry(ref bool reject);
    // Explicit fast phases for contract tests. After enrolled=true, callbacks
    // MUST NOT wait, acquire Gate/store/physical scope, or follow guest data.
    partial void RetainedNativeEntryPrepared(FullBlockFrame* frame, bool enrolled);
    partial void RetainedNativeEntryAccepted(FullBlockFrame* frame);
    partial void RetainedNativeEntryReturned(FullBlockFrame* frame);

    private bool TryPrepareRetainedNativeEntry(Worker worker, bool preissued, FullRepState* rep,
        out TranslationLease lease, out NativeEpochLease epoch, out FullBlockFrame frame, out nint entry)
    {
        lease = default; epoch = default; frame = default; entry = 0;
        bool reject = false;
        RejectRetainedNativeEntry(ref reject);
        if (reject) return false;
        var jit = fullJit!;
        bool enrolled = false, accepted = false;
        try
        {
            if (!jit.TryBorrowRetainedNativeEntry(program!, worker.Machine, ref worker.FullCache,
                worker.FullResume, worker.FullResumeIndex, out lease, out var view, out uint index)) return false;
            var native = worker.FullNativeState!;
            ulong runtime = jit.Authority.Epoch;
            if (!native.RetainedMappingCacheCurrent(jit.Authority, runtime)) return false;
            var header = view.Header[0];
            var machine = worker.Machine;
            worker.NativePoll ??= worker.Stop.AcquireNativePoll(); // outside enrollment
            frame = new() { Version = FullBlockFrame.AbiVersion, Size = (uint)sizeof(FullBlockFrame),
                Machine = machine, Stop = worker.NativePoll.Pointer, Budget = instructionBudget,
                NativeLimit = 0, NextIp = machine->ip, Header = view.Header,
                OwnerCookie = header.OwnerCookie, BodyGeneration = header.BodyGeneration,
                Context = header.Context, GlobalEpochPointer = jit.Authority.EpochPointer,
                ObservedGlobalEpoch = runtime, CodeAuthorityEpochPointer = jit.Authority.CodeEpochPointer,
                ObservedCodeAuthorityEpoch = header.ValidatedGlobalEpoch, ResumeIndex = index,
                HelperIndex = FullBlockFrame.NoHelperIndex, FirstPreissued = preissued ? 1u : 0,
                MemoryTlb = native.Tlb, MemoryTlbMask = FullNativeMemory.TlbCount - 1,
                JumpCache = native.Jumps, JumpCacheMask = FullNativeMemory.JumpCount - 1,
                StoreGenerationPointer = view.GenerationPointer, ObservedStoreGeneration = view.Generation,
                FsBase = machine->fs.@base, GsBase = machine->gs.@base,
                StackMemory = native.StackMemory, Rep = rep, X87 = GetX87NativePointer(worker),
                OwnerHandle = unchecked((nint)worker.FullCache.Serial) };
            PrepareRepLatch(worker, rep);
            *frame.StackMemory = new() { Version = FullStackMemoryDescriptor.CurrentVersion };
            // BEFORE epoch/enrollment: may inject source-only race setup.
            fixed (FullBlockFrame* prepared = &frame) RetainedNativeEntryPrepared(prepared, false);
            if (!jit.Store.TryBeginNativeEpochNonblocking(out epoch)) return false;
            enrolled = jit.Authority.TryEnterNative(runtime);
            if (!enrolled) return false;
            // No blocking lock, physical read, fetch, walk, compile or wait
            // from this point through RET and the shared ExitNative finally.
            fixed (FullBlockFrame* prepared = &frame) RetainedNativeEntryPrepared(prepared, true);
            ValidateOwnedX87KernelFrame(worker, in frame);
            if (!native.RetainedMappingCacheCurrent(jit.Authority, runtime) ||
                !jit.RetainedNativeHeaderCurrent(in lease, in view, in frame)) return false;
            FullNativeMemory.PublishJump(frame.JumpCache, frame.JumpCacheMask,
                view.Generation, view.Header, in header);
            entry = header.Entry;
            fixed (FullBlockFrame* prepared = &frame) RetainedNativeEntryAccepted(prepared);
            accepted = true;
            return true; // caller owns enrollment + epoch + root lease
        }
        finally
        {
            if (!accepted)
            {
                // Always drop enrollment BEFORE any store finalizer or old
                // Gate path. Retry cannot carry any stale header or capability.
                if (enrolled) jit.Authority.ExitNative();
                try { epoch.Dispose(); }
                finally { lease.Dispose(); epoch = default; lease = default; frame = default; entry = 0; }
            }
        }
    }
}
