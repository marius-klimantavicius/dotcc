using System;
using System.Numerics;

namespace Edg.Runtime;

// Additional math for the fixed binary80 profile. The current arithmetic mode
// is nearest-even; this class does not establish directed rounding or FE flags.
public sealed class LibcMath
{
    private readonly Action<int> setErrno;
    public LibcMath(Action<int> setErrno) => this.setErrno=setErrno;
    // INT_MIN/LONG_MIN have no representable positive result (undefined in C).
    // Keep the generated unchecked arithmetic policy instead of BCL Abs throwing.
    public static int Abs(int value) => value<0?unchecked(-value):value;
    public static long Abs(long value) => value<0?unchecked(-value):value;
    public static Float80 Abs(Float80 value) => new(value.Significand,(ushort)(value.SignExponent&0x7fff));
    public static float CopySign(float value,float sign) => BitConverter.UInt32BitsToSingle(
        (BitConverter.SingleToUInt32Bits(value)&0x7fffffffU)|(BitConverter.SingleToUInt32Bits(sign)&0x80000000U));
    public static double CopySign(double value,double sign) => BitConverter.UInt64BitsToDouble(
        (BitConverter.DoubleToUInt64Bits(value)&0x7fffffffffffffffUL)|(BitConverter.DoubleToUInt64Bits(sign)&0x8000000000000000UL));
    public static Float80 CopySign(Float80 value,Float80 sign) => new(value.Significand,
        (ushort)((value.SignExponent&0x7fff)|(sign.SignExponent&0x8000)));
    private static bool IsNaN(Float80 value) => (value.SignExponent&0x7fff)==0x7fff && value.Significand!=0x8000000000000000UL;
    private static Float80 Quiet(Float80 value) => IsNaN(value)?new(value.Significand|0x4000000000000000UL,value.SignExponent):value;
    public Float80 Sqrt(Float80 value)
    {
        if(IsNaN(value))return Quiet(value);
        int raw=value.SignExponent&0x7fff;
        if(value.Significand==0)return value; // Preserve -0.
        if((value.SignExponent&0x8000)!=0) {setErrno(33);return new(0xc000000000000000UL,0xffff);}
        if(raw==0x7fff)return value;
        int shift=BitOperations.LeadingZeroCount(value.Significand);
        ulong significand=value.Significand<<shift;
        int exponent=Math.Max(raw,1)-16383-shift;
        // sqrt(m * 2^e) rounded to a 64-bit significand reduces to the integer
        // square root of a 127/128-bit radicand. UInt128 avoids BigInteger/GC.
        UInt128 radicand=(UInt128)significand<<(63+(exponent&1));
        UInt128 root=(UInt128)1<<64;
        for(;;) {
            UInt128 next=(root+radicand/root)>>1;
            if(next>=root)break;
            root=next;
        }
        UInt128 remainder=radicand-root*root;
        // (root+1/2)^2 has fractional part 1/4, so an integer radicand never
        // ties: round upward exactly when remainder > root.
        if(remainder>root)++root;
        int resultExponent=(exponent>>1)+16383;
        if(root==((UInt128)1<<64)) {root>>=1;++resultExponent;}
        return new((ulong)root,(ushort)resultExponent);
    }
    // direction: 0 truncate, -1 floor, +1 ceil, 2 round ties away from zero.
    private static Float80 Integral(Float80 value,int direction)
    {
        int raw=value.SignExponent&0x7fff;
        if(raw==0x7fff)return Quiet(value);
        if(value.Significand==0)return value;
        bool negative=(value.SignExponent&0x8000)!=0;
        int exponent=Math.Max(raw,1)-16383;
        if(exponent>=63)return value;
        bool away=direction==-1?negative:direction==1?!negative:false;
        if(exponent<0) {
            if(direction==2)away=exponent==-1;
            return away?new Float80(0x8000000000000000UL,(ushort)(16383|(negative?0x8000:0))):
                new Float80(0,(ushort)(negative?0x8000:0));
        }
        int drop=63-exponent;
        ulong mask=(1UL<<drop)-1,fraction=value.Significand&mask;
        if(fraction==0)return value;
        if(direction==2)away=fraction>=(1UL<<(drop-1));
        ulong bits=value.Significand&~mask;
        if(away) {
            bits=unchecked(bits+(1UL<<drop));
            if(bits==0) {bits=0x8000000000000000UL;++raw;}
        }
        return new(bits,(ushort)(raw|(negative?0x8000:0)));
    }
    public static Float80 Truncate(Float80 value)=>Integral(value,0);
    public static Float80 Floor(Float80 value)=>Integral(value,-1);
    public static Float80 Ceiling(Float80 value)=>Integral(value,1);
    public static Float80 Round(Float80 value)=>Integral(value,2);
    public static double Round(double value)=>Math.Round(value,MidpointRounding.AwayFromZero);
    public static float Round(float value)=>MathF.Round(value,MidpointRounding.AwayFromZero);
}
