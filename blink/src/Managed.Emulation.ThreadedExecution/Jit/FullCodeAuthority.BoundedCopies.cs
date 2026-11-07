namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    internal void RequireBoundedGuestWrite(int worker)
    {
        if (!Monitor.IsEntered(Gate) || (uint)worker >= (uint)readers.Length ||
            readers[worker].WriteState < 2 || readers[worker].ShortDepth <= 0 ||
            !SemanticAdmitted(worker) || CopyWriters <= 0)
            throw new InvalidOperationException("Bounded physical copy lacks actual writer admission.");
    }

    internal void RecordBoundedCopyCodeClassification(nint first, nint second)
    {
        if (!Monitor.IsEntered(Gate)) throw new InvalidOperationException("Copy classification requires authority gate.");
        // Preserve the existing registry-role diagnostics. An exact physical
        // range can safely use targeted revocation even when its code/data role
        // is absent from this registry; unknown is not permission to skip it.
        bool unknown = saturated, code = false;
        ClassifyWritePage(first, ref unknown, ref code);
        if (second != 0 && second != first) ClassifyWritePage(second, ref unknown, ref code);
        if (unknown)
        {
            // Retain exact physical code invalidation, but unknown registry
            // provenance cannot carry managed data hints across a role delta.
            Interlocked.Increment(ref unknownCodeWriteScopes); Advance();
        }
        else if (code) Interlocked.Increment(ref knownCodeWriteScopes);
    }
}
