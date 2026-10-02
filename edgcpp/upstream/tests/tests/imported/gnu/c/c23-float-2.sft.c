//type: rp
//options: --c23 -w
# 0 "./c23-float-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-float-2.c"






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./c23-float-2.c" 2





extern void abort (void);
extern void exit (int);

int
main (void)
{
  (void) _Generic (
# 19 "./c23-float-2.c" 3 4
                  (__builtin_inff ())
# 19 "./c23-float-2.c"
                          , float : 0);
  if (!(
# 20 "./c23-float-2.c" 3 4
       (__builtin_inff ()) 
# 20 "./c23-float-2.c"
                >= 3.40282346638528859811704183484516925e+38F
# 20 "./c23-float-2.c"
                          ))
    abort ();
  exit (0);
}
