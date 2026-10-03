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
void math_extrema_set_errno(int value){errno=value;}
int math_extrema_errno(void){return errno;}
int math_extrema_errors(void){
#ifdef ADAPTED
 errno=103;if(fdim(DBL_MAX,-DBL_MAX)!=HUGE_VAL||errno!=ERANGE)return 1;
 errno=104;if(fdimf(FLT_MAX,-FLT_MAX)!=HUGE_VALF||errno!=ERANGE)return 2;
 errno=105;if(fdiml(LDBL_MAX,-LDBL_MAX)!=HUGE_VALL||errno!=ERANGE)return 3;
 errno=106;if(fdim(INFINITY,1.0)!=HUGE_VAL||errno!=106)return 4;
 errno=107;if(!isnan(fdimf(NAN,1.0f))||errno!=107)return 5;
 errno=108;if(fdiml(2.0L,5.0L)!=0.0L||errno!=108)return 6;
 errno=109;if(fdim(DBL_TRUE_MIN,0.0)!=DBL_TRUE_MIN||errno!=109)return 7;
#endif
 if(signbit(fdim(-0.0,0.0))||signbit(fdimf(-0.0f,-0.0f))||signbit(fdiml(-0.0L,-1.0L)))return 8;
 if(!signbit(fmin(0.0,-0.0))||signbit(fmax(-0.0,0.0)))return 9;
 if(!signbit(fminf(-0.0f,0.0f))||signbit(fmaxf(0.0f,-0.0f)))return 10;
 if(!signbit(fminl(0.0L,-0.0L))||signbit(fmaxl(-0.0L,0.0L)))return 11;
 return 0;
}
int math_extrema_noalloc(void){
 union{long double f;struct{uint64_t m;uint16_t se;}i;}a,b;
 a.i.m=0x8000000000000000ULL;a.i.se=16383;b.i.m=0xc000000000000000ULL;b.i.se=16384;
 int sum=0;for(int n=0;n<1000;n++){sum+=fminl(a.f,b.f)==a.f;sum+=fmaxl(a.f,b.f)==b.f;}
 return sum==2000?0:1;
}
#ifdef __cplusplus
}
#endif
int main(void){
 for(int n=0;n<600;++n){
  union{float f;uint32_t i;}xf,yf;union{double f;uint64_t i;}xd,yd;
  union{long double f;struct{uint64_t m;uint16_t se;}i;}xl,yl;
  xf.i=(uint32_t)bits();yf.i=(uint32_t)bits();xd.i=bits();yd.i=bits();
  xl.i.m=bits()|0x8000000000000000ULL;xl.i.se=(uint16_t)bits();yl.i.m=bits()|0x8000000000000000ULL;yl.i.se=(uint16_t)bits();
  if((xl.i.se&0x7fff)==0)xl.i.m>>=1;if((yl.i.se&0x7fff)==0)yl.i.m>>=1;
  hf(fminf(xf.f,yf.f));hf(fmaxf(xf.f,yf.f));hf(fdimf(xf.f,yf.f));
  hd(fmin(xd.f,yd.f));hd(fmax(xd.f,yd.f));hd(fdim(xd.f,yd.f));
  hl(fminl(xl.f,yl.f));hl(fmaxl(xl.f,yl.f));hl(fdiml(xl.f,yl.f));
 }
 long double values[]={0.0L,-0.0L,LDBL_TRUE_MIN,-LDBL_TRUE_MIN,LDBL_MIN,-LDBL_MIN,0.5L,-0.5L,1.0L,-1.0L,1.5L,-1.5L,LDBL_MAX,-LDBL_MAX,HUGE_VALL,-HUGE_VALL,NAN};
 for(unsigned i=0;i<sizeof(values)/sizeof(values[0]);++i)for(unsigned j=0;j<sizeof(values)/sizeof(values[0]);++j){hl(fminl(values[i],values[j]));hl(fmaxl(values[i],values[j]));hl(fdiml(values[i],values[j]));}
 int errors=math_extrema_errors();if(errors)return errors;if(math_extrema_noalloc())return 12;
 printf("%llu\n",(unsigned long long)hash);return 0;
}
