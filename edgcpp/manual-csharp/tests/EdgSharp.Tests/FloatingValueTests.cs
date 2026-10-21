// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored numeric foundation tests. These do not claim parser/type/evaluator
// integration or substitute native acceptance for a managed semantic result.
using System.Numerics;
using EdgSharp;

internal static partial class Tests
{
    private static FloatingLiteralResult FloatingDecode(string spelling, FloatingLimits? limits = null)
        => FloatingLiterals.Decode(spelling, FloatingLiteralProfile.Binary32Binary64, limits);

    private static TargetFloatValue FloatingNumber(string spelling)
    {
        FloatingLiteralResult result = FloatingDecode(spelling);
        Equal(FloatingDecodeStatus.Decoded, result.Status);
        return result.Value ?? throw new Exception("Missing target floating value for " + spelling);
    }

    private static void FloatingBits(ulong bits, FloatingResult result, FloatingFlags flags = FloatingFlags.None)
    {
        Equal(bits, result.Value?.Bits ?? throw new Exception("Missing target floating result."));
        Equal(flags, result.Flags);
    }

    private static void FloatingLiteralBits(string spelling, ulong bits, FloatingFlags flags = FloatingFlags.None)
    {
        FloatingLiteralResult result = FloatingDecode(spelling);
        Equal(FloatingDecodeStatus.Decoded, result.Status);
        FloatingBits(bits, result.Numeric, flags);
    }

    public static void FloatingRepresentations()
    {
        TargetFloatFormat f = TargetFloatFormat.Binary32, d = TargetFloatFormat.Binary64;
        Equal(24, f.Precision); Equal(-126, f.MinNormalExponent); Equal(127, f.MaxNormalExponent); Equal(-149, f.MinSubnormalExponent);
        Equal(53, d.Precision); Equal(-1022, d.MinNormalExponent); Equal(1023, d.MaxNormalExponent); Equal(-1074, d.MinSubnormalExponent);
        foreach (var sample in new (TargetFloatFormat Format, ulong Bits, TargetFloatClass Class)[]
        {
            (f, 0, TargetFloatClass.Zero), (f, 0x80000000, TargetFloatClass.Zero),
            (f, 1, TargetFloatClass.Subnormal), (f, 0x007fffff, TargetFloatClass.Subnormal),
            (f, 0x00800000, TargetFloatClass.Normal), (f, 0x7f7fffff, TargetFloatClass.Normal),
            (f, 0x7f800000, TargetFloatClass.Infinity), (f, 0xff800000, TargetFloatClass.Infinity),
            (f, 0x7fc12345, TargetFloatClass.NaN), (f, 0x7f812345, TargetFloatClass.NaN),
            (d, 1, TargetFloatClass.Subnormal), (d, 0x0010000000000000, TargetFloatClass.Normal),
            (d, 0x7fefffffffffffff, TargetFloatClass.Normal), (d, 0xfff0000000000000, TargetFloatClass.Infinity),
            (d, 0x7ff8000000000123, TargetFloatClass.NaN)
        })
        {
            var value = TargetFloatValue.FromBits(sample.Format, sample.Bits);
            Equal(sample.Bits, value.Bits); Equal(sample.Class, value.Class);
            Equal(sample.Bits, value.Negate().Negate().Bits);
        }
        Throws<ArgumentOutOfRangeException>(() => TargetFloatValue.FromBits(f, 0x100000000));
        Throws<ArgumentOutOfRangeException>(() => TargetFloatValue.NaN(f, 0, signaling: true));
        Throws<ArgumentOutOfRangeException>(() => TargetFloatValue.NaN(f, 0x400000));
        var quiet = TargetFloatValue.NaN(f, 0x12345, negative: true);
        Equal(0xffc12345UL, quiet.Bits); Equal(0x12345UL, quiet.NaNPayload); Check(!quiet.IsSignalingNaN);
        var signaling = TargetFloatValue.NaN(f, 0x12345, signaling: true);
        Check(signaling.IsSignalingNaN); Equal(0x7fc12345UL, signaling.QuietNaN().Bits);
    }

    public static void FloatingGrammar()
    {
        foreach (string spelling in new[] { "1.", "1.e0", "01.0", ".1e1", "1e+0", "1E-0", "0x1p0", "0X.8P1", "0x1.p0", "0x0001.00p+0" })
            FloatingLiteralBits(spelling, 0x3ff0000000000000);
        FloatingLiteralBits("1.0F", 0x3f800000);
        FloatingLiteralBits("1'2.5'0e+0'1f", 0x42fa0000); // 125, separators in all three digit sequences.
        FloatingLiteralBits("0xA'B.C'Dp-0'4", 0x402579a000000000); // 0xA.BCD = 10.737548828125.
        Equal(4, FloatingDecode("1.25f").SuffixOffset);
        Equal(FloatingLiteralSuffix.Float, FloatingDecode("1.25f").Suffix);
        Check(FloatingDecode("0x1p0").IsHexadecimal);
        Check(FloatingDecode("1.25").IsExact);
    }

    public static void FloatingDecimalRounding()
    {
        FloatingLiteralBits("0.1f", 0x3dcccccd, FloatingFlags.Inexact);
        FloatingLiteralBits("0.1", 0x3fb999999999999a, FloatingFlags.Inexact);
        FloatingLiteralBits("16777217.0f", 0x4b800000, FloatingFlags.Inexact);
        FloatingLiteralBits("16777217.0", 0x4170000010000000);
        FloatingLiteralBits("1.000000059604644775390624f", 0x3f800000, FloatingFlags.Inexact);
        FloatingLiteralBits("1.000000059604644775390625f", 0x3f800000, FloatingFlags.Inexact);
        FloatingLiteralBits("1.000000059604644775390626f", 0x3f800001, FloatingFlags.Inexact);
        FloatingLiteralBits("1.000000178813934326171875f", 0x3f800002, FloatingFlags.Inexact);
        FloatingLiteralBits("1.00000000000000011102230246251565404236316680908203125", 0x3ff0000000000000, FloatingFlags.Inexact);
        FloatingLiteralBits("1.00000000000000011102230246251565404236316680908203126", 0x3ff0000000000001, FloatingFlags.Inexact);
        FloatingLiteralBits("9007199254740993.0", 0x4340000000000000, FloatingFlags.Inexact);
        FloatingLiteralBits("9007199254740995.0", 0x4340000000000002, FloatingFlags.Inexact);
    }

    public static void FloatingHexRounding()
    {
        FloatingLiteralBits("0x1.000001p0f", 0x3f800000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.00000100000001p0f", 0x3f800001, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.000000ffffffffp0f", 0x3f800000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.000003p0f", 0x3f800002, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.ffffffp0f", 0x40000000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.00000000000008p0", 0x3ff0000000000000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.00000000000008000001p0", 0x3ff0000000000001, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.00000000000018p0", 0x3ff0000000000002, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.fffffffffffff8p0", 0x4000000000000000, FloatingFlags.Inexact);
    }

    public static void FloatingDirectRounding()
    {
        // A host-double-first implementation loses this decimal tail and would
        // round a second time to the wrong float. The target float rounds UP.
        const string aboveHalf = "1.0000000596046447753906250000000000000001";
        FloatingLiteralBits(aboveHalf + "f", 0x3f800001, FloatingFlags.Inexact);
        FloatingBits(0x3f800000, FloatingValues.Convert(FloatingNumber(aboveHalf), TargetFloatFormat.Binary32), FloatingFlags.Inexact);
        foreach (var pair in new (string Decimal, string Hex)[]
        {
            ("0.5", "0x1p-1"), ("125.0", "0x1.f4p6"), ("0.1f", "0x1.99999ap-4f"),
            ("0.1", "0x1.999999999999ap-4"), ("1.25e-1", "0x1p-3"),
            ("1.40129846432481707092372958328991613128026194187651577175706828388979108268586060148663818836212158203125e-45f", "0x1p-149f")
        }) Equal(FloatingNumber(pair.Decimal).Bits, FloatingNumber(pair.Hex).Bits);
    }

    public static void FloatingSubnormals()
    {
        FloatingFlags tinyLoss = FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact;
        FloatingLiteralBits("0x1p-149f", 1, FloatingFlags.Tiny);
        FloatingLiteralBits("0x1p-150f", 0, tinyLoss);
        FloatingLiteralBits("0x1.000001p-150f", 1, tinyLoss);
        FloatingLiteralBits("0x3p-150f", 2, tinyLoss);
        FloatingLiteralBits("0x0.fffffep-126f", 0x007fffff, FloatingFlags.Tiny);
        FloatingLiteralBits("0x0.ffffffp-126f", 0x00800000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x0.fffffeffffffp-126f", 0x007fffff, tinyLoss);
        FloatingLiteralBits("0x1p-126f", 0x00800000);
        FloatingLiteralBits("0x1p-1074", 1, FloatingFlags.Tiny);
        FloatingLiteralBits("0x1p-1075", 0, tinyLoss);
        FloatingLiteralBits("0x3p-1075", 2, tinyLoss);
        FloatingLiteralBits("0x0.fffffffffffff7p-1022", 0x000fffffffffffff, tinyLoss);
        FloatingLiteralBits("0x0.fffffffffffff7ffffp-1022", 0x000fffffffffffff, tinyLoss);
        FloatingLiteralBits("0x0.fffffffffffff7ffffffp-1022", 0x000fffffffffffff, tinyLoss);
        FloatingLiteralBits("0x0.fffffffffffff8p-1022", 0x0010000000000000, FloatingFlags.Inexact);
        FloatingLiteralBits("0x0.fffffffffffff8000001p-1022", 0x0010000000000000, FloatingFlags.Inexact);
        FloatingLiteralBits("1e-45f", 1, tinyLoss);
        FloatingLiteralBits("5e-324", 1, tinyLoss);
        FloatingLiteralBits("1e-1000", 0, tinyLoss);
        FloatingBits(0x80000000, FloatingValues.RoundRational(TargetFloatFormat.Binary32, true, 1, 1, -150), tinyLoss);

        // Root-authorized narrow exception to the pinned EDG hex carry/reset
        // defect. Prove the correct choice by exact distances in integer units,
        // independently of the implementation's normalization/DivRem procedure.
        // float input is N*2^-174, N=0xfffffeffffff, with 48 fractional bits.
        BigInteger floatUpper = BigInteger.One << 48;
        BigInteger floatLower = ((BigInteger.One << 23) - 1) << 25;
        BigInteger floatInput = new(0xfffffeffffffUL);
        Equal((BigInteger.One << 24) - 1, floatInput - floatLower);
        Equal((BigInteger.One << 24) + 1, floatUpper - floatInput);
        Equal(floatLower + floatUpper - 2, 2 * floatInput);
        Check(floatInput - floatLower < floatUpper - floatInput);
        FloatingBits(0x007fffff, FloatingValues.RoundRational(TargetFloatFormat.Binary32, false, floatInput, 1, -174), tinyLoss);
        FloatingBits(0x00800000, FloatingValues.RoundRational(TargetFloatFormat.Binary32, false, floatInput + 1, 1, -174), FloatingFlags.Inexact);
        FloatingBits(0x00800000, FloatingValues.RoundRational(TargetFloatFormat.Binary32, false, floatInput + 2, 1, -174), FloatingFlags.Inexact);

        // EXACT native double spelling has 18 fractional hex digits (72 bits):
        // 0x0.fffffffffffff7ffffp-1022 = N*2^-1094. Its midpoint deficit is1.
        BigInteger doubleUpper = BigInteger.One << 72;
        BigInteger doubleLower = ((BigInteger.One << 52) - 1) << 20;
        BigInteger doubleInput = (new BigInteger(0xff) << 64) + 0xfffffffffff7ffffUL;
        Equal((BigInteger.One << 19) - 1, doubleInput - doubleLower);
        Equal((BigInteger.One << 19) + 1, doubleUpper - doubleInput);
        Equal(doubleLower + doubleUpper - 2, 2 * doubleInput);
        Check(doubleInput - doubleLower < doubleUpper - doubleInput);
        FloatingBits(0x000fffffffffffff, FloatingValues.RoundRational(TargetFloatFormat.Binary64, false, doubleInput, 1, -1094), tinyLoss);
        FloatingBits(0x0010000000000000, FloatingValues.RoundRational(TargetFloatFormat.Binary64, false, doubleInput + 1, 1, -1094), FloatingFlags.Inexact);
        FloatingBits(0x0010000000000000, FloatingValues.RoundRational(TargetFloatFormat.Binary64, false, doubleInput + 2, 1, -1094), FloatingFlags.Inexact);
    }

    public static void FloatingOverflow()
    {
        FloatingFlags overflow = FloatingFlags.Overflow | FloatingFlags.Inexact;
        FloatingLiteralBits("0x1.fffffep127f", 0x7f7fffff);
        FloatingLiteralBits("0x1.fffffeffffffp127f", 0x7f7fffff, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.ffffffp127f", 0x7f800000, overflow);
        FloatingLiteralBits("0x1.fffffffffffffp1023", 0x7fefffffffffffff);
        FloatingLiteralBits("0x1.fffffffffffff7p1023", 0x7fefffffffffffff, FloatingFlags.Inexact);
        FloatingLiteralBits("0x1.fffffffffffff8p1023", 0x7ff0000000000000, overflow);
        FloatingLiteralBits("1e1000f", 0x7f800000, overflow);
        FloatingLiteralBits("1e309", 0x7ff0000000000000, overflow);
        FloatingBits(0xfff0000000000000, FloatingValues.RoundRational(TargetFloatFormat.Binary64, true, 1, 1, int.MaxValue), overflow);
        FloatingLiteralBits("0e1000", 0);
    }

    public static void FloatingSignedZero()
    {
        foreach (string spelling in new[] { "0.0f", "0x0p-9999f", "000.000e9999f" })
        {
            TargetFloatValue zero = FloatingNumber(spelling), negative = zero.Negate();
            Equal(0UL, zero.Bits); Equal(0x80000000UL, negative.Bits);
            Check(!zero.Equals(negative)); Check(FloatingValues.NumericallyEqual(zero, negative));
            Check(!FloatingValues.ToBoolean(negative));
        }
        TargetFloatValue p = FloatingNumber("0.0f"), n = p.Negate(), one = FloatingNumber("1.0f");
        FloatingBits(0x80000000, FloatingValues.Add(n, n));
        FloatingBits(0, FloatingValues.Add(n, p));
        FloatingBits(0, FloatingValues.Subtract(one, one));
        FloatingBits(0x80000000, FloatingValues.Subtract(n, p));
        FloatingBits(0, FloatingValues.Subtract(n, n));
        FloatingBits(0x80000000, FloatingValues.Multiply(n, one));
        FloatingBits(0, FloatingValues.Multiply(n, one.Negate()));
        FloatingBits(0x80000000, FloatingValues.Divide(p, one.Negate()));
        FloatingBits(0x8000000000000000, FloatingValues.Convert(n, TargetFloatFormat.Binary64));
    }

    public static void FloatingIntegerConversions()
    {
        TargetFloatFormat f = TargetFloatFormat.Binary32, d = TargetFloatFormat.Binary64;
        FloatingBits(0x4b800000, FloatingValues.FromInteger(f, 16_777_217), FloatingFlags.Inexact);
        FloatingBits(0x4b800002, FloatingValues.FromInteger(f, 16_777_219), FloatingFlags.Inexact);
        FloatingBits(0xcb800000, FloatingValues.FromInteger(f, -16_777_217), FloatingFlags.Inexact);
        FloatingBits(0x4340000000000000, FloatingValues.FromInteger(d, (BigInteger.One << 53) + 1), FloatingFlags.Inexact);
        FloatingBits(0x4340000000000002, FloatingValues.FromInteger(d, (BigInteger.One << 53) + 3), FloatingFlags.Inexact);
        FloatingBits(0x43f0000000000000, FloatingValues.FromInteger(d, ulong.MaxValue), FloatingFlags.Inexact);
        foreach (var item in new (string Text, int Width, bool Signed, BigInteger Value, bool Inexact)[]
        {
            ("127.75", 8, true, 127, true), ("128.75", 8, false, 128, true),
            ("255.75", 8, false, 255, true), ("0.0", 1, true, 0, false), ("0x1p63", 64, false, BigInteger.One << 63, false)
        })
        {
            FloatingIntegerResult converted = FloatingValues.ToInteger(FloatingNumber(item.Text), item.Width, item.Signed);
            Equal((BigInteger?)item.Value, converted.Value);
            Equal(item.Inexact ? FloatingFlags.Inexact : FloatingFlags.None, converted.Flags);
        }
        FloatingIntegerResult negativeBoundary = FloatingValues.ToInteger(FloatingNumber("128.75").Negate(), 8, true);
        Equal((BigInteger?)(-128), negativeBoundary.Value); Equal(FloatingFlags.Inexact, negativeBoundary.Flags);
        FloatingIntegerResult negativeFraction = FloatingValues.ToInteger(FloatingNumber("0.75").Negate(), 8, false);
        Equal((BigInteger?)0, negativeFraction.Value); Equal(FloatingFlags.Inexact, negativeFraction.Flags);
        Equal((BigInteger?)(-(BigInteger.One << 63)), FloatingValues.ToInteger(FloatingNumber("0x1p63").Negate(), 64, true).Value);
        foreach (var item in new (TargetFloatValue Value, int Width, bool Signed)[]
        {
            (FloatingNumber("128.0"), 8, true), (FloatingNumber("129.0").Negate(), 8, true),
            (FloatingNumber("256.0"), 8, false), (FloatingNumber("1.0").Negate(), 8, false),
            (FloatingNumber("0x1p63"), 64, true), (FloatingNumber("0x1p64"), 64, false)
        })
        {
            FloatingIntegerResult failed = FloatingValues.ToInteger(item.Value, item.Width, item.Signed);
            Check(failed.Value == null); Check((failed.Flags & FloatingFlags.IntegerOutOfRange) != 0);
        }
        Check(FloatingValues.ToInteger(TargetFloatValue.Infinity(f), 32, true).Value == null);
        Equal(FloatingFlags.InvalidOperation, FloatingValues.ToInteger(TargetFloatValue.NaN(f), 32, false).Flags);
    }

    public static void FloatingFormatConversions()
    {
        FloatingBits(0x3fb99999a0000000, FloatingValues.Convert(FloatingNumber("0.1f"), TargetFloatFormat.Binary64));
        FloatingBits(0x3dcccccd, FloatingValues.Convert(FloatingNumber("0.1"), TargetFloatFormat.Binary32), FloatingFlags.Inexact);
        FloatingBits(1, FloatingValues.Convert(FloatingNumber("0x1p-149"), TargetFloatFormat.Binary32), FloatingFlags.Tiny);
        FloatingBits(0, FloatingValues.Convert(FloatingNumber("0x1p-150"), TargetFloatFormat.Binary32), FloatingFlags.Inexact | FloatingFlags.Tiny | FloatingFlags.Underflow);
        FloatingBits(0x7f800000, FloatingValues.Convert(FloatingNumber("0x1p128"), TargetFloatFormat.Binary32), FloatingFlags.Inexact | FloatingFlags.Overflow);
        var signaling = TargetFloatValue.NaN(TargetFloatFormat.Binary32, 0x123, signaling: true, negative: true);
        Same(signaling, FloatingValues.Convert(signaling, signaling.Format).Value);
        FloatingResult widened = FloatingValues.Convert(signaling, TargetFloatFormat.Binary64);
        Equal(FloatingFlags.InvalidOperation, widened.Flags);
        TargetFloatValue wideValue = widened.Value ?? throw new Exception("Missing widened NaN.");
        Check(wideValue.IsNaN && wideValue.IsNegative && !wideValue.IsSignalingNaN);
        Equal(0x123UL << 29, wideValue.NaNPayload);
        FloatingResult narrowed = FloatingValues.Convert(wideValue, TargetFloatFormat.Binary32);
        Equal(0xffc00123UL, narrowed.Value!.Bits);
        FloatingBits(0x7fc00000, FloatingValues.Convert(TargetFloatValue.NaN(TargetFloatFormat.Binary64, 1), TargetFloatFormat.Binary32));
        Throws<ArgumentException>(() => FloatingValues.Add(FloatingNumber("1.0"), FloatingNumber("1.0f")));
        Throws<ArgumentException>(() => FloatingValues.Compare(FloatingNumber("1.0"), FloatingNumber("1.0f")));
    }

    public static void FloatingAddition()
    {
        TargetFloatValue one = FloatingNumber("1.0f"), halfUlp = FloatingNumber("0x1p-24f");
        FloatingBits(0x3f800000, FloatingValues.Add(one, halfUlp), FloatingFlags.Inexact);
        FloatingBits(0x3f800002, FloatingValues.Add(TargetFloatValue.FromBits(one.Format, 0x3f800001), halfUlp), FloatingFlags.Inexact);
        FloatingBits(0x34000000, FloatingValues.Subtract(TargetFloatValue.FromBits(one.Format, 0x3f800001), one));
        FloatingBits(0xbf800000, FloatingValues.Subtract(one, FloatingNumber("2.0f")));
        FloatingBits(0x40600000, FloatingValues.Add(FloatingNumber("3.0f"), FloatingNumber("0.5f")));
        FloatingBits(2, FloatingValues.Add(FloatingNumber("0x1p-149f"), FloatingNumber("0x1p-149f")), FloatingFlags.Tiny);
        FloatingBits(0x00800000, FloatingValues.Add(FloatingNumber("0x0.fffffep-126f"), FloatingNumber("0x1p-149f")));
        FloatingBits(0x7f800000, FloatingValues.Add(FloatingNumber("0x1.fffffep127f"), FloatingNumber("0x1.fffffep127f")), FloatingFlags.Overflow | FloatingFlags.Inexact);
        FloatingBits(0x3ff0000000000000, FloatingValues.Add(FloatingNumber("1.0"), FloatingNumber("0x1p-53")), FloatingFlags.Inexact);
    }

    public static void FloatingMultiplicationDivision()
    {
        FloatingBits(0x40400000, FloatingValues.Multiply(FloatingNumber("1.5f"), FloatingNumber("2.0f")));
        FloatingBits(0x3f800002, FloatingValues.Multiply(FloatingNumber("0x1.000002p0f"), FloatingNumber("0x1.000002p0f")), FloatingFlags.Inexact);
        FloatingBits(0x00400000, FloatingValues.Multiply(FloatingNumber("0x1p-126f"), FloatingNumber("0.5f")), FloatingFlags.Tiny);
        FloatingBits(0, FloatingValues.Multiply(FloatingNumber("0x1p-149f"), FloatingNumber("0.5f")), FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact);
        FloatingBits(0x7f800000, FloatingValues.Multiply(FloatingNumber("0x1.fffffep127f"), FloatingNumber("2.0f")), FloatingFlags.Overflow | FloatingFlags.Inexact);
        FloatingBits(0x3fc00000, FloatingValues.Divide(FloatingNumber("3.0f"), FloatingNumber("2.0f")));
        FloatingBits(0x3eaaaaab, FloatingValues.Divide(FloatingNumber("1.0f"), FloatingNumber("3.0f")), FloatingFlags.Inexact);
        FloatingBits(0x3fd5555555555555, FloatingValues.Divide(FloatingNumber("1.0"), FloatingNumber("3.0")), FloatingFlags.Inexact);
        FloatingBits(0x3dcccccd, FloatingValues.Divide(FloatingNumber("1.0f"), FloatingNumber("10.0f")), FloatingFlags.Inexact);
        FloatingBits(0x80000002, FloatingValues.Divide(FloatingNumber("0x3p-149f").Negate(), FloatingNumber("2.0f")), FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact);
    }

    public static void FloatingNonfiniteOperations()
    {
        TargetFloatFormat format = TargetFloatFormat.Binary32;
        TargetFloatValue pInf = TargetFloatValue.Infinity(format), nInf = pInf.Negate(), zero = TargetFloatValue.Zero(format), one = FloatingNumber("1.0f");
        FloatingBits(0x7f800000, FloatingValues.Add(pInf, one));
        FloatingBits(0xff800000, FloatingValues.Multiply(nInf, one));
        FloatingBits(0x7f800000, FloatingValues.Divide(one, zero), FloatingFlags.DivideByZero);
        FloatingBits(0xff800000, FloatingValues.Divide(one, zero.Negate()), FloatingFlags.DivideByZero);
        FloatingBits(0x7f800000, FloatingValues.Divide(pInf, zero));
        FloatingBits(0x80000000, FloatingValues.Divide(one, nInf));
        foreach (FloatingResult invalid in new[] { FloatingValues.Add(pInf, nInf), FloatingValues.Subtract(pInf, pInf),
            FloatingValues.Multiply(zero, pInf), FloatingValues.Divide(zero, zero), FloatingValues.Divide(pInf, nInf) })
        {
            Check(invalid.Value!.IsNaN); Equal(FloatingFlags.InvalidOperation, invalid.Flags);
        }
        var quiet = TargetFloatValue.NaN(format, 0x42, negative: true);
        FloatingBits(quiet.Bits, FloatingValues.Add(quiet, one));
        var signaling = TargetFloatValue.NaN(format, 0x17, signaling: true);
        FloatingBits(0x7fc00017, FloatingValues.Divide(one, signaling), FloatingFlags.InvalidOperation);
        FloatingBits(quiet.Bits, FloatingValues.Multiply(quiet, signaling), FloatingFlags.InvalidOperation);
    }

    public static void FloatingOrdering()
    {
        TargetFloatFormat format = TargetFloatFormat.Binary64;
        TargetFloatValue[] ordered = { TargetFloatValue.Infinity(format, true), FloatingNumber("0x1.fffffffffffffp1023").Negate(),
            FloatingNumber("1.0").Negate(), FloatingNumber("0x1p-1074").Negate(), TargetFloatValue.Zero(format),
            FloatingNumber("0x1p-1074"), FloatingNumber("1.0"), FloatingNumber("0x1.fffffffffffffp1023"), TargetFloatValue.Infinity(format) };
        for (int i = 0; i < ordered.Length; ++i)
            for (int j = 0; j < ordered.Length; ++j)
                Equal(i < j ? FloatingOrder.Less : i > j ? FloatingOrder.Greater : FloatingOrder.Equal, FloatingValues.Compare(ordered[i], ordered[j]));
        foreach (bool signaling in new[] { false, true })
        {
            var nan = TargetFloatValue.NaN(format, 17, signaling);
            Check(FloatingValues.ToBoolean(nan)); Check(!FloatingValues.NumericallyEqual(nan, nan));
            foreach (TargetFloatValue value in ordered)
            {
                Equal(FloatingOrder.Unordered, FloatingValues.Compare(nan, value));
                Equal(FloatingOrder.Unordered, FloatingValues.Compare(value, nan));
            }
        }
    }

    public static void FloatingMalformed()
    {
        foreach (string spelling in new[] { "", ".", ".e1", "0x.p1", "0xp1", "1", "1f", "0x1f", "0x1.2", "1e", "1e+", "1e-", "0x1p", "0x1p+", "0x1p-", "0x1p_tag",
            "1.0e+", "1.0e-", "1.0e'1", "1.0e+_tag", ".1E+", ".1E-", ".1E'1",
            "1.'0", "'1.0", "1'.0", "1.0'", "1.0''1", "1e'1", "1e+'1", "1e1'", "0x'1p0", "0x1.'ap0", "0x1p1'a", "1.0f+", "1.0.0", "1.0 f",
            "+1.0", "-0.0", " 1.0", "1,0", "١.0", "1.0\u00a0", "nan", "inf", "1.0_bad\\uD800", "1.0_bad\\u0041", "1.0_\ud800" })
        {
            FloatingLiteralResult result = FloatingDecode(spelling);
            Equal(FloatingDecodeStatus.Malformed, result.Status);
            Check(result.Value == null, "Malformed spelling produced a target value: " + spelling);
            Check(result.ErrorOffset >= 0 && result.ErrorOffset <= spelling.Length);
        }
        Equal(FloatingLiteralIssue.InvalidSeparator, FloatingDecode("1e+'1").Issue);
        Equal(3, FloatingDecode("1e+'1").ErrorOffset);
        Equal(FloatingLiteralIssue.MissingExponent, FloatingDecode("0x1.0").Issue);
        Equal(FloatingLiteralIssue.InvalidSuffix, FloatingDecode("1.0f+").Issue);
        Equal(FloatingLiteralIssue.MissingExponent, FloatingDecode("1.0e+").Issue);
        Equal(FloatingLiteralIssue.MissingExponent, FloatingDecode("1.0e-").Issue);
        Equal(FloatingLiteralIssue.InvalidSeparator, FloatingDecode("1.0e'1").Issue);
    }

    public static void FloatingUnsupported()
    {
        foreach (var item in new (string Spelling, int SuffixOffset)[]
        {
            ("1.0e", 3), ("1.0e_tag", 3), (".1E_tag", 2), ("1.E", 2), (".1e", 2), ("1.0e_\\u03b1", 3)
        })
        {
            FloatingLiteralResult result = FloatingDecode(item.Spelling);
            Equal(FloatingDecodeStatus.ValidUnsupported, result.Status);
            Equal(FloatingLiteralSuffix.UserDefined, result.Suffix);
            Equal(FloatingLiteralIssue.UserDefinedSuffix, result.Issue);
            Equal(item.SuffixOffset, result.SuffixOffset);
            Equal(item.SuffixOffset, result.ErrorOffset);
            Check(result.Value == null);
        }
        foreach (var item in new (string Suffix, FloatingLiteralSuffix Kind)[]
        {
            ("L", FloatingLiteralSuffix.LongDouble), ("f16", FloatingLiteralSuffix.Float16), ("BF16", FloatingLiteralSuffix.BFloat16),
            ("F32", FloatingLiteralSuffix.Float32), ("f64", FloatingLiteralSuffix.Float64), ("F128", FloatingLiteralSuffix.Float128),
            ("f32x", FloatingLiteralSuffix.Float32x), ("F64x", FloatingLiteralSuffix.Float64x),
            ("W", FloatingLiteralSuffix.Float80Extension), ("q", FloatingLiteralSuffix.Float128Extension),
            ("i", FloatingLiteralSuffix.Imaginary), ("fj", FloatingLiteralSuffix.Imaginary), ("jL", FloatingLiteralSuffix.Imaginary),
            ("_scale", FloatingLiteralSuffix.UserDefined), ("ff", FloatingLiteralSuffix.UserDefined),
            ("_α", FloatingLiteralSuffix.UserDefined), ("_\\u03b1", FloatingLiteralSuffix.UserDefined)
        })
        {
            FloatingLiteralResult result = FloatingDecode("1.0" + item.Suffix);
            Equal(FloatingDecodeStatus.ValidUnsupported, result.Status); Equal(item.Kind, result.Suffix);
            Check(result.Value == null); Equal(3, result.SuffixOffset);
        }
        // Syntax is checked even when the eventual format would be unsupported.
        Equal(FloatingDecodeStatus.Malformed, FloatingDecode("1e+L").Status);
        Equal(FloatingDecodeStatus.Malformed, FloatingDecode("1.0L+").Status);
        var older = new FloatingLiteralProfile(TargetFloatFormat.Binary32, TargetFloatFormat.Binary64, digitSeparators: false, hexadecimal: false);
        Equal(FloatingDecodeStatus.ValidUnsupported, FloatingLiterals.Decode("1'0.0", older).Status);
        Equal(FloatingDecodeStatus.ValidUnsupported, FloatingLiterals.Decode("0x1p0", older).Status);
        Equal(FloatingDecodeStatus.Malformed, FloatingLiterals.Decode("0x1p+", older).Status);
    }

    public static void FloatingResourceLimits()
    {
        var token = new FloatingLimits(maxTokenCharacters: 8);
        Equal(FloatingLiteralIssue.TokenLimit, FloatingDecode("123456789.0", token).Issue);
        var digits = new FloatingLimits(maxSignificandDigits: 4);
        Equal(FloatingLiteralIssue.SignificandLimit, FloatingDecode("12345.0", digits).Issue);
        var exponent = new FloatingLimits(maxExponentDigits: 4);
        Equal(FloatingLiteralIssue.ExponentLimit, FloatingDecode("1e12345", exponent).Issue);
        var bits = new FloatingLimits(maxIntermediateBits: 64);
        Equal(FloatingDecodeStatus.ResourceLimit, FloatingDecode("1.2345678901234567890123456789", bits).Status);
        Equal(FloatingDecodeStatus.ResourceLimit, FloatingDecode(new string('9', 5000) + ".0").Status);
        Equal(FloatingDecodeStatus.ResourceLimit, FloatingDecode("1e" + new string('9', 5000)).Status);
        Equal(FloatingDecodeStatus.ResourceLimit, FloatingDecode("0." + new string('0', 20_000)).Status);
        Equal(FloatingFlags.ResourceLimit, FloatingValues.FromInteger(TargetFloatFormat.Binary64, BigInteger.One << 40_000).Flags);
        Equal(FloatingFlags.ResourceLimit, FloatingValues.FromInteger(TargetFloatFormat.Binary64, -(BigInteger.One << 40_000)).Flags);
        Equal(FloatingFlags.ResourceLimit, FloatingValues.RoundRational(TargetFloatFormat.Binary64, false, 1, BigInteger.One << 40_000).Flags);
        Equal(FloatingFlags.ResourceLimit, FloatingValues.ToInteger(FloatingNumber("1.0"), int.MaxValue, true).Flags);
        Equal(FloatingFlags.ResourceLimit, FloatingValues.Multiply(FloatingNumber("0x1.fffffffffffffp0"), FloatingNumber("0x1.fffffffffffffp0"), bits).Flags);
        FloatingLiteralBits("1e2147483648", 0x7ff0000000000000, FloatingFlags.Overflow | FloatingFlags.Inexact);
        FloatingLiteralBits("1e-2147483648", 0, FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact);
        FloatingLiteralBits("1e" + new string('9', 1000), 0x7ff0000000000000, FloatingFlags.Overflow | FloatingFlags.Inexact);
        FloatingLiteralBits("0e" + new string('9', 1000), 0);
        FloatingBits(0x8000000000000000, FloatingValues.RoundRational(TargetFloatFormat.Binary64, true, 0, 1, int.MinValue));
        FloatingBits(0x8000000000000000, FloatingValues.RoundRational(TargetFloatFormat.Binary64, true, 1, 1, int.MinValue), FloatingFlags.Tiny | FloatingFlags.Underflow | FloatingFlags.Inexact);
        Throws<ArgumentOutOfRangeException>(() => new FloatingLimits(maxTokenCharacters: int.MaxValue));
        Throws<ArgumentOutOfRangeException>(() => new FloatingLimits(maxIntermediateBits: int.MaxValue));
        Throws<ArgumentOutOfRangeException>(() => FloatingValues.RoundRational(TargetFloatFormat.Binary32, false, 1, 0));
        Throws<ArgumentOutOfRangeException>(() => FloatingValues.ToInteger(FloatingNumber("1.0"), 0, true));
    }

    public static void FloatingRecoveryConcurrency()
    {
        Parallel.For(0, 64, iteration =>
        {
            for (int repeat = 0; repeat < 8; ++repeat)
            {
                Equal(FloatingDecodeStatus.Malformed, FloatingDecode("0x1p+").Status);
                Equal(FloatingDecodeStatus.ResourceLimit, FloatingDecode("123456789.0", new FloatingLimits(maxTokenCharacters: 8)).Status);
                Equal(FloatingDecodeStatus.ValidUnsupported, FloatingDecode("1.0L").Status);
                FloatingLiteralBits("0.1f", 0x3dcccccd, FloatingFlags.Inexact);
                FloatingLiteralBits("0.1", 0x3fb999999999999a, FloatingFlags.Inexact);
                TargetFloatValue zero = TargetFloatValue.Zero(iteration % 2 == 0 ? TargetFloatFormat.Binary32 : TargetFloatFormat.Binary64, true);
                Check(FloatingValues.Convert(zero, TargetFloatFormat.Binary64).Value!.IsNegative);
            }
        });
    }

    public static void FloatingWideningRoundTrips()
    {
        uint state = 0x13579bdf;
        for (int i = 0; i < 512; ++i)
        {
            // Reproducible raw finite bit patterns across signs/exponents,
            // including tiny values. No CLR floating oracle participates.
            state = unchecked(state * 1664525 + 1013904223);
            uint bits = state;
            if ((bits & 0x7f800000) == 0x7f800000) bits ^= 0x00800000;
            TargetFloatValue original = TargetFloatValue.FromBits(TargetFloatFormat.Binary32, bits);
            FloatingResult widened = FloatingValues.Convert(original, TargetFloatFormat.Binary64);
            Check(widened.IsExact);
            FloatingResult restored = FloatingValues.Convert(widened.Value!, TargetFloatFormat.Binary32);
            Equal(original.Bits, restored.Value!.Bits); Check(restored.IsExact);
        }
        // All integers in this range are exactly representable in both formats.
        for (int value = -2048; value <= 2048; ++value)
        {
            FloatingResult represented = FloatingValues.FromInteger(TargetFloatFormat.Binary32, value);
            Check(represented.IsExact);
            Equal((BigInteger?)value, FloatingValues.ToInteger(represented.Value!, 32, true).Value);
        }
    }
}
