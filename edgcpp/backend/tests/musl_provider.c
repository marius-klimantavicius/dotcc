#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <locale.h>
#ifdef __cplusplus
#include <exception>
#endif
static int calls;
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
 free(allocated);freelocale(locale);
 return 0;
}
