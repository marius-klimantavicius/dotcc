#include <math.h>
#include <float.h>
#include <stdint.h>
#include <errno.h>
#include <stdio.h>
static uint64_t state=0x8a5cd789635d2dffULL,hash=1469598103934665603ULL;
static uint64_t bits(void){state=state*6364136223846793005ULL+1;return state;}
static void mix(uint64_t x){hash^=x;hash*=1099511628211ULL;}
static void hf(float x){union{float f;uint32_t i;}u={x};mix(isnan(x)?0x7fc00000U:u.i);}
static void hd(double x){union{double f;uint64_t i;}u={x};mix(isnan(x)?0x7ff8000000000000ULL:u.i);}
static void hl(long double x){union{long double f;struct{uint64_t m;uint16_t se;}i;}u={x};if(isnan(x)){mix(1);mix(0x7fff);}else{mix(u.i.m);mix(u.i.se);}}
#ifdef __cplusplus
extern "C" {
#endif
void math_remainders_set_errno(int value){errno=value;}
int math_remainders_errno(void){return errno;}
int math_remainders_errors(void){
 int q;
#ifdef ADAPTED
 errno=101;if(!isnan(remainder(1,0))||errno!=EDOM)return 1;
 errno=102;if(!isnan(remquof(INFINITY,1,&q))||errno!=EDOM)return 2;
 errno=103;if(!isnan(remainderl(1,-0.0L))||errno!=EDOM)return 3;
 errno=104;if(!isnan(remquo(NAN,0,&q))||errno!=104)return 4;
 errno=105;if(remainderf(-0.0f,1)!=0||errno!=105)return 5;
 errno=106;if(remquol(3,HUGE_VALL,&q)!=3||errno!=106)return 6;
#endif
 if(remainder(5,2)!=1||remainder(7,2)!=-1||remainder(-7,2)!=1)return 7;
 if(!signbit(remainderf(-4,2))||remquol(-7,2,&q)!=1||q!=-4)return 8;
 return 0;
}
#ifdef __cplusplus
}
#endif
int main(void){
 for(int n=0;n<200;++n){
  union{float f;uint32_t i;}xf,yf;union{double f;uint64_t i;}xd,yd;
  union{long double f;struct{uint64_t m;uint16_t se;}i;}xl,yl;
  xf.i=(uint32_t)bits();yf.i=(uint32_t)bits();xd.i=bits();yd.i=bits();
  xl.i.m=bits()|0x8000000000000000ULL;xl.i.se=(uint16_t)bits();yl.i.m=bits()|0x8000000000000000ULL;yl.i.se=(uint16_t)bits();
  if((xl.i.se&0x7fff)==0)xl.i.m>>=1;if((yl.i.se&0x7fff)==0)yl.i.m>>=1;
  int q;hf(remainderf(xf.f,yf.f));hf(remquof(xf.f,yf.f,&q));mix((unsigned)q);
  hd(remainder(xd.f,yd.f));hd(remquo(xd.f,yd.f,&q));mix((unsigned)q);
  hl(remainderl(xl.f,yl.f));hl(remquol(xl.f,yl.f,&q));mix((unsigned)q);
 }
 long double v[]={0,-0.0L,LDBL_TRUE_MIN,-LDBL_TRUE_MIN,LDBL_MIN,0.5L,-0.5L,1,-1,2,-2,3,5,7,LDBL_MAX,HUGE_VALL,-HUGE_VALL,NAN};
 for(unsigned i=0;i<sizeof(v)/sizeof(v[0]);++i)for(unsigned j=0;j<sizeof(v)/sizeof(v[0]);++j){int q;hl(remainderl(v[i],v[j]));hl(remquol(v[i],v[j],&q));mix((unsigned)q);}
 if(math_remainders_errors())return 77;
 printf("%llu\n",(unsigned long long)hash);return 0;
}
