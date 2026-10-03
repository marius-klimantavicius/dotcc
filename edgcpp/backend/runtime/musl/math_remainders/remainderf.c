#include <errno.h>
#include <math.h>

float __edg_musl_remainderf(float x, float y)
{
	int q;
	return remquof(x, y, &q);
}

/* Native legacy drem alias is outside this selected source component. */

float remainderf(float x, float y)
{
 if (!isnan(x) && !isnan(y) && (isinf(x) || y == 0)) errno = EDOM;
 return __edg_musl_remainderf(x, y);
}
