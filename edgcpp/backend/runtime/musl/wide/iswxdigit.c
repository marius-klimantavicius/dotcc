#include "wide_compat.h"
#include <wctype.h>

int iswxdigit(wint_t wc)
{
	return (unsigned)(wc-'0') < 10 || (unsigned)((wc|32)-'a') < 6;
}

int iswxdigit_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswxdigit(c);
}

