namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Nine Gate-owned causal counters, explicitly charged outside the existing
    // owner allowance. No new per-instruction atomic/global counter operation.
    internal const long RetainedCostCharge = 9L * sizeof(long);
    private long retainedValidationDefers, zeroStampRestamps, dirtyHeaderRestamps, globalStampRestamps;
    private long ticketReadPreSkips, ticketReadPostSkips;
    private long ticketWritePreRevocations, ticketWritePostRevocations, ticketWriteRevokedBodies;
    partial void RetainedValidationDeferred(); // test schedule only; erased in production

    private void RecordRetainedValidationDefer()
    {
        ++retainedValidationDefers;
        RetainedValidationDeferred();
    }

    internal void TicketSourceEdge(nint page0, nint page1, bool writesContent, bool post)
    {
        if (!Monitor.IsEntered(Authority.Gate))
            throw new InvalidOperationException("Ticket source revocation requires authority gate.");
        if (!writesContent)
        {
            if (post) ++ticketReadPostSkips; else ++ticketReadPreSkips;
            return;
        }
        if (post) ++ticketWritePostRevocations; else ++ticketWritePreRevocations;
        ticketWriteRevokedBodies += Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page0, 4096);
        if (page1 != 0 && page1 != page0)
            ticketWriteRevokedBodies += Store.MarkPhysicalAuthorityChanged((ulong)(nuint)page1, 4096);
    }
}
