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
    private static Float80 Pack128(UInt128 magnitude,int power,bool negative) {
        ushort sign=(ushort)(negative?0x8000:0);
        if(magnitude==0)return new(0,sign);
        ulong upper=(ulong)(magnitude>>64);
        int bits=upper!=0?128-BitOperations.LeadingZeroCount(upper):64-BitOperations.LeadingZeroCount((ulong)magnitude);
        int high=bits-1+power;
        if(high>16383)return Inf(negative);
        int quantum=Math.Max(high-63,-16445),drop=quantum-power;
        UInt128 mantissa;
        if(drop<=0)mantissa=magnitude<<-drop;
        else if(drop>128)mantissa=0;
        else if(drop==128)mantissa=magnitude>((UInt128)1<<127)?(UInt128)1:0;
        else {
            mantissa=magnitude>>drop;
            UInt128 remainder=magnitude- (mantissa<<drop),half=(UInt128)1<<(drop-1);
            if(remainder>half || (remainder==half && (mantissa&1)!=0))++mantissa;
        }
        if(mantissa==0)return new(0,sign);
        if(mantissa>((UInt128)ulong.MaxValue)){mantissa>>=1;++quantum;}
        if(quantum>16320)return Inf(negative);
        int exp=mantissa<((UInt128)1<<63)?0:quantum+63+16383;
        return new((ulong)mantissa,(ushort)(sign|exp));
    }
    public static Float80 operator +(Float80 a,Float80 b) {
        if(a.NaN || b.NaN)return QuietNaN;
        if(a.Infinity || b.Infinity)return a.Infinity && b.Infinity && a.Negative!=b.Negative?QuietNaN:a.Infinity?a:b;
        if(a.Zero && b.Zero)return new(0,(ushort)(a.Negative&&b.Negative?0x8000:0));
        if(a.Zero)return Pack128(b.Significand,b.Power,b.Negative);
        if(b.Zero)return Pack128(a.Significand,a.Power,a.Negative);
        int az=BitOperations.LeadingZeroCount(a.Significand),bz=BitOperations.LeadingZeroCount(b.Significand);
        ulong am=a.Significand<<az,bm=b.Significand<<bz;
        int ap=a.Power-az,bp=b.Power-bz;bool an=a.Negative,bn=b.Negative;
        if(ap<bp || (ap==bp && am<bm)){(am,bm)=(bm,am);(ap,bp)=(bp,ap);(an,bn)=(bn,an);}
        UInt128 x=(UInt128)am<<3,y=(UInt128)bm<<3;int drop=ap-bp;
        if(drop>=128)y=1;
        else if(drop>0){UInt128 shifted=y>>drop;y=shifted|((y-(shifted<<drop))!=0?(UInt128)1:0);}
        UInt128 magnitude=an==bn?x+y:x-y;
        return Pack128(magnitude,ap-3,magnitude!=0 && an);
    }
    public static Float80 operator -(Float80 a)=>new(a.Significand,(ushort)(a.SignExponent^0x8000));
    public static Float80 operator +(Float80 a)=>a;
    public static Float80 operator -(Float80 a,Float80 b)=>a+-b;
    public static Float80 operator *(Float80 a,Float80 b) {
        if(a.NaN || b.NaN || (a.Infinity && b.Zero) || (b.Infinity && a.Zero))return QuietNaN;
        if(a.Infinity || b.Infinity)return Inf(a.Negative!=b.Negative);
        return Pack128((UInt128)a.Significand*b.Significand,a.Power+b.Power,a.Negative!=b.Negative);
    }
    public static Float80 operator /(Float80 a,Float80 b) {
        bool negative=a.Negative!=b.Negative;
        if(a.NaN || b.NaN || (a.Infinity && b.Infinity) || (a.Zero && b.Zero))return QuietNaN;
        if(a.Infinity || b.Zero)return Inf(negative);
        if(a.Zero || b.Infinity)return new(0,(ushort)(negative?0x8000:0));
        UInt128 numerator=a.Significand,denominator=b.Significand;
        int ratio=BitOperations.LeadingZeroCount(b.Significand)-BitOperations.LeadingZeroCount(a.Significand);
        if(ratio>=0?numerator<(denominator<<ratio):(numerator<<-ratio)<denominator)--ratio;
        int high=ratio+a.Power-b.Power;
        if(high>16383)return Inf(negative);
        if(high < -16446)return new(0,(ushort)(negative?0x8000:0));
        int quantum=Math.Max(high-63,-16445),shift=a.Power-b.Power-quantum;
        // Given the nonzero-result bound above, scaled numerator uses at most
        // 128 bits; a left-scaled denominator uses at most 65 bits.
        if(shift>=0)numerator<<=shift;else denominator<<=-shift;
        UInt128 q=numerator/denominator,r=numerator-q*denominator;
        if((r<<1)>denominator || ((r<<1)==denominator && (q&1)!=0))++q;
        return Pack128(q,quantum,negative);
    }
    private static int Compare(Float80 a,Float80 b) {
        if(a.Zero && b.Zero)return 0;
        if(a.Negative!=b.Negative)return a.Negative?-1:1;
        int c;
        if(a.Infinity || b.Infinity)c=a.Infinity?(b.Infinity?0:1):-1;
        else if(a.Zero || b.Zero)c=a.Zero?-1:1;
        else {
            // Compare exact binary magnitudes without building (potentially 32K-bit)
            // integers. Normalizing also preserves the existing numerical meaning
            // of finite noncanonical encodings and x87 pseudo-denormals.
            int az=BitOperations.LeadingZeroCount(a.Significand),bz=BitOperations.LeadingZeroCount(b.Significand);
            int ah=Math.Max(1,a.Exponent)-az,bh=Math.Max(1,b.Exponent)-bz;
            c=ah.CompareTo(bh);
            if(c==0)c=(a.Significand<<az).CompareTo(b.Significand<<bz);
        }
        return a.Negative?-c:c;
    }
    public static bool operator ==(Float80 a,Float80 b)=>!a.NaN && !b.NaN && Compare(a,b)==0;
    public static bool operator !=(Float80 a,Float80 b)=>!(a==b);
    public static bool operator <(Float80 a,Float80 b)=>!a.NaN && !b.NaN && Compare(a,b)<0;
    public static bool operator >(Float80 a,Float80 b)=>b<a;
    public static bool operator <=(Float80 a,Float80 b)=>!a.NaN && !b.NaN && Compare(a,b)<=0;
    public static bool operator >=(Float80 a,Float80 b)=>b<=a;
    public static Float80 operator ++(Float80 a)=>a+(Float80)1;
    public static Float80 operator --(Float80 a)=>a-(Float80)1;
    public bool Equals(Float80 other)=>this==other;
    public override bool Equals(object? other)=>other is Float80 f && Equals(f);
    public override int GetHashCode()=>Zero?0:HashCode.Combine(Significand,SignExponent);
    public static implicit operator Float80(Int128 value)=>Pack128(value<0?unchecked((UInt128)(-value)):(UInt128)value,0,value<0);
    public static implicit operator Float80(UInt128 value)=>Pack128(value,0,false);
    public static explicit operator Int128(Float80 value)=>value.SignedInteger();
    public static explicit operator UInt128(Float80 value)=>value.UnsignedInteger();
    public static implicit operator Float80(long value)=>Pack128(value<0?unchecked((ulong)(-value)):(ulong)value,0,value<0);
    public static implicit operator Float80(ulong value)=>Pack128(value,0,false);
    public static implicit operator Float80(int value)=>(Float80)(long)value;
    public static implicit operator Float80(uint value)=>(Float80)(ulong)value;
    public static implicit operator Float80(double value) {
        ulong bits=(ulong)BitConverter.DoubleToInt64Bits(value),fraction=bits&0xfffffffffffffUL;
        int exp=(int)((bits>>52)&2047);bool sign=(bits>>63)!=0;
        if(exp==2047)return fraction!=0?QuietNaN:Inf(sign);
        return Pack128(exp==0?fraction:fraction|(1UL<<52),Math.Max(exp,1)-1023-52,sign);
    }
    public static implicit operator Float80(float value)=>(Float80)(double)value;
    public static implicit operator Float80(Half value)=>(Float80)(double)value;
    private ulong BinaryBits(int precision,int min,int max,int bias,int exponentBits) {
        ulong sign=Negative?1UL<<(precision-1+exponentBits):0;
        ulong infinity=((1UL<<exponentBits)-1)<<(precision-1);
        if(NaN)return sign|infinity|(1UL<<(precision-2));
        if(Infinity)return sign|infinity;
        if(Zero)return sign;
        int high=63-BitOperations.LeadingZeroCount(Significand)+Power;
        if(high>max)return sign|infinity;
        int quantum=Math.Max(high-(precision-1),min-(precision-1));
        int drop=quantum-Power;UInt128 mantissa;
        if(drop<=0)mantissa=(UInt128)Significand<<-drop;
        else if(drop>64)mantissa=0;
        else if(drop==64)mantissa=Significand>0x8000000000000000UL?(UInt128)1:0;
        else {
            mantissa=Significand>>drop;
            ulong remainder=Significand-((ulong)mantissa<<drop),half=1UL<<(drop-1);
            if(remainder>half || (remainder==half && (mantissa&1)!=0))++mantissa;
        }
        if((mantissa>>precision)!=0){mantissa>>=1;++quantum;}
        if(quantum+(precision-1)>max)return sign|infinity;
        int exp=mantissa<((UInt128)1<<(precision-1))?0:quantum+(precision-1)+bias;
        return sign|((ulong)exp<<(precision-1))|((ulong)mantissa&((1UL<<(precision-1))-1));
    }
    public static explicit operator double(Float80 value)=>BitConverter.Int64BitsToDouble((long)value.BinaryBits(53,-1022,1023,1023,11));
    public static explicit operator Half(Float80 value)=>BitConverter.UInt16BitsToHalf((ushort)value.BinaryBits(11,-14,15,15,5));
    public static explicit operator float(Float80 value)=>BitConverter.Int32BitsToSingle((int)value.BinaryBits(24,-126,127,127,8));
    private UInt128 IntegerMagnitude() {
        if(Significand==0)return 0;
        int power=Power;
        if(power<0)return power<=-64?0:(UInt128)(Significand>>-power);
        int bits=64-BitOperations.LeadingZeroCount(Significand);
        if(bits+power>128)throw new OverflowException();
        return (UInt128)Significand<<power;
    }
    private Int128 SignedInteger() {
        UInt128 magnitude=IntegerMagnitude();
        if(Negative){if(magnitude>((UInt128)1<<127))throw new OverflowException();return unchecked(-(Int128)magnitude);}
        return checked((Int128)magnitude);
    }
    private UInt128 UnsignedInteger() {
        UInt128 magnitude=IntegerMagnitude();
        if(Negative && magnitude!=0)throw new OverflowException();
        return magnitude;
    }
    public static explicit operator long(Float80 value)=>checked((long)value.SignedInteger());
    public static explicit operator ulong(Float80 value)=>checked((ulong)value.UnsignedInteger());
    public static explicit operator int(Float80 value)=>checked((int)value.SignedInteger());
    public static explicit operator uint(Float80 value)=>checked((uint)value.UnsignedInteger());
    public static explicit operator short(Float80 value)=>checked((short)value.SignedInteger());
    public static explicit operator ushort(Float80 value)=>checked((ushort)value.UnsignedInteger());
    public static explicit operator sbyte(Float80 value)=>checked((sbyte)value.SignedInteger());
    public static explicit operator byte(Float80 value)=>checked((byte)value.UnsignedInteger());
}
