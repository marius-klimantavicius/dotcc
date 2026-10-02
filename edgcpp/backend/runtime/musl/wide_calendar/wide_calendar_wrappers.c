#include "wide_calendar_compat.h"
size_t wcsftime_l(wchar_t *s, size_t n, const wchar_t *f, const struct tm *tm, locale_t locale)
{ if (!__edg_locale_validate(locale)) return 0; return __wcsftime_l(s,n,f,tm,locale); }
