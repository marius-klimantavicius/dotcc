namespace DotCC.Libc;

public static partial class ZigVec
{
    /// <summary>OR every lane of exact-size vector value storage.</summary>
    public static unsafe T ReduceOr<T>(ZigVector1024<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref v[0]);
        for (var i = 1; i < 128 / sizeof(T); i++)
            result |= System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref v[i * sizeof(T)]);
        return result;
    }

    /// <summary>OR every lane of four-byte vector value storage.</summary>
    public static unsafe T ReduceOr<T>(ZigVector32<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref v[0]);
        for (var i = 1; i < 4 / sizeof(T); i++)
            result |= System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref v[i * sizeof(T)]);
        return result;
    }

    // Fold in T so unsigned high bits and signed lanes keep their exact
    // representation. Start with lane zero and visit every remaining lane,
    // including the upper halves of Vector256 and Vector512.
    /// <summary>Integer <c>@reduce(.Or, v)</c>, evaluating v once by value.</summary>
    public static T ReduceOr<T>(System.Runtime.Intrinsics.Vector64<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = v[0];
        for (var i = 1; i < System.Runtime.Intrinsics.Vector64<T>.Count; i++) result |= v[i];
        return result;
    }

    /// <summary>Integer <c>@reduce(.Or, v)</c>, evaluating v once by value.</summary>
    public static T ReduceOr<T>(System.Runtime.Intrinsics.Vector128<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = v[0];
        for (var i = 1; i < System.Runtime.Intrinsics.Vector128<T>.Count; i++) result |= v[i];
        return result;
    }

    /// <summary>Integer <c>@reduce(.Or, v)</c>, evaluating v once by value.</summary>
    public static T ReduceOr<T>(System.Runtime.Intrinsics.Vector256<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = v[0];
        for (var i = 1; i < System.Runtime.Intrinsics.Vector256<T>.Count; i++) result |= v[i];
        return result;
    }

    /// <summary>Integer <c>@reduce(.Or, v)</c>, evaluating v once by value.</summary>
    public static T ReduceOr<T>(System.Runtime.Intrinsics.Vector512<T> v)
        where T : unmanaged, System.Numerics.IBinaryInteger<T>
    {
        var result = v[0];
        for (var i = 1; i < System.Runtime.Intrinsics.Vector512<T>.Count; i++) result |= v[i];
        return result;
    }
}
