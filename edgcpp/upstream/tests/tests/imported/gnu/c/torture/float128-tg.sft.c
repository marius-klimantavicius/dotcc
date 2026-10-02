//type: rp
//options: 
# 0 "./torture/float128-tg.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128-tg.c"
# 10 "./torture/float128-tg.c"
# 1 "./torture/floatn-tg.h" 1






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./torture/floatn-tg.h" 2
# 24 "./torture/floatn-tg.h"
extern void exit (int);
extern void abort (void);

volatile _Float128 inf = __builtin_inf (), nanval = __builtin_nan ("");
volatile _Float128 zero = 0.0f128, negzero = -0.0f128, one = 1.0f128;
volatile _Float128 true_min = 6.47517511943802511092443895822764655e-4966F128
# 29 "./torture/floatn-tg.h"
                                ;

int
main (void)
{
  if (__builtin_signbit (inf) != 0)
    abort ();
  if (__builtin_signbit (zero) != 0)
    abort ();
  if (__builtin_signbit (negzero) == 0)
    abort ();
  if (__builtin_isfinite (nanval) != 0)
    abort ();
  if (__builtin_isfinite (inf) != 0)
    abort ();
  if (__builtin_isfinite (one) == 0)
    abort ();
  if (__builtin_isinf (nanval) != 0)
    abort ();
  if (__builtin_isinf (inf) == 0)
    abort ();
  if (__builtin_isnan (nanval) == 0)
    abort ();
  if (__builtin_isnan (inf) != 0)
    abort ();
  if (__builtin_isnormal (inf) != 0)
    abort ();
  if (__builtin_isnormal (one) == 0)
    abort ();
  if (__builtin_isnormal (nanval) != 0)
    abort ();
  if (__builtin_isnormal (zero) != 0)
    abort ();
  if (__builtin_isnormal (true_min) != 0)
    abort ();
  if (__builtin_islessequal (zero, one) != 1)
    abort ();
  if (__builtin_islessequal (one, zero) != 0)
    abort ();
  if (__builtin_islessequal (zero, negzero) != 1)
    abort ();
  if (__builtin_islessequal (zero, nanval) != 0)
    abort ();
  if (__builtin_isless (zero, one) != 1)
    abort ();
  if (__builtin_isless (one, zero) != 0)
    abort ();
  if (__builtin_isless (zero, negzero) != 0)
    abort ();
  if (__builtin_isless (zero, nanval) != 0)
    abort ();
  if (__builtin_isgreaterequal (zero, one) != 0)
    abort ();
  if (__builtin_isgreaterequal (one, zero) != 1)
    abort ();
  if (__builtin_isgreaterequal (zero, negzero) != 1)
    abort ();
  if (__builtin_isgreaterequal (zero, nanval) != 0)
    abort ();
  if (__builtin_isgreater (zero, one) != 0)
    abort ();
  if (__builtin_isgreater (one, zero) != 1)
    abort ();
  if (__builtin_isgreater (zero, negzero) != 0)
    abort ();
  if (__builtin_isgreater (zero, nanval) != 0)
    abort ();
  if (__builtin_islessgreater (zero, one) != 1)
    abort ();
  if (__builtin_islessgreater (one, zero) != 1)
    abort ();
  if (__builtin_islessgreater (zero, negzero) != 0)
    abort ();
  if (__builtin_islessgreater (zero, nanval) != 0)
    abort ();
  if (__builtin_isunordered (zero, one) != 0)
    abort ();
  if (__builtin_isunordered (one, zero) != 0)
    abort ();
  if (__builtin_isunordered (zero, negzero) != 0)
    abort ();
  if (__builtin_isunordered (zero, nanval) != 1)
    abort ();
  exit (0);
}
# 11 "./torture/float128-tg.c" 2
