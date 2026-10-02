/* Private import contract. Validate adjusted builtin va_list tag identity. */
#ifndef __EDG_FORMAT_FILE_H
#define __EDG_FORMAT_FILE_H
#include <stdio.h>
#include <stdarg.h>
#ifdef __cplusplus
extern "C" {
#endif
typedef int (*__edg_format_body)(void *, const char *, va_list);
int __edg_with_file_lock(FILE *, __edg_format_body, void *, const char *, va_list);
#ifdef __cplusplus
}
#endif
#endif
