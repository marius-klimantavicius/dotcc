using System;
using System.Runtime.CompilerServices;
namespace DotCC.Libc;
/// <summary>Exact logical-bit transport. Runtime evidence currently covers little-endian hosts.</summary>
internal static unsafe class ZigBitcastWidth
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Mask(int bits) => bits == 128 ? UInt128.MaxValue : ((UInt128)1 << bits) - 1;
    private static void CheckWidth(int bits)
    {
        if (!BitConverter.IsLittleEndian)
            throw new PlatformNotSupportedException("Zig logical bitcast transport has no big-endian runtime witness");
        if (bits is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(bits));
    }
    public static TTarget Scalar<TSource, TTarget>(TSource value, int bits, bool signed)
        where TSource : unmanaged where TTarget : unmanaged
    {
        CheckWidth(bits);
        var bytes = (bits + 7) / 8;
        if (bytes > sizeof(TSource)) throw new ArgumentException("Zig bitcast source storage is too small");
        UInt128 packed = 0;
        var source = (byte*)&value;
        for (var b = 0; b < bytes; b++) packed |= (UInt128)source[b] << (8 * b);
        return Result<TTarget>(packed, bits, signed);
    }
    public static TTarget Array<TTarget>(void* source, int count, int elementBits, int stride, int bits, bool signed)
        where TTarget : unmanaged
    {
        CheckWidth(bits);
        if (count <= 0 || elementBits is < 1 or > 128 || (long)count * elementBits != bits || stride < (elementBits + 7) / 8)
            throw new ArgumentException("Invalid Zig bitcast array layout");
        UInt128 packed = 0;
        var bytes = (elementBits + 7) / 8;
        var mask = Mask(elementBits);
        for (var i = 0; i < count; i++)
        {
            UInt128 element = 0;
            var lane = (byte*)source + (nuint)i * (nuint)stride;
            // Read exactly the logical bytes at each physical stride, including
            // a three-byte buffer ending immediately before an inaccessible page.
            for (var b = 0; b < bytes; b++) element |= (UInt128)lane[b] << (8 * b);
            packed |= (element & mask) << (i * elementBits);
        }
        return Result<TTarget>(packed, bits, signed);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TTarget Result<TTarget>(UInt128 packed, int bits, bool signed) where TTarget : unmanaged
    {
        if (sizeof(TTarget) > 16 || sizeof(TTarget) * 8 < bits) throw new ArgumentException("Zig bitcast target storage is unsupported");
        var mask = Mask(bits);
        packed &= mask;
        if (signed && (packed & ((UInt128)1 << (bits - 1))) != 0) packed |= ~mask;
        TTarget result = default;
        var dest = (byte*)&result;
        for (var b = 0; b < sizeof(TTarget); b++) dest[b] = (byte)(packed >> (8 * b));
        return result;
    }
}
