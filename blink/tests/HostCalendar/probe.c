#define _POSIX_C_SOURCE 200809L
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#ifdef BLINK_MANAGED_CALENDAR
#include "host-calendar.h"
#endif
int Calendar(void) {
  /* Native libc may initialize timezone data and alter errno on its first call. */
  time_t epoch=0; struct tm warm;
  if(!gmtime_r(&epoch,&warm) || !localtime_r(&epoch,&warm))return 10;
  time_t samples[6]={-62135596800L,-1,0,951782400,2147483648L,253402300799L};
  for(int i=0;i<6;i++) {
    struct tm utc, local;
    memset(&utc,0,sizeof(utc)); memset(&local,0,sizeof(local)); errno=123;
    if(gmtime_r(&samples[i],&utc)!=&utc || localtime_r(&samples[i],&local)!=&local || errno!=123)return 1;
    if(utc.tm_sec!=local.tm_sec || utc.tm_min!=local.tm_min || utc.tm_hour!=local.tm_hour || utc.tm_mday!=local.tm_mday || utc.tm_mon!=local.tm_mon || utc.tm_year!=local.tm_year || utc.tm_wday!=local.tm_wday || utc.tm_yday!=local.tm_yday || utc.tm_isdst!=0 || local.tm_isdst!=0)return 2;
    printf("calendar %d %d-%d-%d %d:%d:%d weekday=%d yearday=%d\n",i,utc.tm_year+1900,utc.tm_mon+1,utc.tm_mday,utc.tm_hour,utc.tm_min,utc.tm_sec,utc.tm_wday,utc.tm_yday);
  }
  time_t value;errno=123;
  if(time(&value)==(time_t)-1 || value<=0 || errno!=123)return 3;
  time_t overflow=9223372036854775807L; struct tm output;
  errno=0;if(gmtime_r(&overflow,&output)!=0 || errno!=EOVERFLOW)return 4;
  puts("time and UTC calendar invariants: PASS");return 0;
}
#ifdef BLINK_MANAGED_CALENDAR
long TimeValue(void) { return time(0); }
int CalendarPrivate(int bound) {
  time_t t=253402300800L;struct tm output;memset(&output,0x55,sizeof(output));errno=0;
  if(gmtime_r(&t,&output)!=0 || errno!=(bound?EOVERFLOW:ENODEV) || output.tm_sec!=0x55555555)return 1;
  return 0;
}
#endif
int main(void) { return Calendar(); }
