// Only the two exact-width operations without a direct BCL equivalent remain.
// Ordinary 16/32/64-bit memory and bit operations use closed compiler intrinsics.
using System.Runtime.CompilerServices;

namespace Managed.Zstd.Optimization;

/// <summary>Exactly three readable or writable bytes, including unaligned addresses.
/// These operations do not provide volatile or atomic memory semantics.</summary>
public static unsafe class Primitives
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_readLE24(void* p)
    {
        byte* b = (byte*)p;
        return (uint)b[0] | ((uint)b[1] << 8) | ((uint)b[2] << 16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLE24(void* p, uint value)
    {
        byte* b = (byte*)p;
        b[0] = unchecked((byte)value);
        b[1] = unchecked((byte)(value >> 8));
        b[2] = unchecked((byte)(value >> 16));
    }
}
