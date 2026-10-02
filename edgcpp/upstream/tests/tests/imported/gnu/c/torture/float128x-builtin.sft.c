//type: rp
//options: 
# 0 "./torture/float128x-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128x-builtin.c"
# 10 "./torture/float128x-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float128x test_type;
extern __typeof (__builtin_inff128x ()) test_type;
extern __typeof (__builtin_huge_valf128x ()) test_type;
extern __typeof (__builtin_nanf128x ("")) test_type;
extern __typeof (__builtin_nansf128x ("")) test_type;
extern __typeof (__builtin_fabsf128x (0)) test_type;
extern __typeof (__builtin_copysignf128x (0, 0)) test_type;
extern __typeof (__builtin_acoshf128x (0)) test_type;
extern __typeof (__builtin_acosf128x (0)) test_type;
extern __typeof (__builtin_asinhf128x (0)) test_type;
extern __typeof (__builtin_asinf128x (0)) test_type;
extern __typeof (__builtin_atanhf128x (0)) test_type;
extern __typeof (__builtin_atanf128x (0)) test_type;
extern __typeof (__builtin_cbrtf128x (0)) test_type;
extern __typeof (__builtin_coshf128x (0)) test_type;
extern __typeof (__builtin_cosf128x (0)) test_type;
extern __typeof (__builtin_erfcf128x (0)) test_type;
extern __typeof (__builtin_erff128x (0)) test_type;
extern __typeof (__builtin_exp2f128x (0)) test_type;
extern __typeof (__builtin_expf128x (0)) test_type;
extern __typeof (__builtin_expm1f128x (0)) test_type;
extern __typeof (__builtin_lgammaf128x (0)) test_type;
extern __typeof (__builtin_log10f128x (0)) test_type;
extern __typeof (__builtin_log1pf128x (0)) test_type;
extern __typeof (__builtin_log2f128x (0)) test_type;
extern __typeof (__builtin_logbf128x (0)) test_type;
extern __typeof (__builtin_logf128x (0)) test_type;
extern __typeof (__builtin_nextafterf128x (0, 0)) test_type;
extern __typeof (__builtin_sinhf128x (0)) test_type;
extern __typeof (__builtin_sinf128x (0)) test_type;
extern __typeof (__builtin_tanhf128x (0)) test_type;
extern __typeof (__builtin_tanf128x (0)) test_type;
extern __typeof (__builtin_tgammaf128x (0)) test_type;
extern __typeof (__builtin_atan2f128x (0, 0)) test_type;
extern __typeof (__builtin_fdimf128x (0, 0)) test_type;
extern __typeof (__builtin_fmodf128x (0, 0)) test_type;
extern __typeof (__builtin_frexpf128x (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf128x (0, 0)) test_type;
extern __typeof (__builtin_hypotf128x (0, 0)) test_type;
extern __typeof (__builtin_ilogbf128x (0)) test_i;
extern __typeof (__builtin_llrintf128x (0)) test_ll;
extern __typeof (__builtin_llroundf128x (0)) test_ll;
extern __typeof (__builtin_lrintf128x (0)) test_l;
extern __typeof (__builtin_lroundf128x (0)) test_l;
extern __typeof (__builtin_modff128x (0, &test_type)) test_type;
extern __typeof (__builtin_powf128x (0, 0)) test_type;
extern __typeof (__builtin_remainderf128x (0, 0)) test_type;
extern __typeof (__builtin_remquof128x (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf128x (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf128x (0, 0)) test_type;

volatile _Float128x inf_cst = __builtin_inff128x ();
volatile _Float128x huge_val_cst = __builtin_huge_valf128x ();
volatile _Float128x nan_cst = __builtin_nanf128x ("");
volatile _Float128x nans_cst = __builtin_nansf128x ("");
volatile _Float128x neg0 = -0.0f128x, neg1 = -1.0f128x, one = 1.0;
volatile _Float128x t1 = __builtin_acoshf128x (1.0f128x);
volatile _Float128x t2 = __builtin_acosf128x (1.0f128x);
volatile _Float128x t3 = __builtin_asinhf128x (0.0f128x);
volatile _Float128x t4 = __builtin_asinf128x (0.0f128x);
volatile _Float128x t5 = __builtin_atanhf128x (0.0f128x);
volatile _Float128x t6 = __builtin_atanf128x (0.0f128x);
volatile _Float128x t7 = __builtin_cbrtf128x (27.0f128x);
volatile _Float128x t8 = __builtin_coshf128x (0.0f128x);
volatile _Float128x t9 = __builtin_cosf128x (0.0f128x);
volatile _Float128x t10 = __builtin_erfcf128x (0.0f128x);
volatile _Float128x t11 = __builtin_erff128x (0.0f128x);
volatile _Float128x t12 = __builtin_exp2f128x (1.0f128x);
volatile _Float128x t13 = __builtin_expf128x (0.0f128x);
volatile _Float128x t14 = __builtin_expm1f128x (0.0f128x);
volatile _Float128x t15 = __builtin_log10f128x (1.0f128x);
volatile _Float128x t16 = __builtin_log1pf128x (0.0f128x);
volatile _Float128x t17 = __builtin_log2f128x (1.0f128x);
volatile _Float128x t18 = __builtin_logbf128x (1.0f128x);
volatile _Float128x t19 = __builtin_logf128x (1.0f128x);
volatile _Float128x t20 = __builtin_nextafterf128x (0.0f128x, 0.0f128x);
volatile _Float128x t21 = __builtin_sinhf128x (0.0f128x);
volatile _Float128x t22 = __builtin_sinf128x (0.0f128x);
volatile _Float128x t23 = __builtin_tanhf128x (0.0f128x);
volatile _Float128x t24 = __builtin_tanf128x (0.0f128x);
volatile _Float128x t25 = __builtin_atan2f128x (0.0f128x, 1.0f128x);
volatile _Float128x t26 = __builtin_fdimf128x (0.0f128x, 0.0f128x);
volatile _Float128x t27 = __builtin_fmodf128x (0.0f128x, 1.0f128x);
volatile _Float128x t28 = __builtin_ldexpf128x (1.0f128x, 1);
volatile _Float128x t29 = __builtin_hypotf128x (3.0f128x, 4.0f128x);
volatile int t30 = __builtin_ilogbf128x (1.0f128x);
volatile long long int t31 = __builtin_llroundf128x (42.25f128x);
volatile long int t32 = __builtin_lroundf128x (42.25f128x);
volatile _Float128x t33 = __builtin_powf128x (1.0f128x, 2.0f128x);
volatile _Float128x t34 = __builtin_remainderf128x (7.0f128x, 4.0f128x);
volatile _Float128x t35 = __builtin_scalblnf128x (1.0f128x, 1L);
volatile _Float128x t36 = __builtin_scalbnf128x (1.0f128x, 1);

int
main (void)
{
  volatile _Float128x r;
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
  r = __builtin_fabsf128x (neg1);
  if (r != 1.0f128x)
    abort ();
  r = __builtin_copysignf128x (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf128x (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf128x (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f128x)
    abort ();
  if (t2 != 0.0f128x)
    abort ();
  if (t3 != 0.0f128x)
    abort ();
  if (t4 != 0.0f128x)
    abort ();
  if (t5 != 0.0f128x)
    abort ();
  if (t6 != 0.0f128x)
    abort ();
  if (t7 != 3.0f128x)
    abort ();
  if (t8 != 1.0f128x)
    abort ();
  if (t9 != 1.0f128x)
    abort ();
  if (t10 != 1.0f128x)
    abort ();
  if (t11 != 0.0f128x)
    abort ();
  if (t12 != 2.0f128x)
    abort ();
  if (t13 != 1.0f128x)
    abort ();
  if (t14 != 0.0f128x)
    abort ();
  if (t15 != 0.0f128x)
    abort ();
  if (t16 != 0.0f128x)
    abort ();
  if (t17 != 0.0f128x)
    abort ();
  if (t18 != 0.0f128x)
    abort ();
  if (t19 != 0.0f128x)
    abort ();
  if (t20 != 0.0f128x)
    abort ();
  if (t21 != 0.0f128x)
    abort ();
  if (t22 != 0.0f128x)
    abort ();
  if (t23 != 0.0f128x)
    abort ();
  if (t24 != 0.0f128x)
    abort ();
  if (t25 != 0.0f128x)
    abort ();
  if (t26 != 0.0f128x)
    abort ();
  if (t27 != 0.0f128x)
    abort ();
  if (t28 != 2.0f128x)
    abort ();
  if (t29 != 5.0f128x)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f128x)
    abort ();
  if (t34 != -1.0f128x)
    abort ();
  if (t35 != 2.0f128x)
    abort ();
  if (t36 != 2.0f128x)
    abort ();
  exit (0);
}
# 11 "./torture/float128x-builtin.c" 2
