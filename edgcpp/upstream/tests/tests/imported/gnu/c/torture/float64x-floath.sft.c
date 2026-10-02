//type: rp
//options: 
# 0 "./torture/float64x-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64x-floath.c"
# 10 "./torture/float64x-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float64x a = 1.0f64x;
  for (int i = 0; i >= (-16381)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f64x;
  if (a != 3.36210314311209350626267781732175260e-4932F64x
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 64 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f64x;
  if (a != 3.64519953188247460252840593361941982e-4951F64x
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f64x;
  if (a != 0.0f64x)
    abort ();
  a = 1.08420217248550443400745280086994171e-19F64x
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 64 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f64x;
  if (a != 1.0f64x)
    abort ();
  a = 1.18973149535723176502126385303097021e+4932F64x
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 16384
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f64x;
  if (a != 1.0f64x - 1.08420217248550443400745280086994171e-19F64x 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f64x)
    abort ();
  exit (0);
}
# 11 "./torture/float64x-floath.c" 2
