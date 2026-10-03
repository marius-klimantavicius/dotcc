#define _DEFAULT_SOURCE 1
#include <stdlib.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
static uint64_t hash=1469598103934665603ULL;
static void mix(uint64_t x) { hash^=x; hash*=1099511628211ULL; }
#ifdef __cplusplus
extern "C" {
#endif
void prng_seed(unsigned seed) { srand(seed);srandom(seed);srand48(seed); }
int prng_next(void) { return rand(); }
long prng_random(void) { return random(); }
unsigned long long prng_sum(unsigned count) { unsigned long long sum=0;while(count--)sum+=(unsigned long)random();return sum; }
#ifdef __cplusplus
}
#endif
int main(void) {
  unsigned seeds[]={0,1,17,0x7fffffffU,0xffffffffU};
  for(unsigned si=0;si<5;++si) {
    unsigned state=seeds[si];srand(state);srandom(state);srand48((long)(int)state);
    for(unsigned i=0;i<5000;++i) {
      int r=rand();if(r<0||r>RAND_MAX)return 1;mix((unsigned)r);mix((unsigned)rand_r(&state));mix((unsigned long)random());
      union {double d;uint64_t u;} value;value.d=drand48();if(!(value.d>=0&&value.d<1))return 2;mix(value.u);mix((unsigned long)lrand48());mix((unsigned long)mrand48());
    }
  }
  uint32_t a[64],b[64];unsigned sizes[]={8,31,32,63,64,127,128,255,256};
  for(unsigned i=0;i<9;++i) {
    char *old=initstate(111,(char*)a,sizes[i]);if(!old)return 3;
    long first=random();char *prev=initstate(222,(char*)b,256);if(prev!=(char*)a)return 4;
    for(int j=0;j<37;++j)mix((unsigned long)random());
    if(setstate((char*)a)!=(char*)b)return 5;
    long second=random();if(setstate(old)!=(char*)a)return 6;
    initstate(111,(char*)b,sizes[i]);if(random()!=first||random()!=second)return 7;setstate(old);
    mix(first);mix(second);
  }
  if(initstate(7,(char*)a,7)!=0)return 8;
  unsigned short state[3]={0x1234,0xabcd,0xffff},parameters[7]={1,2,3,4,5,6,7};
  seed48(state);lcong48(parameters);
  for(int i=0;i<1000;++i){union{double d;uint64_t u;}value;value.d=erand48(state);mix(value.u);mix((unsigned long)nrand48(state));mix((unsigned long)jrand48(state));}
  unsigned short *saved=seed48(state);unsigned short copy[3];memcpy(copy,saved,sizeof copy);double next=drand48();seed48(state);if(drand48()!=next)return 9;mix(copy[0]);mix(copy[1]);mix(copy[2]);
  printf("%llu\n",(unsigned long long)hash);return 0;
}
