using System;
using System.Numerics;

namespace Edg.Runtime;

// Bit-exact classification, decomposition and power-of-two scaling. Scaling
// currently implements round-to-nearest, ties-to-even only. This class does
// not establish a floating environment, raise exception flags or set errno.
public static unsafe class LibcFloat
{
    public const int Nan = 0, Infinite = 1, Zero = 2, Subnormal = 3, Normal = 4;
    public static int SignBit(float value) => (int)(BitConverter.SingleToUInt32Bits(value) >> 31);
    public static int SignBit(double value) => (int)(BitConverter.DoubleToUInt64Bits(value) >> 63);
    public static int SignBit(Float80 value) => value.SignExponent >> 15;
    public static int IsFinite(float value) => (BitConverter.SingleToUInt32Bits(value) & 0x7f800000U) != 0x7f800000U ? 1 : 0;
    public static int IsFinite(double value) => (BitConverter.DoubleToUInt64Bits(value) & 0x7ff0000000000000UL) != 0x7ff0000000000000UL ? 1 : 0;
    public static int IsFinite(Float80 value) => (value.SignExponent & 0x7fff) != 0x7fff ? 1 : 0;
    public static int Classify(float value) => ClassifyBits(BitConverter.SingleToUInt32Bits(value), 23, 255);
    public static int Classify(double value) => ClassifyBits(BitConverter.DoubleToUInt64Bits(value), 52, 2047);
    private static int ClassifyBits(ulong bits, int width, int maximum)
    {
        ulong fraction = bits & ((1UL << width) - 1);
        int exponent = (int)((bits >> width) & (uint)maximum);
        return exponent == maximum ? fraction == 0 ? Infinite : Nan :
            exponent == 0 ? fraction == 0 ? Zero : Subnormal : Normal;
    }
    public static int Classify(Float80 value)
    {
        int exponent = value.SignExponent & 0x7fff;
        if (exponent == 0x7fff) return value.Significand == 0x8000000000000000UL ? Infinite : Nan;
        if (value.Significand == 0) return Zero;
        return exponent == 0 && value.Significand < 0x8000000000000000UL ? Subnormal : Normal;
    }
    public static Float80 Frexp(Float80 value, int* exponent)
    {
        int rawExponent = value.SignExponent & 0x7fff;
        if (rawExponent == 0x7fff || value.Significand == 0) { *exponent = 0; return value; }
        int shift = BitOperations.LeadingZeroCount(value.Significand);
        *exponent = Math.Max(rawExponent, 1) - 16383 + 1 - shift;
        return new Float80(value.Significand << shift, (ushort)((value.SignExponent & 0x8000) | 16382));
    }
    public static double Frexp(double value, int* exponent) => BitConverter.UInt64BitsToDouble(
        FrexpBits(BitConverter.DoubleToUInt64Bits(value), 52, 1023, 2047, exponent));
    public static float Frexp(float value, int* exponent) => BitConverter.UInt32BitsToSingle((uint)
        FrexpBits(BitConverter.SingleToUInt32Bits(value), 23, 127, 255, exponent));
    private static ulong FrexpBits(ulong bits, int width, int bias, int maximum, int* exponent)
    {
        ulong hidden = 1UL << width, fraction = bits & (hidden - 1);
        int raw = (int)((bits >> width) & (uint)maximum);
        if (raw == maximum || (raw == 0 && fraction == 0)) { *exponent = 0; return bits; }
        ulong significand = raw == 0 ? fraction : fraction | hidden;
        int shift = BitOperations.LeadingZeroCount(significand) - (63 - width);
        *exponent = Math.Max(raw, 1) - bias + 1 - shift;
        ulong sign = bits & ((ulong)(maximum + 1) << width);
        return sign | ((ulong)(bias - 1) << width) | ((significand << shift) & (hidden - 1));
    }
    private static ulong RoundRight(ulong value, long shift)
    {
        if (shift > 64) return 0;
        if (shift == 64) return value > 0x8000000000000000UL ? 1UL : 0;
        int count = (int)shift;
        ulong quotient = value >> count, remainder = value & ((1UL << count) - 1);
        ulong half = 1UL << (count - 1);
        return quotient + (remainder > half || (remainder == half && (quotient & 1) != 0) ? 1UL : 0);
    }
    public static Float80 ScaleB(Float80 value, int power)
    {
        int raw = value.SignExponent & 0x7fff;
        if (raw == 0x7fff || value.Significand == 0) return value;
        int shift = BitOperations.LeadingZeroCount(value.Significand);
        ulong significand = value.Significand << shift;
        long target = (long)Math.Max(raw, 1) + power - shift;
        ushort sign = (ushort)(value.SignExponent & 0x8000);
        if (target >= 0x7fff) return new Float80(0x8000000000000000UL, (ushort)(sign | 0x7fff));
        if (target > 0) return new Float80(significand, (ushort)(sign | target));
        significand = RoundRight(significand, 1 - target);
        return new Float80(significand, (ushort)(sign | (significand == 0x8000000000000000UL ? 1 : 0)));
    }
    public static double ScaleB(double value, int power) => BitConverter.UInt64BitsToDouble(
        ScaleBits(BitConverter.DoubleToUInt64Bits(value), power, 52, 2047));
    public static float ScaleB(float value, int power) => BitConverter.UInt32BitsToSingle((uint)
        ScaleBits(BitConverter.SingleToUInt32Bits(value), power, 23, 255));
    private static ulong ScaleBits(ulong bits, int power, int width, int maximum)
    {
        ulong hidden = 1UL << width, fraction = bits & (hidden - 1);
        int raw = (int)((bits >> width) & (uint)maximum);
        if (raw == maximum || (raw == 0 && fraction == 0)) return bits;
        ulong significand = raw == 0 ? fraction : fraction | hidden;
        int shift = BitOperations.LeadingZeroCount(significand) - (63 - width);
        significand <<= shift;
        long target = (long)Math.Max(raw, 1) + power - shift;
        ulong sign = bits & ((ulong)(maximum + 1) << width);
        if (target >= maximum) return sign | ((ulong)maximum << width);
        if (target > 0) return sign | ((ulong)target << width) | (significand & (hidden - 1));
        return sign | RoundRight(significand, 1 - target);
    }
}
