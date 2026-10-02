#include "wide_numeric.h"
#include "../strto/floatscan.h"
#include <errno.h>
#include <float.h>
static long double wcstox(const wchar_t *s,wchar_t **p,int precision)
{
 const wchar_t *t=s;while(iswspace(*t))++t;
 struct edg_string_scan f;sh_fromwide(&f,t);shlim(&f,0);
 long double value=__floatscan(&f,precision,1);
 if(p){size_t count=shcnt(&f);*p=(wchar_t *)(count?t+count:s);}
 return value;
}

float wcstof(const wchar_t *s, wchar_t **p)
{
	long double value = wcstox(s, p, 0);
 float result = (float)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > FLT_MAX || result < -FLT_MAX)) ||
     (result != 0 && result < FLT_MIN && result > -FLT_MIN)) errno = ERANGE;
 return result;
}

double wcstod(const wchar_t *s, wchar_t **p)
{
	long double value = wcstox(s, p, 1);
 double result = (double)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > DBL_MAX || result < -DBL_MAX)) ||
     (result != 0 && result < DBL_MIN && result > -DBL_MIN)) errno = ERANGE;
 return result;
}

long double wcstold(const wchar_t *s, wchar_t **p)
{
	long double value = wcstox(s, p, 2);
 long double result = (long double)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > LDBL_MAX || result < -LDBL_MAX)) ||
     (result != 0 && result < LDBL_MIN && result > -LDBL_MIN)) errno = ERANGE;
 return result;
}
