#nullable enable

namespace DotCC.Libc;

/// <summary>An optional Zig error uses error code zero as its null niche.</summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 2)]
public readonly struct ZigOptionalError
{
    private readonly ushort _code;
    private ZigOptionalError(ushort code) => _code = code;
    public bool HasValue => _code != 0;
    public ushort Value => HasValue ? _code : throw new System.InvalidOperationException("optional value is null");
    public static implicit operator ZigOptionalError(ushort code)
        => code != 0 ? new(code) : throw new System.InvalidOperationException("error code zero is not an error value");
    public static unsafe implicit operator ZigOptionalError(void* pointer)
        => pointer == null ? default : throw new System.InvalidOperationException("only null converts to an optional");
    public static unsafe bool operator ==(ZigOptionalError value, void* pointer)
        => pointer == null ? !value.HasValue : throw new System.InvalidOperationException("optional pointer comparison requires null");
    public static unsafe bool operator !=(ZigOptionalError value, void* pointer) => !(value == pointer);
    public static unsafe bool operator ==(void* pointer, ZigOptionalError value) => value == pointer;
    public static unsafe bool operator !=(void* pointer, ZigOptionalError value) => !(value == pointer);
    public static bool operator ==(ZigOptionalError left, ZigOptionalError right) => left._code == right._code;
    public static bool operator !=(ZigOptionalError left, ZigOptionalError right) => !(left == right);
    public static unsafe ushort* PayloadAddress(ZigOptionalError* optional)
        => optional->HasValue ? (ushort*)optional : throw new System.InvalidOperationException("optional value is null");
    public override bool Equals(object? other) => other is ZigOptionalError error && _code == error._code;
    public override int GetHashCode() => _code;
}
