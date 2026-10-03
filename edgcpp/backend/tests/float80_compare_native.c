/* Canonical binary80 comparison oracle, no arithmetic or fast-math. */
#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <float.h>
_Static_assert(sizeof(long double)==16 && LDBL_MANT_DIG==64 && LDBL_MAX_EXP==16384,"binary80 oracle required");
static uint64_t state=UINT64_C(0x73aace003859ed21);
static uint64_t next(void){state^=state<<13;state^=state>>7;state^=state<<17;return state;}
static long double value(uint64_t m,unsigned short e){unsigned char bytes[16]={0};memcpy(bytes,&m,8);memcpy(bytes+8,&e,2);long double x;memcpy(&x,bytes,16);return x;}
static void emit(uint64_t a,unsigned short ae,uint64_t b,unsigned short be){
 volatile long double x=value(a,ae),y=value(b,be);
 unsigned bits=(x==y)|((x!=y)<<1)|((x<y)<<2)|((x>y)<<3)|((x<=y)<<4)|((x>=y)<<5);
 printf("%016llx %04x %016llx %04x %u\n",(unsigned long long)a,ae,(unsigned long long)b,be,bits);
}
int main(void){
 const uint64_t sig[]={0,1,UINT64_C(0x7fffffffffffffff),UINT64_C(0x8000000000000000),UINT64_MAX};
 const unsigned short exp[]={0,1,2,16382,16383,16384,32766,32767};
 for(unsigned i=0;i<80;i++)for(unsigned j=0;j<80;j++){
  unsigned short ae=exp[(i/5)%8],be=exp[(j/5)%8];uint64_t a=sig[i%5],b=sig[j%5];
  if(ae==0)a&=UINT64_C(0x7fffffffffffffff);else a|=UINT64_C(0x8000000000000000);
  if(be==0)b&=UINT64_C(0x7fffffffffffffff);else b|=UINT64_C(0x8000000000000000);
  emit(a,ae|(i>=40?0x8000:0),b,be|(j>=40?0x8000:0));
 }
 for(unsigned i=0;i<12000;i++){
  uint64_t a=next(),b=next();unsigned short ae=(unsigned short)next(),be=(unsigned short)next();
  if(!(ae&0x7fff))a&=UINT64_C(0x7fffffffffffffff);else a|=UINT64_C(0x8000000000000000);
  if(!(be&0x7fff))b&=UINT64_C(0x7fffffffffffffff);else b|=UINT64_C(0x8000000000000000);
  emit(a,ae,b,be);
 }
 return 0;
}
