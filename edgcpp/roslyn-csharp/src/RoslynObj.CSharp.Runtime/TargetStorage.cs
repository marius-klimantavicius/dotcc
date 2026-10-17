using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RoslynObj.CSharp.Runtime;

/// <summary>
/// Stable, zero-initialized object storage with an explicit target alignment.
/// A process or thread owner must retain this object for the native pointer's lifetime.
/// </summary>
public sealed unsafe class PinnedStorage
{
    private readonly byte[] owner;
    public PinnedStorage(int size, int alignment)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);
        owner = GC.AllocateArray<byte>(checked(size + alignment - 1), pinned: true);
        var address = (nuint)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(owner));
        var remainder = address % (nuint)alignment;
        Pointer = (byte*)(remainder == 0 ? address : address + (nuint)alignment - remainder);
    }
    public byte* Pointer { get; }
}

public static unsafe class TargetStorage
{
    public static ref T Copy<T>(ref T destination, ref T source, int bytes) where T : unmanaged
    {
        if (sizeof(T) != bytes) throw new InvalidOperationException("Aggregate storage does not match its captured size.");
        if (Unsafe.AreSame(ref destination, ref source)) return ref destination;
        Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref destination), Unsafe.AsPointer(ref source), checked((uint)bytes));
        return ref destination;
    }
}

/// <summary>Nonvolatile little-endian bit access using exact retained byte/bit coordinates.</summary>
public static unsafe class TargetBitField
{
    public static ulong ReadUnsigned(byte* storage, int byteOffset, int bitOffset, int width)
    {
        Validate(bitOffset, width);
        ulong value = 0;
        for (var bit = 0; bit < width; ++bit)
        {
            var position = bitOffset + bit;
            value |= (ulong)((storage[byteOffset + position / 8] >> (position % 8)) & 1) << bit;
        }
        return value;
    }

    public static long ReadSigned(byte* storage, int byteOffset, int bitOffset, int width)
    {
        var value = ReadUnsigned(storage, byteOffset, bitOffset, width);
        return width == 64 ? unchecked((long)value) : unchecked((long)(value << (64 - width))) >> (64 - width);
    }

    public static void Write(byte* storage, int byteOffset, int bitOffset, int width, ulong value)
    {
        Validate(bitOffset, width);
        for (var bit = 0; bit < width; ++bit)
        {
            var position = bitOffset + bit;
            var address = storage + byteOffset + position / 8;
            var mask = 1 << (position % 8);
            *address = unchecked((byte)((*address & ~mask) | (((value >> bit) & 1) != 0 ? mask : 0)));
        }
    }

    private static void Validate(int bitOffset, int width)
    {
        if ((uint)bitOffset >= 8 || width is < 1 or > 64)
            throw new InvalidOperationException("Unsupported captured bit-field coordinates.");
    }
}
