#ifndef _DOTCC_WCTYPE_H
#define _DOTCC_WCTYPE_H

/* POSIX explicit-locale scalar classification and simple case conversion.
   wint_t is dotcc's existing signed 32-bit scalar type from <wchar.h>; it can
   hold all Unicode scalars plus WEOF. These functions operate on scalar values,
   independently of dotcc's UTF-16 wchar_t string representation.

   C/POSIX classify ASCII; C.UTF-8 uses the validated Linux scalar locale host
   service. Unknown/foreign/freed locale handles set EINVAL. This header only
   declares implemented functions: descriptor APIs and implicit-locale wide
   classification are outside the current shared runtime surface. */
#include <wchar.h>
#include <locale.h>

int iswalnum_l(wint_t c, locale_t locale);
int iswalpha_l(wint_t c, locale_t locale);
int iswblank_l(wint_t c, locale_t locale);
int iswcntrl_l(wint_t c, locale_t locale);
int iswdigit_l(wint_t c, locale_t locale);
int iswgraph_l(wint_t c, locale_t locale);
int iswlower_l(wint_t c, locale_t locale);
int iswprint_l(wint_t c, locale_t locale);
int iswpunct_l(wint_t c, locale_t locale);
int iswspace_l(wint_t c, locale_t locale);
int iswupper_l(wint_t c, locale_t locale);
int iswxdigit_l(wint_t c, locale_t locale);
wint_t towlower_l(wint_t c, locale_t locale);
wint_t towupper_l(wint_t c, locale_t locale);

#endif
