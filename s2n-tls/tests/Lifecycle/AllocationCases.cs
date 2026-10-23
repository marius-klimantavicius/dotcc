using System.Runtime.InteropServices;
using Managed.Security.S2n;
using Raw = Managed.Security.S2n.S2nTls;

namespace ManagedTests;

// Deterministic host-allocation faults at the real public s2n memory callback
// boundary. Each case runs in its own process because callbacks are global.
internal static unsafe class AllocationCases
{
    private static int _fail;
    private static long _live;
    private static long _injectedFailures;
    private static int Init() => 0;
    private static int Cleanup() => 0;
    private static int Allocate(void** output, uint requested, uint* allocated)
    {
        *output = null; *allocated = 0;
        try
        {
            if (Interlocked.Exchange(ref _fail, 0) != 0)
            { Interlocked.Increment(ref _injectedFailures); *Raw.s2n_errno_location() = (int)Raw.s2n_error.S2N_ERR_ALLOC; return -1; }
            void* pointer = NativeMemory.Alloc(requested);
            if (pointer == null) { *Raw.s2n_errno_location() = (int)Raw.s2n_error.S2N_ERR_ALLOC; return -1; }
            *output = pointer; *allocated = requested; Interlocked.Increment(ref _live); return 0;
        }
        catch { *Raw.s2n_errno_location() = (int)Raw.s2n_error.S2N_ERR_ALLOC; return -1; }
    }
    private static int Free(void* pointer, uint size)
    {
        try { NativeMemory.Free(pointer); Interlocked.Decrement(ref _live); return 0; }
        catch { return -1; }
    }
    private static void Install()
    {
        if (Raw.s2n_mem_set_callbacks(&Init, &Cleanup, &Allocate, &Free) != 0)
            throw new InvalidOperationException("Could not install the real s2n memory callbacks before initialization.");
    }
    private static void ExpectAllocationFailure(Action operation)
    {
        Interlocked.Exchange(ref _fail, 1);
        long baseline = Interlocked.Read(ref _injectedFailures);
        try { operation(); }
        // Upstream pointer constructors may replace ALLOC with NULL while
        // propagating failure. Require proof the real allocator rejected once.
        catch (S2nException error) when (error.ErrorName is "S2N_ERR_ALLOC" or "S2N_ERR_NULL")
        {
            if (Interlocked.Read(ref _injectedFailures) != baseline + 1)
                throw new InvalidOperationException("Failure did not originate at the injected allocator.", error);
            return;
        }
        finally { Interlocked.Exchange(ref _fail, 0); }
        throw new InvalidOperationException("Expected a real translated allocation failure.");
    }
    internal static void Failures()
    {
        Install();
        using (var runtime = new S2nRuntime())
        {
            long baseline = Interlocked.Read(ref _live);
            ExpectAllocationFailure(() => { using var config = new S2nConfiguration(runtime); });
            if (Interlocked.Read(ref _live) != baseline) throw new InvalidOperationException("Failed config creation leaked s2n allocations.");
            using var config = new S2nConfiguration(runtime);
            baseline = Interlocked.Read(ref _live);
            var failedTransport = new PendingTransport();
            ExpectAllocationFailure(() => { using var connection = new S2nConnection(config, S2nRole.Server, failedTransport); });
            if (Interlocked.Read(ref _live) != baseline || failedTransport.Disposals != 1)
                throw new InvalidOperationException("Failed connection creation did not return its owners.");
            using var valid = new S2nConnection(config, S2nRole.Server, new PendingTransport());
        }
        if (Interlocked.Read(ref _live) != 0) throw new InvalidOperationException("s2n_cleanup_final left owned allocations live.");
    }
    internal static void InitializationFailure()
    {
        Install();
        ExpectAllocationFailure(() => { using var runtime = new S2nRuntime(); });
        try { using var again = new S2nRuntime(); }
        catch (InvalidOperationException error) when (error.Message.Contains("quarantined", StringComparison.Ordinal)) { return; }
        throw new InvalidOperationException("A partially initialized global domain was restarted.");
    }
}
