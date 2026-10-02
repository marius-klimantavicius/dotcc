/* Synthetic native vectors. Native binary80 oracle requires x86-64 LP64. */
#include <stdio.h>
#include <stdint.h>
#include <inttypes.h>
#include <math.h>
#include <float.h>
#include <limits.h>
#include <string.h>
_Static_assert(LDBL_MANT_DIG==64 && sizeof(long double)==16,"binary80 oracle");
static uint64_t state=0x8937261f28a736e1ULL;
static uint64_t random_bits(void) { state^=state<<13;state^=state>>7;state^=state<<17;return state; }
static int category(int native) {
 switch(native){case FP_NAN:return 0;case FP_INFINITE:return 1;case FP_ZERO:return 2;case FP_SUBNORMAL:return 3;default:return 4;}
}
static void extended(uint64_t significand,unsigned exponent,int power) {
 unsigned char bytes[16]={0};memcpy(bytes,&significand,8);memcpy(bytes+8,&exponent,2);
 long double input;memcpy(&input,bytes,16);int e=0;
 long double f=frexpl(input,&e),s=scalbnl(input,power);uint64_t fs,ss;unsigned short fe,se;
 memcpy(&fs,&f,8);memcpy(&fe,(char*)&f+8,2);memcpy(&ss,&s,8);memcpy(&se,(char*)&s+8,2);
 printf("L %016" PRIx64 " %04x %d %016" PRIx64 " %04x %d %016" PRIx64 " %04x %d %d\n",significand,exponent,power,fs,fe,e,ss,se,category(fpclassify(input)),!!signbit(input));
}
static void binary64(uint64_t bits,int power) {
 double input;memcpy(&input,&bits,8);int e=0;double f=frexp(input,&e),s=scalbn(input,power);uint64_t fb,sb;memcpy(&fb,&f,8);memcpy(&sb,&s,8);
 printf("D %016" PRIx64 " %d %016" PRIx64 " %d %016" PRIx64 " %d %d\n",bits,power,fb,e,sb,category(fpclassify(input)),!!signbit(input));
}
static void binary32(uint32_t bits,int power) {
 float input;memcpy(&input,&bits,4);int e=0;float f=frexpf(input,&e),s=scalbnf(input,power);uint32_t fb,sb;memcpy(&fb,&f,4);memcpy(&sb,&s,4);
 printf("F %08" PRIx32 " %d %08" PRIx32 " %d %08" PRIx32 " %d %d\n",bits,power,fb,e,sb,category(fpclassify(input)),!!signbit(input));
}
int main(void) {
 const int powers[]={INT_MIN,-20000,-16446,-16445,-1075,-1074,-1023,-150,-149,-127,-65,-64,-63,-2,-1,0,1,2,63,64,65,127,1023,16384,20000,INT_MAX};
 const uint64_t sigs[]={0,1,2,3,0x7fffffffffffffffULL,0x8000000000000000ULL,0x8000000000000001ULL,0xffffffffffffffffULL};
 const unsigned exponents[]={0,1,2,16382,16383,32765,32766};
 for(unsigned p=0;p<sizeof powers/sizeof*powers;++p) {
  for(unsigned i=0;i<sizeof sigs/sizeof*sigs;++i)for(unsigned j=0;j<sizeof exponents/sizeof*exponents;++j)for(unsigned sign=0;sign<2;++sign)
   if((exponents[j]==0 && sigs[i]<0x8000000000000000ULL) || (exponents[j]>0 && sigs[i]>=0x8000000000000000ULL))
    extended(sigs[i],exponents[j]|(sign<<15),powers[p]);
  const uint64_t doubles[]={0,1,2,3,0xfffffffffffffULL,0x10000000000000ULL,0x3ff0000000000000ULL,0x7fefffffffffffffULL,0x7ff0000000000000ULL,0x7ff8000000001234ULL};
  for(unsigned i=0;i<sizeof doubles/sizeof*doubles;++i)for(int sign=0;sign<2;++sign)binary64(doubles[i]|((uint64_t)sign<<63),powers[p]);
  const uint32_t floats[]={0,1,2,3,0x7fffff,0x800000,0x3f800000,0x7f7fffff,0x7f800000,0x7fc00123};
  for(unsigned i=0;i<sizeof floats/sizeof*floats;++i)for(int sign=0;sign<2;++sign)binary32(floats[i]|((uint32_t)sign<<31),powers[p]);
 }
 for(int i=0;i<3000;++i) {
  unsigned exp=random_bits()%32767,sign=random_bits()&0x8000;uint64_t sig=random_bits();sig=exp?sig|0x8000000000000000ULL:sig&0x7fffffffffffffffULL;
  extended(sig,exp|sign,(int)(random_bits()%70000)-35000);
  uint64_t bits=random_bits();if((bits&0x7ff0000000000000ULL)==0x7ff0000000000000ULL)bits^=0x0010000000000000ULL;
  binary64(bits,(int)(random_bits()%4500)-2250);
  uint32_t small=random_bits();if((small&0x7f800000U)==0x7f800000U)small^=0x00800000U;
  binary32(small,(int)(random_bits()%600)-300);
 }
 return 0;
}
