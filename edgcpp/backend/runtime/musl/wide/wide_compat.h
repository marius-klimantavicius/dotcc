/* Portable glue; all algorithms and tables retain their pinned musl source. */
#ifndef EDG_WIDE_COMPAT_H
#define EDG_WIDE_COMPAT_H
#include <locale.h>
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
int __edg_locale_validate(locale_t);
#ifdef __cplusplus
}
#endif
#endif
