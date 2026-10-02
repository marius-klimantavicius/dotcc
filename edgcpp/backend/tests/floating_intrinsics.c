/* Keep operands in volatile storage to exercise residual GNU intrinsic calls. */
#define CHECK(x,n) do {if(!(x))return n;}while(0)
#define CLASSIFY(x,n) CHECK(__builtin_fpclassify(11,22,33,44,55,(x))==(n),__LINE__)
static volatile int calls;
static long double once(long double x) {++calls;return x;}
int main(void) {
 volatile float f=1.0f, fz=-0.0f, fs=0x1p-149f;
 volatile double d=1.0, dz=-0.0, ds=0x1p-1074;
 volatile long double l=1.0L, lz=-0.0L, ls=0x1p-16445L;
 volatile float fi=__builtin_inff(),fn=__builtin_nanf("");
 volatile double di=__builtin_inf(),dn=__builtin_nan("");
 volatile long double li=__builtin_infl(),ln=__builtin_nanl("");
 CLASSIFY(f,33);CLASSIFY(fz,55);CLASSIFY(fs,44);CLASSIFY(fi,22);CLASSIFY(fn,11);
 CLASSIFY(d,33);CLASSIFY(dz,55);CLASSIFY(ds,44);CLASSIFY(di,22);CLASSIFY(dn,11);
 CLASSIFY(l,33);CLASSIFY(lz,55);CLASSIFY(ls,44);CLASSIFY(li,22);CLASSIFY(ln,11);
 CHECK(__builtin_isnan(fn)&&__builtin_isnan(dn)&&__builtin_isnan(ln),20);
 CHECK(__builtin_isnanf(fn)&&__builtin_isnanl(ln)&&!__builtin_isnan(d),21);
 CHECK(__builtin_isfinite(f)&&__builtin_isfinite(d)&&__builtin_isfinite(l),22);
 CHECK(!__builtin_isfinite(fi)&&!__builtin_isfinite(dn)&&!__builtin_isfinite(li),23);
 CHECK(__builtin_isnormal(f)&&__builtin_isnormal(d)&&__builtin_isnormal(l),24);
 CHECK(!__builtin_isnormal(fs)&&!__builtin_isnormal(ds)&&!__builtin_isnormal(ls),25);
 CHECK(__builtin_isinf(fi)&&__builtin_isinf(di)&&__builtin_isinf(li),26);
 CHECK(__builtin_isinff(fi)&&__builtin_isinfl(li)&&!__builtin_isinf(fn),27);
 CHECK(__builtin_isinf_sign(-fi)==-1&&__builtin_isinf_sign(di)==1&&__builtin_isinf_sign(ln)==0,28);
 CHECK(__builtin_signbit(fz)&&__builtin_signbit(dz)&&__builtin_signbit(lz),29);
 CHECK(__builtin_signbitf(fz)&&__builtin_signbitl(lz)&&!__builtin_signbit(l),30);
 CHECK(__builtin_isgreater(fz,-f)&&__builtin_isgreaterequal(d,d)&&__builtin_isless(lz,l),31);
 CHECK(__builtin_islessequal(l,l)&&__builtin_islessgreater(fz,f)&&!__builtin_islessgreater(d,d),32);
 CHECK(__builtin_isunordered(fn,f)&&__builtin_isunordered(d,dn)&&__builtin_isunordered(ln,l),33);
 CHECK(!__builtin_isgreater(ln,l)&&!__builtin_isless(fn,f)&&!__builtin_islessgreater(d,dn),34);
 CHECK(__builtin_isgreater(l+1,f)&&__builtin_isless(1,d+1)&&__builtin_isunordered(1,ln),38);
 calls=0;CHECK(__builtin_isnan(once(ln))&&calls==1,35);
 calls=0;CHECK(__builtin_isgreater(once(l),once(lz))&&calls==2,36);
 calls=0;CLASSIFY(once(ls),44);CHECK(calls==1,37);
 return 0;
}
