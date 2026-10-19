/* Actual CPython pystrtod conditional reduction; native is the expectation. */
#include <stdio.h>
#include <math.h>
static double infinity(int negative) { return negative ? -HUGE_VAL : HUGE_VAL; }
static double nan_value(int negative) { return negative ? -NAN : NAN; }
int main(void) {
    double positive = infinity(0), negative = infinity(1);
    printf("ieee positive=%d negative=%d negative_sign=%d nan=%d huge_width=%d infinity_width=%d\n",
        isinf(positive) != 0, isinf(negative) != 0, signbit(negative) != 0,
        isnan(nan_value(0)) != 0 && isnan(nan_value(1)) != 0,
        (int)sizeof(HUGE_VAL), (int)sizeof(INFINITY));
    return !(isinf(positive) && isinf(negative) && signbit(negative)
        && isnan(nan_value(0)) && isnan(nan_value(1))
        && sizeof(HUGE_VAL) == sizeof(double) && sizeof(INFINITY) == sizeof(float));
}
