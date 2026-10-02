#include "wide_compat.h"
#include <wchar.h>
#include <locale.h>

/* collate only by code points */
static size_t transform(wchar_t *restrict dest, const wchar_t *restrict src, size_t n)
{
	size_t l = wcslen(src);
	if (l < n) {
		wmemcpy(dest, src, l+1);
	} else if (n) {
		wmemcpy(dest, src, n-1);
		dest[n-1] = 0;
	}
	return l;
}

size_t wcsxfrm(wchar_t *restrict dest, const wchar_t *restrict src, size_t n)
{
	return transform(dest, src, n);
}


size_t wcsxfrm_l(wchar_t *restrict dest, const wchar_t *restrict src, size_t n, locale_t loc)
{
	if (!__edg_locale_validate(loc)) return 0;
	return transform(dest, src, n);
}
