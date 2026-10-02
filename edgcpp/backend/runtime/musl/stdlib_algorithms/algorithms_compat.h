/* Portable GNU/POSIX qsort_r ABI: comparator(a,b,context), context last.
   This is a translated API, never the host OS qsort_r ABI. */
#ifndef EDG_ALGORITHMS_COMPAT_H
#define EDG_ALGORITHMS_COMPAT_H
#include <stddef.h>
#ifdef __cplusplus
extern "C" {
#endif
void __qsort_r(void *,size_t,size_t,int (*)(const void *,const void *,void *),void *);
void qsort_r(void *,size_t,size_t,int (*)(const void *,const void *,void *),void *);
#ifdef __cplusplus
}
#endif
#endif
