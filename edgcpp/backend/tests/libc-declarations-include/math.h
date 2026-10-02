/* Complete C17 math declarations, portable binary80 profile.
   Signature list audited against musl 1.2.6 include/math.h. Unbound calls reject. */
#ifndef __EDG_DOT_LIBC_MATH_H
#define __EDG_DOT_LIBC_MATH_H
#ifdef __cplusplus
extern "C" {
#endif
typedef float float_t;
typedef double double_t;
#define NAN __builtin_nanf("")
#define INFINITY __builtin_inff()
#define HUGE_VALF __builtin_inff()
#define HUGE_VAL __builtin_inf()
#define HUGE_VALL __builtin_infl()
#define FP_ILOGB0 (-2147483647-1)
#define FP_ILOGBNAN (-2147483647-1)
#define FP_NAN 0
#define FP_INFINITE 1
#define FP_ZERO 2
#define FP_SUBNORMAL 3
#define FP_NORMAL 4
#define MATH_ERREXCEPT 2
#define MATH_ERRNO 1
#define math_errhandling MATH_ERRNO
/* Private classification entry points allow single-evaluation type dispatch. */
int __edg_fpclassifyf(float);
int __edg_fpclassify(double);
int __edg_fpclassifyl(long double);
int __edg_signbitf(float);
int __edg_signbit(double);
int __edg_signbitl(long double);
int __edg_isfinitef(float);
int __edg_isfinite(double);
int __edg_isfinitel(long double);
#define __EDG_FLOAT_SELECT(x, f, d, l) (sizeof(x)==sizeof(float)?f((float)(x)):sizeof(x)==sizeof(double)?d((double)(x)):l((long double)(x)))
#define fpclassify(x) __EDG_FLOAT_SELECT(x, __edg_fpclassifyf, __edg_fpclassify, __edg_fpclassifyl)
#define isinf(x) (fpclassify(x)==FP_INFINITE)
#define isnan(x) (fpclassify(x)==FP_NAN)
#define isnormal(x) (fpclassify(x)==FP_NORMAL)
#define signbit(x) __EDG_FLOAT_SELECT(x, __edg_signbitf, __edg_signbit, __edg_signbitl)
#define isfinite(x) __EDG_FLOAT_SELECT(x, __edg_isfinitef, __edg_isfinite, __edg_isfinitel)
/* Quiet predicate exception flags remain part of the pending fenv work. */
static __inline int __edg_islessf(float x, float y) { return x<y; }
static __inline int __edg_isless(double x, double y) { return x<y; }
static __inline int __edg_islessl(long double x, long double y) { return x<y; }
#define isless(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_islessf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_isless((x),(y)):__edg_islessl((x),(y)))
static __inline int __edg_islessequalf(float x, float y) { return x<=y; }
static __inline int __edg_islessequal(double x, double y) { return x<=y; }
static __inline int __edg_islessequall(long double x, long double y) { return x<=y; }
#define islessequal(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_islessequalf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_islessequal((x),(y)):__edg_islessequall((x),(y)))
static __inline int __edg_isgreaterf(float x, float y) { return x>y; }
static __inline int __edg_isgreater(double x, double y) { return x>y; }
static __inline int __edg_isgreaterl(long double x, long double y) { return x>y; }
#define isgreater(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_isgreaterf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_isgreater((x),(y)):__edg_isgreaterl((x),(y)))
static __inline int __edg_isgreaterequalf(float x, float y) { return x>=y; }
static __inline int __edg_isgreaterequal(double x, double y) { return x>=y; }
static __inline int __edg_isgreaterequall(long double x, long double y) { return x>=y; }
#define isgreaterequal(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_isgreaterequalf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_isgreaterequal((x),(y)):__edg_isgreaterequall((x),(y)))
static __inline int __edg_islessgreaterf(float x, float y) { return x<y || x>y; }
static __inline int __edg_islessgreater(double x, double y) { return x<y || x>y; }
static __inline int __edg_islessgreaterl(long double x, long double y) { return x<y || x>y; }
#define islessgreater(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_islessgreaterf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_islessgreater((x),(y)):__edg_islessgreaterl((x),(y)))
static __inline int __edg_isunorderedf(float x, float y) { return x!=x || y!=y; }
static __inline int __edg_isunordered(double x, double y) { return x!=x || y!=y; }
static __inline int __edg_isunorderedl(long double x, long double y) { return x!=x || y!=y; }
#define isunordered(x,y) (sizeof((x)+(y))==sizeof(float)?__edg_isunorderedf((x),(y)):sizeof((x)+(y))==sizeof(double)?__edg_isunordered((x),(y)):__edg_isunorderedl((x),(y)))

double      acos(double);
float       acosf(float);
long double acosl(long double);

double      acosh(double);
float       acoshf(float);
long double acoshl(long double);

double      asin(double);
float       asinf(float);
long double asinl(long double);

double      asinh(double);
float       asinhf(float);
long double asinhl(long double);

double      atan(double);
float       atanf(float);
long double atanl(long double);

double      atan2(double, double);
float       atan2f(float, float);
long double atan2l(long double, long double);

double      atanh(double);
float       atanhf(float);
long double atanhl(long double);

double      cbrt(double);
float       cbrtf(float);
long double cbrtl(long double);

double      ceil(double);
float       ceilf(float);
long double ceill(long double);

double      copysign(double, double);
float       copysignf(float, float);
long double copysignl(long double, long double);

double      cos(double);
float       cosf(float);
long double cosl(long double);

double      cosh(double);
float       coshf(float);
long double coshl(long double);

double      erf(double);
float       erff(float);
long double erfl(long double);

double      erfc(double);
float       erfcf(float);
long double erfcl(long double);

double      exp(double);
float       expf(float);
long double expl(long double);

double      exp2(double);
float       exp2f(float);
long double exp2l(long double);

double      expm1(double);
float       expm1f(float);
long double expm1l(long double);

double      fabs(double);
float       fabsf(float);
long double fabsl(long double);

double      fdim(double, double);
float       fdimf(float, float);
long double fdiml(long double, long double);

double      floor(double);
float       floorf(float);
long double floorl(long double);

double      fma(double, double, double);
float       fmaf(float, float, float);
long double fmal(long double, long double, long double);

double      fmax(double, double);
float       fmaxf(float, float);
long double fmaxl(long double, long double);

double      fmin(double, double);
float       fminf(float, float);
long double fminl(long double, long double);

double      fmod(double, double);
float       fmodf(float, float);
long double fmodl(long double, long double);

double      frexp(double, int *);
float       frexpf(float, int *);
long double frexpl(long double, int *);

double      hypot(double, double);
float       hypotf(float, float);
long double hypotl(long double, long double);

int         ilogb(double);
int         ilogbf(float);
int         ilogbl(long double);

double      ldexp(double, int);
float       ldexpf(float, int);
long double ldexpl(long double, int);

double      lgamma(double);
float       lgammaf(float);
long double lgammal(long double);

long long   llrint(double);
long long   llrintf(float);
long long   llrintl(long double);

long long   llround(double);
long long   llroundf(float);
long long   llroundl(long double);

double      log(double);
float       logf(float);
long double logl(long double);

double      log10(double);
float       log10f(float);
long double log10l(long double);

double      log1p(double);
float       log1pf(float);
long double log1pl(long double);

double      log2(double);
float       log2f(float);
long double log2l(long double);

double      logb(double);
float       logbf(float);
long double logbl(long double);

long        lrint(double);
long        lrintf(float);
long        lrintl(long double);

long        lround(double);
long        lroundf(float);
long        lroundl(long double);

double      modf(double, double *);
float       modff(float, float *);
long double modfl(long double, long double *);

double      nan(const char *);
float       nanf(const char *);
long double nanl(const char *);

double      nearbyint(double);
float       nearbyintf(float);
long double nearbyintl(long double);

double      nextafter(double, double);
float       nextafterf(float, float);
long double nextafterl(long double, long double);

double      nexttoward(double, long double);
float       nexttowardf(float, long double);
long double nexttowardl(long double, long double);

double      pow(double, double);
float       powf(float, float);
long double powl(long double, long double);

double      remainder(double, double);
float       remainderf(float, float);
long double remainderl(long double, long double);

double      remquo(double, double, int *);
float       remquof(float, float, int *);
long double remquol(long double, long double, int *);

double      rint(double);
float       rintf(float);
long double rintl(long double);

double      round(double);
float       roundf(float);
long double roundl(long double);

double      scalbln(double, long);
float       scalblnf(float, long);
long double scalblnl(long double, long);

double      scalbn(double, int);
float       scalbnf(float, int);
long double scalbnl(long double, int);

double      sin(double);
float       sinf(float);
long double sinl(long double);

double      sinh(double);
float       sinhf(float);
long double sinhl(long double);

double      sqrt(double);
float       sqrtf(float);
long double sqrtl(long double);

double      tan(double);
float       tanf(float);
long double tanl(long double);

double      tanh(double);
float       tanhf(float);
long double tanhl(long double);

double      tgamma(double);
float       tgammaf(float);
long double tgammal(long double);

double      trunc(double);
float       truncf(float);
long double truncl(long double);
#ifdef __cplusplus
}
#endif
#endif
