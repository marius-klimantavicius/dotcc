#include "wide_compat.h"
#include <wctype.h>

int iswgraph(wint_t wc)
{
	/* ISO C defines this function as: */
	return !iswspace(wc) && iswprint(wc);
}

int iswgraph_l(wint_t c, locale_t l)
{
	if (!__edg_locale_validate(l)) return 0;
	return iswgraph(c);
}

