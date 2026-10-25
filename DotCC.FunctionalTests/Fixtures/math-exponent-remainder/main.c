#include <errno.h>
#include <math.h>
#include <stdio.h>
#include <limits.h>

int main(void) {
  volatile double values[] = {1, -8, 0.75, 0x1p-1074, 0x1.fffffffffffffp1023};
  int exponents[] = {0, 3, -1, -1074, 1023};
  double (*exponent)(double) = logb;
  double (*rem)(double, double) = remainder;
  for (int i = 0; i < 5; ++i) {
    errno = 0;
    if (ilogb(values[i]) != exponents[i] || exponent(values[i]) != exponents[i] || errno) return 1;
  }
  if (rem(7,2) != -1 || rem(5,2) != 1 || rem(-7,2) != 1 || rem(7,-2) != -1 ||
      !signbit(rem(-4,2)) || rem(1,INFINITY) != 1) return 2;
  volatile double zero = 0, infinity = INFINITY;
  errno = 0;
  if (ilogb(zero) != FP_ILOGB0 || errno != EDOM) return 3;
  errno = 0;
  if (ilogb(infinity) != INT_MAX || errno != EDOM) return 4;
  errno = 0;
  if (logb(zero) != -INFINITY || errno) return 5;
  errno = 0;
  if (!isnan(rem(1,zero)) || errno != EDOM) return 6;
  puts("math-exponent-remainder OK");
  return 0;
}
