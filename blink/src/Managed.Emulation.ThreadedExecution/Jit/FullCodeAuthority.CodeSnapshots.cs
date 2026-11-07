namespace Managed.Emulation.Execution;

// A coherence witness for bytes/eligibility, never an execution permit. Keep
// the code/global/header checks; targeted R190 writes need the additional
// content proof even when neither global code nor mapping versions change.
internal readonly record struct FullCodeSnapshot(ulong CodeEpoch, ulong MappingEpoch,
    ulong ContentSequence, bool CopyPending, ulong RuntimeEpoch, bool IndependentContent, ulong MappingSequence, bool MappingPending);

internal sealed unsafe partial class FullCodeAuthority
{
    // Includes the ticket cycle flag and conservative managed field padding.
    internal const long ContentSequenceCharge = 16;
    private bool nativeTicketWritesContent;
    private ulong contentSequence = 1;
    // SC read pairs with every begin/end CAS. No parity assumption: overlapping
    // and nested copies each advance the sequence, with CopyWriters supplying
    // the pending test. Not an ABI word, pointer, counter, or per-opcode hook.
    internal ulong ContentSequence => Interlocked.CompareExchange(ref contentSequence, 0, 0);
    private bool TryAdvanceContentSequence()
    {
        while (true)
        {
            ulong previous = ContentSequence;
            if (previous == ulong.MaxValue)
            { Volatile.Write(ref epochExhausted, 1); return false; }
            if (Interlocked.CompareExchange(ref contentSequence, previous + 1, previous) == previous) return true;
        }
    }
    private void AdvanceContentSequence()
    {
        if (!TryAdvanceContentSequence())
            throw new InvalidOperationException("Full-JIT content sequence exhausted.");
    }
    // Nonthrowing release, including failed writes and exhaustion. Existing
    // native runtime pulses/drains remain owned by their original callers.
    private void CompleteContentMutation() => TryAdvanceContentSequence();

    // Only the audited flat-long, non-metal callers opt out of the original
    // runtime fallback. Pending tickets do not block their own source snapshot:
    // source pre/post revocation plus content completion prevent later reuse.
    internal FullCodeSnapshot CaptureCodeSnapshot(bool independentContent = false)
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Code snapshot requires authority gate.");
        ulong mapping = MappingSequence;
        bool mappingPending = MappingMutations != 0;
        ulong content = ContentSequence;
        bool pending = CopyWriters != 0;
        return new(CodeEpoch, MappingAuthorityEpoch, content, pending, Epoch, independentContent, mapping, mappingPending);
    }
    internal bool CodeSnapshotCurrent(in FullCodeSnapshot snapshot)
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Code snapshot requires authority gate.");
        return !snapshot.MappingPending && MappingMutations == 0 && MappingSequence == snapshot.MappingSequence &&
            !snapshot.CopyPending && CopyWriters == 0 && !SemanticQuarantined &&
            snapshot.CodeEpoch != 0 && CodeEpoch == snapshot.CodeEpoch &&
            MappingAuthorityEpoch == snapshot.MappingEpoch && ContentSequence == snapshot.ContentSequence &&
            (snapshot.IndependentContent || Epoch == snapshot.RuntimeEpoch);
    }
}
