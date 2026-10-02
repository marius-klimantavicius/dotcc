#include <time.h>
#include <errno.h>
#include <stddef.h>
#include <limits.h>
#ifdef __cplusplus
extern "C" {
#endif
int clock_cases(void)
{
    struct timespec before,after,unchanged={123,456};time_t stored,stamp;
    int (*sample)(clockid_t,struct timespec*)=clock_gettime;
    int (*sample_utc)(struct timespec*,int)=timespec_get;
    double (*difference)(time_t,time_t)=difftime;
    if(sizeof(struct timespec)!=16 || offsetof(struct timespec,tv_nsec)!=8)return 1;
    errno=71;if(sample(CLOCK_REALTIME,&before)!=0 || errno!=71)return 2;
    stamp=time(&stored);if(stamp!=stored)return 3;
    if(sample_utc(&after,TIME_UTC)!=TIME_UTC || errno!=71 || stamp<before.tv_sec || stamp>after.tv_sec)return 4;
    if(before.tv_nsec<0 || before.tv_nsec>=1000000000L || after.tv_nsec<0 || after.tv_nsec>=1000000000L)return 5;
    if(sample(CLOCK_MONOTONIC,&before)!=0 || sample(CLOCK_MONOTONIC,&after)!=0)return 6;
    if(after.tv_sec<before.tv_sec || (after.tv_sec==before.tv_sec && after.tv_nsec<before.tv_nsec))return 7;
    if(sample(-123,&unchanged)!=-1 || errno!=EINVAL || unchanged.tv_sec!=123 || unchanged.tv_nsec!=456)return 8;
    errno=71;if(sample_utc(&unchanged,0)!=0 || errno!=71 || unchanged.tv_sec!=123)return 9;
    if(difference(LONG_MAX,LONG_MAX-1)!=1 || difference(LONG_MIN+1,LONG_MIN)!=1 || difference(0,1)!=-1)return 10;
    return 0;
}
int clock_sample(int clock,void* output){return clock_gettime(clock,(struct timespec*)output);}
int clock_set_error(int initial,int clock){struct timespec value;errno=initial;clock_gettime(clock,&value);return errno;}
int clock_read_error(void){return errno;}
#ifdef __cplusplus
}
#endif
