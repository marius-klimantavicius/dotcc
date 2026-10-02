#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <locale.h>
#include <wchar.h>
#include <errno.h>
static char fail_buffer[128];
static int fail_mode,fail_allocations,fail_frees;
static void *edg_fail_realloc(void *old,size_t size) {
    ++fail_allocations;
    if(fail_mode || old || size>sizeof fail_buffer){errno=ENOMEM;return 0;}
    return fail_buffer;
}
static void *edg_fail_malloc(size_t size) {(void)size;errno=ENOMEM;return 0;}
static void edg_fail_free(void *p) {if(p==fail_buffer)++fail_frees;}
#define asprintf edg_failing_asprintf
#define vasprintf edg_failing_vasprintf
#define realloc edg_fail_realloc
#define malloc edg_fail_malloc
#define free edg_fail_free
#include "format_allocating.c"
#undef asprintf
#undef vasprintf
#undef realloc
#undef malloc
#undef free
static int via_list(char **a,char **b,const char *format,...)
{
    va_list arguments,copy;va_start(arguments,format);va_copy(copy,arguments);
    int first=vasprintf(a,format,arguments),second=vasprintf(b,format,copy);
    va_end(copy);va_end(arguments);return first==second?first:-99;
}
#define CHECK(x,n) do {if(!(x))return n;}while(0)
#ifdef __cplusplus
extern "C"
#endif
int allocating_printf_cases(void)
{
    char *out=0,*second=0;int result;
    result=asprintf(&out,"");CHECK(result==0 && out && *out==0,1);free(out);
    result=asprintf(&out,"%03000d/%s",7,"done");CHECK(result==3005 && strlen(out)==3005,2);
    for(int i=0;i<2999;++i)CHECK(out[i]=='0',3);CHECK(!strcmp(out+2999,"7/done"),4);free(out);
    int source;memcpy(&source,"abc",4);
    result=asprintf(&out,"%s%n/%d",(char*)&source,&source,42);
    CHECK(result==6 && source==3 && !strcmp(out,"abc/42"),5);free(out);
    memcpy(&source,"abc",4);result=asprintf(&out,"%1$s%2$n",(char*)&source,&source);
    CHECK(result==3 && source==3 && !strcmp(out,"abc"),6);free(out);
    result=asprintf(&out,"%c:%d",0,9);CHECK(result==3 && out[0]==0 && out[1]==':' && out[2]=='9' && out[3]==0,7);free(out);
    CHECK(setlocale(LC_ALL,"C.UTF-8")!=0,8);wchar_t wide[2]={0x20ac,0};
    result=asprintf(&out,"%ls",wide);CHECK(result==3 && !memcmp(out,"\xe2\x82\xac",4),9);free(out);
    result=via_list(&out,&second,"%d/%g/%s",7,1.25,"ok");CHECK(result==9 && !strcmp(out,"7/1.25/ok") && !strcmp(out,second),10);free(out);free(second);
    out=(char*)1;errno=71;CHECK(asprintf(&out,"%Q",1)==-1 && !out && errno==EINVAL,11);
    out=(char*)1;errno=71;CHECK(asprintf(&out,"%2147483648d",1)==-1 && !out && errno==EOVERFLOW,12);
    fail_mode=0;fail_allocations=fail_frees=0;out=(char*)1;
    CHECK(edg_failing_asprintf(&out,"prefix:%0400d",7)==-1 && !out && errno==ENOMEM && fail_allocations>=2 && fail_frees==1,13);
    fail_mode=1;out=(char*)1;CHECK(edg_failing_asprintf(&out,"")==-1 && !out && errno==ENOMEM,14);
    return 0;
}
