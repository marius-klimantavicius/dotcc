using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VM = Managed.Interpreters.QuickJs;

namespace Managed.Interpreters;

/// <summary>Platform services for the translated engine; no JavaScript algorithms live here.</summary>
public static unsafe class QuickJsHost
{
    /// <summary>Allocate a process-wide host class ID through upstream's
    /// CONFIG_ATOMICS-protected class registration.</summary>
    public static uint NewClassId()
    {
        uint id = 0;
        return VM.JS_NewClassID(&id);
    }

    // A 16-byte prefix retains malloc alignment on the supported Linux x64 ABI.
    private const ulong HeaderSize = 16;
    public sealed class AllocationAccount
    {
        public long Blocks;
        public long Bytes;
    }

    private static void Account(VM.JSMallocState* state, long blocks, long bytes)
    {
        if (state->opaque == null) return;
        var account = (AllocationAccount)GCHandle.FromIntPtr((nint)state->opaque).Target!;
        account.Blocks += blocks;
        account.Bytes += bytes;
    }

    public static void* Allocate(VM.JSMallocState* state, ulong size)
    {
        if (size == 0 || size > (ulong)nuint.MaxValue - HeaderSize ||
            size > long.MaxValue - HeaderSize || state->malloc_size > state->malloc_limit ||
            size + HeaderSize > state->malloc_limit - state->malloc_size) return null;
        void* allocation;
        try { allocation = NativeMemory.Alloc((nuint)(size + HeaderSize)); }
        catch (OutOfMemoryException) { return null; }
        if (allocation == null) return null;
        *(ulong*)allocation = size;
        state->malloc_count++;
        state->malloc_size += size + HeaderSize;
        Account(state, 1, (long)(size + HeaderSize));
        return (byte*)allocation + HeaderSize;
    }

    public static ulong UsableSize(void* pointer) => pointer == null ? 0 : *(ulong*)((byte*)pointer - HeaderSize);

    public static void Free(VM.JSMallocState* state, void* pointer)
    {
        if (pointer == null) return;
        ulong bytes = UsableSize(pointer) + HeaderSize;
        state->malloc_count--;
        state->malloc_size -= bytes;
        Account(state, -1, -(long)bytes);
        NativeMemory.Free((byte*)pointer - HeaderSize);
    }

    public static void* Reallocate(VM.JSMallocState* state, void* pointer, ulong size)
    {
        if (pointer == null) return Allocate(state, size);
        if (size == 0) { Free(state, pointer); return null; }
        ulong oldSize = UsableSize(pointer);
        ulong retainedBytes = state->malloc_size - oldSize;
        if (size > (ulong)nuint.MaxValue - HeaderSize || size > long.MaxValue - HeaderSize ||
            retainedBytes > state->malloc_limit || size > state->malloc_limit - retainedBytes) return null;
        void* allocation;
        try { allocation = NativeMemory.Realloc((byte*)pointer - HeaderSize, (nuint)(size + HeaderSize)); }
        catch (OutOfMemoryException) { return null; }
        if (allocation == null) return null; // C realloc failure retains the old allocation.
        *(ulong*)allocation = size;
        state->malloc_size = retainedBytes + size;
        Account(state, 0, (long)size - (long)oldSize);
        return (byte*)allocation + HeaderSize;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static ulong StackPointer()
    {
        byte marker = 0;
        return (ulong)&marker; // Address is used as a number only, never dereferenced after return.
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int CheckStack(VM.JSRuntime* runtime, ulong requestedBytes)
    {
        if (!RuntimeHelpers.TryEnsureSufficientExecutionStack()) return 1;
        ulong pointer = StackPointer();
        // Include room for the throwing/unwinding path and translated managed frames.
        const ulong reserve = 32 * 1024;
        if (requestedBytes > ulong.MaxValue - reserve) return 1;
        ulong needed = requestedBytes + reserve;
        if (pointer < needed || pointer - needed < runtime->stack_limit) return 1;
        return 0;
    }
}
