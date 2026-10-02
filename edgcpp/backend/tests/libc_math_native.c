/* Synthetic native oracle. Only canonical x87 binary80 encodings are inputs. */
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <float.h>
#include <errno.h>
#include <fenv.h>
_Static_assert(LDBL_MANT_DIG==64 && LDBL_MAX_EXP==16384,"binary80 oracle required");
static long double value(uint64_t bits,uint16_t se) {
 unsigned char bytes[sizeof(long double)]={0};long double v;
 memcpy(bytes,&bits,8);memcpy(bytes+8,&se,2);memcpy(&v,bytes,sizeof v);return v;
}
static void output(long double v) {
 uint64_t bits;uint16_t se;memcpy(&bits,&v,8);memcpy(&se,(char*)&v+8,2);
 printf(" %016llx %04x",(unsigned long long)bits,se);
}
static void probe(uint64_t bits,uint16_t se) {
 long double v=value(bits,se);errno=71;long double s=sqrtl(v);int err=errno;
 printf("%016llx %04x",(unsigned long long)bits,se);output(s);output(fabsl(v));
 output(truncl(v));output(floorl(v));output(ceill(v));output(roundl(v));
 output(copysignl(v,-1.0L));printf(" %d\n",err);
}
static uint64_t rng=0x11ca96cb0171baabULL;
static uint64_t next(void) {rng^=rng<<13;rng^=rng>>7;rng^=rng<<17;return rng;}
int main(void) {
 if(fesetround(FE_TONEAREST))return 2;
 const uint16_t exponents[]={0,1,2,8191,16320,16381,16382,16383,16384,16445,16446,16447,32765,32766};
 const uint64_t bits[]={0,1,2,3,0x3fffffffffffffffULL,0x4000000000000000ULL,0x7fffffffffffffffULL,
 0x8000000000000000ULL,0x8000000000000001ULL,0x9fffffffffffffffULL,0xc000000000000000ULL,0xffffffffffffffffULL};
 for(unsigned e=0;e<sizeof exponents/sizeof *exponents;++e)
  for(unsigned i=0;i<sizeof bits/sizeof *bits;++i)for(unsigned sign=0;sign<2;++sign)
   probe(exponents[e]?bits[i]|0x8000000000000000ULL:bits[i]&0x7fffffffffffffffULL,exponents[e]|(sign<<15));
 for(int i=0;i<30000;++i) {
  uint16_t e=(uint16_t)(next()%32767);uint64_t b=next();
  probe(e?b|0x8000000000000000ULL:b&0x7fffffffffffffffULL,e|((i&1)<<15));
 }
 for(unsigned sign=0;sign<2;++sign) {
  probe(0,(uint16_t)(sign<<15));probe(0x8000000000000000ULL,(uint16_t)(0x7fff|(sign<<15)));
  probe(0xc000000000000123ULL,(uint16_t)(0x7fff|(sign<<15)));
  probe(0x8000000000000123ULL,(uint16_t)(0x7fff|(sign<<15)));
 }
 return 0;
}
