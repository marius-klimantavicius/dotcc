#include "libm.h"

/* asinh(x) = sign(x)*log(|x|+sqrt(x*x+1)) ~= x - x^3/6 + o(x^5) */
double __edg_musl_asinh(double x)
{
	union {double f; uint64_t i;} u = {.f = x};
	unsigned e = u.i >> 52 & 0x7ff;
	unsigned s = u.i >> 63;

	/* |x| */
	u.i &= (uint64_t)-1/2;
	x = u.f;

	if (e >= 0x3ff + 26) {
		/* |x| >= 0x1p26 or inf or nan */
		x = log(x) + 0.693147180559945309417232121458176568;
	} else if (e >= 0x3ff + 1) {
		/* |x| >= 2 */
		x = log(2*x + 1/(sqrt(x*x+1)+x));
	} else if (e >= 0x3ff - 26) {
		/* |x| >= 0x1p-26, up to 1.6ulp error in [0.125,0.5] */
		x = log1p(x + x*x/(sqrt(x*x+1)+1));
	} else {
		/* |x| < 0x1p-26, raise inexact if x != 0 */
		FORCE_EVAL(x + 0x1p120f);
	}
	return s ? -x : x;
}

/* Authored portable MATH_ERRNO boundary; the numerical algorithm is retained above. */
#include <errno.h>
double asinh(double x)
{
 int saved=errno;
 int error=0;
 double result=__edg_musl_asinh(x);
 if (!error && isfinite(x) && (isinf(result) || (result==0 && x!=0))) error=ERANGE;
 errno=error?error:saved;
 return result;
}
