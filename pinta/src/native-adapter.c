#include "pinta_tests.h"
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

size_t pinta_test_wcslen(const wchar_t *text) { size_t n=0; while(text[n]) n++; return n; }
int pinta_test_wcscmp(const wchar_t *a,const wchar_t *b) { while(*a && *a==*b) { a++; b++; } return (*a>*b)-(*a<*b); }
static size_t utf8(char *out, size_t cap, const wchar *s, size_t length) {
    size_t pos=0;
    for(size_t i=0;i<length;i++) {
        uint32_t c=s[i]; unsigned n; char bytes[4];
        if(c>=0xd800 && c<=0xdbff && i+1<length && s[i+1]>=0xdc00 && s[i+1]<=0xdfff) { c=0x10000+((c-0xd800)<<10)+(s[++i]-0xdc00); }
        else if(c>=0xd800 && c<=0xdfff) c=0xfffd;
        if(c<0x80) { bytes[0]=(char)c;n=1; }
        else if(c<0x800) {bytes[0]=0xc0|(c>>6);bytes[1]=0x80|(c&63);n=2;}
        else if(c<0x10000) {bytes[0]=0xe0|(c>>12);bytes[1]=0x80|((c>>6)&63);bytes[2]=0x80|(c&63);n=3;}
        else {bytes[0]=0xf0|(c>>18);bytes[1]=0x80|((c>>12)&63);bytes[2]=0x80|((c>>6)&63);bytes[3]=0x80|(c&63);n=4;}
        if(pos+n>=cap) break;
        memcpy(out+pos,bytes,n);pos+=n;
    }
    if(cap)out[pos]=0;
    return pos;
}
int pinta_test_wcstombs_s(size_t *count,char *out,size_t cap,const wchar_t *s,size_t maximum) {
    size_t n=utf8(out,cap<maximum?cap:maximum,(const wchar*)s,pinta_test_wcslen(s)); if(count)*count=n+1;return 0;
}
char *pinta_tests_message(const wchar *format, ...) {
    static char message[2048];size_t pos=0;va_list ap;va_start(ap,format);
    while(*format && pos+1<sizeof(message)) {
        if(format[0]=='%' && format[1]=='l' && format[2]=='s') {
            const wchar *s=va_arg(ap,const wchar*);pos+=utf8(message+pos,sizeof(message)-pos,s,pinta_test_wcslen(s));format+=3;
        } else if(format[0]=='%' && format[1]=='d') {
            int n=snprintf(message+pos,sizeof(message)-pos,"%d",va_arg(ap,int));
            if(n>0)pos+=(size_t)n<sizeof(message)-pos?(size_t)n:sizeof(message)-pos-1;format+=2;
        } else {message[pos++]=(char)*format++;}
    }
    message[pos]=0;va_end(ap);return message;
}
void *pinta_platform_file_open(void *context,void *name_data,uint32_t name_length) {
    const wchar *name=name_data;char path[4096], leaf[1024];size_t first=0;const char *root=getenv("PINTA_FIXTURES");(void)context;
    if(!root || !name || name_length>=1024)return NULL;
    /* Upstream tests use Windows relative paths; only fixtures inside the explicit root are accessible. */
    for(size_t i=0;i<name_length;i++)if(name[i]=='/' || name[i]=='\\')first=i+1;
    utf8(leaf,sizeof(leaf),name+first,name_length-first);
    if(!*leaf || strstr(leaf,".."))return NULL;
    if(snprintf(path,sizeof(path),"%s/%s",root,leaf)>=(int)sizeof(path))return NULL;
    FILE *file=fopen(path,"rb");
    fprintf(stderr,"fixture-open %s %s\n",leaf,file?"ok":"missing");return file;
}
uint32_t pinta_platform_file_size(void *context,void *handle) {
    FILE *file=handle;long size;(void)context;
    if(fseek(file,0,SEEK_END) || (size=ftell(file))<0 || (unsigned long)size>UINT32_MAX || fseek(file,0,SEEK_SET))return 0;
    return (uint32_t)size;
}
uint32_t pinta_platform_file_read(void *context,void *handle,void *buffer,uint32_t length) { (void)context;return (uint32_t)fread(buffer,1,length,handle); }
void pinta_platform_file_close(void *context,void *handle) { (void)context;fclose(handle); }

wint_t pinta_test_putwchar(wchar_t value) { char bytes[8];wchar text[1]={value};size_t n=utf8(bytes,sizeof(bytes),text,1);fwrite(bytes,1,n,stdout);return (wint_t)value; }

/* The only upstream swprintf call copies the literal some_data in gc_tests.c.
 * Reject format directives instead of handing UTF-16 storage to host wchar libc. */
int pinta_test_swprintf(wchar_t *output, size_t capacity, const wchar_t *format, ...) {
    size_t length=pinta_test_wcslen(format);
    if(!output || !capacity || length>=capacity)return -1;
    for(size_t i=0;i<length;i++)if(format[i]=='%')return -1;
    for(size_t i=0;i<=length;i++)output[i]=format[i];
    return (int)length;
}
