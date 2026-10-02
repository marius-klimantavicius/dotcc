#include <stdlib.h>
#include "scan_string.h"
#include "intscan.h"
#include "scan_string.h"
#include <inttypes.h>
#include <limits.h>
#include "scan_ctype.h"

static unsigned long long strtox(const char *s, char **p, int base, unsigned long long lim)
{
	struct edg_string_scan f;
	sh_fromstring(&f, s);
	shlim(&f, 0);
	unsigned long long y = __intscan(&f, base, 1, lim);
	if (p) {
		size_t cnt = shcnt(&f);
		*p = (char *)s + cnt;
	}
	return y;
}

unsigned long long strtoull(const char *s, char **p, int base)
{
	return strtox(s, p, base, ULLONG_MAX);
}

long long strtoll(const char *s, char **p, int base)
{
	return strtox(s, p, base, LLONG_MIN);
}

unsigned long strtoul(const char *s, char **p, int base)
{
	return strtox(s, p, base, ULONG_MAX);
}

long strtol(const char *s, char **p, int base)
{
	return strtox(s, p, base, 0UL+LONG_MIN);
}

intmax_t strtoimax(const char *s, char **p, int base)
{
	return strtoll(s, p, base);
}

uintmax_t strtoumax(const char *s, char **p, int base)
{
	return strtoull(s, p, base);
}

