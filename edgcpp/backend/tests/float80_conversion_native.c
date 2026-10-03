#include <stdint.h>
#include <stdio.h>
#include <fenv.h>
#include <string.h>
static uint64_t state=0x5738126b47123467ULL;
static uint64_t next(void){state=state*6364136223846793005ULL+1;return state;}
typedef union {long double f;struct{uint64_t m;uint16_t e;}b;} extended;
int main(void){
 if(fesetround(FE_TONEAREST))return 1;
 for(int i=0;i<6000;++i){
  uint64_t lo=next(),hi=next();extended x;memset(&x,0,sizeof x);
  unsigned __int128 u=((unsigned __int128)hi<<64)|lo;volatile unsigned __int128 uv=u;x.f=uv;
  printf("u %llx %llx %llx %x\n",(unsigned long long)hi,(unsigned long long)lo,(unsigned long long)x.b.m,x.b.e);
  volatile __int128 sv=(__int128)u;x.f=sv;
  printf("i %llx %llx %llx %x\n",(unsigned long long)hi,(unsigned long long)lo,(unsigned long long)x.b.m,x.b.e);
  union{double f;uint64_t i;}d;d.i=next();volatile double dv=d.f;x.f=dv;
  printf("d %llx %llx %x\n",(unsigned long long)d.i,(unsigned long long)x.b.m,x.b.e);
  x.b.m=next()|0x8000000000000000ULL;x.b.e=(uint16_t)next();if(!(x.b.e&32767))x.b.m>>=1;
  union{float f;uint32_t i;}f;union{_Float16 f;uint16_t i;}h;volatile long double ev=x.f;d.f=ev;f.f=ev;h.f=ev;
  printf("x %llx %x %llx %x %x\n",(unsigned long long)x.b.m,x.b.e,(unsigned long long)d.i,f.i,h.i);
  x.b.m=next()|0x8000000000000000ULL;x.b.e=(uint16_t)(16383-100+(i%227));ev=x.f;uv=(unsigned __int128)ev;
  printf("U %llx %x %llx %llx\n",(unsigned long long)x.b.m,x.b.e,(unsigned long long)(uv>>64),(unsigned long long)uv);
  x.b.e|=0x8000;ev=x.f;sv=(__int128)ev;u=(unsigned __int128)sv;
  printf("I %llx %x %llx %llx\n",(unsigned long long)x.b.m,x.b.e,(unsigned long long)(u>>64),(unsigned long long)u);
 }
}
