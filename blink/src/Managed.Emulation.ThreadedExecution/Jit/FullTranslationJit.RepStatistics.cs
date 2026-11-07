namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Updated at native returns only, never on every REP iteration. Saturating
    // diagnostics cannot turn a valid very long guest operation into a fault.
    private long repIterations, repCompletions;
    private int repIterationCounterSaturated;
    internal void RecordRepProgress(ulong completedIterationsDelta, bool wholeCompletion)
    {
        while (true)
        {
            long previous = Volatile.Read(ref repIterations);
            bool overflow = completedIterationsDelta > (ulong)(long.MaxValue - previous);
            long next = overflow ? long.MaxValue : previous + (long)completedIterationsDelta;
            if (Interlocked.CompareExchange(ref repIterations, next, previous) != previous) continue;
            if (overflow) Volatile.Write(ref repIterationCounterSaturated, 1);
            break;
        }
        if (wholeCompletion)
        {
            while (true)
            {
                long previous = Volatile.Read(ref repCompletions);
                if (previous == long.MaxValue || Interlocked.CompareExchange(ref repCompletions, previous + 1, previous) == previous) break;
            }
        }
    }
}
