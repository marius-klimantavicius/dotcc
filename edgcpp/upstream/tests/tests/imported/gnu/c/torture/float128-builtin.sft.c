//type: rp
//options: 
# 0 "./torture/float128-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128-builtin.c"
# 10 "./torture/float128-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float128 test_type;
extern __typeof (__builtin_inff128 ()) test_type;
extern __typeof (__builtin_huge_valf128 ()) test_type;
extern __typeof (__builtin_nanf128 ("")) test_type;
extern __typeof (__builtin_nansf128 ("")) test_type;
extern __typeof (__builtin_fabsf128 (0)) test_type;
extern __typeof (__builtin_copysignf128 (0, 0)) test_type;
extern __typeof (__builtin_acoshf128 (0)) test_type;
extern __typeof (__builtin_acosf128 (0)) test_type;
extern __typeof (__builtin_asinhf128 (0)) test_type;
extern __typeof (__builtin_asinf128 (0)) test_type;
extern __typeof (__builtin_atanhf128 (0)) test_type;
extern __typeof (__builtin_atanf128 (0)) test_type;
extern __typeof (__builtin_cbrtf128 (0)) test_type;
extern __typeof (__builtin_coshf128 (0)) test_type;
extern __typeof (__builtin_cosf128 (0)) test_type;
extern __typeof (__builtin_erfcf128 (0)) test_type;
extern __typeof (__builtin_erff128 (0)) test_type;
extern __typeof (__builtin_exp2f128 (0)) test_type;
extern __typeof (__builtin_expf128 (0)) test_type;
extern __typeof (__builtin_expm1f128 (0)) test_type;
extern __typeof (__builtin_lgammaf128 (0)) test_type;
extern __typeof (__builtin_log10f128 (0)) test_type;
extern __typeof (__builtin_log1pf128 (0)) test_type;
extern __typeof (__builtin_log2f128 (0)) test_type;
extern __typeof (__builtin_logbf128 (0)) test_type;
extern __typeof (__builtin_logf128 (0)) test_type;
extern __typeof (__builtin_nextafterf128 (0, 0)) test_type;
extern __typeof (__builtin_sinhf128 (0)) test_type;
extern __typeof (__builtin_sinf128 (0)) test_type;
extern __typeof (__builtin_tanhf128 (0)) test_type;
extern __typeof (__builtin_tanf128 (0)) test_type;
extern __typeof (__builtin_tgammaf128 (0)) test_type;
extern __typeof (__builtin_atan2f128 (0, 0)) test_type;
extern __typeof (__builtin_fdimf128 (0, 0)) test_type;
extern __typeof (__builtin_fmodf128 (0, 0)) test_type;
extern __typeof (__builtin_frexpf128 (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf128 (0, 0)) test_type;
extern __typeof (__builtin_hypotf128 (0, 0)) test_type;
extern __typeof (__builtin_ilogbf128 (0)) test_i;
extern __typeof (__builtin_llrintf128 (0)) test_ll;
extern __typeof (__builtin_llroundf128 (0)) test_ll;
extern __typeof (__builtin_lrintf128 (0)) test_l;
extern __typeof (__builtin_lroundf128 (0)) test_l;
extern __typeof (__builtin_modff128 (0, &test_type)) test_type;
extern __typeof (__builtin_powf128 (0, 0)) test_type;
extern __typeof (__builtin_remainderf128 (0, 0)) test_type;
extern __typeof (__builtin_remquof128 (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf128 (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf128 (0, 0)) test_type;

volatile _Float128 inf_cst = __builtin_inff128 ();
volatile _Float128 huge_val_cst = __builtin_huge_valf128 ();
volatile _Float128 nan_cst = __builtin_nanf128 ("");
volatile _Float128 nans_cst = __builtin_nansf128 ("");
volatile _Float128 neg0 = -0.0f128, neg1 = -1.0f128, one = 1.0;
volatile _Float128 t1 = __builtin_acoshf128 (1.0f128);
volatile _Float128 t2 = __builtin_acosf128 (1.0f128);
volatile _Float128 t3 = __builtin_asinhf128 (0.0f128);
volatile _Float128 t4 = __builtin_asinf128 (0.0f128);
volatile _Float128 t5 = __builtin_atanhf128 (0.0f128);
volatile _Float128 t6 = __builtin_atanf128 (0.0f128);
volatile _Float128 t7 = __builtin_cbrtf128 (27.0f128);
volatile _Float128 t8 = __builtin_coshf128 (0.0f128);
volatile _Float128 t9 = __builtin_cosf128 (0.0f128);
volatile _Float128 t10 = __builtin_erfcf128 (0.0f128);
volatile _Float128 t11 = __builtin_erff128 (0.0f128);
volatile _Float128 t12 = __builtin_exp2f128 (1.0f128);
volatile _Float128 t13 = __builtin_expf128 (0.0f128);
volatile _Float128 t14 = __builtin_expm1f128 (0.0f128);
volatile _Float128 t15 = __builtin_log10f128 (1.0f128);
volatile _Float128 t16 = __builtin_log1pf128 (0.0f128);
volatile _Float128 t17 = __builtin_log2f128 (1.0f128);
volatile _Float128 t18 = __builtin_logbf128 (1.0f128);
volatile _Float128 t19 = __builtin_logf128 (1.0f128);
volatile _Float128 t20 = __builtin_nextafterf128 (0.0f128, 0.0f128);
volatile _Float128 t21 = __builtin_sinhf128 (0.0f128);
volatile _Float128 t22 = __builtin_sinf128 (0.0f128);
volatile _Float128 t23 = __builtin_tanhf128 (0.0f128);
volatile _Float128 t24 = __builtin_tanf128 (0.0f128);
volatile _Float128 t25 = __builtin_atan2f128 (0.0f128, 1.0f128);
volatile _Float128 t26 = __builtin_fdimf128 (0.0f128, 0.0f128);
volatile _Float128 t27 = __builtin_fmodf128 (0.0f128, 1.0f128);
volatile _Float128 t28 = __builtin_ldexpf128 (1.0f128, 1);
volatile _Float128 t29 = __builtin_hypotf128 (3.0f128, 4.0f128);
volatile int t30 = __builtin_ilogbf128 (1.0f128);
volatile long long int t31 = __builtin_llroundf128 (42.25f128);
volatile long int t32 = __builtin_lroundf128 (42.25f128);
volatile _Float128 t33 = __builtin_powf128 (1.0f128, 2.0f128);
volatile _Float128 t34 = __builtin_remainderf128 (7.0f128, 4.0f128);
volatile _Float128 t35 = __builtin_scalblnf128 (1.0f128, 1L);
volatile _Float128 t36 = __builtin_scalbnf128 (1.0f128, 1);

int
main (void)
{
  volatile _Float128 r;
  if (!__builtin_isinf (inf_cst))
    abort ();
  if (!__builtin_isinf (huge_val_cst))
    abort ();
  if (inf_cst != huge_val_cst)
    abort ();
  if (!__builtin_isnan (nan_cst))
    abort ();
  if (!__builtin_isnan (nans_cst))
    abort ();
  r = __builtin_fabsf128 (neg1);
  if (r != 1.0f128)
    abort ();
  r = __builtin_copysignf128 (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf128 (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf128 (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f128)
    abort ();
  if (t2 != 0.0f128)
    abort ();
  if (t3 != 0.0f128)
    abort ();
  if (t4 != 0.0f128)
    abort ();
  if (t5 != 0.0f128)
    abort ();
  if (t6 != 0.0f128)
    abort ();
  if (t7 != 3.0f128)
    abort ();
  if (t8 != 1.0f128)
    abort ();
  if (t9 != 1.0f128)
    abort ();
  if (t10 != 1.0f128)
    abort ();
  if (t11 != 0.0f128)
    abort ();
  if (t12 != 2.0f128)
    abort ();
  if (t13 != 1.0f128)
    abort ();
  if (t14 != 0.0f128)
    abort ();
  if (t15 != 0.0f128)
    abort ();
  if (t16 != 0.0f128)
    abort ();
  if (t17 != 0.0f128)
    abort ();
  if (t18 != 0.0f128)
    abort ();
  if (t19 != 0.0f128)
    abort ();
  if (t20 != 0.0f128)
    abort ();
  if (t21 != 0.0f128)
    abort ();
  if (t22 != 0.0f128)
    abort ();
  if (t23 != 0.0f128)
    abort ();
  if (t24 != 0.0f128)
    abort ();
  if (t25 != 0.0f128)
    abort ();
  if (t26 != 0.0f128)
    abort ();
  if (t27 != 0.0f128)
    abort ();
  if (t28 != 2.0f128)
    abort ();
  if (t29 != 5.0f128)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f128)
    abort ();
  if (t34 != -1.0f128)
    abort ();
  if (t35 != 2.0f128)
    abort ();
  if (t36 != 2.0f128)
    abort ();
  exit (0);
}
# 11 "./torture/float128-builtin.c" 2
