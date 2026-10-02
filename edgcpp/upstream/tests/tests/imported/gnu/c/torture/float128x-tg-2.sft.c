//type: rp
//options: 
# 0 "./torture/float128x-tg-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128x-tg-2.c"
# 10 "./torture/float128x-tg-2.c"
# 1 "./torture/floatn-tg-2.h" 1






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./torture/floatn-tg-2.h" 2
# 24 "./torture/floatn-tg-2.h"
extern void exit (int);
extern void abort (void);

volatile _Float128x inf = __builtin_inf (), nanval = __builtin_nan ("");
volatile _Float128x neginf = -__builtin_inf (), negnanval = -__builtin_nan ("");
volatile _Float128x zero = 0.0f128x, negzero = -0.0f128x, one = 1.0f128x;
volatile _Float128x max = FLT128X_MAX, negmax = -FLT128X_MAX;

int
main (void)
{
  if (__builtin_isinf_sign (inf) != 1)
    abort ();
  if (__builtin_isinf_sign (neginf) != -1)
    abort ();
  if (__builtin_isinf_sign (nanval) != 0)
    abort ();
  if (__builtin_isinf_sign (negnanval) != 0)
    abort ();
  if (__builtin_isinf_sign (zero) != 0)
    abort ();
  if (__builtin_isinf_sign (negzero) != 0)
    abort ();
  if (__builtin_isinf_sign (one) != 0)
    abort ();
  if (__builtin_isinf_sign (max) != 0)
    abort ();
  if (__builtin_isinf_sign (negmax) != 0)
    abort ();
  exit (0);
}
# 11 "./torture/float128x-tg-2.c" 2
