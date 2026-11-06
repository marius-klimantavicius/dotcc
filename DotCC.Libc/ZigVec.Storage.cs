#nullable enable

namespace DotCC.Libc;

/// <summary>Exactly four bytes of vector value storage. T identifies the lane type;
/// byte storage keeps the total size independent of T and makes no alignment promise.</summary>
[System.Runtime.CompilerServices.InlineArray(4)]
public struct ZigVector32<T> where T : unmanaged
{
    private byte _first;
}

/// <summary>Exactly 128 bytes of vector value storage, including all lanes beyond the
/// largest .NET SIMD type. Copies have value semantics, without a managed backing array.</summary>
[System.Runtime.CompilerServices.InlineArray(128)]
public struct ZigVector1024<T> where T : unmanaged
{
    private byte _first;
}

public static partial class ZigVec
{
    /// <summary>Widen the lanes of a 512-bit SIMD value into independent 128-byte value storage.</summary>
    public static unsafe ZigVector1024<TTo> Widen1024<TFrom, TTo>(System.Runtime.Intrinsics.Vector512<TFrom> value, TTo witness)
        where TFrom : unmanaged, System.Numerics.IBinaryInteger<TFrom>
        where TTo : unmanaged, System.Numerics.IBinaryInteger<TTo>
    {
        if (System.Runtime.Intrinsics.Vector512<TFrom>.Count != 128 / sizeof(TTo))
            throw new System.ArgumentException("Widening must preserve the lane count.");
        var result = default(ZigVector1024<TTo>);
        for (var i = 0; i < System.Runtime.Intrinsics.Vector512<TFrom>.Count; i++)
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref result[i * sizeof(TTo)], TTo.CreateTruncating(value[i]));
        return result;
    }

    /// <summary>Compare integer lanes into the existing 64-bit bool mask.</summary>
    public static unsafe ulong Eq<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 4 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                == System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare integer lanes into the existing 64-bit bool mask.</summary>
    public static unsafe ulong Ne<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 4 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                != System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare integer lanes into the existing 64-bit bool mask.</summary>
    public static unsafe ulong Eq<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 128 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                == System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare integer lanes into the existing 64-bit bool mask.</summary>
    public static unsafe ulong Ne<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 128 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                != System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare native integer lanes with their signedness into a bool mask.</summary>
    public static unsafe ulong Ge<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 4 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                >= System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare native integer lanes with their signedness into a bool mask.</summary>
    public static unsafe ulong Lt<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 4 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                < System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare integer lanes into a bool mask, reversing the corresponding comparison.</summary>
    public static ulong Le<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T> => Ge(right, left);

    /// <summary>Compare integer lanes into a bool mask, reversing the corresponding comparison.</summary>
    public static ulong Gt<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T> => Lt(right, left);

    /// <summary>Compare native integer lanes with their signedness into a bool mask.</summary>
    public static unsafe ulong Ge<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 128 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                >= System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare native integer lanes with their signedness into a bool mask.</summary>
    public static unsafe ulong Lt<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var count = 128 / sizeof(T);
        if (count > 64) throw new System.NotSupportedException("A vector comparison needs at most 64 lanes.");
        ulong result = 0;
        for (var i = 0; i < count; i++)
            if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref left[i * sizeof(T)])
                < System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref right[i * sizeof(T)]))
                result |= 1UL << i;
        return result;
    }

    /// <summary>Compare integer lanes into a bool mask, reversing the corresponding comparison.</summary>
    public static ulong Le<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T> => Ge(right, left);

    /// <summary>Compare integer lanes into a bool mask, reversing the corresponding comparison.</summary>
    public static ulong Gt<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T> => Lt(right, left);

    /// <summary>Integer bitwise and over all 4 bytes of value storage.</summary>
    public static ZigVector32<T> BitAnd<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 4; i++) left[i] = unchecked((byte)(left[i] & right[i]));
        return left;
    }

    /// <summary>Integer bitwise or over all 4 bytes of value storage.</summary>
    public static ZigVector32<T> BitOr<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 4; i++) left[i] = unchecked((byte)(left[i] | right[i]));
        return left;
    }

    /// <summary>Integer bitwise xor over all 4 bytes of value storage.</summary>
    public static ZigVector32<T> BitXor<T>(ZigVector32<T> left, ZigVector32<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 4; i++) left[i] = unchecked((byte)(left[i] ^ right[i]));
        return left;
    }

    /// <summary>Integer bitwise not over all 4 bytes of value storage.</summary>
    public static ZigVector32<T> BitNot<T>(ZigVector32<T> left)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 4; i++) left[i] = unchecked((byte)(~left[i]));
        return left;
    }

    /// <summary>Integer bitwise and over all 128 bytes of value storage.</summary>
    public static ZigVector1024<T> BitAnd<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 128; i++) left[i] = unchecked((byte)(left[i] & right[i]));
        return left;
    }

    /// <summary>Integer bitwise or over all 128 bytes of value storage.</summary>
    public static ZigVector1024<T> BitOr<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 128; i++) left[i] = unchecked((byte)(left[i] | right[i]));
        return left;
    }

    /// <summary>Integer bitwise xor over all 128 bytes of value storage.</summary>
    public static ZigVector1024<T> BitXor<T>(ZigVector1024<T> left, ZigVector1024<T> right)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 128; i++) left[i] = unchecked((byte)(left[i] ^ right[i]));
        return left;
    }

    /// <summary>Integer bitwise not over all 128 bytes of value storage.</summary>
    public static ZigVector1024<T> BitNot<T>(ZigVector1024<T> left)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        for (var i = 0; i < 128; i++) left[i] = unchecked((byte)(~left[i]));
        return left;
    }

    /// <summary>Evaluate the scalar once and copy it to each lane of a four-byte vector.</summary>
    public static unsafe ZigVector32<T> Splat32<T>(T value) where T : unmanaged
    {
        var result = default(ZigVector32<T>);
        for (var i = 0; i < 4 / sizeof(T); i++)
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref result[i * sizeof(T)], value);
        return result;
    }

    /// <summary>Evaluate the scalar once and copy it to each lane of a 128-byte vector.</summary>
    public static unsafe ZigVector1024<T> Splat1024<T>(T value) where T : unmanaged
    {
        var result = default(ZigVector1024<T>);
        for (var i = 0; i < 128 / sizeof(T); i++)
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref result[i * sizeof(T)], value);
        return result;
    }

    /// <summary>Read exactly four bytes; p need not be aligned.</summary>
    public static unsafe ZigVector32<T> Load32<T>(T* p) where T : unmanaged
        => System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ZigVector32<T>>(p);

    /// <summary>Read exactly 128 bytes; p need not be aligned.</summary>
    public static unsafe ZigVector1024<T> Load1024<T>(T* p) where T : unmanaged
        => System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ZigVector1024<T>>(p);

    /// <summary>Write exactly four bytes, including to unaligned slice storage.</summary>
    public static unsafe void Store<T>(ZigVector32<T> value, T* p) where T : unmanaged
        => System.Runtime.CompilerServices.Unsafe.WriteUnaligned(p, value);

    /// <summary>Write exactly 128 bytes, including to unaligned slice storage.</summary>
    public static unsafe void Store<T>(ZigVector1024<T> value, T* p) where T : unmanaged
        => System.Runtime.CompilerServices.Unsafe.WriteUnaligned(p, value);

    /// <summary>Read a lane without requiring the value storage to align T.</summary>
    public static unsafe T Get<T>(ZigVector32<T> value, ulong index) where T : unmanaged
    {
        if (index >= (ulong)(4 / sizeof(T))) throw new System.IndexOutOfRangeException();
        return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref value[(int)index * sizeof(T)]);
    }

    /// <summary>Read a lane, including those in the upper 64 bytes.</summary>
    public static unsafe T Get<T>(ZigVector1024<T> value, ulong index) where T : unmanaged
    {
        if (index >= (ulong)(128 / sizeof(T))) throw new System.IndexOutOfRangeException();
        return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref value[(int)index * sizeof(T)]);
    }

    /// <summary>Return a value copy with exactly one lane replaced.</summary>
    public static unsafe ZigVector32<T> With<T>(ZigVector32<T> value, ulong index, T lane) where T : unmanaged
    {
        if (index >= (ulong)(4 / sizeof(T))) throw new System.IndexOutOfRangeException();
        System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref value[(int)index * sizeof(T)], lane);
        return value;
    }

    /// <summary>Return a value copy with exactly one lane replaced.</summary>
    public static unsafe ZigVector1024<T> With<T>(ZigVector1024<T> value, ulong index, T lane) where T : unmanaged
    {
        if (index >= (ulong)(128 / sizeof(T))) throw new System.IndexOutOfRangeException();
        System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref value[(int)index * sizeof(T)], lane);
        return value;
    }
}
