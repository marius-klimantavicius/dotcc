#include <math.h>
#include <errno.h>
#include <float.h>

#if LDBL_MANT_DIG == 53 && LDBL_MAX_EXP == 1024
long double fdiml(long double x, long double y)
{
	return fdim(x, y);
}
#else
long double fdiml(long double x, long double y)
{
	if (isnan(x))
		return x;
	if (isnan(y))
		return y;
	long double result = x > y ? x - y : 0;
	/* Portable MATH_ERRNO profile: finite subtraction overflow. */
	if (isinf(result) && isfinite(x) && isfinite(y)) errno = ERANGE;
	return result;
}
#endif
