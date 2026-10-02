#include <stdlib.h>
#include <errno.h>
#include <float.h>
#include "scan_string.h"
#include "floatscan.h"
#include "scan_string.h"

static long double strtox(const char *s, char **p, int prec)
{
	struct edg_string_scan f;
	sh_fromstring(&f, s);
	shlim(&f, 0);
	long double y = __floatscan(&f, prec, 1);
	long cnt = shcnt(&f);
	if (p) *p = cnt ? (char *)s + cnt : (char *)s;
	return y;
}

float strtof(const char *s, char **p)
{
	long double value = strtox(s, p, 0);
 float result = (float)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > FLT_MAX || result < -FLT_MAX)) ||
     (result != 0 && result < FLT_MIN && result > -FLT_MIN)) errno = ERANGE;
 return result;
}

double strtod(const char *s, char **p)
{
	long double value = strtox(s, p, 1);
 double result = (double)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > DBL_MAX || result < -DBL_MAX)) ||
     (result != 0 && result < DBL_MIN && result > -DBL_MIN)) errno = ERANGE;
 return result;
}

long double strtold(const char *s, char **p)
{
	long double value = strtox(s, p, 2);
 long double result = (long double)value;
 /* A finite scan result can overflow when narrowed to its public width.
    The chosen underflow policy reports every nonzero subnormal, even exact. */
 if ((value <= LDBL_MAX && value >= -LDBL_MAX &&
      (result > LDBL_MAX || result < -LDBL_MAX)) ||
     (result != 0 && result < LDBL_MIN && result > -LDBL_MIN)) errno = ERANGE;
 return result;
}
