//type: rp
//options: 
# 0 "./torture/float64-floath.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64-floath.c"
# 10 "./torture/float64-floath.c"
# 1 "./torture/floatn-floath.h" 1





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/floatn-floath.h" 2
# 23 "./torture/floatn-floath.h"
extern void exit (int);
extern void abort (void);

int
main (void)
{
  volatile _Float64 a = 1.0f64;
  for (int i = 0; i >= (-1021)
# 30 "./torture/floatn-floath.h"
                                    ; i--)
    a *= 0.5f64;
  if (a != 2.22507385850720138309023271733240406e-308F64
# 32 "./torture/floatn-floath.h"
                    )
    abort ();
  for (int i = 0; i < 53 
# 34 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 0.5f64;
  if (a != 4.94065645841246544176568792868221372e-324F64
# 36 "./torture/floatn-floath.h"
                         )
    abort ();
  a *= 0.5f64;
  if (a != 0.0f64)
    abort ();
  a = 2.22044604925031308084726333618164062e-16F64
# 41 "./torture/floatn-floath.h"
                   ;
  for (int i = 0; i < 53 
# 42 "./torture/floatn-floath.h"
                                     - 1; i++)
    a *= 2.0f64;
  if (a != 1.0f64)
    abort ();
  a = 1.79769313486231570814527423731704357e+308F64
# 46 "./torture/floatn-floath.h"
               ;
  for (int i = 0; i < 1024
# 47 "./torture/floatn-floath.h"
                                   ; i++)
    a *= 0.5f64;
  if (a != 1.0f64 - 2.22044604925031308084726333618164062e-16F64 
# 49 "./torture/floatn-floath.h"
                                     * 0.5f64)
    abort ();
  exit (0);
}
# 11 "./torture/float64-floath.c" 2
