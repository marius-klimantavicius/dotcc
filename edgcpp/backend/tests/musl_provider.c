#include <strings.h>
#include <threads.h>
#include <math.h>
#include <float.h>
#include <errno.h>
#include <wchar.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <locale.h>
#include <wchar.h>
#include <wctype.h>
#include <time.h>
#ifdef __cplusplus
#include <exception>
#endif
static int calls;
static int provider_worker(void *argument){thrd_exit(*(int*)argument);}
static int compare_ints(const void* a,const void* b) {
 int x=*(const int*)a,y=*(const int*)b;return (x>y)-(x<y);
}
int main(void) {
  wchar_t wide_output[8];if(swprintf(wide_output,8,L"%d",17)!=2 || wcsnlen(wide_output,8)!=2 || wide_output[0]!=L'1' || wide_output[1]!=L'7')return 85;
  long double integral_part; if(fmodl(7.5L,2.0L)!=1.5L || modfl(-3.25L,&integral_part)!=-0.25L || integral_part!=-3.0L)return 84;
  srand(17);int first_random=rand();srand(17);if(rand()!=first_random)return 81;
  srandom(19);long first_bsd=random();srandom(19);if(random()!=first_bsd)return 82;
  srand48(23);double first_48=drand48();srand48(23);if(drand48()!=first_48)return 83;
 char text[80];char* end;int count=-1;
 ++calls;
 if(calls!=1)return 1;
 errno=71;
 if(strtol("-123tail",&end,10)!=-123 || strcmp(end,"tail") || errno!=71)return 2;
 if(strtod("0x1.8p2x",&end)!=6 || *end!='x')return 3;
 if(strtold("1.0000000000000000001084202172485504434",&end)!=0x1.0000000000000002p0L || *end)return 4;
 if(snprintf(text,sizeof(text),"%s:%d:%.2f%n","value",atoi("42"),3.25,&count)!=13 || count!=13 || strcmp(text,"value:42:3.25"))return 5;
#ifdef __cplusplus
 try {throw 7;}catch(int value){if(value!=7)return 6;}
#endif
 locale_t locale=newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);
 if(!locale || strtod_l("2.5x",&end,locale)!=2.5 || *end!='x')return 7;
 char* allocated=0;
 if(asprintf(&allocated,"%s-%d",localeconv()->decimal_point,17)!=4 || strcmp(allocated,".-17"))return 8;
 free(allocated);
 wchar_t wide[24];wcscpy(wide,L"wide ");wcscat(wide,L"storage");
 if(wcslen(wide)!=12 || wcscmp(wide,L"wide storage") ||
    wcsstr(wide,L"storage")!=wide+5)return 9;
 if(!iswalpha(0x03b1) || towupper(0x03b1)!=0x0391 ||
    !iswctype(0x03b1,wctype("alpha")))return 10;
 locale_t previous=uselocale(locale);
 mbstate_t state={0};const char* encoded="\xce\xb1";
 wchar_t decoded[4];
 if(mbsrtowcs(decoded,&encoded,4,&state)!=1 || encoded || decoded[0]!=0x03b1 || decoded[1])return 11;
 wchar_t* wide_end;
 if(wcstol(L"\u2003-29tail",&wide_end,10)!=-29 || wcscmp(wide_end,L"tail"))return 12;
 if(wcstold(L"0x1.8p2x",&wide_end)!=6 || *wide_end!=L'x')return 13;
 time_t epoch=0;struct tm civil;char calendar[80];
 if(!gmtime_r(&epoch,&civil) || strftime(calendar,sizeof(calendar),"%F %T %Z",&civil)!=23 ||
    strcmp(calendar,"1970-01-01 00:00:00 UTC"))return 14;
 wchar_t wide_calendar[80];
 if(wcsftime(wide_calendar,80,L"%F %T %Z",&civil)!=23 ||
    wcscmp(wide_calendar,L"1970-01-01 00:00:00 UTC"))return 15;
 int values[6]={4,-2,9,0,4,1};int key=4;
 qsort(values,6,sizeof(int),compare_ints);
 if(values[0]!=-2 || values[5]!=9 || !bsearch(&key,values,6,sizeof(int),compare_ints))return 16;
 div_t quotient=div(-17,5);
 if(quotient.quot!=-3 || quotient.rem!=-2 || labs(-37)!=37)return 17;
 if(setenv("__EDG_SYNTHETIC_PROVIDER_ENV_791", "first",1) ||
    strcmp(getenv("__EDG_SYNTHETIC_PROVIDER_ENV_791"),"first"))return 18;
 char borrowed[]="__EDG_SYNTHETIC_PROVIDER_ENV_791=second";
 if(putenv(borrowed) || strcmp(getenv("__EDG_SYNTHETIC_PROVIDER_ENV_791"),"second"))return 19;
 if(unsetenv("__EDG_SYNTHETIC_PROVIDER_ENV_791") || getenv("__EDG_SYNTHETIC_PROVIDER_ENV_791"))return 20;
 int scanned=0,consumed=-1;long double real=0;char word[8];
 if(sscanf("-81 0x1.8p2 sample!","%d %Lf %6s%n",&scanned,&real,word,&consumed)!=3 ||
    scanned!=-81 || real!=6 || strcmp(word,"sample") || consumed!=18)return 21;
 if(setenv("TZ","EST5EDT,M3.2.0/2,M11.1.0/2",1))return 22;
 tzset();
 if(!localtime_r(&epoch,&civil) || civil.tm_year!=69 || civil.tm_mon!=11 ||
    civil.tm_mday!=31 || civil.tm_hour!=19 || civil.tm_isdst!=0 || mktime(&civil)!=epoch)return 23;
 time_t summer=1719835200;
 if(!localtime_r(&summer,&civil) || civil.tm_year!=124 || civil.tm_mon!=6 ||
    civil.tm_mday!=1 || civil.tm_hour!=8 || civil.tm_isdst!=1 || mktime(&civil)!=summer)return 24;
 if(setenv("TZ","UTC0",1))return 25;
 tzset();
 if(!localtime_r(&epoch,&civil) || civil.tm_year!=70 || civil.tm_hour!=0)return 26;
 wchar_t wide_word[8];int wide_number=0;
 if(swscanf(L"37 omega",L"%d %7ls",&wide_number,wide_word)!=2 ||
    wide_number!=37 || wcscmp(wide_word,L"omega"))return 27;
 if(strcasecmp("CaLeNdAr","calendar") || strncasecmp_l("AbCd","aBCx",3,locale))return 28;
 char *parsed=strptime("2024-02-29 23:17:59","%F %T",&civil);
 if(!parsed || *parsed || civil.tm_year!=124 || civil.tm_mon!=1 || civil.tm_mday!=29 || civil.tm_hour!=23)return 29;
 int requested=37,result=0;thrd_t thread;
 if(thrd_create(&thread,provider_worker,&requested)!=thrd_success || thrd_join(thread,&result)!=thrd_success || result!=37)return 30;
 if(fmin(-0.0,0.0)!=0 || !signbit(fmin(-0.0,0.0)) || fmaxf(3.0f,7.0f)!=7.0f || fdiml(7.0L,3.0L)!=4.0L)return 31;
 errno=0;if(fdim(DBL_MAX,-DBL_MAX)!=HUGE_VAL || errno!=ERANGE)return 32;
 FILE *temporary=tmpfile();if(!temporary)return 33;
 setbuf(temporary,0);if(fputs("q",temporary)<0)return 34;rewind(temporary);
 if(fgetc(temporary)!='q' || fclose(temporary))return 35;
 void (*error_output)(const char*)=perror;if(!error_output)return 36;
 volatile double logarithm_input=8.0;
 if(__builtin_log2(logarithm_input)!=3.0 || log10f(100.0f)!=2.0f || logl(1.0L)!=0.0L)return 37;
 /* Compare runtime addresses: original EDG C++ folds a direct comparison
    of distinct source entities before either backend sees their addresses. */
#define CHECK_LOG_ADDRESS(name,T) {T (*volatile ordinary)(T)=name; T (*volatile intrinsic)(T)=__builtin_##name; if(ordinary!=intrinsic)return 38;}
 CHECK_LOG_ADDRESS(log,double);CHECK_LOG_ADDRESS(logf,float);CHECK_LOG_ADDRESS(logl,long double);
 CHECK_LOG_ADDRESS(log2,double);CHECK_LOG_ADDRESS(log2f,float);CHECK_LOG_ADDRESS(log2l,long double);
 CHECK_LOG_ADDRESS(log10,double);CHECK_LOG_ADDRESS(log10f,float);CHECK_LOG_ADDRESS(log10l,long double);
#undef CHECK_LOG_ADDRESS
 errno=0;if(log10l(0.0L)!=-HUGE_VALL || errno!=ERANGE)return 39;
 uselocale(previous);freelocale(locale);
 return 0;
}
