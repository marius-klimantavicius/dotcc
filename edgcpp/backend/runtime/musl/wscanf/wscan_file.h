#ifndef EDG_WSCAN_FILE_H
#define EDG_WSCAN_FILE_H
#include <stdio.h>
#include <stdarg.h>
#include <wchar.h>
#include <stdint.h>
#include "../strto/scan_string.h"
struct edg_wscan_input {int (*read)(void *);int (*unread)(int,void *);void *context;};
static inline int edg_wget(struct edg_wscan_input *f){return f->read(f->context);}
static inline int edg_wunget(int value,struct edg_wscan_input *f){return value<0?value:f->unread(value,f->context);}
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
typedef int (*__edg_wscan_body)(void *,const wchar_t *,va_list);
int __edg_with_file_wide_read_lock(FILE *,__edg_wscan_body,void *,const wchar_t *,va_list);
int __edg_musl_vwscan(struct edg_wscan_input *,const wchar_t *,va_list);
#ifdef __cplusplus
}
#endif
#endif
