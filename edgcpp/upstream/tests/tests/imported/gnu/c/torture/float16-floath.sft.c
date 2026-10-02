//type: rp
//options: 
# 0 "./torture/float16-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float16-floath.c"
# 10 "./torture/float16-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float16 a = 1.0f16;
  for (int i = 0; i >= (-13)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f16;
  if (a != 6.10351562500000000000000000000000000e-5F16
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 11 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f16;
  if (a != 5.96046447753906250000000000000000000e-8F16
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f16;
  if (a != 0.0f16)
    abort ();
  a = 9.76562500000000000000000000000000000e-4F16
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 11 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f16;
  if (a != 1.0f16)
    abort ();
  a = 6.55040000000000000000000000000000000e+4F16
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 16
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f16;
  if (a != 1.0f16 - 9.76562500000000000000000000000000000e-4F16 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f16)
    abort ();
  exit (0);
}
# 11 "./torture/float16-floath.c" 2
