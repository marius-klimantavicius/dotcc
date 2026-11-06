#nullable enable
namespace DotCC.Libc;

/// <summary>An error union whose source payload has zero size and alignment one.
/// T supplies nominal identity; only the error code occupies storage.</summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 2, Size = 2)]
public readonly struct ZigErrorUnionZero<T> where T : unmanaged
{
    public readonly ushort Code;
    private ZigErrorUnionZero(ushort code) => Code = code;
    public bool IsErr => Code != 0;
    public T Value => default;
    // The argument is still evaluated before this call, even though it has no bytes.
    public static ZigErrorUnionZero<T> Ok(T value) => new(0);
    public static ZigErrorUnionZero<T> Err(ushort code) => new(code);

    // Curated helpers may retain their ordinary managed carrier. Conversion
    // preserves the code and sole zero-sized payload value in either direction.
    public static implicit operator ZigErrorUnionZero<T>(ErrUnion<T> value) => new(value.Code);
    public static implicit operator ErrUnion<T>(ZigErrorUnionZero<T> value)
        => value.IsErr ? ErrUnion<T>.Err(value.Code) : ErrUnion<T>.Ok(default);
}

public static partial class ErrUnion
{
    public static T Try<T>(ZigErrorUnionZero<T> value) where T : unmanaged
        => value.IsErr ? throw new ZigErrorReturn(value.Code) : value.Value;
    public static T Catch<T>(ZigErrorUnionZero<T> value, T fallback) where T : unmanaged
        => value.IsErr ? fallback : value.Value;
}
