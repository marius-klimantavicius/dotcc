#ifndef EDG_WIDE_NUMERIC_H
#define EDG_WIDE_NUMERIC_H
#include <wchar.h>
#include <wctype.h>
#include <locale.h>
#include <inttypes.h>
#include "../strto/scan_string.h"
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
int __edg_locale_validate(locale_t);
float wcstof_l(const wchar_t *,wchar_t **,locale_t);
double wcstod_l(const wchar_t *,wchar_t **,locale_t);
long double wcstold_l(const wchar_t *,wchar_t **,locale_t);
long wcstol_l(const wchar_t *,wchar_t **,int,locale_t);
unsigned long wcstoul_l(const wchar_t *,wchar_t **,int,locale_t);
long long wcstoll_l(const wchar_t *,wchar_t **,int,locale_t);
unsigned long long wcstoull_l(const wchar_t *,wchar_t **,int,locale_t);

#ifdef __cplusplus
}
#endif
#endif
