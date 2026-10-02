//type: rp
//options: 
# 0 "./torture/float64-tg-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64-tg-3.c"
# 11 "./torture/float64-tg-3.c"
# 1 "./torture/floatn-tg-3.h" 1






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./torture/floatn-tg-3.h" 2
# 28 "./torture/floatn-tg-3.h"
extern void exit (int);
extern void abort (void);
# 41 "./torture/floatn-tg-3.h"
volatile _Float64 inf = __builtin_inf (), nanval = __builtin_nan ("");
volatile _Float64 neginf = -__builtin_inf (), negnanval = -__builtin_nan ("");
volatile _Float64 zero = 0.0f64, negzero = -0.0f64, one = 1.0f64;
volatile _Float64 max = 1.79769313486231570814527423731704357e+308F64
# 44 "./torture/floatn-tg-3.h"
                      , negmax = -1.79769313486231570814527423731704357e+308F64
# 44 "./torture/floatn-tg-3.h"
                                     , min = 2.22507385850720138309023271733240406e-308F64
# 44 "./torture/floatn-tg-3.h"
                                                , negmin = -2.22507385850720138309023271733240406e-308F64
# 44 "./torture/floatn-tg-3.h"
                                                               ;
volatile _Float64 true_min = 4.94065645841246544176568792868221372e-324F64
# 45 "./torture/floatn-tg-3.h"
                                , negtrue_min = -4.94065645841246544176568792868221372e-324F64
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
# 12 "./torture/float64-tg-3.c" 2
