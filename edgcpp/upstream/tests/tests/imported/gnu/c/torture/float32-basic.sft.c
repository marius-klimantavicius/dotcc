//type: rp
//options: 
# 0 "./torture/float32-basic.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32-basic.c"
# 9 "./torture/float32-basic.c"
# 1 "./torture/floatn-basic.h" 1




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./torture/floatn-basic.h" 2
# 24 "./torture/floatn-basic.h"

# 24 "./torture/floatn-basic.h"
extern void exit (int);
extern void abort (void);

volatile _Float32 a = 1.0f32, b = 2.5F32, c = -2.5f32;
volatile _Float32 a2 = 1.0f32, z = 0.0f32, nz = -0.0f32;



_Float32
vafn (_Float32 arg1, ...)
{
  va_list ap;
  _Float32 ret;
  
# 37 "./torture/floatn-basic.h" 3 4
 __builtin_c23_va_start(
# 37 "./torture/floatn-basic.h"
 ap, arg1
# 37 "./torture/floatn-basic.h" 3 4
 )
# 37 "./torture/floatn-basic.h"
                    ;
  ret = arg1 + 
# 38 "./torture/floatn-basic.h" 3 4
              __builtin_va_arg(
# 38 "./torture/floatn-basic.h"
              ap
# 38 "./torture/floatn-basic.h" 3 4
              ,
# 38 "./torture/floatn-basic.h"
              _Float32
# 38 "./torture/floatn-basic.h" 3 4
              )
# 38 "./torture/floatn-basic.h"
                               ;
  
# 39 "./torture/floatn-basic.h" 3 4
 __builtin_va_end(
# 39 "./torture/floatn-basic.h"
 ap
# 39 "./torture/floatn-basic.h" 3 4
 )
# 39 "./torture/floatn-basic.h"
            ;
  return ret;
}

_Float32
krfn (arg)
     _Float32 arg;
{
  return arg + 1;
}

_Float32 krprofn (_Float32);
_Float32
krprofn (arg)
     _Float32 arg;
{
  return arg * 3;
}

_Float32
profn (_Float32 arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Float32 r;
  r = -b;
  if (r != c)
    abort ();
  r = a + b;
  if (r != 3.5f32)
    abort ();
  r = a - b;
  if (r != -1.5f32)
    abort ();
  r = 2 * c;
  if (r != -5)
    abort ();
  r = b * c;
  if (r != -6.25f32)
    abort ();
  r = b / (a + a);
  if (r != 1.25f32)
    abort ();
  r = c * 3;
  if (r != -7.5f32)
    abort ();
  volatile int i = r;
  if (i != -7)
    abort ();
  r = vafn (a, c);
  if (r != -1.5f32)
    abort ();
  r = krfn (b);
  if (r != 3.5f32)
    abort ();
  r = krprofn (a);
  if (r != 3.0f32)
    abort ();
  r = profn (a);
  if (r != 0.25f32)
    abort ();
  if ((a < b) != 1)
    abort ();
  if ((b < a) != 0)
    abort ();
  if ((a < a2) != 0)
    abort ();
  if ((nz < z) != 0)
    abort ();
  if ((a <= b) != 1)
    abort ();
  if ((b <= a) != 0)
    abort ();
  if ((a <= a2) != 1)
    abort ();
  if ((nz <= z) != 1)
    abort ();
  if ((a > b) != 0)
    abort ();
  if ((b > a) != 1)
    abort ();
  if ((a > a2) != 0)
    abort ();
  if ((nz > z) != 0)
    abort ();
  if ((a >= b) != 0)
    abort ();
  if ((b >= a) != 1)
    abort ();
  if ((a >= a2) != 1)
    abort ();
  if ((nz >= z) != 1)
    abort ();
  i = (nz == z);
  if (i != 1)
    abort ();
  i = (a == b);
  if (i != 0)
    abort ();
  exit (0);
}
# 10 "./torture/float32-basic.c" 2
