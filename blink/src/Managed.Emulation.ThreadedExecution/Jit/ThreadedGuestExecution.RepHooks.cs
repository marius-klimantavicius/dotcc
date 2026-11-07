namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Parent REP controller seam, erased from production without a test
    // implementation. Call AFTER all native/ticket/store/fetch/passive cleanup,
    // before a mapping assist/reentry, with only the scope-local state/value
    // identity and independent kernel code-lifetime capability still retained.
    partial void FullControllerRepPaused(FullRepState* state, FullRepReason reason);
}
