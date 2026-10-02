#include <wchar.h>
#include <time.h>
#include <locale.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
int wide_calendar_cases(void)
{
    struct tm tm;time_t stamp=0;wchar_t buffer[128],small[5];
    if(!gmtime_r(&stamp,&tm))return 1;
    if(wcsftime(buffer,128,L"%a %A %b %B %c|%Y %G %V %z %Z %%",&tm)!=74 || wcscmp(buffer,L"Thu Thursday Jan January Thu Jan  1 00:00:00 1970|1970 1970 01 +0000 UTC %"))return 2;
    if(wcsftime(buffer,128,L"\u03b1\U0001f600 %F\n",&tm)!=14 || wcscmp(buffer,L"\u03b1\U0001f600 1970-01-01\n"))return 3;
    if(wcsftime(small,5,L"1234",&tm)!=4 || wcscmp(small,L"1234"))return 4;
    small[0]=99;if(wcsftime(small,0,L"x",&tm)!=0 || small[0]!=99)return 5;
    if(wcsftime(small,4,L"1234",&tm)!=0 || small[3]!=0)return 6;
    if(wcsftime(buffer,128,L"%Q",&tm)!=0)return 7;
    if(wcsftime(buffer,128,L"%-d|%_d|%0d|%Ec|%Od",&tm)!=35 || wcscmp(buffer,L"1| 1|01|Thu Jan  1 00:00:00 1970|01"))return 8;
    locale_t locale=newlocale(LC_ALL_MASK,"C.UTF-8",(locale_t)0);if(!locale)return 9;
    size_t (*format)(wchar_t*,size_t,const wchar_t*,const struct tm*,locale_t)=wcsftime_l;
    if(format(buffer,128,L"%c",&tm,locale)!=24 || wcscmp(buffer,L"Thu Jan  1 00:00:00 1970"))return 10;
    freelocale(locale);return 0;
}
unsigned long wide_calendar_digest(void)
{
    const wchar_t *formats[]={L"%a %A %h %B %c",L"%C %d %D %e %F %g %G",L"%H %I %j %m %M %p %r",L"%R %s %S %t %T %u %U %V %w %W",L"%x %X %y %Y %z %Z %%",L"%+8Y|%8G|%_d|%-m",L"\u03b1\U0001f600 %F"};
    unsigned long h=1469598103934665603UL;struct tm tm;wchar_t buffer[256];
    for(int i=-5;i<410;++i){tm.tm_year=i*100;tm.tm_mon=(i+12)%12;tm.tm_mday=1;tm.tm_hour=13;tm.tm_min=14;tm.tm_sec=15;timegm(&tm);
        for(unsigned int j=0;j<sizeof formats/sizeof formats[0];++j){size_t count=wcsftime(buffer,256,formats[j],&tm);if(!count)return 0;for(size_t k=0;k<count;++k)h=(h^(unsigned int)buffer[k])*1099511628211UL;h=(h^255)*1099511628211UL;}}
    return h;
}
int wide_calendar_invalid(void *l){wchar_t b[16];struct tm t={0};errno=0;return wcsftime_l(b,16,L"%Y",&t,(locale_t)l)==0 && errno==EINVAL;}
#ifdef __cplusplus
}
#endif
