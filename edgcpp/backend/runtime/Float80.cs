// Software x87 binary80, preserving the current EDG LP64 target's 16-byte
// storage and 64-bit significand. Arithmetic rounds once, ties to even.
using System;
using System.Numerics;
using System.Runtime.InteropServices;
namespace Edg.Runtime;
[StructLayout(LayoutKind.Explicit,Size=16,Pack=1)]
public readonly struct Float80 : IEquatable<Float80> {
    [FieldOffset(0)] public readonly ulong Significand;
    [FieldOffset(8)] public readonly ushort SignExponent;
    public Float80(ulong significand,ushort exponent){Significand=significand;SignExponent=exponent;}
    private bool Negative => (SignExponent&0x8000)!=0;
    private int Exponent => SignExponent&0x7fff;
    private int Power => Math.Max(1,Exponent)-16383-63;
    private bool NaN => Exponent==32767 && Significand!=0x8000000000000000UL;
    private bool Infinity => Exponent==32767 && !NaN;
    private bool Zero => Exponent!=32767 && Significand==0;
    private static Float80 Inf(bool negative)=>new(0x8000000000000000UL,(ushort)(0x7fff|(negative?0x8000:0)));
    private static Float80 QuietNaN=>new(0xc000000000000000UL,0x7fff);
    private static BigInteger RoundShift(BigInteger value,int drop) {
        if(drop<=0)return value<<-drop;
        BigInteger q=value>>drop,r=value-(q<<drop),half=BigInteger.One<<(drop-1);
        return r>half || (r==half && !q.IsEven)?q+1:q;
    }
    private static Float80 Pack(BigInteger magnitude,int power,bool negative) {
        ushort sign=(ushort)(negative?0x8000:0);
        if(magnitude.IsZero)return new(0,sign);
        int high=checked((int)magnitude.GetBitLength())-1+power;
        if(high>16383)return Inf(negative);
        int quantum=Math.Max(high-63,-16445);
        BigInteger mantissa=RoundShift(magnitude,quantum-power);
        if(mantissa.IsZero)return new(0,sign);
        if(mantissa.GetBitLength()>64){mantissa>>=1;++quantum;}
        if(quantum>16320)return Inf(negative);
        int exp=mantissa<(BigInteger.One<<63)?0:quantum+63+16383;
        return new((ulong)mantissa,(ushort)(sign|exp));
    }
    public static Float80 operator +(Float80 a,Float80 b) {
        if(a.NaN || b.NaN)return QuietNaN;
        if(a.Infinity || b.Infinity)return a.Infinity && b.Infinity && a.Negative!=b.Negative?QuietNaN:a.Infinity?a:b;
        int power=Math.Min(a.Power,b.Power);
        BigInteger x=(BigInteger)a.Significand<<(a.Power-power),y=(BigInteger)b.Significand<<(b.Power-power);
        if(a.Negative)x=-x;if(b.Negative)y=-y;BigInteger sum=x+y;
        return Pack(BigInteger.Abs(sum),power,sum.Sign<0 || (sum.IsZero && a.Negative && b.Negative));
    }
    public static Float80 operator -(Float80 a)=>new(a.Significand,(ushort)(a.SignExponent^0x8000));
    public static Float80 operator +(Float80 a)=>a;
    public static Float80 operator -(Float80 a,Float80 b)=>a+-b;
    public static Float80 operator *(Float80 a,Float80 b) {
        if(a.NaN || b.NaN || (a.Infinity && b.Zero) || (b.Infinity && a.Zero))return QuietNaN;
        if(a.Infinity || b.Infinity)return Inf(a.Negative!=b.Negative);
        return Pack((BigInteger)a.Significand*b.Significand,a.Power+b.Power,a.Negative!=b.Negative);
    }
    public static Float80 operator /(Float80 a,Float80 b) {
        bool negative=a.Negative!=b.Negative;
        if(a.NaN || b.NaN || (a.Infinity && b.Infinity) || (a.Zero && b.Zero))return QuietNaN;
        if(a.Infinity || b.Zero)return Inf(negative);
        if(a.Zero || b.Infinity)return new(0,(ushort)(negative?0x8000:0));
        BigInteger numerator=a.Significand,denominator=b.Significand;
        int ratio=(int)numerator.GetBitLength()-(int)denominator.GetBitLength();
        if(ratio>=0?numerator<(denominator<<ratio):(numerator<<-ratio)<denominator)--ratio;
        int high=ratio+a.Power-b.Power;
        if(high>16383)return Inf(negative);
        int quantum=Math.Max(high-63,-16445),shift=a.Power-b.Power-quantum;
        if(shift>=0)numerator<<=shift;else denominator<<=-shift;
        BigInteger q=BigInteger.DivRem(numerator,denominator,out BigInteger r);
        if((r<<1)>denominator || ((r<<1)==denominator && !q.IsEven))++q;
        return Pack(q,quantum,negative);
    }
    private static int Compare(Float80 a,Float80 b) {
        if(a.Zero && b.Zero)return 0;
        if(a.Negative!=b.Negative)return a.Negative?-1:1;
        int c;
        if(a.Infinity || b.Infinity)c=a.Infinity?(b.Infinity?0:1):-1;
        else {int power=Math.Min(a.Power,b.Power);c=((BigInteger)a.Significand<<(a.Power-power)).CompareTo((BigInteger)b.Significand<<(b.Power-power));}
        return a.Negative?-c:c;
    }
    public static bool operator ==(Float80 a,Float80 b)=>!a.NaN && !b.NaN && Compare(a,b)==0;
    public static bool operator !=(Float80 a,Float80 b)=>!(a==b);
    public static bool operator <(Float80 a,Float80 b)=>!a.NaN && !b.NaN && Compare(a,b)<0;
    public static bool operator >(Float80 a,Float80 b)=>b<a;
    public static bool operator <=(Float80 a,Float80 b)=>a<b || a==b;
    public static bool operator >=(Float80 a,Float80 b)=>b<=a;
    public static Float80 operator ++(Float80 a)=>a+(Float80)1;
    public static Float80 operator --(Float80 a)=>a-(Float80)1;
    public bool Equals(Float80 other)=>this==other;
    public override bool Equals(object? other)=>other is Float80 f && Equals(f);
    public override int GetHashCode()=>Zero?0:HashCode.Combine(Significand,SignExponent);
    public static implicit operator Float80(Int128 value)=>Pack(BigInteger.Abs((BigInteger)value),0,value<0);
    public static implicit operator Float80(UInt128 value)=>Pack((BigInteger)value,0,false);
    public static explicit operator Int128(Float80 value)=>(Int128)value.Integer();
    public static explicit operator UInt128(Float80 value)=>(UInt128)value.Integer();
    public static implicit operator Float80(long value)=>Pack(BigInteger.Abs((BigInteger)value),0,value<0);
    public static implicit operator Float80(ulong value)=>Pack(value,0,false);
    public static implicit operator Float80(int value)=>(Float80)(long)value;
    public static implicit operator Float80(uint value)=>(Float80)(ulong)value;
    public static implicit operator Float80(double value) {
        ulong bits=(ulong)BitConverter.DoubleToInt64Bits(value),fraction=bits&0xfffffffffffffUL;
        int exp=(int)((bits>>52)&2047);bool sign=(bits>>63)!=0;
        if(exp==2047)return fraction!=0?QuietNaN:Inf(sign);
        return Pack(exp==0?fraction:fraction|(1UL<<52),Math.Max(exp,1)-1023-52,sign);
    }
    public static implicit operator Float80(float value)=>(Float80)(double)value;
    public static implicit operator Float80(Half value)=>(Float80)(double)value;
    private ulong BinaryBits(int precision,int min,int max,int bias,int exponentBits) {
        ulong sign=Negative?1UL<<(precision-1+exponentBits):0;
        ulong infinity=((1UL<<exponentBits)-1)<<(precision-1);
        if(NaN)return sign|infinity|(1UL<<(precision-2));
        if(Infinity)return sign|infinity;
        if(Zero)return sign;
        int high=(int)((BigInteger)Significand).GetBitLength()-1+Power;
        if(high>max)return sign|infinity;
        int quantum=Math.Max(high-(precision-1),min-(precision-1));
        BigInteger mantissa=RoundShift(Significand,quantum-Power);
        if(mantissa.GetBitLength()>precision){mantissa>>=1;++quantum;}
        if(quantum+(precision-1)>max)return sign|infinity;
        int exp=mantissa<(BigInteger.One<<(precision-1))?0:quantum+(precision-1)+bias;
        return sign|((ulong)exp<<(precision-1))|((ulong)mantissa&((1UL<<(precision-1))-1));
    }
    public static explicit operator double(Float80 value)=>BitConverter.Int64BitsToDouble((long)value.BinaryBits(53,-1022,1023,1023,11));
    public static explicit operator Half(Float80 value)=>BitConverter.UInt16BitsToHalf((ushort)value.BinaryBits(11,-14,15,15,5));
    public static explicit operator float(Float80 value)=>BitConverter.Int32BitsToSingle((int)value.BinaryBits(24,-126,127,127,8));
    private BigInteger Integer(){BigInteger n=Power>=0?(BigInteger)Significand<<Power:(BigInteger)Significand>>-Power;return Negative?-n:n;}
    public static explicit operator long(Float80 value)=>(long)value.Integer();
    public static explicit operator ulong(Float80 value)=>(ulong)value.Integer();
    public static explicit operator int(Float80 value)=>(int)value.Integer();
    public static explicit operator uint(Float80 value)=>(uint)value.Integer();
    public static explicit operator short(Float80 value)=>(short)value.Integer();
    public static explicit operator ushort(Float80 value)=>(ushort)value.Integer();
    public static explicit operator sbyte(Float80 value)=>(sbyte)value.Integer();
    public static explicit operator byte(Float80 value)=>(byte)value.Integer();
}
