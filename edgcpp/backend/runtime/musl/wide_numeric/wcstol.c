#include "wide_numeric.h"
#include "../strto/intscan.h"
#include <inttypes.h>
#include <limits.h>
static unsigned long long wcstox(const wchar_t *s,wchar_t **p,int base,unsigned long long lim)
{
 const wchar_t *t=s;while(iswspace(*t))++t;
 struct edg_string_scan f;sh_fromwide(&f,t);shlim(&f,0);
 unsigned long long value=__intscan(&f,base,1,lim);
 if(p){size_t count=shcnt(&f);*p=(wchar_t *)(count?t+count:s);}
 return value;
}

unsigned long long wcstoull(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstox(s, p, base, ULLONG_MAX);
}

long long wcstoll(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstox(s, p, base, LLONG_MIN);
}

unsigned long wcstoul(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstox(s, p, base, ULONG_MAX);
}

long wcstol(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstox(s, p, base, 0UL+LONG_MIN);
}

intmax_t wcstoimax(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstoll(s, p, base);
}

uintmax_t wcstoumax(const wchar_t *restrict s, wchar_t **restrict p, int base)
{
	return wcstoull(s, p, base);
}
