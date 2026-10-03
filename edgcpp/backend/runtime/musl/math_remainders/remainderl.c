#include <errno.h>
#include <math.h>
#include <float.h>

#if LDBL_MANT_DIG == 53 && LDBL_MAX_EXP == 1024
long double __edg_musl_remainderl(long double x, long double y)
{
	return remainder(x, y);
}
#else
long double __edg_musl_remainderl(long double x, long double y)
{
	int q;
	return remquol(x, y, &q);
}
#endif

long double remainderl(long double x, long double y)
{
 if (!isnan(x) && !isnan(y) && (isinf(x) || y == 0)) errno = EDOM;
 return __edg_musl_remainderl(x, y);
}
