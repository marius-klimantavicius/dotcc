namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullTranslationJit
{
    // Lazy while the canonical guest profile is disabled. The finite catalog
    // reserves its entire owner/registry/code charge from Store, exactly once.
    private FullX87KernelCatalog? x87Kernels;
    private bool x87KernelClosing;
    internal bool IsDisposed { get { lock (Authority.Gate) return disposed; } }

    // Called only after this owner is registered for cleanup, before loading
    // guest code or publishing any TB. A failed catalog remains owned here.
    internal void InitializeX87Kernels(bool allowUnverifiedWindows)
    {
        lock (Authority.Gate)
        {
            ObjectDisposedException.ThrowIf(disposed || x87KernelClosing, this);
            if (x87Kernels != null || Store.Publications != 0 || Store.ActiveNativeEpochs != 0)
                throw new InvalidOperationException("x87 catalog requires unpublished owner startup.");
            int slot = compilerScratch.Acquire();
            try
            {
                x87Kernels = new(Authority, Store, allowUnverifiedWindows);
                if (!x87Kernels.Initialize(compilerScratch.Code(slot), out var failure))
                    throw new InvalidOperationException("x87 catalog startup failed: " + failure);
            }
            finally { compilerScratch.Release(slot); }
        }
    }

    internal FullX87KernelBinding GetX87Kernel(FullX87KernelKey key)
    {
        if (!Monitor.IsEntered(Authority.Gate))
            throw new InvalidOperationException("x87 kernel binding requires authority gate.");
        if (disposed || x87KernelClosing || x87Kernels == null || !x87Kernels.AdmissionOpen)
            throw new InvalidOperationException("x87 kernel owner admission is closed.");
        var binding = x87Kernels.Find(key);
        if (!x87Kernels.IsValid(in binding))
            throw new InvalidOperationException("x87 kernel binding is unavailable.");
        return binding;
    }

    internal FullX87KernelCall GetX87KernelCall(FullX87KernelKey key)
    {
        var binding = GetX87Kernel(key);
        var call = new FullX87KernelCall((byte)key, binding.Entry, Store.OwnerCookie, binding.CodeBytes);
        if (!x87Kernels!.Authenticates(in call))
            throw new InvalidOperationException("x87 immutable kernel tuple differs from its owner binding.");
        return call;
    }

    // Checked path: SAME gate as native epoch/enrollment, after all legacy
    // test/prepare hooks. Retained entry uses the post-enrollment predicate
    // below instead. Canonical computation never uses the old
    // register-only cap1 path after failed native admission.
    internal void RequireX87KernelAdmission(in FullBlockFrame frame)
    {
        if (!Monitor.IsEntered(Authority.Gate))
            throw new InvalidOperationException("x87 native admission requires authority gate.");
        if (disposed || x87KernelClosing || frame.X87 != null &&
            (x87Kernels == null || !x87Kernels.AdmissionOpen))
            throw new InvalidOperationException("x87 native catalog admission is closed.");
    }

    // The real root lease/store epoch protect owner/catalog resources here.
    // Do not obtain literal bindings or read mutable registry entries. Closure
    // BEFORE enrollment stales its runtime or is seen here; closure AFTER this
    // check must drain the SC native count and then close the original store.
    internal bool RetainedX87KernelAdmissionOpen(in FullBlockFrame frame) =>
        !Volatile.Read(ref disposed) && !Volatile.Read(ref x87KernelClosing) &&
        (frame.X87 == null || Volatile.Read(ref x87Kernels) is { RetainedNativeAdmissionOpen: true });

    private void DisposeWithX87Kernels()
    {
        lock (Authority.Gate)
        {
            if (disposed) return;
            Volatile.Write(ref x87KernelClosing, true);
            x87Kernels!.CloseAdmission();
        }
        // No callback/ticket finalization is awaited while holding Gate.
        Authority.WaitNativeFrames();
        lock (Authority.Gate)
        {
            if (disposed) return;
            RepKernels.AssertQuiescent();
            Store.Dispose();
        }
        // Catalog owns external quota only; it never disposes its borrowed
        // Store. Store.NativeLifetimeClosed is the final TB lifetime proof.
        x87Kernels!.Dispose();
        lock (Authority.Gate)
        {
            if (disposed) return;
            RepKernels.Dispose(); sharedCode?.Dispose(); compilerScratch.Dispose();
            Memory.Dispose(); Authority.Dispose(); disposed = true;
        }
    }
}
