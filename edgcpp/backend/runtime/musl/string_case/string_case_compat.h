#ifndef EDG_STRING_CASE_COMPAT_H
#define EDG_STRING_CASE_COMPAT_H
#include <strings.h>
#include <locale.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_locale_validate(locale_t);
#ifdef __cplusplus
}
#endif
#endif
