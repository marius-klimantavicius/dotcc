//type: rp
//options: 
# 0 "./torture/float32x-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32x-builtin.c"
# 11 "./torture/float32x-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float32x test_type;
extern __typeof (__builtin_inff32x ()) test_type;
extern __typeof (__builtin_huge_valf32x ()) test_type;
extern __typeof (__builtin_nanf32x ("")) test_type;
extern __typeof (__builtin_nansf32x ("")) test_type;
extern __typeof (__builtin_fabsf32x (0)) test_type;
extern __typeof (__builtin_copysignf32x (0, 0)) test_type;
extern __typeof (__builtin_acoshf32x (0)) test_type;
extern __typeof (__builtin_acosf32x (0)) test_type;
extern __typeof (__builtin_asinhf32x (0)) test_type;
extern __typeof (__builtin_asinf32x (0)) test_type;
extern __typeof (__builtin_atanhf32x (0)) test_type;
extern __typeof (__builtin_atanf32x (0)) test_type;
extern __typeof (__builtin_cbrtf32x (0)) test_type;
extern __typeof (__builtin_coshf32x (0)) test_type;
extern __typeof (__builtin_cosf32x (0)) test_type;
extern __typeof (__builtin_erfcf32x (0)) test_type;
extern __typeof (__builtin_erff32x (0)) test_type;
extern __typeof (__builtin_exp2f32x (0)) test_type;
extern __typeof (__builtin_expf32x (0)) test_type;
extern __typeof (__builtin_expm1f32x (0)) test_type;
extern __typeof (__builtin_lgammaf32x (0)) test_type;
extern __typeof (__builtin_log10f32x (0)) test_type;
extern __typeof (__builtin_log1pf32x (0)) test_type;
extern __typeof (__builtin_log2f32x (0)) test_type;
extern __typeof (__builtin_logbf32x (0)) test_type;
extern __typeof (__builtin_logf32x (0)) test_type;
extern __typeof (__builtin_nextafterf32x (0, 0)) test_type;
extern __typeof (__builtin_sinhf32x (0)) test_type;
extern __typeof (__builtin_sinf32x (0)) test_type;
extern __typeof (__builtin_tanhf32x (0)) test_type;
extern __typeof (__builtin_tanf32x (0)) test_type;
extern __typeof (__builtin_tgammaf32x (0)) test_type;
extern __typeof (__builtin_atan2f32x (0, 0)) test_type;
extern __typeof (__builtin_fdimf32x (0, 0)) test_type;
extern __typeof (__builtin_fmodf32x (0, 0)) test_type;
extern __typeof (__builtin_frexpf32x (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf32x (0, 0)) test_type;
extern __typeof (__builtin_hypotf32x (0, 0)) test_type;
extern __typeof (__builtin_ilogbf32x (0)) test_i;
extern __typeof (__builtin_llrintf32x (0)) test_ll;
extern __typeof (__builtin_llroundf32x (0)) test_ll;
extern __typeof (__builtin_lrintf32x (0)) test_l;
extern __typeof (__builtin_lroundf32x (0)) test_l;
extern __typeof (__builtin_modff32x (0, &test_type)) test_type;
extern __typeof (__builtin_powf32x (0, 0)) test_type;
extern __typeof (__builtin_remainderf32x (0, 0)) test_type;
extern __typeof (__builtin_remquof32x (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf32x (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf32x (0, 0)) test_type;

volatile _Float32x inf_cst = __builtin_inff32x ();
volatile _Float32x huge_val_cst = __builtin_huge_valf32x ();
volatile _Float32x nan_cst = __builtin_nanf32x ("");
volatile _Float32x nans_cst = __builtin_nansf32x ("");
volatile _Float32x neg0 = -0.0f32x, neg1 = -1.0f32x, one = 1.0;
volatile _Float32x t1 = __builtin_acoshf32x (1.0f32x);
volatile _Float32x t2 = __builtin_acosf32x (1.0f32x);
volatile _Float32x t3 = __builtin_asinhf32x (0.0f32x);
volatile _Float32x t4 = __builtin_asinf32x (0.0f32x);
volatile _Float32x t5 = __builtin_atanhf32x (0.0f32x);
volatile _Float32x t6 = __builtin_atanf32x (0.0f32x);
volatile _Float32x t7 = __builtin_cbrtf32x (27.0f32x);
volatile _Float32x t8 = __builtin_coshf32x (0.0f32x);
volatile _Float32x t9 = __builtin_cosf32x (0.0f32x);
volatile _Float32x t10 = __builtin_erfcf32x (0.0f32x);
volatile _Float32x t11 = __builtin_erff32x (0.0f32x);
volatile _Float32x t12 = __builtin_exp2f32x (1.0f32x);
volatile _Float32x t13 = __builtin_expf32x (0.0f32x);
volatile _Float32x t14 = __builtin_expm1f32x (0.0f32x);
volatile _Float32x t15 = __builtin_log10f32x (1.0f32x);
volatile _Float32x t16 = __builtin_log1pf32x (0.0f32x);
volatile _Float32x t17 = __builtin_log2f32x (1.0f32x);
volatile _Float32x t18 = __builtin_logbf32x (1.0f32x);
volatile _Float32x t19 = __builtin_logf32x (1.0f32x);
volatile _Float32x t20 = __builtin_nextafterf32x (0.0f32x, 0.0f32x);
volatile _Float32x t21 = __builtin_sinhf32x (0.0f32x);
volatile _Float32x t22 = __builtin_sinf32x (0.0f32x);
volatile _Float32x t23 = __builtin_tanhf32x (0.0f32x);
volatile _Float32x t24 = __builtin_tanf32x (0.0f32x);
volatile _Float32x t25 = __builtin_atan2f32x (0.0f32x, 1.0f32x);
volatile _Float32x t26 = __builtin_fdimf32x (0.0f32x, 0.0f32x);
volatile _Float32x t27 = __builtin_fmodf32x (0.0f32x, 1.0f32x);
volatile _Float32x t28 = __builtin_ldexpf32x (1.0f32x, 1);
volatile _Float32x t29 = __builtin_hypotf32x (3.0f32x, 4.0f32x);
volatile int t30 = __builtin_ilogbf32x (1.0f32x);
volatile long long int t31 = __builtin_llroundf32x (42.25f32x);
volatile long int t32 = __builtin_lroundf32x (42.25f32x);
volatile _Float32x t33 = __builtin_powf32x (1.0f32x, 2.0f32x);
volatile _Float32x t34 = __builtin_remainderf32x (7.0f32x, 4.0f32x);
volatile _Float32x t35 = __builtin_scalblnf32x (1.0f32x, 1L);
volatile _Float32x t36 = __builtin_scalbnf32x (1.0f32x, 1);

int
main (void)
{
  volatile _Float32x r;
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
  r = __builtin_fabsf32x (neg1);
  if (r != 1.0f32x)
    abort ();
  r = __builtin_copysignf32x (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf32x (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf32x (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f32x)
    abort ();
  if (t2 != 0.0f32x)
    abort ();
  if (t3 != 0.0f32x)
    abort ();
  if (t4 != 0.0f32x)
    abort ();
  if (t5 != 0.0f32x)
    abort ();
  if (t6 != 0.0f32x)
    abort ();
  if (t7 != 3.0f32x)
    abort ();
  if (t8 != 1.0f32x)
    abort ();
  if (t9 != 1.0f32x)
    abort ();
  if (t10 != 1.0f32x)
    abort ();
  if (t11 != 0.0f32x)
    abort ();
  if (t12 != 2.0f32x)
    abort ();
  if (t13 != 1.0f32x)
    abort ();
  if (t14 != 0.0f32x)
    abort ();
  if (t15 != 0.0f32x)
    abort ();
  if (t16 != 0.0f32x)
    abort ();
  if (t17 != 0.0f32x)
    abort ();
  if (t18 != 0.0f32x)
    abort ();
  if (t19 != 0.0f32x)
    abort ();
  if (t20 != 0.0f32x)
    abort ();
  if (t21 != 0.0f32x)
    abort ();
  if (t22 != 0.0f32x)
    abort ();
  if (t23 != 0.0f32x)
    abort ();
  if (t24 != 0.0f32x)
    abort ();
  if (t25 != 0.0f32x)
    abort ();
  if (t26 != 0.0f32x)
    abort ();
  if (t27 != 0.0f32x)
    abort ();
  if (t28 != 2.0f32x)
    abort ();
  if (t29 != 5.0f32x)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f32x)
    abort ();
  if (t34 != -1.0f32x)
    abort ();
  if (t35 != 2.0f32x)
    abort ();
  if (t36 != 2.0f32x)
    abort ();
  exit (0);
}
# 12 "./torture/float32x-builtin.c" 2
