#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Only explicitly bound programs opt into heap ownership. The ordinary C
    // runtime remains interoperable with host allocations when no owner is bound.
    // The global index lets us reject a live foreign pointer before dereferencing
    // it; owners retain their exact allocation set for deterministic reclamation.
    private enum HeapAllocationKind { Plain, Debug, Aligned }
    private readonly record struct OwnedHeapBlock(RuntimeContext Owner, nuint Size, HeapAllocationKind Kind);
    private static readonly Lock ownedHeapLock = new();
    private static readonly Dictionary<nuint, OwnedHeapBlock> ownedHeapBlocks = new();

    private static void ReserveOwnedHeapEntry(RuntimeContext owner)
    {
        // Reserve managed bookkeeping before allocating/reallocating native
        // memory. In particular, a moved realloc must never lose its new address
        // because a dictionary resize failed after the old address was freed.
        ownedHeapBlocks.EnsureCapacity(checked(ownedHeapBlocks.Count + 1));
        owner.NativeAllocations.EnsureCapacity(checked(owner.NativeAllocations.Count + 1));
    }

    private static void RegisterOwnedHeapBlock(void* pointer, RuntimeContext owner, nuint size, HeapAllocationKind kind)
    {
        ownedHeapBlocks.Add((nuint)pointer, new(owner, size, kind));
        owner.NativeAllocations.Add((nuint)pointer);
    }

    private static void* AllocateHeapRaw(nuint size, bool zero, bool debug) => debug
        ? DbgAlloc(size, zero)
        : zero ? NativeMemory.AllocZeroed(size) : NativeMemory.Alloc(size);

    private static void* AllocateHeap(nuint size, bool zero = false)
    {
        var owner = RuntimeContext.Current;
        bool debug = _dbgHeap;
        try
        {
            if (owner is null) return AllocateHeapRaw(size, zero, debug);
            lock (ownedHeapLock)
            {
                ReserveOwnedHeapEntry(owner);
                void* pointer = AllocateHeapRaw(size, zero, debug);
                if (pointer != null)
                    RegisterOwnedHeapBlock(pointer, owner, size, debug ? HeapAllocationKind.Debug : HeapAllocationKind.Plain);
                return pointer;
            }
        }
        catch (OutOfMemoryException) { return null; }
    }

    private static void FreeHeapRaw(void* pointer, HeapAllocationKind kind)
    {
        if (kind == HeapAllocationKind.Aligned)
        {
            if (!FreeAlignedBlock(pointer)) throw new InvalidOperationException("Owned aligned allocation lost its backing record.");
        }
        else if (kind == HeapAllocationKind.Debug) DbgFree(pointer);
        else NativeMemory.Free(pointer);
    }

    private static bool FindOwnedHeapBlock(void* pointer, string operation, out OwnedHeapBlock block)
    {
        var owner = RuntimeContext.Current;
        if (ownedHeapBlocks.TryGetValue((nuint)pointer, out block))
        {
            if (!ReferenceEquals(owner, block.Owner))
                throw new InvalidOperationException($"Cannot {operation} an allocation belonging to a different C runtime context.");
            return true;
        }
        if (owner is not null)
            throw new InvalidOperationException($"Cannot {operation} a foreign or already released allocation in a C runtime context.");
        return false;
    }

    private static void ReleaseOwnedHeapBlock(void* pointer, OwnedHeapBlock block)
    {
        FreeHeapRaw(pointer, block.Kind);
        ownedHeapBlocks.Remove((nuint)pointer);
        block.Owner.NativeAllocations.Remove((nuint)pointer);
    }

    private static void FreeHeap(void* pointer)
    {
        if (pointer == null) return;
        lock (ownedHeapLock)
        {
            if (FindOwnedHeapBlock(pointer, "free", out var block))
                ReleaseOwnedHeapBlock(pointer, block);
            else if (!FreeAlignedBlock(pointer))
            {
                if (_dbgHeap) DbgFree(pointer);
                else NativeMemory.Free(pointer);
            }
        }
    }

    private static void* ReallocateHeap(void* pointer, int size)
    {
        if (pointer == null) return AllocateHeap((nuint)size);
        lock (ownedHeapLock)
        {
            if (!FindOwnedHeapBlock(pointer, "reallocate", out var block))
            {
                if (ReallocAlignedBlock(pointer, size, out var aligned)) return aligned;
                try { return _dbgHeap ? DbgRealloc(pointer, (nuint)size) : NativeMemory.Realloc(pointer, (nuint)size); }
                catch (OutOfMemoryException) { return null; }
            }
            // C permits this zero-size convention. Applying it explicitly also
            // avoids treating realloc's NULL-after-free result as an OOM failure.
            if (size == 0) { ReleaseOwnedHeapBlock(pointer, block); return null; }
            nuint bytes = (nuint)size;
            void* replacement;
            var kind = block.Kind;
            try
            {
                ReserveOwnedHeapEntry(block.Owner);
                if (kind == HeapAllocationKind.Aligned)
                {
                    lock (_alignedLock)
                        ValidateAlignedBlock((nuint)pointer, _alignedBlocks[(nuint)pointer], "realloc");
                    bool debug = _dbgHeap;
                    replacement = AllocateHeapRaw(bytes, false, debug);
                    if (replacement == null) return null;
                    NativeMemory.Copy(pointer, replacement, block.Size < bytes ? block.Size : bytes);
                    FreeHeapRaw(pointer, kind);
                    kind = debug ? HeapAllocationKind.Debug : HeapAllocationKind.Plain;
                }
                else replacement = kind == HeapAllocationKind.Debug
                    ? DbgRealloc(pointer, bytes) : NativeMemory.Realloc(pointer, bytes);
            }
            catch (OutOfMemoryException) { return null; }
            if (replacement == null) return null; // Original and ownership remain intact.
            ownedHeapBlocks.Remove((nuint)pointer);
            block.Owner.NativeAllocations.Remove((nuint)pointer);
            RegisterOwnedHeapBlock(replacement, block.Owner, bytes, kind);
            return replacement;
        }
    }

    /// <summary>Allocate aligned storage owned by the bound program, or by the
    /// legacy caller when unbound. Failure preserves errno and the output pointer.</summary>
    public static int posix_memalign(void** memptr, ulong alignment, ulong size)
    {
        var owner = RuntimeContext.Current;
        if (owner is null) return AllocateAlignedRaw(memptr, alignment, size);
        lock (ownedHeapLock)
        {
            try { ReserveOwnedHeapEntry(owner); }
            catch (OutOfMemoryException) { return ENOMEM; }
            void* pointer = null;
            int status = AllocateAlignedRaw(&pointer, alignment, size);
            if (status != 0) return status;
            RegisterOwnedHeapBlock(pointer, owner, (nuint)size, HeapAllocationKind.Aligned);
            *memptr = pointer;
            return 0;
        }
    }

    // RuntimeContext calls this only after rejecting active bindings, retained
    // callbacks and workers. It deliberately uses the captured owner, not the
    // ambient context (which can belong to another program during disposal).
    private static void DisposeOwnedHeap(RuntimeContext owner)
    {
        lock (ownedHeapLock)
        {
            foreach (nuint address in owner.NativeAllocations)
            {
                var block = ownedHeapBlocks[address];
                FreeHeapRaw((void*)address, block.Kind);
                ownedHeapBlocks.Remove(address);
            }
            owner.NativeAllocations.Clear();
        }
    }
}
