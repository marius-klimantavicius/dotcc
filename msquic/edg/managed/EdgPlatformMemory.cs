using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost
{
    private readonly Lock _allocationGate = new();
    private readonly Dictionary<nuint, PlatformAllocation> _platformAllocations = new();
    private readonly record struct PlatformAllocation(ulong Size, uint Tag);

    internal int OutstandingPlatformAllocations
    {
        get { lock (_allocationGate) return _platformAllocations.Count; }
    }

    internal void* AllocatePlatformMemory(ulong size, uint tag, bool zero = true)
    {
        // Preserve the existing host's bounded-size contract and 16-byte ABI
        // alignment. Zero-size allocations must remain independently freeable.
        if (size > (ulong)Array.MaxLength - 15) return null;
        nuint capacity = ((nuint)Math.Max(size, 1UL) + 15) & ~(nuint)15;
        void* pointer = null;
        try
        {
            pointer = NativeMemory.AlignedAlloc(capacity, 16);
            if (pointer == null) return null;
            if (zero) NativeMemory.Clear(pointer, capacity);
            lock (_allocationGate)
                _platformAllocations.Add((nuint)pointer, new(size, tag));
            return pointer;
        }
        catch (OutOfMemoryException)
        {
            if (pointer != null) NativeMemory.AlignedFree(pointer);
            return null;
        }
    }

    internal void FreePlatformMemory(void* address, uint tag)
    {
        if (address == null) return;
        lock (_allocationGate)
        {
            if (!_platformAllocations.TryGetValue((nuint)address, out var allocation) || allocation.Tag != tag)
                FatalInvariant("CxPlatFree: wrong allocator domain, tag, or double free");
            _platformAllocations.Remove((nuint)address);
            NativeMemory.AlignedFree(address);
        }
    }
}
