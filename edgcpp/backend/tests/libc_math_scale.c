#include <math.h>
#include <float.h>
#include <stdint.h>
#include <errno.h>
#include <stdio.h>
#include <limits.h>
static uint64_t state=0x8a5cd789635d2dffULL,hash=1469598103934665603ULL;
static uint64_t bits(void){state=state*6364136223846793005ULL+1;return state;}
static void mix(uint64_t x){hash^=x;hash*=1099511628211ULL;}
static void hf(float x){union{float f;uint32_t i;}u={x};mix(isnan(x)?0x7fc00000U:u.i);}
static void hd(double x){union{double f;uint64_t i;}u={x};mix(isnan(x)?0x7ff8000000000000ULL:u.i);}
static void hl(long double x){union{long double f;struct{uint64_t m;uint16_t se;}i;}u={x};if(isnan(x)){mix(1);mix(0x7fff);}else{mix(u.i.m);mix(u.i.se);}}
#ifdef __cplusplus
extern "C" {
#endif
void math_scale_set_errno(int value){errno=value;}
int math_scale_errno(void){return errno;}
int math_scale_errors(void){
#ifdef ADAPTED
 errno=101;if(scalbn(1,INT_MAX)!=HUGE_VAL||errno!=ERANGE)return 1;
 errno=102;if(scalbnf(-1,INT_MIN)!=0||errno!=ERANGE)return 2;
 errno=103;if(scalblnl(1,LONG_MIN)!=0||errno!=ERANGE)return 3;
 errno=104;if(!isnan(ldexp(NAN,INT_MAX))||errno!=104)return 4;
 errno=105;if(scalbnf(FLT_MIN,-1)!=FLT_MIN/2||errno!=105)return 5;
 errno=106;if(scalbnl(LDBL_MIN,-1)!=LDBL_MIN/2||errno!=106)return 6;
 errno=107;if(scalbn(DBL_MIN,-1)!=DBL_MIN/2||errno!=107)return 7;
 errno=108;if(scalbnl(LDBL_TRUE_MIN,-1)!=0||errno!=ERANGE)return 8;
 errno=109;if(scalbnf(3*FLT_TRUE_MIN,-1)!=2*FLT_TRUE_MIN||errno!=ERANGE)return 9;
 errno=110;if(ldexp(-0.0,INT_MAX)!=0||errno!=110)return 10;
 errno=111;if(scalblnf(INFINITY,LONG_MIN)!=INFINITY||errno!=111)return 11;
 errno=112;if(scalbn(3*DBL_TRUE_MIN,-1)!=2*DBL_TRUE_MIN||errno!=ERANGE)return 12;
 errno=113;if(scalbnl(3*LDBL_TRUE_MIN,-1)!=2*LDBL_TRUE_MIN||errno!=ERANGE)return 13;
#endif
 return 0;
}
#ifdef __cplusplus
}
#endif
int main(void){
 int shifts[]={INT_MIN,-32830,-16446,-16445,-16444,-16383,-16382,-1075,-1074,-1023,-1022,-150,-149,-127,-126,-64,-1,0,1,64,127,128,1023,1024,16383,16384,32830,INT_MAX};
 for(int i=0;i<200;++i){
  union{float f;uint32_t i;}xf;union{double f;uint64_t i;}xd;
  union{long double f;struct{uint64_t m;uint16_t se;}i;}xl;
  xf.i=(uint32_t)bits();xd.i=bits();xl.i.m=bits()|0x8000000000000000ULL;xl.i.se=(uint16_t)bits();if((xl.i.se&0x7fff)==0)xl.i.m>>=1;
  int n=shifts[i%(sizeof(shifts)/sizeof(shifts[0]))];
  hf(scalbnf(xf.f,n));hf(ldexpf(xf.f,n));hf(scalblnf(xf.f,n));
  hd(scalbn(xd.f,n));hd(ldexp(xd.f,n));hd(scalbln(xd.f,n));
  hl(scalbnl(xl.f,n));hl(ldexpl(xl.f,n));hl(scalblnl(xl.f,n));
 }
 long double values[]={0,-0.0L,LDBL_TRUE_MIN,3*LDBL_TRUE_MIN,LDBL_MIN,1,1.5L,LDBL_MAX,HUGE_VALL,-HUGE_VALL,NAN};
 for(unsigned i=0;i<sizeof(values)/sizeof(values[0]);++i){for(unsigned j=0;j<sizeof(shifts)/sizeof(shifts[0]);++j)hl(scalbnl(values[i],shifts[j]));hl(scalblnl(values[i],LONG_MIN));hl(scalblnl(values[i],LONG_MAX));}
 if(math_scale_errors())return 77;
 printf("%llu\n",(unsigned long long)hash);return 0;
}
