#nullable enable

namespace DotCC.Libc;

public static partial class ZigVec
{
    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector64<T> Shl<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector64<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector64<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector64<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector64.WithElement(result, i, value[i] << shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector64<T> Shr<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector64<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector64<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector64<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector64.WithElement(result, i, value[i] >> shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector128<T> Shl<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector128<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector128<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector128<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector128.WithElement(result, i, value[i] << shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector128<T> Shr<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector128<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector128<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector128<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector128.WithElement(result, i, value[i] >> shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector256<T> Shl<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector256<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector256<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector256<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector256.WithElement(result, i, value[i] << shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector256<T> Shr<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector256<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector256<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector256<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector256.WithElement(result, i, value[i] >> shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector512<T> Shl<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector512<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector512<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector512<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector512.WithElement(result, i, value[i] << shift);
        }
        return result;
    }

    /// <summary>Shift each native integer lane by its corresponding unsigned count, preserving value semantics.</summary>
    public static unsafe System.Runtime.Intrinsics.Vector512<T> Shr<T, TCounts, TCount>(
        System.Runtime.Intrinsics.Vector512<T> value, TCounts counts, TCount witness)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = System.Runtime.Intrinsics.Vector512<T>.Count;
        if (sizeof(TCounts) != count * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must have one lane per value lane.");
        var result = default(System.Runtime.Intrinsics.Vector512<T>);
        ref byte first = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref first, i * sizeof(TCount))));
            result = System.Runtime.Intrinsics.Vector512.WithElement(result, i, value[i] >> shift);
        }
        return result;
    }

    /// <summary>Shift native integer lanes in bounded value storage, using one count or one per lane.</summary>
    public static unsafe TValue ShlStorage<TValue, T, TCounts, TCount>(
        TValue value, TCounts counts, T witness, TCount countWitness, bool uniform)
        where TValue : unmanaged
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = sizeof(TValue) / sizeof(T);
        if (sizeof(TValue) % sizeof(T) != 0 || sizeof(TCounts) != (uniform ? 1 : count) * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must match the value lanes.");
        ref byte data = ref System.Runtime.CompilerServices.Unsafe.As<TValue, byte>(ref value);
        ref byte countData = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            ref byte slot = ref System.Runtime.CompilerServices.Unsafe.Add(ref data, i * sizeof(T));
            var lane = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref slot);
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref countData, (uniform ? 0 : i) * sizeof(TCount))));
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref slot, lane << shift);
        }
        return value;
    }

    /// <summary>Shift native integer lanes in bounded value storage, using one count or one per lane.</summary>
    public static unsafe TValue ShrStorage<TValue, T, TCounts, TCount>(
        TValue value, TCounts counts, T witness, TCount countWitness, bool uniform)
        where TValue : unmanaged
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
        where TCounts : unmanaged
        where TCount : unmanaged, System.Numerics.IBinaryInteger<TCount>
    {
        var count = sizeof(TValue) / sizeof(T);
        if (sizeof(TValue) % sizeof(T) != 0 || sizeof(TCounts) != (uniform ? 1 : count) * sizeof(TCount))
            throw new System.ArgumentException("Shift counts must match the value lanes.");
        ref byte data = ref System.Runtime.CompilerServices.Unsafe.As<TValue, byte>(ref value);
        ref byte countData = ref System.Runtime.CompilerServices.Unsafe.As<TCounts, byte>(ref counts);
        for (var i = 0; i < count; i++)
        {
            ref byte slot = ref System.Runtime.CompilerServices.Unsafe.Add(ref data, i * sizeof(T));
            var lane = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref slot);
            var shift = int.CreateChecked(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<TCount>(
                ref System.Runtime.CompilerServices.Unsafe.Add(ref countData, (uniform ? 0 : i) * sizeof(TCount))));
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref slot, lane >> shift);
        }
        return value;
    }

}
