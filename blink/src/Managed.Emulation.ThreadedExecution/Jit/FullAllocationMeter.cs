namespace Managed.Emulation.Execution;

// Per-worker exclusive allocation categories. A nested compilation/publication
// temporarily suspends its helper category; recursive guest handlers therefore
// do not count the same CLR bytes twice. Setup and caller-side diagnostics have
// separate explicit windows. This measures managed host allocation, not guest
// malloc, reserved native pages, or every allocation in unrelated runtime code.
internal sealed class FullAllocationMeter
{
    internal enum Category { None, Helper, Compilation, Publication }
    private struct Worker { public Category Current; public long Started; }
    private readonly Worker[] workers;
    private readonly long[] totals = new long[4];
    internal long Charge => 256 + workers.LongLength * 16 + totals.LongLength * sizeof(long);
    internal FullAllocationMeter(int count) => workers = new Worker[count];
    internal long Total(Category category) => Volatile.Read(ref totals[(int)category]);
    internal Scope Enter(int worker, Category category)
    {
        ref var state = ref workers[worker];
        long now = GC.GetAllocatedBytesForCurrentThread();
        if (state.Current != Category.None) Interlocked.Add(ref totals[(int)state.Current], now - state.Started);
        Category previous = state.Current;
        state.Current = category; state.Started = now;
        return new(this, worker, previous);
    }
    private void Leave(int worker, Category previous)
    {
        ref var state = ref workers[worker];
        long now = GC.GetAllocatedBytesForCurrentThread();
        Interlocked.Add(ref totals[(int)state.Current], now - state.Started);
        state.Current = previous; state.Started = now;
    }
    internal readonly struct Scope : IDisposable
    {
        private readonly FullAllocationMeter owner;
        private readonly int worker;
        private readonly Category previous;
        internal Scope(FullAllocationMeter owner, int worker, Category previous)
        { this.owner = owner; this.worker = worker; this.previous = previous; }
        public void Dispose() => owner.Leave(worker, previous);
    }
}
