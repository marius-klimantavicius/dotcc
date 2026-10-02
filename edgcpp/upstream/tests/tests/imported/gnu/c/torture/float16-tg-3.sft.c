//type: rp
//options: 
# 0 "./torture/float16-tg-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float16-tg-3.c"
# 10 "./torture/float16-tg-3.c"
# 1 "./torture/floatn-tg-3.h" 1






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./torture/floatn-tg-3.h" 2
# 28 "./torture/floatn-tg-3.h"
extern void exit (int);
extern void abort (void);
# 41 "./torture/floatn-tg-3.h"
volatile _Float16 inf = __builtin_inf (), nanval = __builtin_nan ("");
volatile _Float16 neginf = -__builtin_inf (), negnanval = -__builtin_nan ("");
volatile _Float16 zero = 0.0f16, negzero = -0.0f16, one = 1.0f16;
volatile _Float16 max = 6.55040000000000000000000000000000000e+4F16
# 44 "./torture/floatn-tg-3.h"
                      , negmax = -6.55040000000000000000000000000000000e+4F16
# 44 "./torture/floatn-tg-3.h"
                                     , min = 6.10351562500000000000000000000000000e-5F16
# 44 "./torture/floatn-tg-3.h"
                                                , negmin = -6.10351562500000000000000000000000000e-5F16
# 44 "./torture/floatn-tg-3.h"
                                                               ;
volatile _Float16 true_min = 5.96046447753906250000000000000000000e-8F16
# 45 "./torture/floatn-tg-3.h"
                                , negtrue_min = -5.96046447753906250000000000000000000e-8F16
# 45 "./torture/floatn-tg-3.h"
                                                         ;
# 62 "./torture/floatn-tg-3.h"
int
main (void)
{
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (inf)) != (1)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (inf)); if (c != (1)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (neginf)) != (1)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (neginf)); if (c != (1)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (nanval)) != (0)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (nanval)); if (c != (0)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (negnanval)) != (0)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (negnanval)); if (c != (0)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (zero)) != (2)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (zero)); if (c != (2)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (negzero)) != (2)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (negzero)); if (c != (2)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (one)) != (4)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (one)); if (c != (4)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (max)) != (4)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (max)); if (c != (4)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (negmax)) != (4)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (negmax)); if (c != (4)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (min)) != (4)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (min)); if (c != (4)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (negmin)) != (4)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (negmin)); if (c != (4)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (true_min)) != (3)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (true_min)); if (c != (3)) abort (); } while (0);
  do { volatile int c; if (__builtin_fpclassify (0, 1, 4, 3, 2, (negtrue_min)) != (3)) abort (); c = __builtin_fpclassify (0, 1, 4, 3, 2, (negtrue_min)); if (c != (3)) abort (); } while (0);
  exit (0);
}
# 11 "./torture/float16-tg-3.c" 2
