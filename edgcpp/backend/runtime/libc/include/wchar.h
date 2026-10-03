/* Portable wide-character declarations; only registered/selected APIs bind. */
#ifndef __EDG_DOT_LIBC_WCHAR_H
#define __EDG_DOT_LIBC_WCHAR_H
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <time.h>
#include <stdarg.h>
#include <locale.h>
#ifndef __EDG_DOT_LIBC_WINT_T
#define __EDG_DOT_LIBC_WINT_T
typedef unsigned int wint_t;
#endif
/* Stateful UTF-8 conversion storage; provider ABI is independent of host mbstate_t. */
typedef struct { unsigned int __state, __value; } mbstate_t;
#define WEOF 0xffffffffU
#ifdef __cplusplus
extern "C" {
#endif
double wcstod(const wchar_t *, wchar_t **);
float wcstof(const wchar_t *, wchar_t **);
long double wcstold(const wchar_t *, wchar_t **);
long wcstol(const wchar_t *, wchar_t **, int);
unsigned long wcstoul(const wchar_t *, wchar_t **, int);
long long wcstoll(const wchar_t *, wchar_t **, int);
unsigned long long wcstoull(const wchar_t *, wchar_t **, int);
float wcstof_l(const wchar_t *, wchar_t **, locale_t);
double wcstod_l(const wchar_t *, wchar_t **, locale_t);
long double wcstold_l(const wchar_t *, wchar_t **, locale_t);
long wcstol_l(const wchar_t *, wchar_t **, int, locale_t);
unsigned long wcstoul_l(const wchar_t *, wchar_t **, int, locale_t);
long long wcstoll_l(const wchar_t *, wchar_t **, int, locale_t);
unsigned long long wcstoull_l(const wchar_t *, wchar_t **, int, locale_t);
int wcscoll(const wchar_t *, const wchar_t *);
int wcscoll_l(const wchar_t *, const wchar_t *, locale_t);
size_t wcsxfrm(wchar_t *, const wchar_t *, size_t);
size_t wcsxfrm_l(wchar_t *, const wchar_t *, size_t, locale_t);
size_t wcsftime(wchar_t *, size_t, const wchar_t *, const struct tm *);
size_t wcsftime_l(wchar_t *, size_t, const wchar_t *, const struct tm *, locale_t);
int fwide(FILE *, int);
int fwprintf(FILE *, const wchar_t *, ...);
int wprintf(const wchar_t *, ...);
int swprintf(wchar_t *, size_t, const wchar_t *, ...);
int vfwprintf(FILE *, const wchar_t *, va_list);
int vwprintf(const wchar_t *, va_list);
int vswprintf(wchar_t *, size_t, const wchar_t *, va_list);
int fwscanf(FILE *, const wchar_t *, ...);
int wscanf(const wchar_t *, ...);
int swscanf(const wchar_t *, const wchar_t *, ...);
int vfwscanf(FILE *, const wchar_t *, va_list);
int vwscanf(const wchar_t *, va_list);
int vswscanf(const wchar_t *, const wchar_t *, va_list);
wint_t fgetwc(FILE *);
wint_t getwc(FILE *);
wint_t getwchar(void);
wint_t fputwc(wchar_t, FILE *);
wint_t putwc(wchar_t, FILE *);
wint_t putwchar(wchar_t);
wchar_t *fgetws(wchar_t *, int, FILE *);
int fputws(const wchar_t *, FILE *);
wint_t ungetwc(wint_t, FILE *);
wint_t btowc(int);
int wctob(wint_t);
int mbsinit(const mbstate_t *);
size_t mbrtowc(wchar_t *, const char *, size_t, mbstate_t *);
size_t mbrlen(const char *, size_t, mbstate_t *);
size_t wcrtomb(char *, wchar_t, mbstate_t *);
size_t mbsnrtowcs(wchar_t *, const char **, size_t, size_t, mbstate_t *);
size_t wcsnrtombs(char *, const wchar_t **, size_t, size_t, mbstate_t *);
size_t mbsrtowcs(wchar_t *, const char **, size_t, mbstate_t *);
size_t wcsrtombs(char *, const wchar_t **, size_t, mbstate_t *);
size_t wcslen(const wchar_t *);
size_t wcsnlen(const wchar_t *, size_t);
wchar_t *wcscpy(wchar_t *, const wchar_t *);
wchar_t *wcsncpy(wchar_t *, const wchar_t *, size_t);
wchar_t *wcscat(wchar_t *, const wchar_t *);
wchar_t *wcsncat(wchar_t *, const wchar_t *, size_t);
int wcscmp(const wchar_t *, const wchar_t *);
int wcsncmp(const wchar_t *, const wchar_t *, size_t);
wchar_t *wcschr(const wchar_t *, wchar_t);
wchar_t *wcsrchr(const wchar_t *, wchar_t);
size_t wcscspn(const wchar_t *, const wchar_t *);
size_t wcsspn(const wchar_t *, const wchar_t *);
wchar_t *wcspbrk(const wchar_t *, const wchar_t *);
wchar_t *wcsstr(const wchar_t *, const wchar_t *);
wchar_t *wcstok(wchar_t *, const wchar_t *, wchar_t **);
wchar_t *wmemchr(const wchar_t *, wchar_t, size_t);
int wmemcmp(const wchar_t *, const wchar_t *, size_t);
wchar_t *wmemcpy(wchar_t *, const wchar_t *, size_t);
wchar_t *wmemmove(wchar_t *, const wchar_t *, size_t);
wchar_t *wmemset(wchar_t *, wchar_t, size_t);
#ifdef __cplusplus
}
#endif
#endif
