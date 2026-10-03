#include <stdint.h>
#include <stdio.h>
#include <fenv.h>
#include <string.h>
static uint64_t state=0x98e4a638fb471232ULL;
static uint64_t next(void){state=state*6364136223846793005ULL+1;return state;}
static void check(uint64_t am,uint16_t ae,uint64_t bm,uint16_t be){
 union {long double f;struct{uint64_t m;uint16_t e;}b;}a,b,c;
 memset(&a,0,sizeof a);memset(&b,0,sizeof b);a.b.m=am;a.b.e=ae;b.b.m=bm;b.b.e=be;
 volatile long double x=a.f,y=b.f;c.f=x+y;
 printf("%llx %x %llx %x %llx %x\n",(unsigned long long)am,ae,(unsigned long long)bm,be,(unsigned long long)c.b.m,c.b.e);
}
int main(void){
 if(fesetround(FE_TONEAREST))return 1;
 for(int i=0;i<12000;++i){uint64_t a=next()|0x8000000000000000ULL,b=next()|0x8000000000000000ULL;uint16_t ae=(uint16_t)next(),be=(uint16_t)next();if(i%2){int ex=(ae&32767)+(i%129)-64;if(ex<0)ex=0;if(ex>32766)ex=32766;be=(be&32768)|(uint16_t)ex;}if(!(ae&32767))a>>=1;if(!(be&32767))b>>=1;check(a,ae,b,be);}
 uint64_t m[]={0,1,2,3,0x7fffffffffffffffULL,0x8000000000000000ULL,0x8000000000000001ULL,0xc000000000000000ULL,0xffffffffffffffffULL};
 uint16_t e[]={0,1,2,0x3ffe,0x3fff,0x4000,0x7ffd,0x7ffe,0x7fff};
 for(unsigned i=0;i<9;++i)for(unsigned j=0;j<9;++j)for(unsigned k=0;k<9;++k)for(unsigned l=0;l<9;++l){
 if(e[j] && m[i]<0x8000000000000000ULL)continue;if(e[l] && m[k]<0x8000000000000000ULL)continue;
 if(!e[j] && m[i]>=0x8000000000000000ULL)continue;if(!e[l] && m[k]>=0x8000000000000000ULL)continue;
 check(m[i],e[j],m[k],e[l]);check(m[i],e[j]|0x8000,m[k],e[l]);}
}
