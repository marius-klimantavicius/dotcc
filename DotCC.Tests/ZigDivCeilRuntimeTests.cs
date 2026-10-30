using System;
using System.Numerics;
using DotCC.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigDivCeilRuntimeTests
{
    [Fact]
    public void All_integer_carriers_preserve_sign_rounding_and_limit_bits()
    {
        Check<sbyte>(); Check<byte>(); Check<short>(); Check<ushort>(); Check<int>();
        Check<uint>(); Check<long>(); Check<ulong>(); Check<Int128>(); Check<UInt128>();
    }

    private static void Check<T>() where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        ZigMath.DivCeil(T.CreateChecked(7), T.CreateChecked(3)).ShouldBe(T.CreateChecked(3));
        ZigMath.DivCeil(T.Zero, T.CreateChecked(3)).ShouldBe(T.Zero);
        ZigMath.DivCeil(T.MaxValue, T.One).ShouldBe(T.MaxValue);
        // BigInteger gives an independent boundary expectation without overflowing T.
        var half = T.CreateChecked((BigInteger.CreateChecked(T.MaxValue) + 1) / 2);
        ZigMath.DivCeil(T.MaxValue, T.CreateChecked(2)).ShouldBe(half);
        ZigMath.DivCeil(T.MaxValue, T.MaxValue - T.One).ShouldBe(T.CreateChecked(2));
        Should.Throw<DivideByZeroException>(() => ZigMath.DivCeil(T.One, T.Zero));
        if (!T.IsNegative(T.MinValue)) return;
        ZigMath.DivCeil(T.CreateChecked(-7), T.CreateChecked(3)).ShouldBe(T.CreateChecked(-2));
        ZigMath.DivCeil(T.CreateChecked(7), T.CreateChecked(-3)).ShouldBe(T.CreateChecked(-2));
        ZigMath.DivCeil(T.CreateChecked(-7), T.CreateChecked(-3)).ShouldBe(T.CreateChecked(3));
        ZigMath.DivCeil(T.CreateChecked(-6), T.CreateChecked(3)).ShouldBe(T.CreateChecked(-2));
        ZigMath.DivCeil(T.MinValue, T.One).ShouldBe(T.MinValue);
        Should.Throw<OverflowException>(() => ZigMath.DivCeil(T.MinValue, -T.One));
    }

    [Fact]
    public void Logical_signed_width_rejects_overflow_in_a_wider_carrier()
    {
        ZigMath.DivCeil<sbyte>(-4, 3, 3).ShouldBe((sbyte)-1);
        ZigMath.DivCeil<sbyte>(-4, -2, 3).ShouldBe((sbyte)2);
        Should.Throw<OverflowException>(() => ZigMath.DivCeil<sbyte>(-4, -1, 3));
        Should.Throw<ArgumentOutOfRangeException>(() => ZigMath.DivCeil<sbyte>(1, 1, 9));
    }
}
