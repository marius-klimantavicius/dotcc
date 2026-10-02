#include "wide_compat.h"
#include <wctype.h>

int iswupper(wint_t wc)
{
	return towlower(wc) != wc;
}

int iswupper_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswupper(c);
}

