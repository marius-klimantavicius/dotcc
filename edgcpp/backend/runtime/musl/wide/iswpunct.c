#include "wide_compat.h"
#include <wctype.h>

static const unsigned char table[] = {
#include "punct.h"
};

int iswpunct(wint_t wc)
{
	if (wc<0x20000U)
		return (table[table[wc>>8]*32+((wc&255)>>3)]>>(wc&7))&1;
	return 0;
}

int iswpunct_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswpunct(c);
}

