//type: rp
//options: --c23
# 0 "./c23-float-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-float-6.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./c23-float-6.c" 2
# 23 "./c23-float-6.c"
volatile float f = 
# 23 "./c23-float-6.c" 3 4
                  (__builtin_nansf (""))
# 23 "./c23-float-6.c"
                          ;
volatile double d = 
# 24 "./c23-float-6.c" 3 4
                   (__builtin_nans (""))
# 24 "./c23-float-6.c"
                           ;
volatile long double ld = 
# 25 "./c23-float-6.c" 3 4
                         (__builtin_nansl (""))
# 25 "./c23-float-6.c"
                                  ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 33 "./c23-float-6.c" 3 4
                  (__builtin_nansf (""))
# 33 "./c23-float-6.c"
                          , float : 0);
  (void) _Generic (
# 34 "./c23-float-6.c" 3 4
                  (__builtin_nans (""))
# 34 "./c23-float-6.c"
                          , double : 0);
  (void) _Generic (
# 35 "./c23-float-6.c" 3 4
                  (__builtin_nansl (""))
# 35 "./c23-float-6.c"
                           , long double : 0);
  if (!__builtin_isnan (
# 36 "./c23-float-6.c" 3 4
                       (__builtin_nansf (""))
# 36 "./c23-float-6.c"
                               ))
    abort ();
  if (!__builtin_isnan (f))
    abort ();
  if (!__builtin_isnan (
# 40 "./c23-float-6.c" 3 4
                       (__builtin_nans (""))
# 40 "./c23-float-6.c"
                               ))
    abort ();
  if (!__builtin_isnan (d))
    abort ();
  if (!__builtin_isnan (
# 44 "./c23-float-6.c" 3 4
                       (__builtin_nansl (""))
# 44 "./c23-float-6.c"
                                ))
    abort ();
  if (!__builtin_isnan (ld))
    abort ();
  exit (0);
}
