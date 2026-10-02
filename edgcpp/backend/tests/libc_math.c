#include <math.h>
#include <stdlib.h>
#include <stdint.h>
#include <errno.h>
#define CHECK(x,n) do {if(!(x))return n;}while(0)
/* Variables and external address calls deliberately keep intrinsic calls in IL. */
int main(void) {
 volatile float f=9;volatile double d=25;volatile long double l=81;
 float (*sf)(float)=sqrtf;double (*sd)(double)=sqrt;long double (*sl)(long double)=sqrtl;
 CHECK(sf(f)==3 && sd(d)==5 && sl(l)==9,1);
 CHECK(__builtin_sqrtf(f)==3 && __builtin_sqrt(d)==5 && __builtin_sqrtl(l)==9,2);
 CHECK(sf==__builtin_sqrtf && sd==__builtin_sqrt && sl==__builtin_sqrtl,3);
 volatile int i=-7;volatile long a=-123456789123L;volatile long long b=-987654321123LL;
 CHECK(abs(i)==7 && labs(a)==123456789123L && llabs(b)==987654321123LL,4);
 CHECK(__builtin_abs(i)==7 && __builtin_labs(a)==123456789123L && __builtin_llabs(b)==987654321123LL,5);
 int (*ai)(int)=abs;long (*al)(long)=labs;long long (*all)(long long)=llabs;
 CHECK(ai==__builtin_abs && al==__builtin_labs && all==__builtin_llabs,6);
 f=-3.25f;d=-4.75;l=-5.5L;
 CHECK(fabsf(f)==3.25f && fabs(d)==4.75 && fabsl(l)==5.5L,7);
 CHECK(__builtin_fabsf(f)==3.25f && __builtin_fabs(d)==4.75 && __builtin_fabsl(l)==5.5L,8);
 float (*af)(float)=fabsf;double (*ad)(double)=fabs;long double (*ald)(long double)=fabsl;
 CHECK(ald==__builtin_fabsl && ad==__builtin_fabs && af==__builtin_fabsf,9);
 CHECK(truncl(l)==-5 && floorl(l)==-6 && ceill(l)==-5 && roundl(l)==-6,10);
 CHECK(__builtin_truncl(l)==-5 && __builtin_floorl(l)==-6 && __builtin_ceill(l)==-5 && __builtin_roundl(l)==-6,11);
 CHECK(signbit(copysignl(l,-0.0L)) && !signbit(copysignl(l,0.0L)),12);
 CHECK(__builtin_copysignf(f,0.0f)==3.25f && __builtin_copysign(d,0.0)==4.75 && __builtin_copysignl(l,0.0L)==5.5L,13);
 errno=71;l=-1;CHECK(isnan(sl(l)) && errno==EDOM,14);
 l=-0.0L;errno=71;CHECK(signbit(sl(l)) && sl(l)==0 && errno==71,15);
 l=0x1.0000000000000002p0L;CHECK(sl(l)==1.0L,16);
 l=0x1p-16445L;CHECK(sl(l)==0x1.6a09e667f3bcc908p-8223L,17);
 l=0x1.fffffffffffffffep16383L;CHECK(sl(l)==0x1.fffffffffffffffep8191L,18);
 return 0;
}
