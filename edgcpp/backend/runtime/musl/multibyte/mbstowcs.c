#include "mb_compat.h"
#include <stdlib.h>
#include <wchar.h>

size_t mbstowcs(wchar_t *restrict ws, const char *restrict s, size_t wn)
{
	const char *source = s;
	return mbsrtowcs(ws, &source, wn, 0);
}
