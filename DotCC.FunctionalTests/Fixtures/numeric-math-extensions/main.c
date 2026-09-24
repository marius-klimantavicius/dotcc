#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>

static int calls;
static float small(void) { calls++; return 0x1p-149f; }
int main(void) {
    if (llrint(2.5) != 2 || llrint(-1.5) != -2 || llroundl(2.5L) != 3 || llroundl(-2.5L) != -3) return 1;
    if (ceill(-1.5L) != -1.0L) return 2;
    double whole, fraction = modf(-3.75, &whole);
    if (whole != -3.0 || fraction != -0.75) return 3;
    fraction = modf(-INFINITY, &whole);
    if (!isinf(whole) || fraction != 0.0 || 1.0 / fraction != -INFINITY) return 4;
    if (fpclassify(small()) != FP_SUBNORMAL || calls != 1) return 5;
    if (fpclassify((double)0x1p-149f) != FP_NORMAL || fpclassify(0x1p-1074) != FP_SUBNORMAL) return 6;
    if (fpclassify(-0.0) != FP_ZERO || fpclassify(INFINITY) != FP_INFINITE || fpclassify(NAN) != FP_NAN) return 7;
    char *end;
    errno = 0;
    long double number = strtold("  -0x1.8p+2rest", &end);
    if (number != -6.0L || strcmp(end, "rest") || errno != 0) return 8;
    errno = 0;
    number = strtold("1e99999", &end);
    if (!isinf(number) || *end || errno != ERANGE) return 9;
    errno = 0;
    number = strtold("1e-99999", &end);
    if (number != 0.0L || *end || errno != ERANGE) return 10;
    errno = 0;
    number = strtold("0e-99999", &end);
    if (number != 0.0L || *end || errno != 0) return 11;
    puts("rounding split classification parsing ok");
    return 0;
}
