using System;
using System.Runtime.InteropServices;

namespace Managed.Smb;

/// <summary>A four-byte token owned by the SMB socket registry, never a Libc fd.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct t_socket : IEquatable<t_socket>, IComparable<t_socket>
{
    internal readonly int Value;
    private t_socket(int value) => Value = value;
    public static explicit operator int(t_socket value) => value.Value;
    public static implicit operator t_socket(int value) => new(value);
    public bool Equals(t_socket other) => Value == other.Value;
    public override bool Equals(object? value) => value is t_socket other && Equals(other);
    public override int GetHashCode() => Value;
    public int CompareTo(t_socket other) => Value.CompareTo(other.Value);
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    public static bool operator ==(t_socket a, t_socket b) => a.Value == b.Value;
    public static bool operator !=(t_socket a, t_socket b) => a.Value != b.Value;
    public static bool operator <(t_socket a, t_socket b) => a.Value < b.Value;
    public static bool operator >(t_socket a, t_socket b) => a.Value > b.Value;
    public static bool operator <=(t_socket a, t_socket b) => a.Value <= b.Value;
    public static bool operator >=(t_socket a, t_socket b) => a.Value >= b.Value;
}
