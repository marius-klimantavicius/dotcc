/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_MATH_H
#define __EDG_DOT_LIBC_MATH_H
#ifdef __cplusplus
extern "C" {
#endif
#define MATH_ERRNO 1
#define math_errhandling MATH_ERRNO
/* Private classification entry points allow single-evaluation type dispatch. */
int __edg_signbitf(float);
int __edg_signbit(double);
int __edg_signbitl(long double);
int __edg_isfinitef(float);
int __edg_isfinite(double);
int __edg_isfinitel(long double);
#define __EDG_FLOAT_SELECT(x, f, d, l) (sizeof(x)==sizeof(float)?f((float)(x)):sizeof(x)==sizeof(double)?d((double)(x)):l((long double)(x)))
#define signbit(x) __EDG_FLOAT_SELECT(x, __edg_signbitf, __edg_signbit, __edg_signbitl)
#define isfinite(x) __EDG_FLOAT_SELECT(x, __edg_isfinitef, __edg_isfinite, __edg_isfinitel)
float frexpf(float, int *);
double frexp(double, int *);
long double frexpl(long double, int *);
float scalbnf(float, int);
double scalbn(double, int);
long double scalbnl(long double, int);
double trunc(double);
float truncf(float);
double floor(double);
float floorf(float);
double ceil(double);
float ceilf(float);
double fabs(double);
float fabsf(float);
double sqrt(double);
float sqrtf(float);
#ifdef __cplusplus
}
#endif
#endif
