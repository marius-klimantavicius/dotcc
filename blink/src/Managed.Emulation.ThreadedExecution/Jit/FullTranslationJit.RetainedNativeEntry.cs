using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    internal bool TryBorrowRetainedNativeEntry(Blink program, Blink.Machine* machine,
        scoped ref TranslationWorkerCache worker, TranslationToken resume, uint resumeIndex,
        out TranslationLease lease, out TranslationLeaseView view, out uint index)
    {
        lease = default; view = default; index = 0;
        // Only the synchronous compiler of a worker can designate its id. Do
        // not cancel even our own pending publication on this path. A foreign
        // publisher racing this observation is excluded by the store epoch.
        if (Volatile.Read(ref publicationWorker) != -1) return false;
        return TryAcquireRetainedEntry(program, machine, ref worker, resume, resumeIndex,
            out lease, out view, out index, gateFree: true);
    }

    // Re-read mutable trust only AFTER real store epoch + SC native enrollment.
    // A lease authenticated the exact key/range and instruction index before
    // any pointer escaped Store. Header/payload/entry identities are immutable
    // for that lease; revocation and code stamps remain volatile authorities.
    internal bool RetainedNativeHeaderCurrent(in TranslationLease lease,
        in TranslationLeaseView view, in FullBlockFrame frame)
    {
        var h = view.Header;
        return lease.Owner == Store && h == frame.Header && h->OwnerCookie == Store.OwnerCookie &&
            h->BodyGeneration == lease.Token.Generation && h->Slot == (uint)lease.Token.Slot &&
            h->Context == frame.Context && (h->Context & FlatLongContext) != 0 &&
            h->Entry != 0 &&
            Volatile.Read(ref h->Live) == 1 && Volatile.Read(ref h->Reserved136) == 1 &&
            Volatile.Read(ref h->CodeEpoch) != 0 &&
            Volatile.Read(ref h->CodeEpoch) == Volatile.Read(ref h->ValidatedCodeEpoch) &&
            Volatile.Read(ref h->ValidatedGlobalEpoch) == frame.ObservedCodeAuthorityEpoch &&
            Authority.CodeEpoch == frame.ObservedCodeAuthorityEpoch &&
            Authority.Epoch == frame.ObservedGlobalEpoch &&
            Volatile.Read(ref *view.GenerationPointer) == view.Generation &&
            RetainedX87KernelAdmissionOpen(in frame);
    }
}
