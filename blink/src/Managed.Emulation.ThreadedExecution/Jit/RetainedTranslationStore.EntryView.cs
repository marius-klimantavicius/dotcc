namespace Managed.Emulation.Execution;

// This is a borrowed view, NOT another capability. The associated real
// TranslationLease must remain active for every use. A ref struct prevents
// storage on a Worker or any heap object; never use it after lease.Dispose().
internal readonly unsafe ref struct TranslationLeaseView
{
    internal readonly TranslationBlockHeader* Header;
    internal readonly ulong* GenerationPointer;
    internal readonly ulong Generation;
    internal TranslationLeaseView(TranslationBlockHeader* header, ulong* generation, ulong observed)
    { Header = header; GenerationPointer = generation; Generation = observed; }
}

internal sealed unsafe partial class RetainedTranslationStore
{
    // Authenticate BEFORE returning an address. In particular, an old hint
    // must never be dereferenced merely because the new store generation is
    // available. Even failed/non-live candidates acquire no speculative pointer.
    internal bool TryAcquireEntryView(scoped ref TranslationWorkerCache worker,
        TranslationToken resume, TranslationKey key, out TranslationLease lease,
        out TranslationLeaseView view, out bool resumed, out bool rangeRejected,
        out TranslationStoreFailure failure)
    {
        lock (gate)
        {
            ThrowDisposed(); ValidateWorker(in worker);
            view = default; resumed = false; rangeRejected = false;
            Body* body = null;
            if (resume.Generation != 0)
            {
                ++lookups; ++tokenLookups;
                body = TokenBody(resume, Resident);
                if (body != null)
                {
                    rangeRejected = unchecked(key.Pc - body->Header.GuestPc) >= body->Header.GuestLength;
                    if (rangeRejected || body->Header.Context != key.Context) body = null;
                    else resumed = true;
                }
            }
            int hintIndex = -1;
            if (body == null)
            {
                ++lookups; ++workerLookups;
                hintIndex = worker.Worker * options.HintsPerWorker +
                    (int)(Hash(key) & (uint)(options.HintsPerWorker - 1));
                body = TokenBody(hints[hintIndex], Resident);
                if (body != null && body->Header.GuestPc == key.Pc && body->Header.Context == key.Context) ++hintHits;
                else body = Find(key);
            }
            if (!AcquireCore(body, out lease, out failure, requireLive: false)) return false;
            if (hintIndex >= 0) hints[hintIndex] = lease.Token;
            // activeManaged/body.Leases now exclude flush, arena trim and
            // owner disposal. Retirement cannot reclaim this header/payload.
            view = new(&body->Header, generationWord, Volatile.Read(ref *generationWord));
            return true;
        }
    }

    internal TranslationLeaseView GetLeaseView(scoped in TranslationLease lease)
    {
        lock (gate)
        {
            ValidateLease(in lease);
            return new(&records[lease.Token.Slot]->Header, generationWord, Volatile.Read(ref *generationWord));
        }
    }
}
