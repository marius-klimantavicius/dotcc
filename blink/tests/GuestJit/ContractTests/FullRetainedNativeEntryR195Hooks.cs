using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class RetainedTranslationStore
{
    // Never call GetStatistics under fast native enrollment: it takes Store.gate.
    internal int TestNativeEpochs195 => Volatile.Read(ref activeNative);
}

internal sealed unsafe partial class FullTranslationJit
{
    internal void TestCloseCatalog195()
    { lock (Authority.Gate) { x87KernelClosing = true; x87Kernels!.CloseAdmission(); } }
    internal void TestDisposeCatalog195() => x87Kernels!.Dispose();
    internal bool TestCatalogOpen195
    { get { lock (Authority.Gate) return x87Kernels!.AdmissionOpen; } }
}

public sealed unsafe partial class ThreadedGuestExecution
{
    private Action<nint, bool>? testRetainedPhase195;
    private Action<nint>? testRetainedAccepted195, testRetainedReturned195;
    private int testRetainedBorrowed195, testRetainedEnrolled195, testRetainedEntries195, testRetainedReturns195;
    partial void RejectRetainedNativeEntry(ref bool reject)
    {
        // Existing callbacks were authored for the Gate path; in particular
        // many acquire locks/wait. Keep their previous contract exactly.
        reject |= testFullNativePrepared != null || testFullNativeReturned != null ||
            current.Value?.FullNativeState?.TestMappingRetagged != null;
    }
    partial void RetainedNativeEntryPrepared(FullBlockFrame* frame, bool enrolled)
    {
        if (enrolled) ++testRetainedEnrolled195; else ++testRetainedBorrowed195;
        testRetainedPhase195?.Invoke((nint)frame, enrolled);
    }
    partial void RetainedNativeEntryAccepted(FullBlockFrame* frame)
    { ++testRetainedEntries195; testRetainedAccepted195?.Invoke((nint)frame); }
    partial void RetainedNativeEntryReturned(FullBlockFrame* frame)
    {
        ++testRetainedReturns195;
        var worker = current.Value!;
        if (Monitor.IsEntered(fullJit!.Authority.Gate) || fullJit.Authority.FullTestR185NativeFrames <= 0 ||
            fullJit.Store.TestNativeEpochs195 <= 0 || worker.X87NativeInFlight ||
            frame->Completed == null || frame->Issued == null)
            throw new InvalidOperationException("R195 fast RET lost enrollment/epoch/private completion/x87 return ordering.");
        fixed (long* global = &completed)
            if (frame->Completed == global) throw new InvalidOperationException("R195 fast RET used global completion pointer.");
        testRetainedReturned195?.Invoke((nint)frame);
    }

    internal sealed partial class FullControllerFixture
    {
        internal (int Borrowed, int Enrolled, int Entered, int Returned) Entry195Counts =>
            (Owner.testRetainedBorrowed195, Owner.testRetainedEnrolled195, Owner.testRetainedEntries195, Owner.testRetainedReturns195);
        internal Action<nint, bool>? Entry195Phase { set => Owner.testRetainedPhase195 = value; }
        internal Action<nint>? Entry195Accepted { set => Owner.testRetainedAccepted195 = value; }
        internal Action<nint>? Entry195Returned { set => Owner.testRetainedReturned195 = value; }
        internal void Entry195Ready()
        {
            var worker = testWorkers[0];
            worker.NativePoll ??= worker.Stop.AcquireNativePoll();
            lock (Jit.Authority.Gate)
                Require(worker.FullNativeState!.PrepareMappingCache(Jit.Authority, Jit.Authority.Epoch),
                    "R195 checked current worker provenance");
        }
        internal void Entry195Predecessor(TranslationToken token, ulong pc)
        {
            // Pending VALUE candidate: CompleteLastExitLink must still run;
            // intentionally stale fields exercise its independent rejection.
            testWorkers[0].FullLastExit = new(token, Jit.Store.Generation, Jit.Store.OwnerCookie,
                FullTranslationJit.Context(Program, Machine()), pc, pc + 5, pc,
                1, Jit.Authority.CodeEpoch, testWorkers[0].Instructions, 1, 1);
        }
        internal bool Entry195DirectAttempt()
        {
            Select(0); var worker = testWorkers[0];
            FullRepState rep = default;
            bool accepted = Owner.TryPrepareRetainedNativeEntry(worker, true, &rep,
                out var lease, out var epoch, out _, out _);
            try { return accepted; }
            finally
            {
                if (accepted) Jit.Authority.ExitNative();
                try { epoch.Dispose(); } finally { lease.Dispose(); }
            }
        }
    }
}
