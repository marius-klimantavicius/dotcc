#include <errno.h>
#include <float.h>
#include <math.h>
#include <stdint.h>

float __edg_musl_scalbnf(float x, int n)
{
	union {float f; uint32_t i;} u;
	float_t y = x;

	if (n > 127) {
		y *= 0x1p127f;
		n -= 127;
		if (n > 127) {
			y *= 0x1p127f;
			n -= 127;
			if (n > 127)
				n = 127;
		}
	} else if (n < -126) {
		y *= 0x1p-126f * 0x1p24f;
		n += 126 - 24;
		if (n < -126) {
			y *= 0x1p-126f * 0x1p24f;
			n += 126 - 24;
			if (n < -126)
				n = -126;
		}
	}
	u.i = (uint32_t)(0x7f+n)<<23;
	x = y * u.f;
	return x;
}

float scalbnf(float x, int n)
{
 float result=__edg_musl_scalbnf(x,n);
 if (isfinite(x) && x != 0) {
  if (isinf(result) || result == 0) errno=ERANGE;
  /* Nonzero tiny results are range errors only when scaling lost bits.
     Here n cannot be INT_MIN: every finite nonzero input then rounds to 0. */
  else if (n < 0 && fabsf(result) < FLT_MIN && __edg_musl_scalbnf(result,-n) != x) errno=ERANGE;
 }
 return result;
}
