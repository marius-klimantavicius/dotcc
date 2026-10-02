using System;
using System.Numerics;
namespace Edg.Runtime;
// Private finite helper for selected musl floatscan, not a public fmodl binding.
public static class LibcScanMath
{
    public static Float80 Remainder(Float80 x,Float80 y)
    {
        int xr=x.SignExponent&0x7fff,yr=y.SignExponent&0x7fff;
        if(xr==0x7fff || yr==0x7fff || y.Significand==0 || (y.SignExponent&0x8000)!=0)
            throw new ArgumentOutOfRangeException(nameof(y),"numeric scanner requires finite operands and positive divisor");
        if(x.Significand==0)return x;
        int xs=BitOperations.LeadingZeroCount(x.Significand),ys=BitOperations.LeadingZeroCount(y.Significand);
        int xe=Math.Max(xr,1)-16383-xs,ye=Math.Max(yr,1)-16383-ys;
        ulong a=x.Significand<<xs,b=y.Significand<<ys;
        if(xe<ye || (xe==ye && a<b))return x;
        UInt128 remainder=(UInt128)a%b;
        int distance=xe-ye;
        // Each chunk fits UInt128 since the previous remainder is below b.
        while(distance>0) {int shift=Math.Min(distance,64);remainder=(remainder<<shift)%b;distance-=shift;}
        if(remainder==0)return new(0,(ushort)(x.SignExponent&0x8000));
        ulong bits=(ulong)remainder;int normalize=BitOperations.LeadingZeroCount(bits);
        int exponent=ye+16383-normalize;bits<<=normalize;
        if(exponent<=0) {bits>>=1-exponent;exponent=0;}
        return new(bits,(ushort)(exponent|(x.SignExponent&0x8000)));
    }
}
