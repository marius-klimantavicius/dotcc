#include "wide_compat.h"
#include <wctype.h>

int iswalnum(wint_t wc)
{
	return iswdigit(wc) || iswalpha(wc);
}

int iswalnum_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswalnum(c);
}

