#include "wide_compat.h"
#include <wctype.h>
#include <string.h>

wctrans_t wctrans(const char *property_name)
{
	if (!strcmp(property_name, "toupper")) return (wctrans_t)1;
	if (!strcmp(property_name, "tolower")) return (wctrans_t)2;
	return 0;
}

wint_t towctrans(wint_t wc, wctrans_t trans)
{
	if (trans == (wctrans_t)1) return towupper(wc);
	if (trans == (wctrans_t)2) return towlower(wc);
	return wc;
}

wctrans_t wctrans_l(const char *s, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return wctrans(s);
}

wint_t towctrans_l(wint_t c, wctrans_t t, locale_t l)
{
	if (!__edg_locale_validate(l)) return c;
	return towctrans(c, t);
}

