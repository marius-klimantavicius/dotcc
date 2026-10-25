using System;
using CMath = DotCC.Libc.Libc;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed class MathExponentTests
{
    [Theory]
    [InlineData(1.0, 0)]
    [InlineData(-8.0, 3)]
    [InlineData(0.75, -1)]
    [InlineData(double.Epsilon, -1074)]
    [InlineData(double.MaxValue, 1023)]
    public void Binary_exponents_include_subnormal_and_negative_values(double value, int expected)
    {
        CMath.errno = 0;
        CMath.ilogb(value).ShouldBe(expected);
        CMath.logb(value).ShouldBe((double)expected);
        CMath.errno.ShouldBe(0);
    }

    [Fact]
    public void Exponent_special_values_preserve_documented_errno_and_integer_results()
    {
        foreach (double value in new[] { 0.0, -0.0, double.NaN })
        {
            CMath.errno = 0; CMath.ilogb(value).ShouldBe(int.MinValue); CMath.errno.ShouldBe(CMath.EDOM);
        }
        CMath.errno = 0; CMath.ilogb(double.PositiveInfinity).ShouldBe(int.MaxValue); CMath.errno.ShouldBe(CMath.EDOM);
        CMath.errno = CMath.EDOM; CMath.logb(0).ShouldBe(double.NegativeInfinity); CMath.errno.ShouldBe(CMath.EDOM);
        CMath.errno = 0; CMath.logb(double.NegativeInfinity).ShouldBe(double.PositiveInfinity); CMath.errno.ShouldBe(0);
        double.IsNaN(CMath.logb(double.NaN)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(7.0, 2.0, -1.0)]
    [InlineData(5.0, 2.0, 1.0)]
    [InlineData(-7.0, 2.0, 1.0)]
    [InlineData(7.0, -2.0, -1.0)]
    [InlineData(1.0, double.PositiveInfinity, 1.0)]
    public void Remainder_uses_nearest_even_quotient(double x, double y, double expected)
        => CMath.remainder(x, y).ShouldBe(expected);

    [Fact]
    public void Remainder_keeps_signed_zero_and_domain_errno()
    {
        BitConverter.DoubleToInt64Bits(CMath.remainder(-4, 2)).ShouldBe(long.MinValue);
        CMath.errno = 0; double.IsNaN(CMath.remainder(1, 0)).ShouldBeTrue(); CMath.errno.ShouldBe(CMath.EDOM);
        CMath.errno = 0; double.IsNaN(CMath.remainder(double.PositiveInfinity, 2)).ShouldBeTrue(); CMath.errno.ShouldBe(CMath.EDOM);
        CMath.errno = 0; double.IsNaN(CMath.remainder(double.NaN, 0)).ShouldBeTrue(); CMath.errno.ShouldBe(0);
    }
}
