using System.Runtime.CompilerServices;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

// Synchronous worker-thread lease. The generated Globals byte array is pinned
// (BlinkCore.cs __DotCcContext.Values); __DotCcRetain prevents context disposal.
// This uses the EXACT existing generated Rdrand and Libc mutex registry, so
// selected interpreted handlers and native workers share state and exclusion.
internal sealed unsafe class FullProcessorEnvironmentLease : IDisposable
{
    private static long nextCookie;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private Blink? program;
    private FullCodeAuthority? authority;
    private IDisposable? retained;
    private Blink.Libc.RuntimeBinding binding;
    private FullCodeAuthority.SemanticReadScope admission;
    private int* mutex;
    internal ulong* State { get; private set; }
    internal uint* Count { get; private set; }
    internal ulong Cookie { get; private set; }
    internal bool Sampled { get; private set; }
    internal bool IsHeld => program != null;
    internal bool HasRng => mutex != null;
    internal bool GateHeld => authority != null && Monitor.IsEntered(authority.Gate);

    private FullProcessorEnvironmentLease() { }

    // Call outside Gate and before native/store enrollment. Try admission comes
    // BEFORE runtime/registry/mutex locks. A pending incoming ticket declines
    // without touching generated globals. No blocking global lock is used.
    internal static bool TryAcquire(Blink p, FullCodeAuthority authority, int worker,
        bool rng, out FullProcessorEnvironmentLease? result)
    {
        result = null;
        if (Monitor.IsEntered(authority.Gate)) throw new InvalidOperationException("ENV1 lease under Gate");
        if (!authority.TryEnterProcessorEnvironmentRead(worker, out var admitted)) return false;
        FullProcessorEnvironmentLease? lease = null;
        try
        {
            lease = new FullProcessorEnvironmentLease { admission = admitted, authority = authority };
            lease.retained = p.__DotCcRetain();
            lease.binding = p.__DotCcEnter();
            if (rng)
            {
                var g = (Blink.Rdrand*)Unsafe.AsPointer(ref p.Globals.g_rdrand__unit_2D5AE066DBCACEBBF21F0F23C80E9C4B510F23E759CD47D8B553E5446EE49B6C);
                int error = Blink.Libc.pthread_mutex_trylock(&g->@lock);
                if (error == Blink.Libc.EBUSY) return false;
                if (error != 0) throw new InvalidOperationException("ENV1 actual RNG mutex unavailable");
                lease.mutex = &g->@lock;
                lease.State = &g->state; lease.Count = &g->count;
            }
            long cookie = Interlocked.Increment(ref nextCookie);
            if (cookie <= 0) throw new InvalidOperationException("ENV1 lease cookie exhausted");
            lease.Cookie = (ulong)cookie; lease.program = p;
            result = lease; return true;
        }
        finally
        {
            if (result == null)
            {
                if (lease != null) lease.Dispose();
                else admitted.Dispose();
            }
        }
    }

    internal void MarkSampled()
    {
        if (!IsHeld || Sampled || thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("ENV1 sampling lease invalid");
        Sampled = true;
    }

    // The parent MUST release before any retry/yield/wait. On normal sampled
    // completion call RequireConsumed BEFORE this method, after native RET.
    public void Dispose()
    {
        if (thread != Environment.CurrentManagedThreadId) throw new InvalidOperationException("ENV1 lease moved worker");
        try
        {
            if (mutex != null)
            {
                int* held = mutex; mutex = null;
                if (Blink.Libc.pthread_mutex_unlock(held) != 0)
                    throw new InvalidOperationException("ENV1 RNG mutex release failed");
            }
        }
        finally
        {
            program = null; authority = null; State = null; Count = null;
            try { binding.Dispose(); }
            finally
            {
                var lifetime = retained; retained = null;
                try { lifetime?.Dispose(); }
                finally { admission.Dispose(); }
            }
        }
    }
}
