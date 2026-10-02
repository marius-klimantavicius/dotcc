/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_MATH_H
#define __EDG_DOT_LIBC_MATH_H
#ifdef __cplusplus
extern "C" {
#endif
#define MATH_ERRNO 1
#define math_errhandling MATH_ERRNO
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
