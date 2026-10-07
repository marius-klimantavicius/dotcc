using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Managed.Security;

// Provider and translated protocol exchange ownership of buffers. Always use
// the translated library's allocator, including its allocation registry.
public static unsafe partial class PicoTls
{
    public static void* ProviderAllocate(ulong size) => __edg_malloc(size);
    public static void ProviderFree(void* pointer) => __edg_free(pointer);
}

public static unsafe class EdgProviderMemory
{
    public static void* malloc(int size) => size < 0 ? null : PicoTls.ProviderAllocate((ulong)size);
    public static void* malloc(ulong size) => PicoTls.ProviderAllocate(size);
    public static void free(void* pointer) => PicoTls.ProviderFree(pointer);

    // Algorithm tables have process lifetime. Their names are immutable pinned
    // global data; ordinary provider allocations still use the native heap.
    private static readonly Dictionary<string, byte[]> Names = new(StringComparer.Ordinal);

    public static sbyte* L(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty || bytes[^1] != 0)
            throw new ArgumentException("Provider names must have an explicit NUL terminator.", nameof(bytes));
        string key = Convert.ToHexString(bytes);
        lock (Names)
        {
            if (!Names.TryGetValue(key, out var storage))
            {
                storage = GC.AllocateUninitializedArray<byte>(bytes.Length, pinned: true);
                bytes.CopyTo(storage);
                Names.Add(key, storage);
            }
            return (sbyte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(storage));
        }
    }
}
