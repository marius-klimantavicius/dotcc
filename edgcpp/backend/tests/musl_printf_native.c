/* Synthetic native contract test for the selected musl core and buffer sink.
   This is not generated-C# or whole-libc acceptance. */
#include "format_sink.h"
#include <stdio.h>
#include <string.h>
#include <stdint.h>
#include <inttypes.h>
#include <stdlib.h>
#include <errno.h>
#include <math.h>
#include <float.h>
#include <fenv.h>
#include <locale.h>
#include <wchar.h>
/* Native bridge for the explicitly private selected-core scaling dependency. */
double __edg_scalbn_nearest(double value, int power) { return scalbn(value,power); }
static int checks;
static void compare(size_t capacity,const char *format,...) {
    char actual[2048],expected[2048];memset(actual,0xa5,sizeof actual);memset(expected,0xa5,sizeof expected);
    va_list first,second;va_start(first,format);va_copy(second,first);
    int before=errno;
    int a=__edg_musl_vsnprintf(capacity?actual:0,capacity,format,first);errno=before;
    int b=vsnprintf(capacity?expected:0,capacity,format,second);
    va_end(first);va_end(second);
    size_t bytes=a<0?0:(size_t)a;
    if(capacity && bytes>=capacity)bytes=capacity-1;
    if(a!=b || (capacity && memcmp(actual,expected,bytes+1))) {
        fprintf(stderr,"mismatch capacity=%zu fmt=%s actual=%d:[%s] expected=%d:[%s]\n",capacity,format,a,capacity?actual:"",b,capacity?expected:"");exit(1);
    }
    ++checks;
}
static int format(char *dst,size_t size,const char *fmt,...) {
    va_list ap;va_start(ap,fmt);
    int n=__edg_musl_vsnprintf(dst,size,fmt,ap);va_end(ap);return n;
}
static int fail_writer(void *state,const char *text,size_t n){(void)state;(void)text;(void)n;errno=EIO;return -1;}
static int fail_format(const char *fmt,...){struct edg_format_sink sink={fail_writer,0,0};va_list ap;va_start(ap,fmt);int n=__edg_musl_vformat(&sink,fmt,ap);va_end(ap);return n;}
int main(void) {
    for(size_t size=0;size<=128;size=size?size*2:1) {
        compare(size,"literal%%:%s:%c:end","a\xffz",0);
        compare(size,"%+08d/%-8.3u/%#08x/%#.0o",-42,12u,0xabc,0u);
        compare(size,"%hhd/%hhu/%hd/%hu",-128,255,-32768,65535);
        compare(size,"%ld/%lu/%lld/%llu/%jd/%zu/%td",LONG_MIN,ULONG_MAX,LLONG_MIN,ULLONG_MAX,(intmax_t)INTMAX_MIN,(size_t)-1,(ptrdiff_t)-17);
        compare(size,"%*.*f/%#g",-15,7,0.1,1000.0);
        compare(size,"%3$*1$.*2$f|%3$g",12,4,1.25);
        compare(size,"%2$d/%1$d/%2$d",17,-99);
    }
    double values[]={0.0,-0.0,0x1p-1074,0x1p-1022,DBL_MAX,0.1,1.25,999.999,INFINITY,-INFINITY,NAN};
    for(unsigned i=0;i<sizeof values/sizeof*values;++i) {
        compare(2048,"%+.17g|%.17e|%.10f",values[i],values[i],values[i]);
        if(values[i]==0 || fabs(values[i])>=DBL_MIN)compare(2048,"%a|%.7A",values[i],values[i]);
        char hex[256];format(hex,sizeof hex,"%a",values[i]);
        double parsed=strtod(hex,0);if(!(isnan(parsed)&&isnan(values[i])) && memcmp(&parsed,&values[i],sizeof parsed))return 6;
        ++checks;
    }
    long double extended[]={0.L,-0.L,0x1p-16445L,0x1p-16382L,0x1.fffffffffffffffep+16383L,0.1L,1+0x1p-63L};
    for(unsigned i=0;i<sizeof extended/sizeof*extended;++i)compare(2048,"%.21Lg|%.24Le",extended[i],extended[i]);
    for(int i=0;i<4;++i) {
        int modes[]={FE_TONEAREST,FE_UPWARD,FE_DOWNWARD,FE_TOWARDZERO};fesetround(modes[i]);
        compare(2048,"%.0f/%.0f/%.1Lf/%.2g",2.5,-2.5,1.25L,9.95);
    }
    fesetround(FE_TONEAREST);
    char output[256]; signed char hh=0;short h=0;int n=0;long l=0;long long ll=0;size_t z=0;intmax_t j=0;
    int count=format(output,sizeof output,"abc%hhn%hn%n%ln%lln%zn%jn",&hh,&h,&n,&l,&ll,&z,&j);
    if(count!=3 || hh!=3 || h!=3 || n!=3 || l!=3 || ll!=3 || z!=3 || j!=3)return 2;++checks;
    errno=0;if(format(output,sizeof output,"%2147483648d",1)!=-1 || errno!=EOVERFLOW)return 3;++checks;
    if(fail_format("literal%d",3)!=-1 || errno!=EIO)return 4;++checks;
    if(!setlocale(LC_CTYPE,"C.UTF-8"))return 5;
    compare(2048,"%lc|%.5ls|%10ls",(wint_t)0x17e,L"ž日z",L"xy");
    printf("%d synthetic sink/glibc cases passed\n",checks);
    return 0;
}
