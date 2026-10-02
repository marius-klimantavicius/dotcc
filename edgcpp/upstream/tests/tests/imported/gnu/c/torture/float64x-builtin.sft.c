//type: rp
//options: 
# 0 "./torture/float64x-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64x-builtin.c"
# 10 "./torture/float64x-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float64x test_type;
extern __typeof (__builtin_inff64x ()) test_type;
extern __typeof (__builtin_huge_valf64x ()) test_type;
extern __typeof (__builtin_nanf64x ("")) test_type;
extern __typeof (__builtin_nansf64x ("")) test_type;
extern __typeof (__builtin_fabsf64x (0)) test_type;
extern __typeof (__builtin_copysignf64x (0, 0)) test_type;
extern __typeof (__builtin_acoshf64x (0)) test_type;
extern __typeof (__builtin_acosf64x (0)) test_type;
extern __typeof (__builtin_asinhf64x (0)) test_type;
extern __typeof (__builtin_asinf64x (0)) test_type;
extern __typeof (__builtin_atanhf64x (0)) test_type;
extern __typeof (__builtin_atanf64x (0)) test_type;
extern __typeof (__builtin_cbrtf64x (0)) test_type;
extern __typeof (__builtin_coshf64x (0)) test_type;
extern __typeof (__builtin_cosf64x (0)) test_type;
extern __typeof (__builtin_erfcf64x (0)) test_type;
extern __typeof (__builtin_erff64x (0)) test_type;
extern __typeof (__builtin_exp2f64x (0)) test_type;
extern __typeof (__builtin_expf64x (0)) test_type;
extern __typeof (__builtin_expm1f64x (0)) test_type;
extern __typeof (__builtin_lgammaf64x (0)) test_type;
extern __typeof (__builtin_log10f64x (0)) test_type;
extern __typeof (__builtin_log1pf64x (0)) test_type;
extern __typeof (__builtin_log2f64x (0)) test_type;
extern __typeof (__builtin_logbf64x (0)) test_type;
extern __typeof (__builtin_logf64x (0)) test_type;
extern __typeof (__builtin_nextafterf64x (0, 0)) test_type;
extern __typeof (__builtin_sinhf64x (0)) test_type;
extern __typeof (__builtin_sinf64x (0)) test_type;
extern __typeof (__builtin_tanhf64x (0)) test_type;
extern __typeof (__builtin_tanf64x (0)) test_type;
extern __typeof (__builtin_tgammaf64x (0)) test_type;
extern __typeof (__builtin_atan2f64x (0, 0)) test_type;
extern __typeof (__builtin_fdimf64x (0, 0)) test_type;
extern __typeof (__builtin_fmodf64x (0, 0)) test_type;
extern __typeof (__builtin_frexpf64x (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf64x (0, 0)) test_type;
extern __typeof (__builtin_hypotf64x (0, 0)) test_type;
extern __typeof (__builtin_ilogbf64x (0)) test_i;
extern __typeof (__builtin_llrintf64x (0)) test_ll;
extern __typeof (__builtin_llroundf64x (0)) test_ll;
extern __typeof (__builtin_lrintf64x (0)) test_l;
extern __typeof (__builtin_lroundf64x (0)) test_l;
extern __typeof (__builtin_modff64x (0, &test_type)) test_type;
extern __typeof (__builtin_powf64x (0, 0)) test_type;
extern __typeof (__builtin_remainderf64x (0, 0)) test_type;
extern __typeof (__builtin_remquof64x (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf64x (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf64x (0, 0)) test_type;

volatile _Float64x inf_cst = __builtin_inff64x ();
volatile _Float64x huge_val_cst = __builtin_huge_valf64x ();
volatile _Float64x nan_cst = __builtin_nanf64x ("");
volatile _Float64x nans_cst = __builtin_nansf64x ("");
volatile _Float64x neg0 = -0.0f64x, neg1 = -1.0f64x, one = 1.0;
volatile _Float64x t1 = __builtin_acoshf64x (1.0f64x);
volatile _Float64x t2 = __builtin_acosf64x (1.0f64x);
volatile _Float64x t3 = __builtin_asinhf64x (0.0f64x);
volatile _Float64x t4 = __builtin_asinf64x (0.0f64x);
volatile _Float64x t5 = __builtin_atanhf64x (0.0f64x);
volatile _Float64x t6 = __builtin_atanf64x (0.0f64x);
volatile _Float64x t7 = __builtin_cbrtf64x (27.0f64x);
volatile _Float64x t8 = __builtin_coshf64x (0.0f64x);
volatile _Float64x t9 = __builtin_cosf64x (0.0f64x);
volatile _Float64x t10 = __builtin_erfcf64x (0.0f64x);
volatile _Float64x t11 = __builtin_erff64x (0.0f64x);
volatile _Float64x t12 = __builtin_exp2f64x (1.0f64x);
volatile _Float64x t13 = __builtin_expf64x (0.0f64x);
volatile _Float64x t14 = __builtin_expm1f64x (0.0f64x);
volatile _Float64x t15 = __builtin_log10f64x (1.0f64x);
volatile _Float64x t16 = __builtin_log1pf64x (0.0f64x);
volatile _Float64x t17 = __builtin_log2f64x (1.0f64x);
volatile _Float64x t18 = __builtin_logbf64x (1.0f64x);
volatile _Float64x t19 = __builtin_logf64x (1.0f64x);
volatile _Float64x t20 = __builtin_nextafterf64x (0.0f64x, 0.0f64x);
volatile _Float64x t21 = __builtin_sinhf64x (0.0f64x);
volatile _Float64x t22 = __builtin_sinf64x (0.0f64x);
volatile _Float64x t23 = __builtin_tanhf64x (0.0f64x);
volatile _Float64x t24 = __builtin_tanf64x (0.0f64x);
volatile _Float64x t25 = __builtin_atan2f64x (0.0f64x, 1.0f64x);
volatile _Float64x t26 = __builtin_fdimf64x (0.0f64x, 0.0f64x);
volatile _Float64x t27 = __builtin_fmodf64x (0.0f64x, 1.0f64x);
volatile _Float64x t28 = __builtin_ldexpf64x (1.0f64x, 1);
volatile _Float64x t29 = __builtin_hypotf64x (3.0f64x, 4.0f64x);
volatile int t30 = __builtin_ilogbf64x (1.0f64x);
volatile long long int t31 = __builtin_llroundf64x (42.25f64x);
volatile long int t32 = __builtin_lroundf64x (42.25f64x);
volatile _Float64x t33 = __builtin_powf64x (1.0f64x, 2.0f64x);
volatile _Float64x t34 = __builtin_remainderf64x (7.0f64x, 4.0f64x);
volatile _Float64x t35 = __builtin_scalblnf64x (1.0f64x, 1L);
volatile _Float64x t36 = __builtin_scalbnf64x (1.0f64x, 1);

int
main (void)
{
  volatile _Float64x r;
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
  r = __builtin_fabsf64x (neg1);
  if (r != 1.0f64x)
    abort ();
  r = __builtin_copysignf64x (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf64x (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf64x (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f64x)
    abort ();
  if (t2 != 0.0f64x)
    abort ();
  if (t3 != 0.0f64x)
    abort ();
  if (t4 != 0.0f64x)
    abort ();
  if (t5 != 0.0f64x)
    abort ();
  if (t6 != 0.0f64x)
    abort ();
  if (t7 != 3.0f64x)
    abort ();
  if (t8 != 1.0f64x)
    abort ();
  if (t9 != 1.0f64x)
    abort ();
  if (t10 != 1.0f64x)
    abort ();
  if (t11 != 0.0f64x)
    abort ();
  if (t12 != 2.0f64x)
    abort ();
  if (t13 != 1.0f64x)
    abort ();
  if (t14 != 0.0f64x)
    abort ();
  if (t15 != 0.0f64x)
    abort ();
  if (t16 != 0.0f64x)
    abort ();
  if (t17 != 0.0f64x)
    abort ();
  if (t18 != 0.0f64x)
    abort ();
  if (t19 != 0.0f64x)
    abort ();
  if (t20 != 0.0f64x)
    abort ();
  if (t21 != 0.0f64x)
    abort ();
  if (t22 != 0.0f64x)
    abort ();
  if (t23 != 0.0f64x)
    abort ();
  if (t24 != 0.0f64x)
    abort ();
  if (t25 != 0.0f64x)
    abort ();
  if (t26 != 0.0f64x)
    abort ();
  if (t27 != 0.0f64x)
    abort ();
  if (t28 != 2.0f64x)
    abort ();
  if (t29 != 5.0f64x)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f64x)
    abort ();
  if (t34 != -1.0f64x)
    abort ();
  if (t35 != 2.0f64x)
    abort ();
  if (t36 != 2.0f64x)
    abort ();
  exit (0);
}
# 11 "./torture/float64x-builtin.c" 2
