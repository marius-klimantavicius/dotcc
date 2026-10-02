//type: rp
//options: 
# 0 "./torture/float32x-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32x-floath.c"
# 10 "./torture/float32x-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float32x a = 1.0f32x;
  for (int i = 0; i >= (-1021)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f32x;
  if (a != 2.22507385850720138309023271733240406e-308F32x
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 53 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f32x;
  if (a != 4.94065645841246544176568792868221372e-324F32x
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f32x;
  if (a != 0.0f32x)
    abort ();
  a = 2.22044604925031308084726333618164062e-16F32x
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 53 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f32x;
  if (a != 1.0f32x)
    abort ();
  a = 1.79769313486231570814527423731704357e+308F32x
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 1024
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f32x;
  if (a != 1.0f32x - 2.22044604925031308084726333618164062e-16F32x 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f32x)
    abort ();
  exit (0);
}
# 11 "./torture/float32x-floath.c" 2
