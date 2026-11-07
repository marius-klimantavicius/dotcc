namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    internal void RevokeBoundedCopy(int worker, nint first, nint second)
    {
        lock (Authority.Gate)
        {
            Authority.RequireBoundedGuestWrite(worker);
            if (first == 0 || ((nuint)first & 4095) != 0 || ((nuint)second & 4095) != 0)
                throw new InvalidOperationException("Bounded physical copy pages are invalid.");
            // BeginGuestWrite already closed enrollment, advanced the runtime
            // epoch and drained every native frame. Both pages are actual host
            // identities from a <=4096-byte copy, not inferred virtual keys.
            // MarkPhysicalAuthorityChanged revokes ALL unknown-provenance
            // records conservatively and the exact physical aliases of known
            // records, including drafts. RevokeCore changes Live/CodeEpoch and
            // clears incoming links before any guest byte may be written.
            Authority.RecordBoundedCopyCodeClassification(first, second);
            Store.MarkPhysicalAuthorityChanged((ulong)(nuint)first, 4096);
            if (second != 0 && second != first)
                Store.MarkPhysicalAuthorityChanged((ulong)(nuint)second, 4096);
            // No global code pulse: an unaffected body remains valid. A body
            // copied/compiled during this writer cannot acquire trust:
            // SnapshotCodeStamp returns zero for CopyWriters or a changed
            // content sequence, including a copy completed between its two
            // pending checks. EndGuestWrite publishes content completion before
            // reopening admission. Pure readers cannot dirty this proof.
            // General/unknown-range writers retain their existing global path.
        }
    }
}
