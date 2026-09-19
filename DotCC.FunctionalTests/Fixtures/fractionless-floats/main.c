#include <stdio.h>

float one(void) { return 1.f; }
double two(void) { return 2.; }
float thirty(void) { return 3.e1f; }
long double extended(void) { return 1.L; }

int main(void) {
  union Bits { float f; unsigned bits; } zero = {-0.f}, exact = {0x1.p0f};
  if (zero.bits != 0x80000000u || exact.bits != 0x3f800000u) return 1;
  if (1.e-1F != .1f || extended() != 1.0L) return 2;
  printf("%.1f\n", one() + two() + thirty() + 9.F);
  return 0;
}
