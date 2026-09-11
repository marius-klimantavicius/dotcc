using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// A standalone callback/lifetime experiment. Not registered with translated picotls.
static unsafe class HashContextProbe {
    static readonly delegate* unmanaged[Cdecl]<hash_with_handle*, void*, nuint, void> UpdatePointer = &Update;
    static readonly delegate* unmanaged[Cdecl]<hash_with_handle*, void*, int, void> FinalPointer = &Final;
    static readonly delegate* unmanaged[Cdecl]<hash_with_handle*, hash_with_handle*> ClonePointer = &Clone;
    [ThreadStatic] static Exception? failure;
    static int live;
    static hash_with_handle* Create(IncrementalHash hash) {
        var ctx = (hash_with_handle*)NativeMemory.AllocZeroed((nuint)sizeof(hash_with_handle));
        try {
            ctx->handle = GCHandle.ToIntPtr(GCHandle.Alloc(hash));
            ctx->header = new() { update = (nint)UpdatePointer, final = (nint)FinalPointer, clone_ = (nint)ClonePointer };
            live++;
            return ctx;
        } catch { hash.Dispose(); NativeMemory.Free(ctx); throw; }
    }
    static IncrementalHash State(hash_with_handle* ctx) => (IncrementalHash)GCHandle.FromIntPtr(ctx->handle).Target!;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void Update(hash_with_handle* ctx, void* data, nuint length) {
        try { State(ctx).AppendData(new ReadOnlySpan<byte>(data, checked((int)length))); }
        catch (Exception ex) { failure ??= ex; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static hash_with_handle* Clone(hash_with_handle* ctx) {
        try { return Create(State(ctx).Clone()); }
        catch (Exception ex) { failure ??= ex; return null; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void Final(hash_with_handle* ctx, void* output, int mode) {
        try {
            var hash = State(ctx);
            if (mode is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(mode));
            if (output != null || mode == 1) {
                byte[] result = mode == 1 ? hash.GetHashAndReset() : hash.GetCurrentHash();
                if (output != null) result.CopyTo(new Span<byte>(output, result.Length));
                CryptographicOperations.ZeroMemory(result);
            }
        } catch (Exception ex) { failure ??= ex; }
        finally {
            if (mode == 0) {
                try { State(ctx).Dispose(); GCHandle.FromIntPtr(ctx->handle).Free(); ctx->handle = 0; live--; }
                catch (Exception ex) { failure ??= ex; }
                NativeMemory.Free(ctx);
            }
        }
    }
    static void Boundary() { if (failure is { } ex) { failure = null; throw new Exception("Callback failure captured at owning entry boundary", ex); } }
    static void Feed(hash_with_handle* ctx, ReadOnlySpan<byte> bytes) {
        fixed (byte* data = bytes) ((delegate* unmanaged[Cdecl]<hash_with_handle*, void*, nuint, void>)ctx->header.update)(ctx, data, (nuint)bytes.Length);
        Boundary();
    }
    static byte[] Digest(hash_with_handle* ctx, int mode, int size) {
        byte[] result = new byte[size];
        fixed (byte* data = result) ((delegate* unmanaged[Cdecl]<hash_with_handle*, void*, int, void>)ctx->header.final)(ctx, data, mode);
        Boundary(); return result;
    }
    public static void Run() {
        foreach (var algorithm in new[] { HashAlgorithmName.SHA256, HashAlgorithmName.SHA384 }) {
            int size = algorithm == HashAlgorithmName.SHA256 ? 32 : 48;
            byte[] Expected(ReadOnlySpan<byte> data) => algorithm == HashAlgorithmName.SHA256 ? SHA256.HashData(data) : SHA384.HashData(data);
            var ctx = Create(IncrementalHash.CreateHash(algorithm));
            Feed(ctx, "ab"u8);
            var clone = ((delegate* unmanaged[Cdecl]<hash_with_handle*, hash_with_handle*>)ctx->header.clone_)(ctx);
            Boundary();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Probe.Equal(Digest(ctx, 2, size), Expected("ab"u8), "SNAPSHOT");
            Feed(ctx, "c"u8); Feed(clone, "d"u8);
            Probe.Equal(Digest(ctx, 1, size), Expected("abc"u8), "RESET result");
            Probe.Equal(Digest(ctx, 2, size), Expected([]), "RESET empty state");
            Probe.Equal(Digest(clone, 0, size), Expected("abd"u8), "independent clone FREE");
            Feed(ctx, "discard"u8); FinalPointer(ctx, null, 1); Boundary();
            Probe.Equal(Digest(ctx, 2, size), Expected([]), "null-output RESET");
            Feed(ctx, "retain"u8); FinalPointer(ctx, null, 2); Boundary();
            Probe.Equal(Digest(ctx, 2, size), Expected("retain"u8), "null-output SNAPSHOT");
            // Checked size_t-to-span failure cannot unwind through an unmanaged callback.
            UpdatePointer(ctx, null, nuint.MaxValue);
            bool captured = false; try { Boundary(); } catch (Exception ex) when (ex.InnerException is OverflowException) { captured = true; }
            Probe.Assert(captured, "callback exception captured");
            FinalPointer(ctx, null, 0); Boundary();
        }
        for (int i = 0; i < 1000; i++) {
            var ctx = Create(IncrementalHash.CreateHash(HashAlgorithmName.SHA256));
            if (i % 100 == 0) GC.Collect();
            FinalPointer(ctx, null, 0); Boundary();
        }
        Probe.Assert(live == 0, "all hash GCHandles released");
        Console.WriteLine("PASS SHA256/SHA384 independent Clone, SNAPSHOT/RESET/FREE, null-output modes, static unmanaged callbacks, forced GC, 1000 allocation/free cycles, captured overflow; live handles=0");
    }
}
