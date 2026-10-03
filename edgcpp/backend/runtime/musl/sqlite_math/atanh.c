#include "libm.h"

/* atanh(x) = log((1+x)/(1-x))/2 = log1p(2x/(1-x))/2 ~= x + x^3/3 + o(x^5) */
double __edg_musl_atanh(double x)
{
	union {double f; uint64_t i;} u = {.f = x};
	unsigned e = u.i >> 52 & 0x7ff;
	unsigned s = u.i >> 63;
	double_t y;

	/* |x| */
	u.i &= (uint64_t)-1/2;
	y = u.f;

	if (e < 0x3ff - 1) {
		if (e < 0x3ff - 32) {
			/* handle underflow */
			if (e == 0)
				FORCE_EVAL((float)y);
		} else {
			/* |x| < 0.5, up to 1.7ulp error */
			y = 0.5*log1p(2*y + 2*y*y/(1-y));
		}
	} else {
		/* avoid overflow */
		y = 0.5*log1p(2*(y/(1-y)));
	}
	return s ? -y : y;
}

/* Authored portable MATH_ERRNO boundary; the numerical algorithm is retained above. */
#include <errno.h>
double atanh(double x)
{
 int saved=errno;
 int error=fabs(x)>1 ? EDOM : fabs(x)==1 ? ERANGE : 0;
 double result=__edg_musl_atanh(x);
 if (!error && isfinite(x) && (isinf(result) || (result==0 && x!=0))) error=ERANGE;
 errno=error?error:saved;
 return result;
}
