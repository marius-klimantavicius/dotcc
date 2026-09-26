#include "pinta_tests.h"
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Runners embed the pinned fixture directory so direct executable invocations
 * do not depend on a Visual Studio working directory or an environment setup. */
#ifndef PINTA_TEST_FIXTURE_ROOT
#define PINTA_TEST_FIXTURE_ROOT ""
#endif

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
static const char *pinta_test_exception_name(PintaException exception) {
    switch(exception) {
#define PINTA_ERROR_NAME(name) case name: return #name;
        PINTA_ERROR_NAME(PINTA_OK)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_STACK_OVERFLOW)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_STACK_UNDERFLOW)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_TYPE_MISMATCH)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_OUT_OF_MEMORY)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_NULL_REFERENCE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_BAD_FORMAT)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_OUT_OF_RANGE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INVALID_OPERATION)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INVALID_OPCODE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_NOT_IMPLEMENTED)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_ENGINE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INDEX_OUT_OF_BOUNDS)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_DIVISION_BY_ZERO)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INVALID_MODULE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_FILE_NOT_FOUND)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INVALID_ARGUMENTS)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_PLATFORM)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_NOT_REACHABLE)
        PINTA_ERROR_NAME(PINTA_EXCEPTION_INVALID_SEQUENCE)
#undef PINTA_ERROR_NAME
        default: return "PINTA_EXCEPTION_UNKNOWN";
    }
}
char *pinta_tests_exception_message(PintaException expected, PintaException actual) {
    static char message[256];
    snprintf(message,sizeof(message),"Expected result - %s (%d / 0x%08X), actual result - %s (%d / 0x%08X)",
        pinta_test_exception_name(expected),(int)expected,(unsigned)expected,
        pinta_test_exception_name(actual),(int)actual,(unsigned)actual);
    return message;
}
static int pinta_test_trace_enabled(void) {
    const char *value=getenv("PINTA_TRACE");
    return value && strcmp(value,"1")==0;
}
void pinta_test_trace_step(void *value, unsigned code) {
    PintaThread *thread=value;
    if(!pinta_test_trace_enabled())return;
    fprintf(stderr,"pinta-trace step offset=%d opcode=0x%02X stack=%d\n",
        (int)(thread->code_pointer-thread->frame->code_start),code,
        (int)(thread->frame->stack-thread->frame->stack_start)+1);
    for(PintaReference *slot=thread->frame->stack_start;slot<=thread->frame->stack;slot++) {
        PintaHeapObject *object=slot->reference;
        fprintf(stderr,"pinta-trace stack-slot=%d kind=%u\n",(int)(slot-thread->frame->stack_start),
            object?(unsigned)object->block_kind:255);
    }
}
void pinta_test_trace_gc(void *parent, void *child, unsigned index, unsigned field) {
    PintaHeapObject *object=parent,*item=child;
    if(!pinta_test_trace_enabled())return;
    fprintf(stderr,"pinta-trace gc-edge parent-kind=%u child-kind=%u next-index=%u next-field=%u\n",
        object?(unsigned)object->block_kind:255,item?(unsigned)item->block_kind:255,index,field);
}
void pinta_test_trace_scratch(void *start, void *end, unsigned count) {
    if(!pinta_test_trace_enabled())return;
    size_t bytes=(size_t)((uintptr_t)end-(uintptr_t)start);
    fprintf(stderr,"pinta-trace gc-scratch bytes=%zu entry-size=%zu capacity=%u byte-capacity=%zu\n",
        bytes,sizeof(PintaHeapReloc),count,bytes/sizeof(PintaHeapReloc));
}
void pinta_test_trace_type(void *value) {
    PintaHeapObject *object=value;
    if(!pinta_test_trace_enabled() || !object || object->block_kind<PINTA_KIND_LENGTH)return;
    fprintf(stderr,"pinta-trace invalid-object-kind=%u (0x%02X), valid kinds < %u\n",
        (unsigned)object->block_kind,(unsigned)object->block_kind,(unsigned)PINTA_KIND_LENGTH);
}
void pinta_test_trace_load(int exception, void *domain) {
    if(!pinta_test_trace_enabled())return;
    fprintf(stderr,"pinta-trace module-load result=%s (%d / 0x%08X) domain=%s\n",
        pinta_test_exception_name((PintaException)exception),exception,(unsigned)exception,
        domain?"present":"missing");
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
    if(!root || !*root)root=PINTA_TEST_FIXTURE_ROOT;
    if(!*root) { fprintf(stderr,"fixture-root missing: set PINTA_FIXTURES\n");return NULL; }
    if(!name || name_length>=1024)return NULL;
    /* Upstream tests use Windows relative paths; only fixtures inside the explicit root are accessible. */
    for(size_t i=0;i<name_length;i++) {
        if(!name[i])return NULL;
        if(name[i]=='/' || name[i]=='\\')first=i+1;
    }
    utf8(leaf,sizeof(leaf),name+first,name_length-first);
    if(!*leaf || strstr(leaf,".."))return NULL;
    if(snprintf(path,sizeof(path),"%s/%s",root,leaf)>=(int)sizeof(path))return NULL;
    FILE *file=fopen(path,"rb");
    if(pinta_test_trace_enabled())fprintf(stderr,"pinta-trace fixture-path=%s\n",path);
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
