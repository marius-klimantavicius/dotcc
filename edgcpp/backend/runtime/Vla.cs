using System;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

// The EDG VLA ABI passes an addressable pointer slot for allocation/normal
// release, and the array address for EH release. Every translated activation
// additionally restores its mark in finally, so CLR unwinding needs no native
// stack-address heuristics (compare lib_src/vla_alloc.c).
public static unsafe class Vla
{
    private struct Allocation
    {
        public Allocation* Previous;
        public void* Base;
        public void* Data;
    }
    [ThreadStatic] private static Allocation* head;
    public static nint Mark() => (nint)head;
    public static void Allocate(void* slot, long bytes, nuint alignment)
    {
        if (bytes < 0) Environment.FailFast("Negative variable length array size");
        if (alignment < 16) alignment = 16;
        if (alignment > 4096 || (alignment & (alignment - 1)) != 0)
            Environment.FailFast("Invalid variable length array alignment");
        nuint size = bytes == 0 ? 1 : (nuint)bytes;
        if (size > nuint.MaxValue - (nuint)sizeof(Allocation) - alignment)
            Environment.FailFast("Variable length array allocation overflow");
        void* storage;
        try { storage = NativeMemory.Alloc(size + (nuint)sizeof(Allocation) + alignment - 1); }
        catch (OutOfMemoryException error)
        {
            Environment.FailFast("Unable to allocate variable length array", error);
            throw;
        }
        if (storage == null) Environment.FailFast("Unable to allocate variable length array");
        var node = (Allocation*)storage;
        void* data = (void*)(((nuint)storage + (nuint)sizeof(Allocation) + alignment - 1) & ~(alignment - 1));
        node->Base = storage; node->Data = data; node->Previous = head;
        head = node;
        *(void**)slot = data;
    }
    private static void Pop()
    {
        var node = head;
        head = node->Previous;
        NativeMemory.Free(node->Base);
    }
    public static void Release(void* data)
    {
        // EDG may coalesce a scope's releases into a release of its first VLA.
        for (;;)
        {
            if (head == null) Environment.FailFast("Invalid variable length array release");
            bool found = head->Data == data;
            Pop();
            if (found) return;
        }
    }
    public static void Restore(nint mark)
    {
        while (head != (Allocation*)mark)
        {
            if (head == null) Environment.FailFast("Invalid variable length array frame");
            Pop();
        }
    }
}
