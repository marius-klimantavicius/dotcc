/* Synthetic UTC/calendar contracts; no external project input. */
#include <time.h>
#include <locale.h>
#include <langinfo.h>
#include <string.h>
#include <errno.h>
#include <limits.h>
#include <stddef.h>
#ifdef __cplusplus
extern "C" {
#endif
static unsigned long mix(unsigned long h, unsigned long v) { return (h ^ v) * 1099511628211UL; }
static unsigned long text_hash(unsigned long h, const char *s) { while (*s) h=mix(h,(unsigned char)*s++); return mix(h,255); }
int calendar_cases(void)
{
    struct tm value={0}, copy; char buffer[256], small[8]; time_t epoch=0;
    if (sizeof(struct tm)!=56 || offsetof(struct tm,tm_gmtoff)!=40 || offsetof(struct tm,tm_zone)!=48) return 1;
    errno=71;
    if (gmtime_r(&epoch,&value)!=&value || errno!=71 || value.tm_year!=70 || value.tm_mon!=0 || value.tm_mday!=1 || value.tm_wday!=4 || value.tm_yday!=0 || value.tm_isdst!=0 || value.tm_gmtoff!=0 || strcmp(value.tm_zone,"UTC")) return 2;
    if (!strftime(buffer,sizeof buffer,"%a %A %b %B %c|%C %d %D %e %F %g %G %h %H %I %j %m %M %p %r %R %s %S %T %u %U %V %w %W %x %X %y %Y %z %Z %%",&value)) return 3;
    if (strcmp(buffer,"Thu Thursday Jan January Thu Jan  1 00:00:00 1970|19 01 01/01/70  1 1970-01-01 70 1970 Jan 00 12 001 01 00 AM 12:00:00 AM 00:00 0 00 00:00:00 4 00 01 4 00 01/01/70 00:00:00 70 1970 +0000 UTC %")) return 4;
    if (strftime(small,5,"1234",&value)!=4 || strcmp(small,"1234")) return 5;
    small[0]='!'; if (strftime(small,0,"x",&value)!=0 || small[0]!='!') return 6;
    if (strftime(small,4,"1234",&value)!=0 || small[3]!=0) return 7;
    if (strftime(buffer,sizeof buffer,"%Q",&value)!=0) return 8;
    if (!strftime(buffer,sizeof buffer,"%-d|%_d|%0d|%+6Y|%Ec|%Od",&value) || strcmp(buffer,"1| 1|01|+01970|Thu Jan  1 00:00:00 1970|01")) return 9;
    if (asctime_r(&value,buffer)!=buffer || strcmp(buffer,"Thu Jan  1 00:00:00 1970\n")) return 10;
    epoch=-1; if (!gmtime_r(&epoch,&value) || value.tm_year!=69 || value.tm_mon!=11 || value.tm_mday!=31 || value.tm_hour!=23 || value.tm_sec!=59 || timegm(&value)!=-1 || errno!=71) return 11;
    value.tm_year=100;value.tm_mon=1;value.tm_mday=30;value.tm_hour=0;value.tm_min=0;value.tm_sec=0;
    if (timegm(&value)!=951868800L || value.tm_mon!=2 || value.tm_mday!=1 || value.tm_yday!=60) return 12;
    value.tm_year=0;value.tm_mon=1;value.tm_mday=29;if(timegm(&value)!=-2203891200L || value.tm_mon!=2 || value.tm_mday!=1) return 13;
    value.tm_year=8100;value.tm_mon=0;value.tm_mday=1;if(timegm(&value)!=253402300800L || strftime(buffer,sizeof buffer,"%Y-%m-%d",&value)!=12 || strcmp(buffer,"+10000-01-01")) return 14;
    copy=value;epoch=LONG_MAX;errno=0;if(gmtime_r(&epoch,&value)!=0 || errno!=EOVERFLOW || value.tm_year!=copy.tm_year) return 15;
    value.tm_year=INT_MAX;value.tm_mon=INT_MAX;errno=0;if(timegm(&value)!=-1 || errno!=EOVERFLOW || value.tm_year!=INT_MAX) return 16;
    epoch=0;gmtime_r(&epoch,&value);value.tm_gmtoff=-19800;if(strftime(buffer,sizeof buffer,"%z %s",&value)!=11 || strcmp(buffer,"-0530 19800")) return 17;
    value.tm_isdst=-1;if(strftime(buffer,sizeof buffer,"a%z%Zb",&value)!=2 || strcmp(buffer,"ab")) return 18;
    value.tm_isdst=0;value.tm_zone=(const char*)1;if(strftime(buffer,sizeof buffer,"a%Zb",&value)!=2 || strcmp(buffer,"ab")) return 19;
    if(strcmp(nl_langinfo(DAY_1),"Sunday") || strcmp(nl_langinfo(MON_12),"December") || strcmp(nl_langinfo(RADIXCHAR),".") || strcmp(nl_langinfo(THOUSEP),"") || strcmp(nl_langinfo(CRNCYSTR),"")) return 20;
    locale_t c=newlocale(LC_ALL_MASK,"C",(locale_t)0),u=newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);
    if(!c || !u) return 21;
    if(strcmp(nl_langinfo_l(CODESET,c),"ASCII") || strcmp(nl_langinfo_l(CODESET,u),"UTF-8") || strcmp(nl_langinfo_l(_NL_LOCALE_NAME(LC_TIME),u),"C.UTF-8")) return 22;
    if(!strftime_l(buffer,sizeof buffer,"%c",&value,u) || strcmp(buffer,"Thu Jan  1 00:00:00 1970")) return 23;
    locale_t old=uselocale(u);if(strcmp(nl_langinfo(CODESET),"UTF-8"))return 24;uselocale(old);
    freelocale(c);freelocale(u);return 0;
}
unsigned long calendar_digest(void)
{
    unsigned long h=1469598103934665603UL;struct tm value;char buffer[256];
    /* Every day of a Gregorian 400-year cycle, and out-of-DateTime-range dates. */
    for(int i=0;i<146097;++i){time_t t=951868800L+(long)i*86400+45296;if(!gmtime_r(&t,&value) || timegm(&value)!=t)return 0;
        h=mix(h,(unsigned long)value.tm_year);h=mix(h,value.tm_mon);h=mix(h,value.tm_mday);h=mix(h,value.tm_yday);h=mix(h,value.tm_wday);
        if((i&127)==0){if(!strftime(buffer,sizeof buffer,"%Y %G %g %V %U %W %j %u %w %s %F %z %Z",&value))return 0;h=text_hash(h,buffer);}}
    const long years[]={INT_MIN,-1000000,-2001,-1901,-1900,-1899,-1,0,100,200,8100,1000000,INT_MAX};
    for(unsigned int i=0;i<sizeof years/sizeof years[0];++i){memset(&value,0,sizeof value);value.tm_year=(int)years[i];value.tm_mon=6;value.tm_mday=7;value.tm_hour=12;time_t t=timegm(&value);struct tm copy;if(!gmtime_r(&t,&copy) || copy.tm_year!=years[i])return 0;if(!strftime(buffer,sizeof buffer,"%Y %G %V %j %s %F",&copy))return 0;h=text_hash(h,buffer);}
    for(int category=0;category<6;++category)for(int item=0;item<52;++item)h=text_hash(h,nl_langinfo((category<<16)|item));
    return h;
}
void calendar_fatal(void){struct tm value={0};value.tm_year=8100;value.tm_mday=1;asctime(&value);}
void *calendar_static_time(long t){return gmtime(&t);}
char *calendar_static_text(long t){struct tm value;return gmtime_r(&t,&value)?asctime(&value):(char*)0;}
void *calendar_locale_create(void){return newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);}
void calendar_locale_free(void *l){freelocale((locale_t)l);}
int calendar_invalid_locale(void *l){char b[32];struct tm t={0};errno=0;return strftime_l(b,sizeof b,"%Y",&t,(locale_t)l)==0 && errno==EINVAL;}
int calendar_select_locale(int utf8){return setlocale(LC_ALL,utf8?"C.UTF-8":"C")!=0;}
int calendar_codeset(void){return strcmp(nl_langinfo(CODESET),"UTF-8")==0;}
void *calendar_thread_locale(void *l){return uselocale((locale_t)l);}
char *calendar_zone(void *v,char *buffer){return strftime(buffer,16,"%Z",(struct tm*)v)?buffer:(char*)0;}
#ifdef __cplusplus
}
#endif
