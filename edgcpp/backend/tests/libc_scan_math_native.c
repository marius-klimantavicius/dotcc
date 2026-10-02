#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <math.h>
#include <float.h>
_Static_assert(LDBL_MANT_DIG==64,"binary80 required");
static long double value(uint64_t s,uint16_t e) {unsigned char b[sizeof(long double)]={0};long double x;memcpy(b,&s,8);memcpy(b+8,&e,2);memcpy(&x,b,sizeof x);return x;}
static uint64_t state=0xef01982a98518434ULL;
static uint64_t next(void) {state^=state<<13;state^=state>>7;state^=state<<17;return state;}
int main(void) {
 for(int i=0;i<8000;++i) {
  uint16_t xe=next()%32767,ye=next()%32767;uint64_t x=next(),y=next();
  if(i<100)xe=ye=0;
  x=xe?x|1ULL<<63:x&~(1ULL<<63);y=ye?y|1ULL<<63:y&~(1ULL<<63);if(!y)y=1;
  xe|=(i&1)<<15;long double result=fmodl(value(x,xe),value(y,ye));uint64_t r;uint16_t re;
  memcpy(&r,&result,8);memcpy(&re,(char*)&result+8,2);
  printf("%016llx %04x %016llx %04x %016llx %04x\n",(unsigned long long)x,xe,(unsigned long long)y,ye,(unsigned long long)r,re);
 }
}
