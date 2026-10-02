#include "wide_compat.h"
#include <wctype.h>

int iswlower(wint_t wc)
{
	return towupper(wc) != wc;
}

int iswlower_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswlower(c);
}

