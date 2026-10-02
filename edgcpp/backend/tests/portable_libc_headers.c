#include <float.h>
#include <inttypes.h>
#include <wchar.h>
#include <math.h>
#include <stdlib.h>
#include <limits.h>
#ifdef __cplusplus
#define ASSERT static_assert
#else
#define ASSERT _Static_assert
#endif
ASSERT(sizeof(float)==4 && sizeof(double)==8 && sizeof(long double)==16,"storage profile");
ASSERT(FLT_MANT_DIG==24 && DBL_MANT_DIG==53 && LDBL_MANT_DIG==64,"precision profile");
ASSERT(LDBL_MIN_EXP==-16381 && LDBL_MAX_EXP==16384 && LDBL_DECIMAL_DIG==21,"binary80 limits");
ASSERT(sizeof(wchar_t)==4 && sizeof(wint_t)==4 && sizeof(mbstate_t)==8,"wide ABI");
ASSERT(sizeof(imaxdiv_t)==16 && sizeof(intmax_t)==8,"maximum integer ABI");
ASSERT(WEOF==UINT32_MAX && MB_LEN_MAX==4,"wide and UTF-8 constants");
ASSERT(sizeof(signbit((long double)0))==sizeof(int) && sizeof(isfinite((float)0))==sizeof(int),"classification result ABI");
/* Repeated declarations also detect any incompatible public declaration. */
#ifdef __cplusplus
extern "C" {
#endif
long double frexpl(long double, int *);
double scalbn(double, int);
int wctomb(char *, wchar_t);
#ifdef __cplusplus
}
#endif
int main(void) {
 return PRId64[0]!='l' || SCNd8[0]!='h' || SCNd8[1]!='h' ||
  SCNuFAST16[0]!='u' || PRIuPTR[0]!='l' || SCNxMAX[0]!='l' ||
  !(FLT_TRUE_MIN>0 && FLT_TRUE_MIN<FLT_MIN && DBL_TRUE_MIN>0 && DBL_TRUE_MIN<DBL_MIN) ||
  !(LDBL_TRUE_MIN>0 && LDBL_TRUE_MIN<LDBL_MIN && LDBL_MAX>1 && 1+LDBL_EPSILON>1);
}
