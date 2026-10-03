#include <time.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <limits.h>

#ifdef __cplusplus
extern "C" {
#endif

static unsigned long digest=1469598103934665603UL;
static void hash(long value) { digest^=(unsigned long)value;digest*=1099511628211UL; }
static void hash_tm(const struct tm *t) {
 hash(t->tm_sec);hash(t->tm_min);hash(t->tm_hour);hash(t->tm_mday);hash(t->tm_mon);hash(t->tm_year);hash(t->tm_wday);hash(t->tm_yday);hash(t->tm_isdst);hash(t->tm_gmtoff);
 const char *p=t->tm_zone;while(*p)hash((unsigned char)*p++);hash(-1);
}
int timezone_select(const char *zone) { int r=setenv("TZ",zone,1);if(!r)tzset();return r; }
long timezone_offset(long seconds) { time_t s=seconds;struct tm t;if(!localtime_r(&s,&t))return LONG_MIN;return t.tm_gmtoff; }
void *timezone_static(long seconds) { time_t s=seconds;return localtime(&s); }
char *timezone_name(long seconds) { time_t s=seconds;struct tm t;return localtime_r(&s,&t)?(char *)t.tm_zone:0; }
int timezone_format_zone(const void *tm,char *buffer) { return (int)strftime(buffer,32,"%Z",(const struct tm*)tm); }
int timezone_validate_name(const char *name,char *buffer) { struct tm t={0};t.tm_zone=name;return (int)strftime(buffer,32,"%Z",&t); }
int timezone_globals(void) { return timezone==18000 && daylight==1 && !strcmp(tzname[0],"EST") && !strcmp(tzname[1],"EDT"); }
long timezone_hot(long seconds,int count) { long result=0;for(int i=0;i<count;++i)result+=timezone_offset(seconds+i);return result; }
unsigned long timezone_digest(void) { return digest; }
int timezone_cases(const char *file) {
 const char *zones[]={"UTC0","EST5EDT,M3.2.0,M11.1.0","AEST-10AEDT,M10.1.0,M4.1.0/3","NST3:30NDT,M3.2.0,M11.1.0","XST-5:45","STD0DST,J60/-1,J300/25","STD0DST,60,300",file};
 struct tm t,back;time_t s;char text[128];digest=1469598103934665603UL;
 for(unsigned z=0;z<sizeof zones/sizeof zones[0];++z) {
  if(timezone_select(zones[z]))return 1;
  for(long day=-2922;day<21915;day+=3) {
   s=(time_t)day*86400+12345;if(!localtime_r(&s,&t))return 2;hash_tm(&t);back=t;
   if(mktime(&back)!=s || memcmp(&t,&back,9*sizeof(int)))return 3;
   if(!strftime(text,sizeof text,"%Y-%m-%d %H:%M:%S %z %Z",&t))return 4;
   for(char *p=text;*p;++p)hash((unsigned char)*p);hash(-2);
  }
  const int years[]={-50000,-400,0,70,100,124,50000};
  for(unsigned y=0;y<sizeof years/sizeof years[0];++y)for(int m=0;m<12;++m) {
   memset(&t,0,sizeof t);t.tm_year=years[y];t.tm_mon=m;t.tm_mday=15;t.tm_hour=12;t.tm_isdst=-1;s=mktime(&t);hash(s);hash_tm(&t);
   if(!localtime_r(&s,&back)||memcmp(&t,&back,9*sizeof(int)))return 5;
  }
 }
 if(timezone_select("EST5EDT,M3.2.0,M11.1.0")||!timezone_globals())return 6;
 for(int month=2;month<=10;month+=8)for(int hint=-1;hint<=1;++hint) {
  memset(&t,0,sizeof t);t.tm_year=124;t.tm_mon=month;t.tm_mday=month==2?10:3;t.tm_hour=month==2?2:1;t.tm_min=30;t.tm_isdst=hint;
  s=mktime(&t);hash(s);hash_tm(&t);
 }
 if(timezone_select("UTC0"))return 7;s=0;char a[26],b[26];if(!ctime_r(&s,a)||!asctime_r(localtime_r(&s,&t),b)||strcmp(a,b)||strcmp(ctime(&s),a))return 8;
 errno=0;s=LONG_MAX;if(localtime_r(&s,&t)||errno!=EOVERFLOW)return 9;
 errno=0;s=LONG_MIN;if(localtime_r(&s,&t)||errno!=EOVERFLOW)return 10;
 if(timezone_select(":missing.synthetic.zone"))return 11;if(timezone_offset(0)!=0)return 12;
 if(timezone_select(""))return 13;if(timezone_offset(0)!=0)return 14;
 return 0;
}

#ifdef __cplusplus
}
#endif
