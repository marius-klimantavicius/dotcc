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
void math_parts_set_errno(int value){errno=value;}
int math_parts_errno(void){return errno;}
int math_parts_errors(void){
#ifdef ADAPTED
  errno=103;if(!isnan(fmod(1.0,0.0))||errno!=EDOM)return 1;
  errno=104;if(!isnan(fmodf(INFINITY,1.0f))||errno!=EDOM)return 2;
  errno=105;if(!isnan(fmodl(-HUGE_VALL,2.0L))||errno!=EDOM)return 3;
  errno=106;if(!isnan(fmod(NAN,0.0))||errno!=106)return 4;
  errno=107;if(!isnan(fmodf(INFINITY,NAN))||errno!=107)return 5;
  errno=108;if(fmodl(7.0L,4.0L)!=3.0L||errno!=108)return 6;
#endif
  return 0;
}
#ifdef __cplusplus
}
#endif
int main(void){
  for(int n=0;n<500;++n){
    union{float f;uint32_t i;}xf,yf;union{double f;uint64_t i;}xd,yd;
    union{long double f;struct{uint64_t m;uint16_t se;}i;}xl,yl;
    xf.i=(uint32_t)bits();yf.i=(uint32_t)bits();xd.i=bits();yd.i=bits();
    xl.i.m=bits()|0x8000000000000000ULL;xl.i.se=(uint16_t)bits();yl.i.m=bits()|0x8000000000000000ULL;yl.i.se=(uint16_t)bits();
    if((xl.i.se&0x7fff)==0)xl.i.m>>=1;if((yl.i.se&0x7fff)==0)yl.i.m>>=1;
    float fi;double di;long double li;
    hf(modff(xf.f,&fi));hf(fi);hd(modf(xd.f,&di));hd(di);hl(modfl(xl.f,&li));hl(li);
    hf(fmodf(xf.f,yf.f));hd(fmod(xd.f,yd.f));hl(fmodl(xl.f,yl.f));
  }
  long double values[]={0.0L,-0.0L,LDBL_TRUE_MIN,-LDBL_TRUE_MIN,LDBL_MIN,-LDBL_MIN,0.5L,-0.5L,1.0L,-1.0L,1.5L,-1.5L,LDBL_MAX,-LDBL_MAX,HUGE_VALL,-HUGE_VALL,NAN};
  for(unsigned i=0;i<sizeof(values)/sizeof(values[0]);++i){long double integral;hl(modfl(values[i],&integral));hl(integral);for(unsigned j=0;j<sizeof(values)/sizeof(values[0]);++j)hl(fmodl(values[i],values[j]));}
  int errors=math_parts_errors();if(errors)return errors;
  printf("%llu\n",(unsigned long long)hash);return 0;
}
