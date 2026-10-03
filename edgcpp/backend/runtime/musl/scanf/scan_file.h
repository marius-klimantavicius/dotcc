#ifndef EDG_SCAN_FILE_H
#define EDG_SCAN_FILE_H
#include <stdio.h>
#include <stdarg.h>
#include "../strto/scan_string.h"
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
typedef int (*__edg_scan_body)(void *,const char *,va_list);
int __edg_with_file_read_lock(FILE *,__edg_scan_body,void *,const char *,va_list);
int __edg_musl_vscan(struct edg_string_scan *,const char *,va_list);
#ifdef __cplusplus
}
#endif
#endif
