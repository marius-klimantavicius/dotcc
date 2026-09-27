using System;
using CMath = DotCC.Libc.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MathExtendedTests
{
    [Theory]
    [InlineData(2.5, 2L)]
    [InlineData(3.5, 4L)]
    [InlineData(-2.5, -2L)]
    [InlineData(-3.5, -4L)]
    [InlineData(5000000000.0, 5000000000L)]
    public void Lrint_preserves_LP64_width_and_default_ties_even_rounding(double value, long expected)
        => CMath.lrint(value).ShouldBe(expected);

    [Fact]
    public void Lrint_invalid_range_uses_shared_integer_conversion_contract()
    {
        CMath.lrint(double.NaN).ShouldBe(long.MinValue);
        CMath.lrint(double.PositiveInfinity).ShouldBe(long.MinValue);
        CMath.lrint(double.NegativeInfinity).ShouldBe(long.MinValue);
        CMath.lrint(9223372036854775808.0).ShouldBe(long.MinValue);
        CMath.lrint(-9223372036854775808.0).ShouldBe(long.MinValue);
    }

    [Fact]
    public void Signbit_observes_sign_of_zero_and_nan_without_numeric_comparison()
    {
        foreach (long bits in new[] { 0L, 1L, 0x3ff0000000000000L, 0x7ff0000000000000L, 0x7ff8000000000001L })
        {
            CMath.signbit(BitConverter.Int64BitsToDouble(bits)).ShouldBe(0);
            CMath.signbit(BitConverter.Int64BitsToDouble(bits | long.MinValue)).ShouldBe(1);
        }
        CMath.signbit(BitConverter.Int32BitsToSingle(unchecked((int)0xffc00001))).ShouldBe(1);
        CMath.signbit(BitConverter.Int32BitsToSingle(0x7fc00001)).ShouldBe(0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1e-20)]
    [InlineData(-1e-20)]
    [InlineData(double.Epsilon)]
    [InlineData(-double.Epsilon)]
    public void Tiny_values_and_signed_zero_survive_cancellation(double x)
    {
        BitConverter.DoubleToInt64Bits(CMath.expm1(x)).ShouldBe(BitConverter.DoubleToInt64Bits(x));
        BitConverter.DoubleToInt64Bits(CMath.log1p(x)).ShouldBe(BitConverter.DoubleToInt64Bits(x));
    }

    [Fact]
    public void Domain_and_nonfinite_values_match_C_math()
    {
        double minusZero = BitConverter.Int64BitsToDouble(long.MinValue);
        BitConverter.DoubleToInt64Bits(CMath.expm1(minusZero)).ShouldBe(long.MinValue);
        BitConverter.DoubleToInt64Bits(CMath.log1p(minusZero)).ShouldBe(long.MinValue);
        CMath.expm1(double.NegativeInfinity).ShouldBe(-1);
        CMath.expm1(double.PositiveInfinity).ShouldBe(double.PositiveInfinity);
        double.IsNaN(CMath.expm1(double.NaN)).ShouldBeTrue();
        CMath.log1p(-1d).ShouldBe(double.NegativeInfinity);
        CMath.log1p(double.PositiveInfinity).ShouldBe(double.PositiveInfinity);
        double.IsNaN(CMath.log1p(double.NaN)).ShouldBeTrue();
        double.IsNaN(CMath.log1p(Math.BitDecrement(-1))).ShouldBeTrue();
        CMath.log1p(Math.BitIncrement(-1)).ShouldBe(-36.7368005696771, 1e-14);
        CMath.hypot(double.NaN, double.PositiveInfinity).ShouldBe(double.PositiveInfinity);
        CMath.hypot(double.NegativeInfinity, double.NaN).ShouldBe(double.PositiveInfinity);
        BitConverter.DoubleToInt64Bits(CMath.hypot(-0.0, -0.0)).ShouldBe(0);
    }
}
