/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_STDLIB_H
#define __EDG_DOT_LIBC_STDLIB_H
#ifdef __cplusplus
extern "C" {
#endif
#include <stddef.h>
#define EXIT_SUCCESS 0
#define EXIT_FAILURE 1
void *malloc(size_t);
void *calloc(size_t, size_t);
void *realloc(void *, size_t);
void free(void *);
void *aligned_alloc(size_t, size_t);
int posix_memalign(void **, size_t, size_t);
int wctomb(char *, wchar_t);
#ifdef __cplusplus
}
#endif
#endif
