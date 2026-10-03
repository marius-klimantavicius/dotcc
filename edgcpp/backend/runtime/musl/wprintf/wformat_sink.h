#ifndef EDG_WFORMAT_SINK_H
#define EDG_WFORMAT_SINK_H
#include <stdio.h>
#include <stdarg.h>
#include <wchar.h>
#include <wctype.h>
#include <stddef.h>
#include "../printf/format_sink.h"
enum { EDG_WFORMAT_NL_ARGMAX = 9 };
struct edg_wformat_sink {int (*write)(void *,const wchar_t *,size_t);void *context;int error;};
static int edg_wformat_error(struct edg_wformat_sink *f) {return f->error;}
static int edg_wformat_write(struct edg_wformat_sink *f,const wchar_t *text,size_t count) {
 if(f->error)return -1;
 if(f->write(f->context,text,count)<0){f->error=1;return -1;}return 0;
}
static void edg_wformat_put(wchar_t c,struct edg_wformat_sink *f) {(void)edg_wformat_write(f,&c,1);}
#ifdef __cplusplus
extern "C" {
#endif
size_t wcsnlen(const wchar_t *,size_t);
int edg_wformat_narrow(struct edg_wformat_sink *,const char *,...);
int __edg_musl_vwformat(struct edg_wformat_sink *,FILE *,const wchar_t *,va_list);
int __edg_with_file_wide_lock(FILE *,int (*)(void *,const wchar_t *,va_list),void *,const wchar_t *,va_list);
#ifdef __cplusplus
}
#endif
#endif
