#include "wide_compat.h"
#include <wctype.h>

#undef iswdigit

int iswdigit(wint_t wc)
{
	return (unsigned)wc-'0' < 10;
}

int iswdigit_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswdigit(c);
}

