#include "mb_compat.h"
#include <uchar.h>
#include <wchar.h>

size_t mbrtoc32(char32_t *restrict pc32, const char *restrict s, size_t n, mbstate_t *restrict ps)
{
	static EDG_MB_TLS unsigned internal_state;
	if (!ps) ps = (mbstate_t *)&internal_state;
	if (!s) return mbrtoc32(0, "", 1, ps);
	wchar_t wc;
	size_t ret = mbrtowc(&wc, s, n, ps);
	if (ret <= 4 && pc32) *pc32 = wc;
	return ret;
}
