namespace DotCC.Libc;

/// <summary>Raw x87 ABI storage only. No numeric conversion or arithmetic.</summary>
[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = 16)]
public struct LongDouble80Storage
{
    [global::System.Runtime.InteropServices.FieldOffset(0)] public ulong Mantissa;
    [global::System.Runtime.InteropServices.FieldOffset(8)] public ushort SignExponent;
}
