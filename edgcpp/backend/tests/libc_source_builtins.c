#include <math.h>
#include <stdlib.h>
#include <errno.h>
int main(void){
 volatile float f=-7.5f;volatile double d=-7.5;volatile long double l=-7.5L;
 float fi;double di;long double li;
 if(__builtin_fmodf(f,2.0f)!=-1.5f||__builtin_fmod(d,2.0)!=-1.5||__builtin_fmodl(l,2.0L)!=-1.5L)return 1;
 if(__builtin_modff(f,&fi)!=-0.5f||fi!=-7.0f)return 2;
 if(__builtin_modf(d,&di)!=-0.5||di!=-7.0)return 3;
 if(__builtin_modfl(l,&li)!=-0.5L||li!=-7.0L)return 4;
 volatile int i=-17;if(__builtin_abs(i)!=17)return 5;
 int exponent;if(__builtin_frexp(d,&exponent)!=-0.9375||exponent!=3)return 6;
 l=HUGE_VALL;errno=111;if(!isnan(__builtin_fmodl(l,2.0L))||errno!=EDOM)return 7;
 return 0;
}
