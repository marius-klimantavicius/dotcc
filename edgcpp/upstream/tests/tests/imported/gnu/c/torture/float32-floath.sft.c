//type: rp
//options: 
# 0 "./torture/float32-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32-floath.c"
# 10 "./torture/float32-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float32 a = 1.0f32;
  for (int i = 0; i >= (-125)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f32;
  if (a != 1.17549435082228750796873653722224568e-38F32
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 24 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f32;
  if (a != 1.40129846432481707092372958328991613e-45F32
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f32;
  if (a != 0.0f32)
    abort ();
  a = 1.19209289550781250000000000000000000e-7F32
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 24 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f32;
  if (a != 1.0f32)
    abort ();
  a = 3.40282346638528859811704183484516925e+38F32
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 128
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f32;
  if (a != 1.0f32 - 1.19209289550781250000000000000000000e-7F32 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f32)
    abort ();
  exit (0);
}
# 11 "./torture/float32-floath.c" 2
