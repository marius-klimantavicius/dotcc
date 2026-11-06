#nullable enable

namespace DotCC.Libc;

/// <summary>Zig ?void stores only its presence byte. Unit is an expression
/// result and is never a stored payload.</summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1, Size = 1)]
public readonly struct ZigOptionalVoid
{
    private readonly byte _present;
    private ZigOptionalVoid(byte present) => _present = present;
    public bool HasValue => _present != 0;
    public Unit Value => HasValue ? default : throw new System.InvalidOperationException("optional value is null");
    public static ZigOptionalVoid Some(Unit value) => new(1);
    public static bool operator ==(ZigOptionalVoid left, ZigOptionalVoid right) => left.HasValue == right.HasValue;
    public static bool operator !=(ZigOptionalVoid left, ZigOptionalVoid right) => !(left == right);
    public override bool Equals(object? other) => other is ZigOptionalVoid value && this == value;
    public override int GetHashCode() => HasValue ? 1 : 0;
    public static unsafe void* PayloadAddress(ZigOptionalVoid* optional)
        => optional->HasValue ? optional : throw new System.InvalidOperationException("optional value is null");
}
