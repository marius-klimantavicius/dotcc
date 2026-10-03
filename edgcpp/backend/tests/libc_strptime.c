#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <limits.h>
#ifdef __cplusplus
extern "C" {
#endif
/* Declared privately until provider/header integration is accepted. */
char *strptime(const char *,const char *,struct tm *);
static unsigned long digest=1469598103934665603UL;
static void hash(long x){digest^=(unsigned long)x;digest*=1099511628211UL;}
static void init(struct tm *t){memset(t,0,sizeof *t);t->tm_year=111;t->tm_mon=8;t->tm_mday=9;t->tm_wday=4;t->tm_yday=211;t->tm_isdst=-7;t->tm_gmtoff=19;}
static void hash_tm(const struct tm *t){hash(t->tm_year);hash(t->tm_mon);hash(t->tm_mday);hash(t->tm_hour);hash(t->tm_min);hash(t->tm_sec);hash(t->tm_wday);hash(t->tm_yday);hash(t->tm_isdst);hash(t->tm_gmtoff);}
unsigned long strptime_digest(void){return digest;}
int strptime_select(const char *zone){return setenv("TZ",zone,1);}
int strptime_zone(const char *name){struct tm t;init(&t);return strptime(name,"%Z",&t)?t.tm_isdst:-99;}
int strptime_cases(const char *unused){
 (void)unused;digest=1469598103934665603UL;setenv("TZ","EST5EDT,M3.2.0,M11.1.0",1);tzset();
 for(int year=1996;year<=2024;++year)for(int month=1;month<=12;++month)for(int day=1;day<=28;++day){
  char text[64];snprintf(text,sizeof text,"%04d-%02d-%02d 23:17:59tail",year,month,day);struct tm t;init(&t);char *p=strptime(text,"%Y-%m-%d %H:%M:%S",&t);
  if(!p||strcmp(p,"tail")||t.tm_year!=year-1900||t.tm_mon!=month-1||t.tm_mday!=day||t.tm_hour!=23||t.tm_min!=17||t.tm_sec!=59)return 1;hash(p-text);hash_tm(&t);
 }
 const char *inputs[]={"Sun sunday","jAn January Jan","01/02/24tail","2024-02-29tail","20 24","24 20","68","69","99","-0044","+50000","366","00 53 53 24 2024 -12345","1","6","+0545","-0330","ESTtail","EDTtail","11:22:33 PM","12:00:00 AM","23:45","23:45:60","\t \n2024","%","Thu Feb 29 23:17:59 2024","02/29/24","23:17:59"};
 const char *formats[]={"%a %A","%b %B %h","%D","%F","%C %y","%y %C","%y","%y","%y","%Y","%12Y","%j","%U %W %V %g %G %s","%u","%u","%z","%z","%Z","%Z","%r","%r","%R","%T","%n%Y","%%","%c","%x","%X"};
 for(unsigned i=0;i<sizeof inputs/sizeof inputs[0];++i){struct tm t;init(&t);char *p=strptime(inputs[i],formats[i],&t);if(!p)return 10+(int)i;hash(p-inputs[i]);hash_tm(&t);}
 const char *bad[]={"0","32","0","13","24","0","13","60","61","0","367","7","+","+123","x","wrong"};
 const char *bad_fmt[]={"%d","%d","%m","%m","%H","%I","%I","%M","%S","%j","%j","%w","%z","%z","%Y","%A"};
 for(unsigned i=0;i<sizeof bad/sizeof bad[0];++i){struct tm t;init(&t);if(strptime(bad[i],bad_fmt[i],&t))return 50+(int)i;hash(i);hash_tm(&t);}
 return 0;
}
int strptime_extensions(void){
 struct tm t;char *p;const char *s;
 setenv("TZ","AAA-2BBB,M3.2.0,M11.1.0",1);
 if(strptime_zone("AAA")!=0||strptime_zone("BBB")!=1)return 101;
 for(unsigned n=0;n<64;++n){char *text=(char*)malloc(n+1);if(!text)return 102;memset(text,'Q',n);text[n]=0;init(&t);p=strptime(text,"%Z",&t);int good=p==text+n&&t.tm_isdst==-7;free(text);if(!good)return 103;}
 s="BOGUS!";init(&t);if(strptime(s,"%Z",&t)!=s+5)return 104;
 s="7";init(&t);if(strptime(s,"%u",&t)!=s+1||t.tm_wday!=0)return 105;
 s="2024-02-29 23:17:59";init(&t);p=strptime(s,"%EY-%Om-%Od %OH:%OM:%OS",&t);if(!p||*p||t.tm_year!=124||t.tm_mon!=1||t.tm_mday!=29||t.tm_hour!=23||t.tm_min!=17||t.tm_sec!=59)return 106;
 s="2147485547";init(&t);if(!strptime(s,"%12Y",&t)||t.tm_year!=INT_MAX)return 107;
 s="-2147481748";init(&t);if(!strptime(s,"%12Y",&t)||t.tm_year!=INT_MIN)return 108;
 if(strptime("2147485548","%12Y",&t)||strptime("-2147481749","%12Y",&t)||strptime("999999999999","%12Y",&t))return 109;
 if(strptime("2147483647 99","%12C %y",&t)||strptime("2024","%999999999999999999999999Y",&t))return 110;
 if(strptime("2024","%OY",&t)||strptime("29","%Ed",&t)||strptime("","%E",&t))return 111;
 return 0;
}
#ifdef __cplusplus
}
#endif
