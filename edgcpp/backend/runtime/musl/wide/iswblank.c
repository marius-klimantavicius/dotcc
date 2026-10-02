#include "wide_compat.h"
#include <wctype.h>
#include <ctype.h>

int iswblank(wint_t wc)
{
	return isblank(wc);
}

int iswblank_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswblank(c);
}

