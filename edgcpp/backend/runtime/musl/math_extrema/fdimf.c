#include <math.h>
#include <errno.h>

float fdimf(float x, float y)
{
	if (isnan(x))
		return x;
	if (isnan(y))
		return y;
	float result = x > y ? x - y : 0;
	/* Portable MATH_ERRNO profile: finite subtraction overflow. */
	if (isinf(result) && isfinite(x) && isfinite(y)) errno = ERANGE;
	return result;
}
