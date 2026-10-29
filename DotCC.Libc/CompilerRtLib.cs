using System;
using System.Runtime.CompilerServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Compiler-rt signed multiplication: low 32 bits plus a 0/1 overflow flag.</summary>
    public static int __mulosi4(int lhs, int rhs, int* overflow)
    {
        long product = (long)lhs * rhs;
        *overflow = product < int.MinValue || product > int.MaxValue ? 1 : 0;
        return unchecked((int)product);
    }

    /// <summary>Low 128 bits of the product, independent of signed overflow.</summary>
    public static Int128 __multi3(Int128 lhs, Int128 rhs) => unchecked(lhs * rhs);
    public static UInt128 __udivti3(UInt128 lhs, UInt128 rhs) => lhs / rhs;
    public static UInt128 __umodti3(UInt128 lhs, UInt128 rhs) => lhs % rhs;

    /// <summary>Truncate toward zero using unsigned magnitudes. Compiler-rt wraps
    /// min/-1 to min; the CLR signed division operator would throw instead.
    /// A zero divisor raises DivideByZeroException.</summary>
    public static Int128 __divti3(Int128 lhs, Int128 rhs)
    {
        UInt128 quotient = CompilerRtMagnitude(lhs) / CompilerRtMagnitude(rhs);
        return unchecked((Int128)((lhs < 0) != (rhs < 0) ? UInt128.Zero - quotient : quotient));
    }

    /// <summary>Remainder has the dividend's sign, including zero for min%-1.</summary>
    public static Int128 __modti3(Int128 lhs, Int128 rhs)
    {
        UInt128 remainder = CompilerRtMagnitude(lhs) % CompilerRtMagnitude(rhs);
        return unchecked((Int128)(lhs < 0 ? UInt128.Zero - remainder : remainder));
    }

    private static UInt128 CompilerRtMagnitude(Int128 value)
        => value < 0 ? unchecked(UInt128.Zero - (UInt128)value) : (UInt128)value;

    // C backends may represent a 128-bit integer as a two-limb aggregate rather
    // than __int128. Preserve that representation without referencing generated
    // TU types. Unsafe.BitCast rejects any representation whose size is not 16.
    public static T __multi3<T>(T lhs, T rhs) where T : unmanaged
        => CompilerRtValue<T>(unchecked(CompilerRtBits(lhs) * CompilerRtBits(rhs)));

    public static T __udivti3<T>(T lhs, T rhs) where T : unmanaged
        => CompilerRtValue<T>(__udivti3(CompilerRtBits(lhs), CompilerRtBits(rhs)));

    public static T __umodti3<T>(T lhs, T rhs) where T : unmanaged
        => CompilerRtValue<T>(__umodti3(CompilerRtBits(lhs), CompilerRtBits(rhs)));

    public static T __divti3<T>(T lhs, T rhs) where T : unmanaged
        => CompilerRtValue<T>(unchecked((UInt128)__divti3(
            unchecked((Int128)CompilerRtBits(lhs)), unchecked((Int128)CompilerRtBits(rhs)))));

    public static T __modti3<T>(T lhs, T rhs) where T : unmanaged
        => CompilerRtValue<T>(unchecked((UInt128)__modti3(
            unchecked((Int128)CompilerRtBits(lhs)), unchecked((Int128)CompilerRtBits(rhs)))));

    private static UInt128 CompilerRtBits<T>(T value) where T : unmanaged
        => Unsafe.BitCast<T, UInt128>(value);

    private static T CompilerRtValue<T>(UInt128 value) where T : unmanaged
        => Unsafe.BitCast<UInt128, T>(value);
}
