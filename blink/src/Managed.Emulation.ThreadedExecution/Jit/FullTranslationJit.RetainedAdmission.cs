using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Called with Authority.Gate and FullFetchActive, but no guest fetch has
    // happened yet. Opening a passive physical read is necessary only for the
    // checked path. forceChecked covers signal and pending-memory interactions.
    // The caller owns fetchBorrow and releases it in its existing outer finally.
    internal bool AcquireNativeEntry(Blink program, Blink.Machine* machine,
        scoped ref TranslationWorkerCache worker, TranslationToken resume, uint resumeIndex,
        bool forceChecked, scoped ref bool fetchBorrow, out TranslationLease lease,
        out TranslationLeaseView view, out uint index, out bool multiple,
        out bool retainedAdmission, out int error, uint requestedCount = 0)
    {
        if (!Monitor.IsEntered(Authority.Gate))
            throw new InvalidOperationException("Retained native admission requires authority gate.");
        lease = default; view = default; index = 0; multiple = false;
        retainedAdmission = false; error = 0;
        // Return NO capability on a foreign denial. Returning multiple=true
        // here would still allow the legacy unenrolled cap1 fallback. Do not
        // open even a passive read, clear trust, or validate bytes on this path.
        if (!forceChecked && requestedCount == 0 &&
            (publicationWorker < 0 || publicationWorker == worker.Worker) &&
            Authority.ShouldDeferTrustedSource(worker.Worker) &&
            TryAcquireRetainedEntry(program, machine, ref worker, resume, resumeIndex,
                out lease, out view, out index, structuralOnly: true))
        {
            lease.Dispose(); lease = default; view = default; index = 0;
            RecordRetainedValidationDefer();
            return false;
        }
        if (!forceChecked && requestedCount == 0 &&
            (publicationWorker < 0 || publicationWorker == worker.Worker) &&
            Authority.CanReuseRetainedEligibility && Authority.SemanticReadersDrained() &&
            TryAcquireRetainedEntry(program, machine, ref worker, resume, resumeIndex,
                out lease, out view, out index))
        {
            multiple = retainedAdmission = true;
            ++retainedEligibilityHits; ++trustedAcquisitions;
            CancelPublication(worker.Worker);
            return true;
        }
        // A semantic reader can require R183's pointer-free cold request
        // staging. Keep that interaction on the existing checked path rather
        // than repeatedly trying an enrollment it cannot yet obtain.
        // No native/store epoch or mapping lock is held across any wait. This
        // BeginRead is passive and non-waiting with respect to guest writers.
        // The existing checked Acquire retains all demand-fetch/fault rules.
        if (!fetchBorrow) { BeginRead(program, semantic: false); fetchBorrow = true; }
        if (!Acquire(program, machine, ref worker, resume, resumeIndex, out lease,
            out index, out multiple, out error, requestedCount)) return false;
        view = Store.GetLeaseView(in lease);
        return true;
    }

    private bool TryAcquireRetainedEntry(Blink program, Blink.Machine* machine,
        scoped ref TranslationWorkerCache worker, TranslationToken resume, uint resumeIndex,
        out TranslationLease lease, out TranslationLeaseView view, out uint index, bool gateFree = false,
        bool structuralOnly = false)
    {
        if (structuralOnly && (gateFree || !Monitor.IsEntered(Authority.Gate)))
            throw new InvalidOperationException("Source-only retained lookup requires authority gate.");
        var key = new TranslationKey(machine->ip, Context(program, machine));
        lease = default; view = default; index = 0;
        // At most two lookups: a token can name a range containing the current
        // IP but no instruction boundary. Its exact-key target is still valid.
        for (int attempt = 0; attempt != 2; ++attempt)
        {
            bool found = Store.TryAcquireEntryView(ref worker, resume, key, out lease, out view,
                out bool resumed, out bool rangeRejected, out var failure);
            // Existing diagnostic is Gate-owned; the new path adds no shared
            // counter mutation or allocation on a retained invocation.
            if (rangeRejected && !gateFree) ++resumeRangeRejects;
            if (!found)
            {
                if (failure != TranslationStoreFailure.NotFound)
                    throw new InvalidOperationException("Full-JIT retained entry lookup denied: " + failure);
                return false;
            }
            var h = view.Header[0];
            bool trusted = h.Context == key.Context && (h.Context & FlatLongContext) != 0 &&
                h.Live == 1 && h.Entry != 0 && h.CodeEpoch != 0 &&
                h.CodeEpoch == h.ValidatedCodeEpoch && h.ValidatedGlobalEpoch == Authority.CodeEpoch &&
                h.Reserved136 == 1 && (structuralOnly ||
                    (gateFree ? Authority.RetainedNativeEntryOpen : Authority.CanReuseRetainedEligibility)) &&
                machine->mode.omode == Blink.XED_MODE_LONG && machine->mode.genmode == Blink.XED_GEN_MODE_PROTECTED &&
                program.GetPc(machine) == (long)machine->ip;
            if (!trusted) { lease.Dispose(); view = default; return false; }
            if (!resumed) return true; // exact key is the body's first plan
            int count = checked((int)h.PayloadBytes) / sizeof(Plan);
            var plans = new ReadOnlySpan<Plan>((void*)h.Payload, count);
            if (resumeIndex < (uint)count && plans[(int)resumeIndex].Ip == machine->ip)
            { index = resumeIndex; return true; }
            for (int i = 0; i < count; ++i)
                if (plans[i].Ip == machine->ip) { index = (uint)i; return true; }
            lease.Dispose(); view = default; resume = default;
        }
        return false;
    }
}
