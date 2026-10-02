//type: rp
//options: --c23
# 0 "./c2x-float-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-float-3.c"






# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 8 "./c2x-float-3.c" 2





volatile float f = 
# 13 "./c2x-float-3.c" 3 4
                  (__builtin_inff ())
# 13 "./c2x-float-3.c"
                          ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 21 "./c2x-float-3.c" 3 4
                  (__builtin_inff ())
# 21 "./c2x-float-3.c"
                          , float : 0);
  if (!(
# 22 "./c2x-float-3.c" 3 4
       (__builtin_inff ()) 
# 22 "./c2x-float-3.c"
                > 3.40282346638528859811704183484516925e+38F
# 22 "./c2x-float-3.c"
                         ))
    abort ();
  if (!(f > 3.40282346638528859811704183484516925e+38F
# 24 "./c2x-float-3.c"
                  ))
    abort ();
  exit (0);
}
