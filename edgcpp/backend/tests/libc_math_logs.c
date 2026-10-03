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
void math_logs_set_errno(int value){errno=value;}
int math_logs_errno(void){return errno;}
int math_logs_errors(void){
#ifdef ADAPTED
 errno=103;if(log(0.0)!=-HUGE_VAL||errno!=ERANGE)return 1;
 errno=104;if(log2f(-0.0f)!=-HUGE_VALF||errno!=ERANGE)return 2;
 errno=105;if(log10l(0.0L)!=-HUGE_VALL||errno!=ERANGE)return 3;
 errno=106;if(!isnan(log(-1.0))||errno!=EDOM)return 4;
 errno=107;if(!isnan(log10f(-INFINITY))||errno!=EDOM)return 5;
 errno=108;if(!isnan(log2l(-LDBL_TRUE_MIN))||errno!=EDOM)return 6;
 errno=109;if(!isnan(log(NAN))||errno!=109)return 7;
 errno=110;if(log2f(1.0f)!=0.0f||errno!=110)return 8;
 errno=111;if(log10l(HUGE_VALL)!=HUGE_VALL||errno!=111)return 9;
#endif
 if(signbit(log(1.0))||signbit(log2f(1.0f))||signbit(log10l(1.0L)))return 10;
 return 0;
}
#ifdef __cplusplus
}
#endif
int main(void){
 for(int n=0;n<350;++n){
  union{float f;uint32_t i;}xf;union{double f;uint64_t i;}xd;
  union{long double f;struct{uint64_t m;uint16_t se;}i;}xl;
  xf.i=(uint32_t)bits();xd.i=bits();xl.i.m=bits()|0x8000000000000000ULL;xl.i.se=(uint16_t)bits();
  if((xl.i.se&0x7fff)==0)xl.i.m>>=1;
  hf(logf(xf.f));hf(log2f(xf.f));hf(log10f(xf.f));
  hd(log(xd.f));hd(log2(xd.f));hd(log10(xd.f));
  hl(logl(xl.f));hl(log2l(xl.f));hl(log10l(xl.f));
 }
 long double values[]={0.0L,-0.0L,LDBL_TRUE_MIN,-LDBL_TRUE_MIN,LDBL_MIN,-LDBL_MIN,0.5L,-0.5L,1.0L,-1.0L,1.5L,-1.5L,LDBL_MAX,-LDBL_MAX,HUGE_VALL,-HUGE_VALL,NAN,0x1.fffffffffffffffep-1L,0x1.0000000000000002p0L,2.0L,10.0L};
 for(unsigned i=0;i<sizeof(values)/sizeof(values[0]);++i){hl(logl(values[i]));hl(log2l(values[i]));hl(log10l(values[i]));}
 double doubles[]={DBL_TRUE_MIN,DBL_MIN,0x1.fffffffffffffp-1,1.0,0x1.0000000000001p0,2.0,10.0,DBL_MAX};
 for(unsigned i=0;i<sizeof(doubles)/sizeof(doubles[0]);++i){hd(log(doubles[i]));hd(log2(doubles[i]));hd(log10(doubles[i]));}
 float floats[]={FLT_TRUE_MIN,FLT_MIN,0x1.fffffep-1f,1.0f,0x1.000002p0f,2.0f,10.0f,FLT_MAX};
 for(unsigned i=0;i<sizeof(floats)/sizeof(floats[0]);++i){hf(logf(floats[i]));hf(log2f(floats[i]));hf(log10f(floats[i]));}
 int errors=math_logs_errors();if(errors)return errors;
 printf("%llu\n",(unsigned long long)hash);return 0;
}
