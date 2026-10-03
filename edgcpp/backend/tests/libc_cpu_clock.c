#include <time.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
static volatile unsigned long sink;
long cpu_clock_sample(void) { clock_t (*sample)(void)=clock;return sample(); }
void cpu_clock_work(void) { unsigned long x=123;for(int i=0;i<1000000;++i)x=x*6364136223846793005UL+1;sink=x; }
int cpu_clock_errno(int value) { errno=value;(void)cpu_clock_sample();return errno; }
int cpu_clock_cases(void) {
 if(CLOCKS_PER_SEC!=1000000L)return 1;
 clock_t first=cpu_clock_sample();if(first<0)return 2;
 for(int i=0;i<3;++i){cpu_clock_work();clock_t next=cpu_clock_sample();if(next<first)return 3;first=next;}
 return 0;
}
#ifdef __cplusplus
}
#endif
