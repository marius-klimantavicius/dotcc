using System;
using System.Numerics;
namespace Edg.Runtime;

// C conversions round directly to the destination format, without an
// intermediate double (which can lose binary80 bits or double-round float).
public readonly partial struct Float128
{
    private static Float128 FromUnsigned(UInt128 value)
    {
        if (value == 0) return Zero;
        int high = 127-(int)UInt128.LeadingZeroCount(value);
        UInt128 significand;
        if (high <= 112) significand = value << (112-high);
        else {
            significand = RoundShiftRight(value,high-112);
            if ((significand >> 113) != 0) { significand >>= 1; ++high; }
        }
        return FromBits(((UInt128)(high+16383)<<112) | (significand & ((UInt128.One<<112)-1)));
    }
    public static implicit operator Float128(ulong value) => FromUnsigned(value);
    public static implicit operator Float128(uint value) => FromUnsigned(value);
    public static implicit operator Float128(int value) => FromInt64(value);
    public static implicit operator Float128(UInt128 value) => FromUnsigned(value);
    public static implicit operator Float128(Int128 value)
    {
        UInt128 magnitude=value < 0 ? unchecked((UInt128)(-(value+1)))+1 : (UInt128)value;
        Float128 result=FromUnsigned(magnitude);
        return value < 0 ? -result : result;
    }
    public static implicit operator Float128(float value) => FromDouble(value);
    public static implicit operator Float128(Half value) => FromDouble((double)value);
    public static implicit operator Float128(Float80 value)
    {
        bool sign = (value.SignExponent & 0x8000) != 0;
        int exponent = value.SignExponent & 0x7fff;
        if (exponent == 32767)
            return value.Significand == 0x8000000000000000UL
                ? (sign ? NegativeInfinity : PositiveInfinity) : NaN;
        return RoundToBinary128(sign, value.Significand, Math.Max(exponent, 1)-16383-63, false);
    }
    private BigInteger TruncatedInteger()
    {
        if (IsNaN(this) || IsInfinity(this)) return BigInteger.Zero;
        Decompose(this, out bool sign, out BigInteger significand, out int power);
        BigInteger value = power >= 0 ? significand << power : significand >> -power;
        return sign ? -value : value;
    }
    public static explicit operator UInt128(Float128 value) => (UInt128)value.TruncatedInteger();
    public static explicit operator Int128(Float128 value) => (Int128)value.TruncatedInteger();
    public static explicit operator ulong(Float128 value) => (ulong)value.TruncatedInteger();
    public static explicit operator uint(Float128 value) => (uint)value.TruncatedInteger();
    public static explicit operator short(Float128 value) => (short)value.TruncatedInteger();
    public static explicit operator ushort(Float128 value) => (ushort)value.TruncatedInteger();
    public static explicit operator sbyte(Float128 value) => (sbyte)value.TruncatedInteger();
    public static explicit operator byte(Float128 value) => (byte)value.TruncatedInteger();

    private UInt128 RoundedSignificand(int precision, int minimumPower, out int power)
    {
        UInt128 magnitude = TrailingSignificand;
        power = Math.Max(BiasedExponent, 1)-16383-112;
        if (BiasedExponent != 0) magnitude |= UInt128.One << 112;
        if (magnitude == 0) { power = minimumPower; return 0; }
        int bits = 128 - (int)UInt128.LeadingZeroCount(magnitude);
        int quantum = Math.Max(power + bits - precision, minimumPower);
        int drop = quantum - power;
        UInt128 rounded = drop > 113 ? 0 : drop > 0
            ? RoundShiftRight(magnitude, drop) : magnitude << -drop;
        if ((rounded >> precision) != 0) { rounded >>= 1; ++quantum; }
        power = quantum;
        return rounded;
    }
    private ulong NarrowBits(int precision, int exponentBits, int bias)
    {
        int fractionBits = precision - 1;
        ulong sign = SignBit ? 1UL << (fractionBits + exponentBits) : 0;
        ulong infinity = ((1UL << exponentBits)-1) << fractionBits;
        if (IsNaN(this)) return sign | infinity | (1UL << (fractionBits-1));
        if (IsInfinity(this)) return sign | infinity;
        UInt128 significand = RoundedSignificand(precision, 1-bias-fractionBits, out int power);
        if (significand == 0) return sign;
        if (power + fractionBits > bias) return sign | infinity;
        int exponent = significand < (UInt128.One << fractionBits) ? 0 : power + fractionBits + bias;
        return sign | (ulong)exponent << fractionBits | (ulong)(significand & ((UInt128.One << fractionBits)-1));
    }
    public static explicit operator float(Float128 value) => BitConverter.UInt32BitsToSingle((uint)value.NarrowBits(24,8,127));
    public static explicit operator Half(Float128 value) => BitConverter.UInt16BitsToHalf((ushort)value.NarrowBits(11,5,15));
    public static explicit operator Float80(Float128 value)
    {
        ushort sign = (ushort)(value.SignBit ? 0x8000 : 0);
        if (IsNaN(value)) return new Float80(0xc000000000000000UL,(ushort)(sign|0x7fff));
        if (IsInfinity(value)) return new Float80(0x8000000000000000UL,(ushort)(sign|0x7fff));
        UInt128 significand = value.RoundedSignificand(64,-16445,out int power);
        if (significand == 0) return new Float80(0,sign);
        if (power+63>16383) return new Float80(0x8000000000000000UL,(ushort)(sign|0x7fff));
        int exponent = significand < (UInt128.One << 63) ? 0 : power+63+16383;
        return new Float80((ulong)significand,(ushort)(sign|exponent));
    }
}
