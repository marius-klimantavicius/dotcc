//type: rp
//options: --c23
# 0 "./dfp/c2x-float-dfp-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c2x-float-dfp-5.c"




# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 6 "./dfp/c2x-float-dfp-5.c" 2





volatile _Decimal32 d = 
# 11 "./dfp/c2x-float-dfp-5.c" 3 4
                       (__builtin_nand32 (""))
# 11 "./dfp/c2x-float-dfp-5.c"
                              ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 19 "./dfp/c2x-float-dfp-5.c" 3 4
                  (__builtin_nand32 (""))
# 19 "./dfp/c2x-float-dfp-5.c"
                         , _Decimal32 : 0);
  if (!__builtin_isnan (
# 20 "./dfp/c2x-float-dfp-5.c" 3 4
                       (__builtin_nand32 (""))
# 20 "./dfp/c2x-float-dfp-5.c"
                              ))
    abort ();
  if (!__builtin_isnan (d))
    abort ();
  exit (0);
}
