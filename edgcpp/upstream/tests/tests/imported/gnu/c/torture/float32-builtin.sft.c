//type: rp
//options: 
# 0 "./torture/float32-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32-builtin.c"
# 10 "./torture/float32-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float32 test_type;
extern __typeof (__builtin_inff32 ()) test_type;
extern __typeof (__builtin_huge_valf32 ()) test_type;
extern __typeof (__builtin_nanf32 ("")) test_type;
extern __typeof (__builtin_nansf32 ("")) test_type;
extern __typeof (__builtin_fabsf32 (0)) test_type;
extern __typeof (__builtin_copysignf32 (0, 0)) test_type;
extern __typeof (__builtin_acoshf32 (0)) test_type;
extern __typeof (__builtin_acosf32 (0)) test_type;
extern __typeof (__builtin_asinhf32 (0)) test_type;
extern __typeof (__builtin_asinf32 (0)) test_type;
extern __typeof (__builtin_atanhf32 (0)) test_type;
extern __typeof (__builtin_atanf32 (0)) test_type;
extern __typeof (__builtin_cbrtf32 (0)) test_type;
extern __typeof (__builtin_coshf32 (0)) test_type;
extern __typeof (__builtin_cosf32 (0)) test_type;
extern __typeof (__builtin_erfcf32 (0)) test_type;
extern __typeof (__builtin_erff32 (0)) test_type;
extern __typeof (__builtin_exp2f32 (0)) test_type;
extern __typeof (__builtin_expf32 (0)) test_type;
extern __typeof (__builtin_expm1f32 (0)) test_type;
extern __typeof (__builtin_lgammaf32 (0)) test_type;
extern __typeof (__builtin_log10f32 (0)) test_type;
extern __typeof (__builtin_log1pf32 (0)) test_type;
extern __typeof (__builtin_log2f32 (0)) test_type;
extern __typeof (__builtin_logbf32 (0)) test_type;
extern __typeof (__builtin_logf32 (0)) test_type;
extern __typeof (__builtin_nextafterf32 (0, 0)) test_type;
extern __typeof (__builtin_sinhf32 (0)) test_type;
extern __typeof (__builtin_sinf32 (0)) test_type;
extern __typeof (__builtin_tanhf32 (0)) test_type;
extern __typeof (__builtin_tanf32 (0)) test_type;
extern __typeof (__builtin_tgammaf32 (0)) test_type;
extern __typeof (__builtin_atan2f32 (0, 0)) test_type;
extern __typeof (__builtin_fdimf32 (0, 0)) test_type;
extern __typeof (__builtin_fmodf32 (0, 0)) test_type;
extern __typeof (__builtin_frexpf32 (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf32 (0, 0)) test_type;
extern __typeof (__builtin_hypotf32 (0, 0)) test_type;
extern __typeof (__builtin_ilogbf32 (0)) test_i;
extern __typeof (__builtin_llrintf32 (0)) test_ll;
extern __typeof (__builtin_llroundf32 (0)) test_ll;
extern __typeof (__builtin_lrintf32 (0)) test_l;
extern __typeof (__builtin_lroundf32 (0)) test_l;
extern __typeof (__builtin_modff32 (0, &test_type)) test_type;
extern __typeof (__builtin_powf32 (0, 0)) test_type;
extern __typeof (__builtin_remainderf32 (0, 0)) test_type;
extern __typeof (__builtin_remquof32 (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf32 (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf32 (0, 0)) test_type;

volatile _Float32 inf_cst = __builtin_inff32 ();
volatile _Float32 huge_val_cst = __builtin_huge_valf32 ();
volatile _Float32 nan_cst = __builtin_nanf32 ("");
volatile _Float32 nans_cst = __builtin_nansf32 ("");
volatile _Float32 neg0 = -0.0f32, neg1 = -1.0f32, one = 1.0;
volatile _Float32 t1 = __builtin_acoshf32 (1.0f32);
volatile _Float32 t2 = __builtin_acosf32 (1.0f32);
volatile _Float32 t3 = __builtin_asinhf32 (0.0f32);
volatile _Float32 t4 = __builtin_asinf32 (0.0f32);
volatile _Float32 t5 = __builtin_atanhf32 (0.0f32);
volatile _Float32 t6 = __builtin_atanf32 (0.0f32);
volatile _Float32 t7 = __builtin_cbrtf32 (27.0f32);
volatile _Float32 t8 = __builtin_coshf32 (0.0f32);
volatile _Float32 t9 = __builtin_cosf32 (0.0f32);
volatile _Float32 t10 = __builtin_erfcf32 (0.0f32);
volatile _Float32 t11 = __builtin_erff32 (0.0f32);
volatile _Float32 t12 = __builtin_exp2f32 (1.0f32);
volatile _Float32 t13 = __builtin_expf32 (0.0f32);
volatile _Float32 t14 = __builtin_expm1f32 (0.0f32);
volatile _Float32 t15 = __builtin_log10f32 (1.0f32);
volatile _Float32 t16 = __builtin_log1pf32 (0.0f32);
volatile _Float32 t17 = __builtin_log2f32 (1.0f32);
volatile _Float32 t18 = __builtin_logbf32 (1.0f32);
volatile _Float32 t19 = __builtin_logf32 (1.0f32);
volatile _Float32 t20 = __builtin_nextafterf32 (0.0f32, 0.0f32);
volatile _Float32 t21 = __builtin_sinhf32 (0.0f32);
volatile _Float32 t22 = __builtin_sinf32 (0.0f32);
volatile _Float32 t23 = __builtin_tanhf32 (0.0f32);
volatile _Float32 t24 = __builtin_tanf32 (0.0f32);
volatile _Float32 t25 = __builtin_atan2f32 (0.0f32, 1.0f32);
volatile _Float32 t26 = __builtin_fdimf32 (0.0f32, 0.0f32);
volatile _Float32 t27 = __builtin_fmodf32 (0.0f32, 1.0f32);
volatile _Float32 t28 = __builtin_ldexpf32 (1.0f32, 1);
volatile _Float32 t29 = __builtin_hypotf32 (3.0f32, 4.0f32);
volatile int t30 = __builtin_ilogbf32 (1.0f32);
volatile long long int t31 = __builtin_llroundf32 (42.25f32);
volatile long int t32 = __builtin_lroundf32 (42.25f32);
volatile _Float32 t33 = __builtin_powf32 (1.0f32, 2.0f32);
volatile _Float32 t34 = __builtin_remainderf32 (7.0f32, 4.0f32);
volatile _Float32 t35 = __builtin_scalblnf32 (1.0f32, 1L);
volatile _Float32 t36 = __builtin_scalbnf32 (1.0f32, 1);

int
main (void)
{
  volatile _Float32 r;
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
  r = __builtin_fabsf32 (neg1);
  if (r != 1.0f32)
    abort ();
  r = __builtin_copysignf32 (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf32 (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf32 (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f32)
    abort ();
  if (t2 != 0.0f32)
    abort ();
  if (t3 != 0.0f32)
    abort ();
  if (t4 != 0.0f32)
    abort ();
  if (t5 != 0.0f32)
    abort ();
  if (t6 != 0.0f32)
    abort ();
  if (t7 != 3.0f32)
    abort ();
  if (t8 != 1.0f32)
    abort ();
  if (t9 != 1.0f32)
    abort ();
  if (t10 != 1.0f32)
    abort ();
  if (t11 != 0.0f32)
    abort ();
  if (t12 != 2.0f32)
    abort ();
  if (t13 != 1.0f32)
    abort ();
  if (t14 != 0.0f32)
    abort ();
  if (t15 != 0.0f32)
    abort ();
  if (t16 != 0.0f32)
    abort ();
  if (t17 != 0.0f32)
    abort ();
  if (t18 != 0.0f32)
    abort ();
  if (t19 != 0.0f32)
    abort ();
  if (t20 != 0.0f32)
    abort ();
  if (t21 != 0.0f32)
    abort ();
  if (t22 != 0.0f32)
    abort ();
  if (t23 != 0.0f32)
    abort ();
  if (t24 != 0.0f32)
    abort ();
  if (t25 != 0.0f32)
    abort ();
  if (t26 != 0.0f32)
    abort ();
  if (t27 != 0.0f32)
    abort ();
  if (t28 != 2.0f32)
    abort ();
  if (t29 != 5.0f32)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f32)
    abort ();
  if (t34 != -1.0f32)
    abort ();
  if (t35 != 2.0f32)
    abort ();
  if (t36 != 2.0f32)
    abort ();
  exit (0);
}
# 11 "./torture/float32-builtin.c" 2
