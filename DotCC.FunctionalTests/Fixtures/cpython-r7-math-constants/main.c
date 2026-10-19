/* Standard C IEEE constant types, promotions and storage. long double has its
   selected ABI width; no false native16/target8 layout equality is required. */
#include <math.h>
#include <stdio.h>
#define CHECK(x) do { if (!(x)) { printf("IEEE constant failure line=%d\n", __LINE__); return 1; } checks++; } while (0)
static double values[] = { HUGE_VAL, -HUGE_VAL, NAN };
static float small[] = { HUGE_VALF, -INFINITY, NAN };
static long double wide[] = { HUGE_VALL, -HUGE_VALL };
static double signed_inf(int negative) { return negative ? -HUGE_VAL : HUGE_VAL; }
static float signed_float(int negative) { return negative ? -HUGE_VALF : HUGE_VALF; }
static long double signed_wide(int negative) { return negative ? -HUGE_VALL : HUGE_VALL; }
static double signed_nan(int negative) { return negative ? -NAN : NAN; }
int main(void) {
    int checks = 0;
    CHECK(sizeof(HUGE_VAL) == sizeof(double));
    CHECK(sizeof(HUGE_VALF) == sizeof(float));
    CHECK(sizeof(HUGE_VALL) == sizeof(long double));
    CHECK(sizeof(INFINITY) == sizeof(float));
    CHECK(sizeof(NAN) == sizeof(float));
    CHECK(isinf(signed_inf(0)) && !signbit(signed_inf(0)));
    CHECK(isinf(signed_inf(1)) && signbit(signed_inf(1)));
    CHECK(isinf(signed_float(0)) && !signbit(signed_float(0)));
    CHECK(isinf(signed_float(1)) && signbit(signed_float(1)));
    CHECK(isinf((double)signed_wide(0)) && !signbit((double)signed_wide(0)));
    CHECK(isinf((double)signed_wide(1)) && signbit((double)signed_wide(1)));
    CHECK(isnan(signed_nan(0)) && isnan(signed_nan(1)));
    CHECK(isinf(values[0]) && !signbit(values[0]) && isinf(values[1]) && signbit(values[1]) && isnan(values[2]));
    CHECK(isinf(small[0]) && !signbit(small[0]) && isinf(small[1]) && signbit(small[1]) && isnan(small[2]));
    CHECK(isinf((double)wide[0]) && !signbit((double)wide[0]) && isinf((double)wide[1]) && signbit((double)wide[1]));
    CHECK(isnan(NAN + 1.0) && NAN != NAN && !(NAN == NAN));
    printf("ieee-constant-types passed checks=%d double=%d float=%d nan_width=%d\n", checks, (int)sizeof(HUGE_VAL), (int)sizeof(INFINITY), (int)sizeof(NAN));
    return 0;
}
