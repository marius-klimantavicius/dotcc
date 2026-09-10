#include <math.h>
#include <stdio.h>
/* SQLite stores these math designators in its builtin function registry. */
static double (*functions[])(double) = {acosh, asinh, atanh};
static float (*float_functions[])(float) = {acoshf, asinhf, atanhf};
int main(void) {
    int ordinary = fabs(functions[0](cosh(2.0))-2.0)<1e-12
        && fabs(functions[1](sinh(-2.0))+2.0)<1e-12
        && fabs(functions[2](tanh(0.5))-0.5)<1e-12;
    int single = fabsf(float_functions[0](coshf(2.0f))-2.0f)<1e-5f
        && fabsf(float_functions[1](sinhf(-2.0f))+2.0f)<1e-5f
        && fabsf(float_functions[2](tanhf(0.5f))-0.5f)<1e-5f;
    int domain = isnan(acosh(0.5)) && isnan(atanh(2.0))
        && isnan(acoshf(0.5f)) && isnan(atanhf(2.0f));
    int poles = isinf(atanh(1.0)) && atanh(-1.0)<0.0
        && isinf(atanhf(1.0f)) && atanhf(-1.0f)<0.0f;
    int zeros = asinh(0.0)==0.0 && atanh(0.0)==0.0 && acosh(1.0)==0.0
        && 1.0/asinh(-0.0)<0.0 && 1.0/atanh(-0.0)<0.0;
    printf("inverse hyperbolic: %d %d %d %d %d\n", ordinary, single, domain, poles, zeros);
    return !(ordinary && single && domain && poles && zeros);
}
