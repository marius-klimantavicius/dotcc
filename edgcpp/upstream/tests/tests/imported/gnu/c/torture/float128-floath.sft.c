//type: rp
//options: 
# 0 "./torture/float128-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128-floath.c"
# 10 "./torture/float128-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float128 a = 1.0f128;
  for (int i = 0; i >= (-16381)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f128;
  if (a != 3.36210314311209350626267781732175260e-4932F128
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 113 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f128;
  if (a != 6.47517511943802511092443895822764655e-4966F128
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f128;
  if (a != 0.0f128)
    abort ();
  a = 1.92592994438723585305597794258492732e-34F128
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 113 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f128;
  if (a != 1.0f128)
    abort ();
  a = 1.18973149535723176508575932662800702e+4932F128
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 16384
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f128;
  if (a != 1.0f128 - 1.92592994438723585305597794258492732e-34F128 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f128)
    abort ();
  exit (0);
}
# 11 "./torture/float128-floath.c" 2
