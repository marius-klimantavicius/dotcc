// Typed implementations of selected zstd-1.5.7 byte and bit primitives.
// The campaign verifies original C signatures and physical declaration files.
using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Managed.Zstd.Optimization;

/// <summary>Ordinary unaligned memory operations for the LP64 translated core.
/// Callers retain upstream readable/writable extent and nonzero-bit preconditions.
/// These operations do not provide volatile or atomic memory semantics.</summary>
public static unsafe class Primitives
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_isLittleEndian() => BitConverter.IsLittleEndian ? 1U : 0U;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort MEM_read16(void* p) => Unsafe.ReadUnaligned<ushort>(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_read32(void* p) => Unsafe.ReadUnaligned<uint>(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_read64(void* p) => Unsafe.ReadUnaligned<ulong>(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_readST(void* p) => Unsafe.ReadUnaligned<ulong>(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_write16(void* p, ushort value) => Unsafe.WriteUnaligned(p, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_write32(void* p, uint value) => Unsafe.WriteUnaligned(p, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_write64(void* p, ulong value) => Unsafe.WriteUnaligned(p, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort MEM_readLE16(void* p) => BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(p, 2));
    // Exactly three readable bytes. Never use a masked four-byte load here.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_readLE24(void* p)
    {
        byte* b = (byte*)p;
        return (uint)b[0] | ((uint)b[1] << 8) | ((uint)b[2] << 16);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_readLE32(void* p) => BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(p, 4));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_readLE64(void* p) => BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(p, 8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_readLEST(void* p) => BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(p, 8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLE16(void* p, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(p, 2), value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLE24(void* p, uint value)
    {
        byte* b = (byte*)p;
        b[0] = unchecked((byte)value);
        b[1] = unchecked((byte)(value >> 8));
        b[2] = unchecked((byte)(value >> 16));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLE32(void* p, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(new Span<byte>(p, 4), value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLE64(void* p, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(p, 8), value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeLEST(void* p, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(p, 8), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_readBE32(void* p) => BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(p, 4));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_readBE64(void* p) => BinaryPrimitives.ReadUInt64BigEndian(new ReadOnlySpan<byte>(p, 8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_readBEST(void* p) => BinaryPrimitives.ReadUInt64BigEndian(new ReadOnlySpan<byte>(p, 8));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeBE32(void* p, uint value) => BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(p, 4), value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeBE64(void* p, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(new Span<byte>(p, 8), value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MEM_writeBEST(void* p, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(new Span<byte>(p, 8), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MEM_swap32(uint value) => BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_swap64(ulong value) => BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MEM_swapST(ulong value) => BinaryPrimitives.ReverseEndianness(value);

    // Upstream requires nonzero inputs. BCL zero results do not widen that contract.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_countTrailingZeros32(uint value) => (uint)BitOperations.TrailingZeroCount(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_countLeadingZeros32(uint value) => (uint)BitOperations.LeadingZeroCount(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_countTrailingZeros64(ulong value) => (uint)BitOperations.TrailingZeroCount(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_countLeadingZeros64(ulong value) => (uint)BitOperations.LeadingZeroCount(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_highbit32(uint value) => 31U - (uint)BitOperations.LeadingZeroCount(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ZSTD_NbCommonBytes(ulong value) => (uint)(BitConverter.IsLittleEndian
        ? BitOperations.TrailingZeroCount(value) : BitOperations.LeadingZeroCount(value)) >> 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int XXH_isLittleEndian() => BitConverter.IsLittleEndian ? 1 : 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH_read32(void* p) => MEM_read32(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH_read64(void* p) => MEM_read64(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH_readLE32(void* p) => MEM_readLE32(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH_readLE64(void* p) => MEM_readLE64(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH_readBE32(void* p) => MEM_readBE32(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH_readBE64(void* p) => MEM_readBE64(p);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XXH_swap32(uint value) => MEM_swap32(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XXH_swap64(ulong value) => MEM_swap64(value);
}
