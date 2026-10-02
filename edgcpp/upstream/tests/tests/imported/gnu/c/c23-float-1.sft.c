//type: rp
//options: --c23
# 0 "./c23-float-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-float-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 6 "./c23-float-1.c" 2
# 19 "./c23-float-1.c"
extern void abort (void);
extern void exit (int);

int
main (void)
{
  if (3.40282346638528859811704183484516925e+38F 
# 25 "./c23-float-1.c"
                  != 3.40282346638528859811704183484516925e+38F
# 25 "./c23-float-1.c"
                            )
    abort ();
  if (((double)1.79769313486231570814527423731704357e+308L) 
# 27 "./c23-float-1.c"
                  != ((double)1.79769313486231570814527423731704357e+308L)
# 27 "./c23-float-1.c"
                            )
    abort ();




  if (1.18973149535723176502126385303097021e+4932L 
# 33 "./c23-float-1.c"
                   != 1.18973149535723176502126385303097021e+4932L
# 33 "./c23-float-1.c"
                              )
    abort ();

  exit (0);
}
