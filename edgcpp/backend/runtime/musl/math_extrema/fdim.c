#include <math.h>
#include <errno.h>

double fdim(double x, double y)
{
	if (isnan(x))
		return x;
	if (isnan(y))
		return y;
	double result = x > y ? x - y : 0;
	/* Portable MATH_ERRNO profile: finite subtraction overflow. */
	if (isinf(result) && isfinite(x) && isfinite(y)) errno = ERANGE;
	return result;
}
