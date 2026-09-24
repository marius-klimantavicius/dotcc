using System;
using System.Text;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class NumericMathTests
{
    [Theory]
    [InlineData(0.5, 0L, 1L)]
    [InlineData(1.5, 2L, 2L)]
    [InlineData(2.5, 2L, 3L)]
    [InlineData(-0.5, 0L, -1L)]
    [InlineData(-1.5, -2L, -2L)]
    [InlineData(-2.5, -2L, -3L)]
    public void Integer_rounding_distinguishes_default_even_from_away(double value, long even, long away)
    {
        RuntimeLibc.llrint(value).ShouldBe(even);
        RuntimeLibc.llroundl(value).ShouldBe(away);
    }

    [Fact]
    public void Integer_rounding_handles_binary64_range_boundary_and_domain_errors()
    {
        RuntimeLibc.errno = 0;
        RuntimeLibc.llrint(-9223372036854775808.0).ShouldBe(long.MinValue);
        RuntimeLibc.errno.ShouldBe(0);
        RuntimeLibc.llroundl(9223372036854774784.0).ShouldBe(9223372036854774784L);
        foreach (double bad in new[] { 9223372036854775808.0, double.PositiveInfinity, double.NaN })
        {
            RuntimeLibc.errno = 0;
            RuntimeLibc.llrint(bad).ShouldBe(long.MinValue);
            RuntimeLibc.errno.ShouldBe(RuntimeLibc.EDOM);
        }
    }

    [Theory]
    [InlineData(3.75, 3.0, 0.75)]
    [InlineData(-3.75, -3.0, -0.75)]
    [InlineData(-0.0, -0.0, -0.0)]
    [InlineData(-3.0, -3.0, -0.0)]
    [InlineData(double.NegativeInfinity, double.NegativeInfinity, -0.0)]
    public void Modf_preserves_signed_zero_and_integral_fractional_parts(double value, double expectedWhole, double expectedPart)
    {
        double whole;
        double part = RuntimeLibc.modf(value, &whole);
        BitConverter.DoubleToInt64Bits(whole).ShouldBe(BitConverter.DoubleToInt64Bits(expectedWhole));
        BitConverter.DoubleToInt64Bits(part).ShouldBe(BitConverter.DoubleToInt64Bits(expectedPart));
    }

    [Fact]
    public void Classification_preserves_width_and_nonfinite_values()
    {
        RuntimeLibc.fpclassify(float.Epsilon).ShouldBe(RuntimeLibc.FP_SUBNORMAL);
        RuntimeLibc.fpclassify((double)float.Epsilon).ShouldBe(RuntimeLibc.FP_NORMAL);
        RuntimeLibc.fpclassify(double.Epsilon).ShouldBe(RuntimeLibc.FP_SUBNORMAL);
        RuntimeLibc.fpclassify(-0.0).ShouldBe(RuntimeLibc.FP_ZERO);
        RuntimeLibc.fpclassify(double.PositiveInfinity).ShouldBe(RuntimeLibc.FP_INFINITE);
        RuntimeLibc.fpclassify(double.NaN).ShouldBe(RuntimeLibc.FP_NAN);
        double whole;
        double.IsNaN(RuntimeLibc.modf(double.NaN, &whole)).ShouldBeTrue();
        double.IsNaN(whole).ShouldBeTrue();
        BitConverter.DoubleToInt64Bits(RuntimeLibc.ceill(-0.25)).ShouldBe(long.MinValue);
        RuntimeLibc.ceill(2.25).ShouldBe(3.0);
    }

    [Theory]
    [InlineData("1e99999", double.PositiveInfinity, 34)]
    [InlineData("-1e-99999", -0.0, 34)]
    [InlineData("0e-99999", 0.0, 0)]
    [InlineData("-inf", double.NegativeInfinity, 0)]
    [InlineData("  -0x1.8p+2rest", -6.0, 0)]
    public void Long_double_parser_reports_range_and_keeps_end_pointer(string text, double expected, int error)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text + "\0");
        fixed (byte* input = bytes)
        {
            byte* end;
            RuntimeLibc.errno = 0;
            double actual = RuntimeLibc.strtold(input, &end);
            BitConverter.DoubleToInt64Bits(actual).ShouldBe(BitConverter.DoubleToInt64Bits(expected));
            RuntimeLibc.errno.ShouldBe(error);
            (end - input).ShouldBe(text.EndsWith("rest") ? text.Length - 4L : text.Length);
        }
    }
}
