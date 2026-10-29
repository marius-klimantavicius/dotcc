using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

public sealed unsafe class CompilerRtArithmeticTests
{
    [Theory]
    [InlineData(0, int.MinValue, 0, 0)]
    [InlineData(int.MinValue, 1, int.MinValue, 0)]
    [InlineData(int.MinValue, -1, int.MinValue, 1)]
    [InlineData(int.MaxValue, 2, -2, 1)]
    [InlineData(46340, 46340, 2147395600, 0)]
    [InlineData(46341, 46341, -2147479015, 1)]
    [InlineData(-46341, 46341, 2147479015, 1)]
    [InlineData(int.MinValue, int.MinValue, 0, 1)]
    public void Signed_32_bit_product_returns_low_bits_and_overwrites_overflow(int lhs, int rhs, int result, int flag)
    {
        int overflow = 73;
        __mulosi4(lhs, rhs, &overflow).ShouldBe(result);
        overflow.ShouldBe(flag);
    }

    [Fact]
    public void Signed_128_bit_division_and_remainder_match_independent_BigInteger_magnitudes()
    {
        Int128[] values = [Int128.MinValue, Int128.MinValue + 1, -((Int128)1 << 100), -17, -1, 0, 1, 17,
            ((Int128)1 << 100) + 1, Int128.MaxValue];
        Int128[] divisors = [Int128.MinValue, -((Int128)1 << 64) - 3, -5, -1, 1, 5, ((Int128)1 << 64) + 3, Int128.MaxValue];
        foreach (Int128 lhs in values)
        foreach (Int128 rhs in divisors)
        {
            BigInteger quotient = BigInteger.DivRem((BigInteger)lhs, (BigInteger)rhs, out var remainder);
            __divti3(lhs, rhs).ShouldBe(SignedLowBits(quotient));
            __modti3(lhs, rhs).ShouldBe((Int128)remainder);
        }
        __divti3(Int128.MinValue, -1).ShouldBe(Int128.MinValue);
        __modti3(Int128.MinValue, -1).ShouldBe((Int128)0);
    }

    [Fact]
    public void Unsigned_128_bit_high_limb_division_and_remainder_match_BigInteger()
    {
        UInt128[] values = [0, 1, 17, (UInt128)1 << 64, ((UInt128)1 << 127) + 7, UInt128.MaxValue];
        UInt128[] divisors = [1, 2, 5, ((UInt128)1 << 64) + 3, (UInt128)1 << 127, UInt128.MaxValue];
        foreach (UInt128 lhs in values)
        foreach (UInt128 rhs in divisors)
        {
            BigInteger quotient = BigInteger.DivRem((BigInteger)lhs, (BigInteger)rhs, out var remainder);
            __udivti3(lhs, rhs).ShouldBe((UInt128)quotient);
            __umodti3(lhs, rhs).ShouldBe((UInt128)remainder);
        }
    }

    [Fact]
    public void Multiplication_keeps_low_128_bits_across_both_limbs_and_signed_overflow()
    {
        Int128[] values = [Int128.MinValue, -((Int128)1 << 100), -3, -1, 0, 1, 7,
            ((Int128)1 << 64) + 1, Int128.MaxValue];
        foreach (Int128 lhs in values)
        foreach (Int128 rhs in values)
            __multi3(lhs, rhs).ShouldBe(SignedLowBits((BigInteger)lhs * (BigInteger)rhs));
    }

    [Fact]
    public void Zig_two_limb_aggregate_signatures_preserve_exact_integer_bits()
    {
        var minimum = new SignedLimbs { Lo = 0, Hi = long.MinValue };
        var minusOne = new SignedLimbs { Lo = ulong.MaxValue, Hi = -1 };
        var negative17 = new SignedLimbs { Lo = unchecked((ulong)-17L), Hi = -1 };
        var five = new SignedLimbs { Lo = 5, Hi = 0 };
        Int128 Signed(SignedLimbs value) => Unsafe.BitCast<SignedLimbs, Int128>(value);
        Signed(__multi3(minimum, minusOne)).ShouldBe(Int128.MinValue);
        Signed(__divti3(minimum, minusOne)).ShouldBe(Int128.MinValue);
        Signed(__modti3(minimum, minusOne)).ShouldBe((Int128)0);
        Signed(__divti3(negative17, five)).ShouldBe((Int128)(-3));
        Signed(__modti3(negative17, five)).ShouldBe((Int128)(-2));
        var maximum = new UnsignedLimbs { Lo = ulong.MaxValue, Hi = ulong.MaxValue };
        var two = new UnsignedLimbs { Lo = 2, Hi = 0 };
        var quotient = __udivti3(maximum, two);
        quotient.Lo.ShouldBe(ulong.MaxValue); quotient.Hi.ShouldBe(0x7fffffffffffffffUL);
        var remainder = __umodti3(maximum, two);
        remainder.Lo.ShouldBe(1UL); remainder.Hi.ShouldBe(0UL);
    }

    [Fact]
    public void Zero_divisors_fail_instead_of_returning_a_substitute_value()
    {
        Should.Throw<DivideByZeroException>(() => __udivti3((UInt128)7, UInt128.Zero));
        Should.Throw<DivideByZeroException>(() => __umodti3((UInt128)7, UInt128.Zero));
        Should.Throw<DivideByZeroException>(() => __divti3((Int128)(-7), Int128.Zero));
        Should.Throw<DivideByZeroException>(() => __modti3((Int128)(-7), Int128.Zero));
        var seven = new SignedLimbs { Lo = 7 };
        Should.Throw<DivideByZeroException>(() => __divti3(seven, default(SignedLimbs)));
    }

    [Fact]
    public void Aggregate_overloads_reject_non_128_bit_storage()
    {
        Should.Throw<NotSupportedException>(() => __multi3(1UL, 2UL));
        Should.Throw<NotSupportedException>(() => __udivti3(1UL, 2UL));
    }

    private static Int128 SignedLowBits(BigInteger value)
    {
        BigInteger modulus = BigInteger.One << 128;
        BigInteger bits = value & (modulus - 1);
        if (bits >= (BigInteger.One << 127)) bits -= modulus;
        return (Int128)bits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SignedLimbs { public ulong Lo; public long Hi; }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnsignedLimbs { public ulong Lo; public ulong Hi; }
}
