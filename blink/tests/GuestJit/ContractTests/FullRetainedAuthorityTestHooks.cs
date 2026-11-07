using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    internal readonly record struct EligibilityQueryCounts(long Calls, long PageIdentity, long Contains,
        long Available, long Shared, long Lease, long Owned);
    private EligibilityQueryCounts testEligibilityQueries;
    internal EligibilityQueryCounts FullTestEligibilityQueries => testEligibilityQueries;
    partial void ObserveEligibilityQuery(FullEligibilityQuery query)
    {
        testEligibilityQueries = query switch
        {
            FullEligibilityQuery.Entry => testEligibilityQueries with { Calls = testEligibilityQueries.Calls + 1 },
            FullEligibilityQuery.PageIdentity => testEligibilityQueries with { PageIdentity = testEligibilityQueries.PageIdentity + 1 },
            FullEligibilityQuery.Contains => testEligibilityQueries with { Contains = testEligibilityQueries.Contains + 1 },
            FullEligibilityQuery.Available => testEligibilityQueries with { Available = testEligibilityQueries.Available + 1 },
            FullEligibilityQuery.Shared => testEligibilityQueries with { Shared = testEligibilityQueries.Shared + 1 },
            FullEligibilityQuery.Lease => testEligibilityQueries with { Lease = testEligibilityQueries.Lease + 1 },
            FullEligibilityQuery.Owned => testEligibilityQueries with { Owned = testEligibilityQueries.Owned + 1 },
            _ => throw new InvalidOperationException("Unknown test eligibility query.")
        };
    }
}

internal static unsafe partial class JitPageIdentity
{
    [ThreadStatic] private static long testIdentityReads;
    internal static long FullTestIdentityReads => testIdentityReads;
    static partial void ObservePageIdentityRead() => ++testIdentityReads;
}

public sealed unsafe partial class ThreadedGuestExecution
{
    // Same passive/fetch/Gate envelope as FullControllerFixture.Acquire, with
    // the real worker's resume hint supplied explicitly. No authority is forged.
    internal void FullRetainedTestMapReadOnlyShared(int fd, ulong pc)
    {
        var worker = current.Value!;
        using var scope = new FullDataScope(this, worker);
        Require(program!.ReserveVirtual(system, (long)pc, 4096, program.SetProtection(5), fd, 0, true, true) == (long)pc,
            "R98 real regular shared RX mapping");
    }

    internal bool FullRetainedTestAcquire(TranslationToken resume, uint resumeIndex,
        out TranslationLease lease, out uint index, out bool multiple, out int error,
        uint requestedCount = 0)
    {
        var worker = current.Value!;
        bool previous = worker.FullFetchActive;
        fullJit!.BeginRead(program!, semantic: false);
        try
        {
            worker.FullFetchActive = true;
            lock (fullJit.Authority.Gate)
                return fullJit.Acquire(program!, worker.Machine, ref worker.FullCache,
                    resume, resumeIndex, out lease, out index, out multiple, out error, requestedCount);
        }
        finally { worker.FullFetchActive = previous; fullJit.EndRead(program!); }
    }
}
