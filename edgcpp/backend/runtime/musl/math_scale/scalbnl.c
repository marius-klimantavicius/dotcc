#include <errno.h>
#include <float.h>
#include "libm.h"

#if LDBL_MANT_DIG == 53 && LDBL_MAX_EXP == 1024
long double __edg_musl_scalbnl(long double x, int n)
{
	return scalbn(x, n);
}
#elif (LDBL_MANT_DIG == 64 || LDBL_MANT_DIG == 113) && LDBL_MAX_EXP == 16384
long double __edg_musl_scalbnl(long double x, int n)
{
	union ldshape u;

	if (n > 16383) {
		x *= 0x1p16383L;
		n -= 16383;
		if (n > 16383) {
			x *= 0x1p16383L;
			n -= 16383;
			if (n > 16383)
				n = 16383;
		}
	} else if (n < -16382) {
		x *= 0x1p-16382L * 0x1p113L;
		n += 16382 - 113;
		if (n < -16382) {
			x *= 0x1p-16382L * 0x1p113L;
			n += 16382 - 113;
			if (n < -16382)
				n = -16382;
		}
	}
	u.f = 1.0;
	u.i.se = 0x3fff + n;
	return x * u.f;
}
#endif

long double scalbnl(long double x, int n)
{
 long double result=__edg_musl_scalbnl(x,n);
 if (isfinite(x) && x != 0) {
  if (isinf(result) || result == 0) errno=ERANGE;
  /* Nonzero tiny results are range errors only when scaling lost bits.
     Here n cannot be INT_MIN: every finite nonzero input then rounds to 0. */
  else if (n < 0 && fabsl(result) < LDBL_MIN && __edg_musl_scalbnl(result,-n) != x) errno=ERANGE;
 }
 return result;
}
