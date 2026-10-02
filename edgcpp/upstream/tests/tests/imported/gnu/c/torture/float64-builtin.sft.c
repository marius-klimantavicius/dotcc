//type: rp
//options: 
# 0 "./torture/float64-builtin.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64-builtin.c"
# 11 "./torture/float64-builtin.c"
# 1 "./torture/floatn-builtin.h" 1
# 20 "./torture/floatn-builtin.h"
extern void exit (int);
extern void abort (void);

extern int test_i;
extern long int test_l;
extern long long int test_ll;
extern _Float64 test_type;
extern __typeof (__builtin_inff64 ()) test_type;
extern __typeof (__builtin_huge_valf64 ()) test_type;
extern __typeof (__builtin_nanf64 ("")) test_type;
extern __typeof (__builtin_nansf64 ("")) test_type;
extern __typeof (__builtin_fabsf64 (0)) test_type;
extern __typeof (__builtin_copysignf64 (0, 0)) test_type;
extern __typeof (__builtin_acoshf64 (0)) test_type;
extern __typeof (__builtin_acosf64 (0)) test_type;
extern __typeof (__builtin_asinhf64 (0)) test_type;
extern __typeof (__builtin_asinf64 (0)) test_type;
extern __typeof (__builtin_atanhf64 (0)) test_type;
extern __typeof (__builtin_atanf64 (0)) test_type;
extern __typeof (__builtin_cbrtf64 (0)) test_type;
extern __typeof (__builtin_coshf64 (0)) test_type;
extern __typeof (__builtin_cosf64 (0)) test_type;
extern __typeof (__builtin_erfcf64 (0)) test_type;
extern __typeof (__builtin_erff64 (0)) test_type;
extern __typeof (__builtin_exp2f64 (0)) test_type;
extern __typeof (__builtin_expf64 (0)) test_type;
extern __typeof (__builtin_expm1f64 (0)) test_type;
extern __typeof (__builtin_lgammaf64 (0)) test_type;
extern __typeof (__builtin_log10f64 (0)) test_type;
extern __typeof (__builtin_log1pf64 (0)) test_type;
extern __typeof (__builtin_log2f64 (0)) test_type;
extern __typeof (__builtin_logbf64 (0)) test_type;
extern __typeof (__builtin_logf64 (0)) test_type;
extern __typeof (__builtin_nextafterf64 (0, 0)) test_type;
extern __typeof (__builtin_sinhf64 (0)) test_type;
extern __typeof (__builtin_sinf64 (0)) test_type;
extern __typeof (__builtin_tanhf64 (0)) test_type;
extern __typeof (__builtin_tanf64 (0)) test_type;
extern __typeof (__builtin_tgammaf64 (0)) test_type;
extern __typeof (__builtin_atan2f64 (0, 0)) test_type;
extern __typeof (__builtin_fdimf64 (0, 0)) test_type;
extern __typeof (__builtin_fmodf64 (0, 0)) test_type;
extern __typeof (__builtin_frexpf64 (0, &test_i)) test_type;
extern __typeof (__builtin_ldexpf64 (0, 0)) test_type;
extern __typeof (__builtin_hypotf64 (0, 0)) test_type;
extern __typeof (__builtin_ilogbf64 (0)) test_i;
extern __typeof (__builtin_llrintf64 (0)) test_ll;
extern __typeof (__builtin_llroundf64 (0)) test_ll;
extern __typeof (__builtin_lrintf64 (0)) test_l;
extern __typeof (__builtin_lroundf64 (0)) test_l;
extern __typeof (__builtin_modff64 (0, &test_type)) test_type;
extern __typeof (__builtin_powf64 (0, 0)) test_type;
extern __typeof (__builtin_remainderf64 (0, 0)) test_type;
extern __typeof (__builtin_remquof64 (0, 0, &test_i)) test_type;
extern __typeof (__builtin_scalblnf64 (0, 0L)) test_type;
extern __typeof (__builtin_scalbnf64 (0, 0)) test_type;

volatile _Float64 inf_cst = __builtin_inff64 ();
volatile _Float64 huge_val_cst = __builtin_huge_valf64 ();
volatile _Float64 nan_cst = __builtin_nanf64 ("");
volatile _Float64 nans_cst = __builtin_nansf64 ("");
volatile _Float64 neg0 = -0.0f64, neg1 = -1.0f64, one = 1.0;
volatile _Float64 t1 = __builtin_acoshf64 (1.0f64);
volatile _Float64 t2 = __builtin_acosf64 (1.0f64);
volatile _Float64 t3 = __builtin_asinhf64 (0.0f64);
volatile _Float64 t4 = __builtin_asinf64 (0.0f64);
volatile _Float64 t5 = __builtin_atanhf64 (0.0f64);
volatile _Float64 t6 = __builtin_atanf64 (0.0f64);
volatile _Float64 t7 = __builtin_cbrtf64 (27.0f64);
volatile _Float64 t8 = __builtin_coshf64 (0.0f64);
volatile _Float64 t9 = __builtin_cosf64 (0.0f64);
volatile _Float64 t10 = __builtin_erfcf64 (0.0f64);
volatile _Float64 t11 = __builtin_erff64 (0.0f64);
volatile _Float64 t12 = __builtin_exp2f64 (1.0f64);
volatile _Float64 t13 = __builtin_expf64 (0.0f64);
volatile _Float64 t14 = __builtin_expm1f64 (0.0f64);
volatile _Float64 t15 = __builtin_log10f64 (1.0f64);
volatile _Float64 t16 = __builtin_log1pf64 (0.0f64);
volatile _Float64 t17 = __builtin_log2f64 (1.0f64);
volatile _Float64 t18 = __builtin_logbf64 (1.0f64);
volatile _Float64 t19 = __builtin_logf64 (1.0f64);
volatile _Float64 t20 = __builtin_nextafterf64 (0.0f64, 0.0f64);
volatile _Float64 t21 = __builtin_sinhf64 (0.0f64);
volatile _Float64 t22 = __builtin_sinf64 (0.0f64);
volatile _Float64 t23 = __builtin_tanhf64 (0.0f64);
volatile _Float64 t24 = __builtin_tanf64 (0.0f64);
volatile _Float64 t25 = __builtin_atan2f64 (0.0f64, 1.0f64);
volatile _Float64 t26 = __builtin_fdimf64 (0.0f64, 0.0f64);
volatile _Float64 t27 = __builtin_fmodf64 (0.0f64, 1.0f64);
volatile _Float64 t28 = __builtin_ldexpf64 (1.0f64, 1);
volatile _Float64 t29 = __builtin_hypotf64 (3.0f64, 4.0f64);
volatile int t30 = __builtin_ilogbf64 (1.0f64);
volatile long long int t31 = __builtin_llroundf64 (42.25f64);
volatile long int t32 = __builtin_lroundf64 (42.25f64);
volatile _Float64 t33 = __builtin_powf64 (1.0f64, 2.0f64);
volatile _Float64 t34 = __builtin_remainderf64 (7.0f64, 4.0f64);
volatile _Float64 t35 = __builtin_scalblnf64 (1.0f64, 1L);
volatile _Float64 t36 = __builtin_scalbnf64 (1.0f64, 1);

int
main (void)
{
  volatile _Float64 r;
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
  r = __builtin_fabsf64 (neg1);
  if (r != 1.0f64)
    abort ();
  r = __builtin_copysignf64 (one, neg0);
  if (r != neg1)
    abort ();
  r = __builtin_copysignf64 (inf_cst, neg1);
  if (r != -huge_val_cst)
    abort ();
  r = __builtin_copysignf64 (-inf_cst, one);
  if (r != huge_val_cst)
    abort ();
  if (t1 != 0.0f64)
    abort ();
  if (t2 != 0.0f64)
    abort ();
  if (t3 != 0.0f64)
    abort ();
  if (t4 != 0.0f64)
    abort ();
  if (t5 != 0.0f64)
    abort ();
  if (t6 != 0.0f64)
    abort ();
  if (t7 != 3.0f64)
    abort ();
  if (t8 != 1.0f64)
    abort ();
  if (t9 != 1.0f64)
    abort ();
  if (t10 != 1.0f64)
    abort ();
  if (t11 != 0.0f64)
    abort ();
  if (t12 != 2.0f64)
    abort ();
  if (t13 != 1.0f64)
    abort ();
  if (t14 != 0.0f64)
    abort ();
  if (t15 != 0.0f64)
    abort ();
  if (t16 != 0.0f64)
    abort ();
  if (t17 != 0.0f64)
    abort ();
  if (t18 != 0.0f64)
    abort ();
  if (t19 != 0.0f64)
    abort ();
  if (t20 != 0.0f64)
    abort ();
  if (t21 != 0.0f64)
    abort ();
  if (t22 != 0.0f64)
    abort ();
  if (t23 != 0.0f64)
    abort ();
  if (t24 != 0.0f64)
    abort ();
  if (t25 != 0.0f64)
    abort ();
  if (t26 != 0.0f64)
    abort ();
  if (t27 != 0.0f64)
    abort ();
  if (t28 != 2.0f64)
    abort ();
  if (t29 != 5.0f64)
    abort ();
  if (t30 != 0)
    abort ();
  if (t31 != 42)
    abort ();
  if (t32 != 42)
    abort ();
  if (t33 != 1.0f64)
    abort ();
  if (t34 != -1.0f64)
    abort ();
  if (t35 != 2.0f64)
    abort ();
  if (t36 != 2.0f64)
    abort ();
  exit (0);
}
# 12 "./torture/float64-builtin.c" 2
