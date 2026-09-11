using System.Runtime.InteropServices;
using System.Security.Cryptography;

// Simulates the pinned core's malloc/setup/dispose/free order without TLS code.
static unsafe class ContextOwnershipProbe {
    sealed class State : IDisposable {
        public readonly AesGcm Cipher = new(new byte[16], 16);
        public bool Disposed;
        public void Dispose() { Cipher.Dispose(); Disposed = true; }
    }
    static int live;
    static void DisposeCrypto(aead_with_handle* context) {
        if (context->handle == 0) return; // Also supports partial setup cleanup.
        var handle = GCHandle.FromIntPtr(context->handle);
        ((State)handle.Target!).Dispose(); handle.Free(); context->handle = 0; live--;
    }
    public static void Run() {
        foreach (bool failSetup in new[] { false, true }) {
            var context = (aead_with_handle*)NativeMemory.Alloc((nuint)sizeof(aead_with_handle));
            // The core initializes only the header. The provider must initialize its tail.
            context->header = default; context->header.algo = 123;
            context->handle = 0;
            var state = new State();
            context->handle = GCHandle.ToIntPtr(GCHandle.Alloc(state)); live++;
            try {
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                Probe.Assert(ReferenceEquals(GCHandle.FromIntPtr(context->handle).Target, state), "AEAD GCHandle root");
                if (failSetup) {
                    // Core frees the allocation directly when setup fails; setup must unwind handles.
                    DisposeCrypto(context);
                } else {
                    ((State)GCHandle.FromIntPtr(context->handle).Target!).Cipher.Encrypt(new byte[12], ReadOnlySpan<byte>.Empty, Span<byte>.Empty, new byte[16]);
                    DisposeCrypto(context);
                }
                Probe.Assert(context->header.algo == 123, "provider preserves core-owned algorithm pointer");
                Probe.Assert(context->handle == 0 && state.Disposed, "AEAD state disposed before core frees memory");
                DisposeCrypto(context); // Tail cleanup can be repeated before the core's final free.
            } finally { DisposeCrypto(context); NativeMemory.Free(context); }
        }
        Probe.Assert(live == 0, "all AEAD handles released");
        Console.WriteLine("PASS appended AEAD GCHandle: forced GC, success/failure setup cleanup, provider dispose then core-owned free; live handles=0");
    }
}
