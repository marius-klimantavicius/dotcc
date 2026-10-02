//type: rp
//options: --c23
# 0 "./c2x-float-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-float-6.c"





# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 7 "./c2x-float-6.c" 2
# 23 "./c2x-float-6.c"
volatile float f = 
# 23 "./c2x-float-6.c" 3 4
                  (__builtin_nansf (""))
# 23 "./c2x-float-6.c"
                          ;
volatile double d = 
# 24 "./c2x-float-6.c" 3 4
                   (__builtin_nans (""))
# 24 "./c2x-float-6.c"
                           ;
volatile long double ld = 
# 25 "./c2x-float-6.c" 3 4
                         (__builtin_nansl (""))
# 25 "./c2x-float-6.c"
                                  ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 33 "./c2x-float-6.c" 3 4
                  (__builtin_nansf (""))
# 33 "./c2x-float-6.c"
                          , float : 0);
  (void) _Generic (
# 34 "./c2x-float-6.c" 3 4
                  (__builtin_nans (""))
# 34 "./c2x-float-6.c"
                          , double : 0);
  (void) _Generic (
# 35 "./c2x-float-6.c" 3 4
                  (__builtin_nansl (""))
# 35 "./c2x-float-6.c"
                           , long double : 0);
  if (!__builtin_isnan (
# 36 "./c2x-float-6.c" 3 4
                       (__builtin_nansf (""))
# 36 "./c2x-float-6.c"
                               ))
    abort ();
  if (!__builtin_isnan (f))
    abort ();
  if (!__builtin_isnan (
# 40 "./c2x-float-6.c" 3 4
                       (__builtin_nans (""))
# 40 "./c2x-float-6.c"
                               ))
    abort ();
  if (!__builtin_isnan (d))
    abort ();
  if (!__builtin_isnan (
# 44 "./c2x-float-6.c" 3 4
                       (__builtin_nansl (""))
# 44 "./c2x-float-6.c"
                                ))
    abort ();
  if (!__builtin_isnan (ld))
    abort ();
  exit (0);
}
