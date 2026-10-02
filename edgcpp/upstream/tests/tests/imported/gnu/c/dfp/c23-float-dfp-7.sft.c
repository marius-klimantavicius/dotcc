//type: fp
//options: --c23
# 0 "./dfp/c23-float-dfp-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c23-float-dfp-7.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 5 "./dfp/c23-float-dfp-7.c" 2
# 18 "./dfp/c23-float-dfp-7.c"
volatile _Decimal32 d32 = 
# 18 "./dfp/c23-float-dfp-7.c" 3 4
                         (__builtin_nansd32 (""))
# 18 "./dfp/c23-float-dfp-7.c"
                                   ;
volatile _Decimal64 d64 = 
# 19 "./dfp/c23-float-dfp-7.c" 3 4
                         (__builtin_nansd64 (""))
# 19 "./dfp/c23-float-dfp-7.c"
                                   ;
volatile _Decimal128 d128 = 
# 20 "./dfp/c23-float-dfp-7.c" 3 4
                           (__builtin_nansd128 (""))
# 20 "./dfp/c23-float-dfp-7.c"
                                      ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 28 "./dfp/c23-float-dfp-7.c" 3 4
                  (__builtin_nansd32 (""))
# 28 "./dfp/c23-float-dfp-7.c"
                            , _Decimal32 : 0);
  if (!__builtin_isnan (
# 29 "./dfp/c23-float-dfp-7.c" 3 4
                       (__builtin_nansd32 (""))
# 29 "./dfp/c23-float-dfp-7.c"
                                 ))
    abort ();
  if (!__builtin_isnan (d32))
    abort ();
  (void) _Generic (
# 33 "./dfp/c23-float-dfp-7.c" 3 4
                  (__builtin_nansd64 (""))
# 33 "./dfp/c23-float-dfp-7.c"
                            , _Decimal64 : 0);
  if (!__builtin_isnan (
# 34 "./dfp/c23-float-dfp-7.c" 3 4
                       (__builtin_nansd64 (""))
# 34 "./dfp/c23-float-dfp-7.c"
                                 ))
    abort ();
  if (!__builtin_isnan (d64))
    abort ();
  (void) _Generic (
# 38 "./dfp/c23-float-dfp-7.c" 3 4
                  (__builtin_nansd128 (""))
# 38 "./dfp/c23-float-dfp-7.c"
                             , _Decimal128 : 0);
  if (!__builtin_isnan (
# 39 "./dfp/c23-float-dfp-7.c" 3 4
                       (__builtin_nansd128 (""))
# 39 "./dfp/c23-float-dfp-7.c"
                                  ))
    abort ();
  if (!__builtin_isnan (d128))
    abort ();
  exit (0);
}
