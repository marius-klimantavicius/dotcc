//type: rp
//options: 
# 0 "./torture/float16-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float16-builtin.c"
# 10 "./torture/float16-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float16 test_type;
extern __typeof (__builtin_inff16 ()) test_type;
extern __typeof (__builtin_huge_valf16 ()) test_type;
extern __typeof (__builtin_nanf16 ("")) test_type;
extern __typeof (__builtin_nansf16 ("")) test_type;
extern __typeof (__builtin_fabsf16 (0)) test_type;
extern __typeof (__builtin_copysignf16 (0, 0)) test_type;
extern __typeof (__builtin_acoshf16 (0)) test_type;
extern __typeof (__builtin_acosf16 (0)) test_type;
extern __typeof (__builtin_asinhf16 (0)) test_type;
extern __typeof (__builtin_asinf16 (0)) test_type;
extern __typeof (__builtin_atanhf16 (0)) test_type;
extern __typeof (__builtin_atanf16 (0)) test_type;
extern __typeof (__builtin_cbrtf16 (0)) test_type;
extern __typeof (__builtin_coshf16 (0)) test_type;
extern __typeof (__builtin_cosf16 (0)) test_type;
extern __typeof (__builtin_erfcf16 (0)) test_type;
extern __typeof (__builtin_erff16 (0)) test_type;
extern __typeof (__builtin_exp2f16 (0)) test_type;
extern __typeof (__builtin_expf16 (0)) test_type;
extern __typeof (__builtin_expm1f16 (0)) test_type;
extern __typeof (__builtin_lgammaf16 (0)) test_type;
extern __typeof (__builtin_log10f16 (0)) test_type;
extern __typeof (__builtin_log1pf16 (0)) test_type;
extern __typeof (__builtin_log2f16 (0)) test_type;
extern __typeof (__builtin_logbf16 (0)) test_type;
extern __typeof (__builtin_logf16 (0)) test_type;
extern __typeof (__builtin_nextafterf16 (0, 0)) test_type;
extern __typeof (__builtin_sinhf16 (0)) test_type;
extern __typeof (__builtin_sinf16 (0)) test_type;
extern __typeof (__builtin_tanhf16 (0)) test_type;
extern __typeof (__builtin_tanf16 (0)) test_type;
extern __typeof (__builtin_tgammaf16 (0)) test_type;
extern __typeof (__builtin_atan2f16 (0, 0)) test_type;
extern __typeof (__builtin_fdimf16 (0, 0)) test_type;
extern __typeof (__builtin_fmodf16 (0, 0)) test_type;
extern __typeof (__builtin_frexpf16 (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf16 (0, 0)) test_type;
extern __typeof (__builtin_hypotf16 (0, 0)) test_type;
extern __typeof (__builtin_ilogbf16 (0)) test_i;
extern __typeof (__builtin_llrintf16 (0)) test_ll;
extern __typeof (__builtin_llroundf16 (0)) test_ll;
extern __typeof (__builtin_lrintf16 (0)) test_l;
extern __typeof (__builtin_lroundf16 (0)) test_l;
extern __typeof (__builtin_modff16 (0, &test_type)) test_type;
extern __typeof (__builtin_powf16 (0, 0)) test_type;
extern __typeof (__builtin_remainderf16 (0, 0)) test_type;
extern __typeof (__builtin_remquof16 (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf16 (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf16 (0, 0)) test_type;

volatile _Float16 inf_cst = __builtin_inff16 ();
volatile _Float16 huge_val_cst = __builtin_huge_valf16 ();
volatile _Float16 nan_cst = __builtin_nanf16 ("");
volatile _Float16 nans_cst = __builtin_nansf16 ("");
volatile _Float16 neg0 = -0.0f16, neg1 = -1.0f16, one = 1.0;
volatile _Float16 t1 = __builtin_acoshf16 (1.0f16);
volatile _Float16 t2 = __builtin_acosf16 (1.0f16);
volatile _Float16 t3 = __builtin_asinhf16 (0.0f16);
volatile _Float16 t4 = __builtin_asinf16 (0.0f16);
volatile _Float16 t5 = __builtin_atanhf16 (0.0f16);
volatile _Float16 t6 = __builtin_atanf16 (0.0f16);
volatile _Float16 t7 = __builtin_cbrtf16 (27.0f16);
volatile _Float16 t8 = __builtin_coshf16 (0.0f16);
volatile _Float16 t9 = __builtin_cosf16 (0.0f16);
volatile _Float16 t10 = __builtin_erfcf16 (0.0f16);
volatile _Float16 t11 = __builtin_erff16 (0.0f16);
volatile _Float16 t12 = __builtin_exp2f16 (1.0f16);
volatile _Float16 t13 = __builtin_expf16 (0.0f16);
volatile _Float16 t14 = __builtin_expm1f16 (0.0f16);
volatile _Float16 t15 = __builtin_log10f16 (1.0f16);
volatile _Float16 t16 = __builtin_log1pf16 (0.0f16);
volatile _Float16 t17 = __builtin_log2f16 (1.0f16);
volatile _Float16 t18 = __builtin_logbf16 (1.0f16);
volatile _Float16 t19 = __builtin_logf16 (1.0f16);
volatile _Float16 t20 = __builtin_nextafterf16 (0.0f16, 0.0f16);
volatile _Float16 t21 = __builtin_sinhf16 (0.0f16);
volatile _Float16 t22 = __builtin_sinf16 (0.0f16);
volatile _Float16 t23 = __builtin_tanhf16 (0.0f16);
volatile _Float16 t24 = __builtin_tanf16 (0.0f16);
volatile _Float16 t25 = __builtin_atan2f16 (0.0f16, 1.0f16);
volatile _Float16 t26 = __builtin_fdimf16 (0.0f16, 0.0f16);
volatile _Float16 t27 = __builtin_fmodf16 (0.0f16, 1.0f16);
volatile _Float16 t28 = __builtin_ldexpf16 (1.0f16, 1);
volatile _Float16 t29 = __builtin_hypotf16 (3.0f16, 4.0f16);
volatile int t30 = __builtin_ilogbf16 (1.0f16);
volatile long long int t31 = __builtin_llroundf16 (42.25f16);
volatile long int t32 = __builtin_lroundf16 (42.25f16);
volatile _Float16 t33 = __builtin_powf16 (1.0f16, 2.0f16);
volatile _Float16 t34 = __builtin_remainderf16 (7.0f16, 4.0f16);
volatile _Float16 t35 = __builtin_scalblnf16 (1.0f16, 1L);
volatile _Float16 t36 = __builtin_scalbnf16 (1.0f16, 1);

int
main (void)
{
  volatile _Float16 r;
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
  r = __builtin_fabsf16 (neg1);
  if (r != 1.0f16)
    abort ();
  r = __builtin_copysignf16 (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf16 (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf16 (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f16)
    abort ();
  if (t2 != 0.0f16)
    abort ();
  if (t3 != 0.0f16)
    abort ();
  if (t4 != 0.0f16)
    abort ();
  if (t5 != 0.0f16)
    abort ();
  if (t6 != 0.0f16)
    abort ();
  if (t7 != 3.0f16)
    abort ();
  if (t8 != 1.0f16)
    abort ();
  if (t9 != 1.0f16)
    abort ();
  if (t10 != 1.0f16)
    abort ();
  if (t11 != 0.0f16)
    abort ();
  if (t12 != 2.0f16)
    abort ();
  if (t13 != 1.0f16)
    abort ();
  if (t14 != 0.0f16)
    abort ();
  if (t15 != 0.0f16)
    abort ();
  if (t16 != 0.0f16)
    abort ();
  if (t17 != 0.0f16)
    abort ();
  if (t18 != 0.0f16)
    abort ();
  if (t19 != 0.0f16)
    abort ();
  if (t20 != 0.0f16)
    abort ();
  if (t21 != 0.0f16)
    abort ();
  if (t22 != 0.0f16)
    abort ();
  if (t23 != 0.0f16)
    abort ();
  if (t24 != 0.0f16)
    abort ();
  if (t25 != 0.0f16)
    abort ();
  if (t26 != 0.0f16)
    abort ();
  if (t27 != 0.0f16)
    abort ();
  if (t28 != 2.0f16)
    abort ();
  if (t29 != 5.0f16)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f16)
    abort ();
  if (t34 != -1.0f16)
    abort ();
  if (t35 != 2.0f16)
    abort ();
  if (t36 != 2.0f16)
    abort ();
  exit (0);
}
# 11 "./torture/float16-builtin.c" 2
