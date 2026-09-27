#include <math.h>
#include <stdio.h>
#include <stdint.h>
static int near(double actual, double expected) {
    return fabs(actual - expected) <= fabs(expected) * 5e-16;
}
typedef double (*Unary)(double);
int main(void) {
    Unary functions[] = { expm1, log1p };
    if (functions[0](1e-20) != 1e-20 || functions[1](1e-20) != 1e-20) return 1;
    if (functions[0](-1e-20) != -1e-20 || functions[1](-1e-20) != -1e-20) return 2;
    if (1.0/functions[0](-0.0) != -1.0/0.0 || 1.0/functions[1](-0.0) != -1.0/0.0) return 3;
    if (!near(expm1(0.4), 0.49182469764127035) || !near(expm1(-0.4), -0.32967995396436073)) return 4;
    if (!near(log1p(0.4), 0.33647223662121295) || !near(log1p(-0.4), -0.5108256237659907)) return 5;
    if (!isinf(log1p(-1.0)) || !isnan(log1p(-2.0))) return 6;
    if (!near(hypot(3e200, 4e200), 5e200) || !near(hypot(3e-200,4e-200), 5e-200)) return 7;
    if (hypot(3,4) != 5 || expm1(-1.0/0.0) != -1) return 8;
    if (expm1f(1e-10f) != 1e-10f || log1pf(1e-10f) != 1e-10f || hypotf(3,4) != 5) return 9;
    if (lrint(2.5) != 2 || lrint(3.5) != 4 || lrint(-2.5) != -2 || lrintf(-3.5f) != -4) return 10;
    if (lrint(5000000000.0) != 5000000000L || sizeof(lrint(0)) != sizeof(long)) return 11;
    union { uint64_t bits; double value; } nan;
    nan.bits = 0xfff8000000000001ULL;
    if (!signbit(-0.0) || signbit(0.0) || !signbit(nan.value)) return 12;
    nan.bits = 0x7ff8000000000001ULL;
    if (signbit(nan.value)) return 13;
    printf("accurate math and function pointers passed\n");
    return 0;
}
