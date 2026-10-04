/* Authored declarations for the two binary128 conversion services used by
   EDG. No libquadmath implementation or native header is incorporated. */
#ifndef EDG_SELF_QUADMATH_H
#define EDG_SELF_QUADMATH_H
#include <stddef.h>
#include <math.h>
#ifdef __cplusplus
/* libc++18's C math wrapper removes the C isnan macro, but its floating-point
   trait does not include EDG's binary128 extension. Preserve exact native
   classification rather than narrowing the value to a standard float type. */
inline bool isnan(__float128 value) noexcept { return __builtin_isnan(value); }
extern "C" {
#endif
__float128 strtoflt128(const char *, char **);
int quadmath_snprintf(char *, size_t, const char *, ...);
#ifdef __cplusplus
}
#endif
#endif
