#nullable enable
namespace DotCC.Libc;

/// <summary>A Zig optional with an empty, alignment-one payload stores only presence.
/// T supplies nominal type identity and method dispatch; no CLR-sized T is stored.</summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1, Size = 1)]
public readonly unsafe struct ZigOptionalZero<T> where T : unmanaged
{
    private readonly byte _present;
    private ZigOptionalZero(byte present) => _present = present;
    public bool HasValue => _present != 0;
    public T Value => HasValue ? default : throw new System.InvalidOperationException("optional value is null");
    public static ZigOptionalZero<T> Some(T value) => new(1);
    public static implicit operator ZigOptionalZero<T>(T value) => Some(value);
    public static implicit operator ZigOptionalZero<T>(void* pointer) => pointer == null ? default
        : throw new System.InvalidOperationException("only null converts to an optional");
    public static bool operator ==(ZigOptionalZero<T> value, void* pointer) => pointer == null ? !value.HasValue
        : throw new System.InvalidOperationException("optional pointer comparison requires null");
    public static bool operator !=(ZigOptionalZero<T> value, void* pointer) => !(value == pointer);
    public static bool operator ==(void* pointer, ZigOptionalZero<T> value) => value == pointer;
    public static bool operator !=(void* pointer, ZigOptionalZero<T> value) => !(value == pointer);
    public override bool Equals(object? other) => other is ZigOptionalZero<T> value && HasValue == value.HasValue;
    public override int GetHashCode() => HasValue ? 1 : 0;
    public static T* PayloadAddress(ZigOptionalZero<T>* value) => value->HasValue ? (T*)value
        : throw new System.InvalidOperationException("optional value is null");
    // Reading/writing a source zero-byte place evaluates its operands but touches no byte.
    public static T Read(void* address) => default;
    public static T Write(void* address, T value) => value;
}
