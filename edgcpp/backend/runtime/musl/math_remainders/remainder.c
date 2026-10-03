#include <errno.h>
#include <math.h>

double __edg_musl_remainder(double x, double y)
{
	int q;
	return remquo(x, y, &q);
}

/* Native legacy drem alias is outside this selected source component. */

double remainder(double x, double y)
{
 if (!isnan(x) && !isnan(y) && (isinf(x) || y == 0)) errno = EDOM;
 return __edg_musl_remainder(x, y);
}
