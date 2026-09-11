#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Keep ownership out of the allocation itself: inspecting a header before
    // an ordinary malloc pointer is unsafe. Entries exist only for live aligned
    // blocks; the backing allocation is always paired with NativeMemory.Free,
    // including on Windows, where AlignedAlloc cannot be paired with Free.
    private readonly record struct AlignedBlock(nuint Base, nuint Size, bool Debug);
    private static readonly Dictionary<nuint, AlignedBlock> _alignedBlocks = new();
    private static readonly Lock _alignedLock = new();
    private static int _alignedCount;

    /// <summary>POSIX aligned allocation. Returns EINVAL/ENOMEM directly,
    /// preserving errno and the output pointer on failure. Zero size succeeds
    /// with a unique freeable pointer. LP64 size_t arguments retain all 64 bits.</summary>
    public static int posix_memalign(void** memptr, ulong alignment, ulong size)
    {
        if (alignment < (ulong)sizeof(void*) || (alignment & (alignment - 1)) != 0)
            return EINVAL;
        if (alignment > nuint.MaxValue || size > nuint.MaxValue)
            return ENOMEM;
        nuint align = checked((nuint)alignment), bytes = checked((nuint)size);
        bool debug = _dbgHeap;
        nuint red = debug ? _dbgRed : 0;
        // Allocate at least one byte, even for size zero. Guard both additions
        // before making a native allocation; no rounded-size wrap is possible.
        nuint payload = bytes == 0 ? 1 : bytes;
        if (payload > nuint.MaxValue - (align - 1) ||
            payload + (align - 1) > nuint.MaxValue - red)
            return ENOMEM;
        if (debug && _dbgScan) DbgScanAll("posix_memalign");
        void* allocation = null;
        try
        {
            allocation = NativeMemory.Alloc(payload + (align - 1) + red);
            if (allocation == null) return ENOMEM;
            nuint address = ((nuint)allocation + (align - 1)) & ~(align - 1);
            if (debug) NativeMemory.Fill((byte*)address + bytes, red, _dbgCanary);
            lock (_alignedLock)
            {
                _alignedBlocks.Add(address, new AlignedBlock((nuint)allocation, bytes, debug));
                Volatile.Write(ref _alignedCount, _alignedBlocks.Count);
            }
            *memptr = (void*)address;
            return 0;
        }
        catch (OutOfMemoryException)
        {
            NativeMemory.Free(allocation);
            return ENOMEM;
        }
    }

    private static void ValidateAlignedBlock(nuint address, AlignedBlock block, string where)
    {
        if (!block.Debug) return;
        byte* red = (byte*)address + block.Size;
        for (nuint i = 0; i < _dbgRed; i++)
        {
            if (red[i] != _dbgCanary)
            {
                System.Console.Error.WriteLine(
                    $"[dotcc debug-heap] {where}: write past end of {block.Size}-byte block 0x{address:x} (redzone[{i}]=0x{red[i]:x2})\n{System.Environment.StackTrace}");
                break;
            }
        }
    }

    private static void ScanAlignedBlocks(string where)
    {
        lock (_alignedLock)
        {
            foreach (var item in _alignedBlocks)
                ValidateAlignedBlock(item.Key, item.Value, where);
        }
    }

    private static bool FreeAlignedBlock(void* pointer)
    {
        if (pointer == null || Volatile.Read(ref _alignedCount) == 0) return false;
        AlignedBlock block;
        lock (_alignedLock)
        {
            if (!_alignedBlocks.Remove((nuint)pointer, out block)) return false;
            Volatile.Write(ref _alignedCount, _alignedBlocks.Count);
        }
        if (block.Debug && _dbgScan) DbgScanAll("free");
        ValidateAlignedBlock((nuint)pointer, block, "free");
        NativeMemory.Free((void*)block.Base);
        return true;
    }

    // realloc need only preserve ordinary malloc alignment. Copy into its
    // existing allocator so subsequent free/realloc keep their normal route.
    private static bool ReallocAlignedBlock(void* pointer, int size, out void* replacement)
    {
        replacement = null;
        if (pointer == null || Volatile.Read(ref _alignedCount) == 0) return false;
        AlignedBlock block;
        lock (_alignedLock)
        {
            if (!_alignedBlocks.TryGetValue((nuint)pointer, out block)) return false;
        }
        ValidateAlignedBlock((nuint)pointer, block, "realloc");
        replacement = malloc(size);
        if (replacement == null) return true; // Retain original on failure.
        nuint length = block.Size < (nuint)size ? block.Size : (nuint)size;
        NativeMemory.Copy(pointer, replacement, length);
        FreeAlignedBlock(pointer);
        return true;
    }
}
