#include "wide_compat.h"
#include <wchar.h>
#include <locale.h>

/* FIXME: stub */
int wcscoll_l(const wchar_t *l, const wchar_t *r, locale_t locale)
{
	if (!__edg_locale_validate(locale)) return 0;
	return wcscmp(l, r);
}

int wcscoll(const wchar_t *l, const wchar_t *r)
{
	return wcscmp(l, r);
}

