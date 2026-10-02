/* Portable wide declaration surface, musl 1.2.6 ABI. Calls require bindings. */
#ifndef __EDG_DOT_LIBC_WCTYPE_H
#define __EDG_DOT_LIBC_WCTYPE_H
#include <locale.h>
#ifndef __EDG_DOT_LIBC_WINT_T
#define __EDG_DOT_LIBC_WINT_T
typedef unsigned int wint_t;
#endif
typedef unsigned long wctype_t;
typedef const int *wctrans_t;
#ifndef WEOF
#define WEOF 0xffffffffU
#endif
#ifdef __cplusplus
extern "C" {
#endif
int iswalnum(wint_t);
int iswalnum_l(wint_t, locale_t);
int iswalpha(wint_t);
int iswalpha_l(wint_t, locale_t);
int iswblank(wint_t);
int iswblank_l(wint_t, locale_t);
int iswcntrl(wint_t);
int iswcntrl_l(wint_t, locale_t);
int iswdigit(wint_t);
int iswdigit_l(wint_t, locale_t);
int iswgraph(wint_t);
int iswgraph_l(wint_t, locale_t);
int iswlower(wint_t);
int iswlower_l(wint_t, locale_t);
int iswprint(wint_t);
int iswprint_l(wint_t, locale_t);
int iswpunct(wint_t);
int iswpunct_l(wint_t, locale_t);
int iswspace(wint_t);
int iswspace_l(wint_t, locale_t);
int iswupper(wint_t);
int iswupper_l(wint_t, locale_t);
int iswxdigit(wint_t);
int iswxdigit_l(wint_t, locale_t);
int iswctype(wint_t, wctype_t);
int iswctype_l(wint_t, wctype_t, locale_t);
wint_t towlower(wint_t);
wint_t towupper(wint_t);
wint_t towctrans(wint_t, wctrans_t);
wctype_t wctype(const char *);
wctrans_t wctrans(const char *);
wint_t towlower_l(wint_t, locale_t);
wint_t towupper_l(wint_t, locale_t);
wint_t towctrans_l(wint_t, wctrans_t, locale_t);
wctype_t wctype_l(const char *, locale_t);
wctrans_t wctrans_l(const char *, locale_t);
#ifdef __cplusplus
}
#endif
#endif
