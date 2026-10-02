//type: fp
//options: --c23
# 0 "./dfp/c23-float-dfp-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c23-float-dfp-4.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 5 "./dfp/c23-float-dfp-4.c" 2





volatile _Decimal32 d = 
# 10 "./dfp/c23-float-dfp-4.c" 3 4
                       (__builtin_infd32 ())
# 10 "./dfp/c23-float-dfp-4.c"
                                   ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 18 "./dfp/c23-float-dfp-4.c" 3 4
                  (__builtin_infd32 ())
# 18 "./dfp/c23-float-dfp-4.c"
                              , _Decimal32 : 0);
  if (!(
# 19 "./dfp/c23-float-dfp-4.c" 3 4
       (__builtin_infd32 ()) 
# 19 "./dfp/c23-float-dfp-4.c"
                    > 9.999999E96DF
# 19 "./dfp/c23-float-dfp-4.c"
                               ))
    abort ();
  if (!(d > 9.999999E96DF
# 21 "./dfp/c23-float-dfp-4.c"
                    ))
    abort ();
  exit (0);
}
