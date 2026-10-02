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
static int compare_ints(const void* a,const void* b) {
 int x=*(const int*)a,y=*(const int*)b;return (x>y)-(x<y);
}
int main(void) {
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
 uselocale(previous);freelocale(locale);
 return 0;
}
