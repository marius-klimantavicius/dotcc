using System.Runtime.InteropServices;
using Runtime = Managed.Zstd.Native.Zstd.Libc;
using VM = Managed.Zstd.Native.Zstd;

namespace Managed.Zstd;

// Generated static C tables have assembly lifetime. A private, permanent runtime
// roots them and gives every allocation/free the same owner, including finalizers.
// This is one program with independent zstd contexts, not program-state isolation.
internal static class ZstdRuntime
{
    internal static readonly Runtime.RuntimeContext Owner = new();
    internal static Runtime.RuntimeBinding Enter() => Owner.Enter();
}

internal sealed unsafe class ContextHandle : SafeHandle
{
    private readonly bool compression;
    private readonly Runtime.RuntimeContext owner;
    private IDisposable? lease;
    internal static int ActiveHandles;
    internal static int ReleaseFailures;
    public override bool IsInvalid => handle == IntPtr.Zero;
    internal ContextHandle(bool compression) : base(IntPtr.Zero, ownsHandle: true)
    {
        this.compression = compression;
        owner = ZstdRuntime.Owner;
        lease = owner.RetainLease();
        try
        {
            using var binding = owner.Enter();
            SetHandle((nint)(compression ? VM.dz_create_cctx() : VM.dz_create_dctx()));
            if (IsInvalid) throw new OutOfMemoryException("zstd context allocation failed.");
            Interlocked.Increment(ref ActiveHandles);
        }
        catch { lease.Dispose(); lease = null; throw; }
    }
    internal Operation Enter()
    {
        ObjectDisposedException.ThrowIf(IsClosed || IsInvalid, this);
        return new Operation(this);
    }
    // A raw pointer passed to translated managed code does not itself root the
    // SafeHandle. Retain a dangerous reference until the complete call returns,
    // including GCs triggered by translated allocations on another thread.
    internal struct Operation : IDisposable
    {
        private ContextHandle? context;
        private Runtime.RuntimeBinding binding;
        internal Operation(ContextHandle context)
        {
            this.context = null;
            binding = default;
            bool retained = false;
            try
            {
                context.DangerousAddRef(ref retained);
                binding = context.owner.Enter();
                this.context = context;
            }
            catch { if (retained) context.DangerousRelease(); throw; }
        }
        public void Dispose()
        {
            if (context == null) return;
            var retained = context;
            context = null;
            try { binding.Dispose(); }
            finally { retained.DangerousRelease(); }
        }
    }
    internal void* Pointer => (void*)handle;
    protected override bool ReleaseHandle()
    {
        try
        {
            using var binding = owner.Enter();
            ulong result = compression ? VM.dz_free_cctx((void*)handle) : VM.dz_free_dctx((void*)handle);
            bool released = VM.dz_is_error(result) == 0;
            if (released) Interlocked.Decrement(ref ActiveHandles);
            else Interlocked.Increment(ref ReleaseFailures);
            return released;
        }
        catch { Interlocked.Increment(ref ReleaseFailures); return false; }
        finally { lease?.Dispose(); lease = null; }
    }
}
