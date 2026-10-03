#include "string_case_compat.h"
#include <strings.h>
#include <ctype.h>

int strcasecmp(const char *_l, const char *_r)
{
	const unsigned char *l=(const unsigned char *)_l, *r=(const unsigned char *)_r;
	for (; *l && *r && (*l == *r || tolower(*l) == tolower(*r)); l++, r++);
	return tolower(*l) - tolower(*r);
}

int strcasecmp_l(const char *l, const char *r, locale_t loc)
{
	if (!__edg_locale_validate(loc)) return 0;
	return strcasecmp(l, r);
}

