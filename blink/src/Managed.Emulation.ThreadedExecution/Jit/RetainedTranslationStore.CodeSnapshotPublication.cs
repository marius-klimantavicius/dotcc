namespace Managed.Emulation.Execution;

internal unsafe sealed partial class RetainedTranslationStore
{
    // Caller owns Authority.Gate. This takes ONLY the store gate and reads
    // scalars, never IO or a second authority gate. Physical retirement may run
    // under producer locks and skip drafts: sampling and Draft->Resident must
    // therefore be indivisible with respect to its store revocation.
    internal bool PublishCodeSnapshot(ref TranslationDraft draft, int codeBytes, int payloadBytes,
        FullCodeAuthority authority, in FullCodeSnapshot snapshot, out TranslationStoreFailure failure)
    {
        lock (gate)
        {
            ulong stamp = authority.CodeSnapshotCurrent(in snapshot) ? snapshot.CodeEpoch : 0;
            return Publish(ref draft, codeBytes, payloadBytes, 1, stamp, out failure);
        }
    }
}
