#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Edg.Runtime;

/// <summary>A by-value variadic argument snapshot with its EDG storage layout.</summary>
/// <remarks>
/// Small values keep their exact bytes inline. Larger unmanaged records own an
/// ordinary, unpinned array so that a later argument throwing cannot leak native
/// storage or leave a pointer to a departed conversion helper's stack frame.
/// The array is never exposed as a retained native pointer.
/// </remarks>
public readonly unsafe struct VaArg
{
    public const int MaximumPayloadBytes = 64 * 1024 * 1024;
    private readonly UInt128 inlineBits;
    private readonly byte[]? snapshot;
    public int Size { get; }
    public int Alignment { get; }

    private VaArg(UInt128 bits, byte[]? bytes, int size, int alignment)
    {
        inlineBits = bits;
        snapshot = bytes;
        Size = size;
        Alignment = alignment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VaArg From<T>(T value, int alignment) where T : unmanaged
    {
        int size = sizeof(T);
        ValidateLayout(size, alignment);
        if (size <= 16)
        {
            UInt128 bits = 0;
            new ReadOnlySpan<byte>(&value, size).CopyTo(new Span<byte>(&bits, size));
            return new VaArg(bits, null, size, alignment);
        }
        byte[] bytes = GC.AllocateUninitializedArray<byte>(size);
        new ReadOnlySpan<byte>(&value, size).CopyTo(bytes);
        return new VaArg(0, bytes, size, alignment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VaArg From<T>(T value, int expectedSize, int alignment) where T : unmanaged
    {
        if (sizeof(T) != expectedSize)
            throw new ArgumentException("Variadic argument managed and EDG sizes differ.", nameof(expectedSize));
        return From(value, alignment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static VaArg FromPointer(void* value) => From((nuint)value, sizeof(void*));

    // C default argument promotions are applied by the emitter before these
    // conversions. Unusual alignment and wider/aggregate values use From.
    public static implicit operator VaArg(int value) => From(value, 4);
    public static implicit operator VaArg(uint value) => From(value, 4);
    public static implicit operator VaArg(long value) => From(value, 8);
    public static implicit operator VaArg(ulong value) => From(value, 8);
    public static implicit operator VaArg(double value) => From(value, 8);
    public static implicit operator VaArg(void* value) => FromPointer(value);

    internal static void ValidateLayout(int size, int alignment)
    {
        if (size <= 0 || size > MaximumPayloadBytes)
            throw new ArgumentOutOfRangeException(nameof(size), "Variadic values require 1..64 MiB of storage.");
        if (alignment <= 0 || alignment > 4096 || (alignment & (alignment - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(alignment), "Variadic alignment must be a power of two up to 4096.");
    }

    internal void CopyTo(byte* destination, int remainingBytes)
    {
        ValidateLayout(Size, Alignment);
        if (Size > remainingBytes)
            throw new ArgumentException("Variadic arguments changed while their pack was being constructed.");
        var target = new Span<byte>(destination, Size);
        if (snapshot is { } bytes)
        {
            if (bytes.Length != Size)
                throw new InvalidOperationException("Invalid variadic argument snapshot size.");
            bytes.AsSpan().CopyTo(target);
        }
        else
        {
            if (Size > 16)
                throw new InvalidOperationException("Missing variadic argument snapshot.");
            // Copy from a stack local. The carrier itself may reside in a
            // movable array and contains a GC reference; never take its pointer.
            UInt128 bits = inlineBits;
            new ReadOnlySpan<byte>(&bits, Size).CopyTo(target);
        }
    }
}
