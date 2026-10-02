//type: rp
//options: --c23
# 0 "./dfp/c2x-float-dfp-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c2x-float-dfp-7.c"




# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 6 "./dfp/c2x-float-dfp-7.c" 2
# 19 "./dfp/c2x-float-dfp-7.c"
volatile _Decimal32 d32 = 
# 19 "./dfp/c2x-float-dfp-7.c" 3 4
                         (__builtin_nansd32 (""))
# 19 "./dfp/c2x-float-dfp-7.c"
                                   ;
volatile _Decimal64 d64 = 
# 20 "./dfp/c2x-float-dfp-7.c" 3 4
                         (__builtin_nansd64 (""))
# 20 "./dfp/c2x-float-dfp-7.c"
                                   ;
volatile _Decimal128 d128 = 
# 21 "./dfp/c2x-float-dfp-7.c" 3 4
                           (__builtin_nansd128 (""))
# 21 "./dfp/c2x-float-dfp-7.c"
                                      ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 29 "./dfp/c2x-float-dfp-7.c" 3 4
                  (__builtin_nansd32 (""))
# 29 "./dfp/c2x-float-dfp-7.c"
                            , _Decimal32 : 0);
  if (!__builtin_isnan (
# 30 "./dfp/c2x-float-dfp-7.c" 3 4
                       (__builtin_nansd32 (""))
# 30 "./dfp/c2x-float-dfp-7.c"
                                 ))
    abort ();
  if (!__builtin_isnan (d32))
    abort ();
  (void) _Generic (
# 34 "./dfp/c2x-float-dfp-7.c" 3 4
                  (__builtin_nansd64 (""))
# 34 "./dfp/c2x-float-dfp-7.c"
                            , _Decimal64 : 0);
  if (!__builtin_isnan (
# 35 "./dfp/c2x-float-dfp-7.c" 3 4
                       (__builtin_nansd64 (""))
# 35 "./dfp/c2x-float-dfp-7.c"
                                 ))
    abort ();
  if (!__builtin_isnan (d64))
    abort ();
  (void) _Generic (
# 39 "./dfp/c2x-float-dfp-7.c" 3 4
                  (__builtin_nansd128 (""))
# 39 "./dfp/c2x-float-dfp-7.c"
                             , _Decimal128 : 0);
  if (!__builtin_isnan (
# 40 "./dfp/c2x-float-dfp-7.c" 3 4
                       (__builtin_nansd128 (""))
# 40 "./dfp/c2x-float-dfp-7.c"
                                  ))
    abort ();
  if (!__builtin_isnan (d128))
    abort ();
  exit (0);
}
