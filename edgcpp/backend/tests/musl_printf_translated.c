/* Synthetic formatting corpus shared by original EDG C and generated C#. */
#include <stdio.h>
#include <stdlib.h>
#include <ctype.h>
#include "format_file.h"
#include <stdarg.h>
#include <stdint.h>
#include <float.h>
#include <limits.h>
#include <math.h>
#include <string.h>
#include <errno.h>
#include <locale.h>
static int via_va(char *destination,size_t capacity,const char *format,...)
{
    va_list arguments;va_start(arguments,format);int result=vsnprintf(destination,capacity,format,arguments);va_end(arguments);return result;
}
static int lock_contract_body(void *context,const char *format,va_list arguments)
{
    int value=va_arg(arguments,int);
    return fprintf((FILE*)context,"%s%d",format,value);
}
static int lock_contract_call(FILE *file,...)
{
    va_list arguments;va_start(arguments,file);
    int (*volatile invoke)(FILE*,__edg_format_body,void*,const char*,va_list)=__edg_with_file_lock;
    int result=invoke(file,lock_contract_body,file,"L",arguments);va_end(arguments);return result;
}
#ifdef __cplusplus
extern "C" {
#endif
unsigned long format_locale_width(void) {return MB_CUR_MAX;}
char *format_error_pointer(void) {return strerror(EINVAL);}
int format_case(int index,char *out,size_t size)
{
    switch(index) {
    case 0:return snprintf(out,size,"%+08d/%-8.3u/%#08x/%#.0o",-42,12u,0xabcu,0u);
    case 1:return snprintf(out,size,"%ld/%lu/%lld/%llu/%jd/%zu/%td",LONG_MIN,ULONG_MAX,LLONG_MIN,ULLONG_MAX,(intmax_t)INTMAX_MIN,(size_t)-1,(ptrdiff_t)-17);
    case 2:return snprintf(out,size,"%hhd/%hhu/%hd/%hu",-128,255,-32768,65535);
    case 3:return snprintf(out,size,"%*.*f/%#g",-15,7,0.1,1000.0);
    case 4:return snprintf(out,size,"%3$*1$.*2$f|%3$g",12,4,1.25);
    case 5:return snprintf(out,size,"A%cB%s%%",0,"\xffz");
    case 6:return snprintf(out,size,"%g|%+.0f|%a",-0.0,-0.0,-0.0);
    case 7:return snprintf(out,size,"%a|%.17g|%.17e",DBL_TRUE_MIN,DBL_MAX,DBL_MIN);
    case 8:return snprintf(out,size,"%.0f/%.0f/%.2f/%.2g",2.5,-2.5,1.125,9.95);
    case 9:return snprintf(out,size,"%.21Lg|%.21Le",LDBL_TRUE_MIN,LDBL_MAX);
    case 10:return snprintf(out,size,"%.21Lg|%.24Le|%La",1.0L+LDBL_EPSILON,0.1L,LDBL_MIN);
    case 11:return snprintf(out,size,"%g|%g|%Lg",(double)INFINITY,(double)NAN,-HUGE_VALL);
    case 12:return snprintf(out,size,"[%s]|[%10.3s]",(char*)0,"abcdef");
    case 13:errno=EEXIST;return snprintf(out,size,"errno:%m");
    case 14:setlocale(LC_CTYPE,"C");return snprintf(out,size,"%ls|%lc",L"abc",(unsigned int)'Z');
    case 15:setlocale(LC_CTYPE,"C.UTF-8");return snprintf(out,size,"%lc|%.5ls|%10ls",(unsigned int)0x17e,L"ž日z",L"xy");
    case 16:setlocale(LC_CTYPE,"C.UTF-8");errno=0;{int n=snprintf(out,size,"%lc",(unsigned int)0xd800);return n==-1 && errno==EILSEQ?-1:-999;}
    case 17:{signed char hh=0;short h=0;int n=0;long l=0;long long ll=0;size_t z=0;intmax_t j=0;
        int result=snprintf(out,size,"abc%hhn%hn%n%ln%lln%zn%jn",&hh,&h,&n,&l,&ll,&z,&j);
        return hh==3 && h==3 && n==3 && l==3 && ll==3 && z==3 && j==3?result:-999;}
    case 18:return via_va(out,size,"v:%d:%s:%.4Lf",17,"pack",1.25L);
    case 19:return snprintf(0,0,"count:%d",18);
    case 20:return snprintf(out,1,"abc");
    case 21:return snprintf(out,5,"abcdef%d",7);
    case 22:errno=0;{int n=snprintf(out,size,"%Q",1);return n==-1 && errno==EINVAL?-1:-999;}
    case 23:errno=0;{int n=snprintf(out,size,"%2147483648d",1);return n==-1 && errno==EOVERFLOW?-1:-999;}
    case 24:{int (*volatile digit)(int)=isdigit;char *(*volatile message)(int)=strerror;
        long double (*volatile fraction)(long double,int*)=frexpl;int exponent=0;
        if(!digit('3') || digit('x') || strcmp(message(EINVAL),"Invalid argument") || fraction(8.0L,&exponent)!=0.5L || exponent!=4)return -999;
        return snprintf(out,size,"%2$d/%1$d/%2$d",17,-99);}
    default:return -999;
    }
}
int format_file_case(const char *path)
{
    FILE *file=fopen(path,"w+b");if(!file)return 0;
    if(fprintf(file,"f:%d|%.3Lf\n",7,1.25L)!=10)return 0;
    FILE *old=stdout;stdout=file;int n=printf("p:%u\n",8u);stdout=old;
    if(n!=4 || lock_contract_call(file,9)!=2 || fseek(file,0,SEEK_SET))return 0;
    char buffer[32];size_t count=fread(buffer,1,sizeof(buffer)-1,file);buffer[count]=0;
    int correct=count==16 && strcmp(buffer,"f:7|1.250\np:8\nL9")==0;
    return fclose(file)==0 && correct;
}
#ifdef __cplusplus
}
#endif
