//type: rp
//options: 
# 0 "./torture/float16-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float16-complex.c"
# 9 "./torture/float16-complex.c"
# 1 "./torture/floatn-complex.h" 1
# 23 "./torture/floatn-complex.h"
extern void exit (int);
extern void abort (void);

volatile _Float16 a = 1.0f16;
volatile _Complex _Float16 b = 2.0f16 + 3.0if16;
volatile _Complex _Float16 c = 2.0f16 + 3.0F16i;
volatile _Complex _Float16 d = __builtin_complex (2.0f16, 3.0f16);

_Complex _Float16
fn (_Complex _Float16 arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Complex _Float16 r;
  if (b != c)
    abort ();
  if (b != d)
    abort ();
  r = a + b;
  if (__real__ r != 3.0f16 || __imag__ r != 3.0f16)
    abort ();
  r += d;
  if (__real__ r != 5.0f16 || __imag__ r != 6.0f16)
    abort ();
  r -= a;
  if (__real__ r != 4.0f16 || __imag__ r != 6.0f16)
    abort ();
  r /= (a + a);
  if (__real__ r != 2.0f16 || __imag__ r != 3.0f16)
    abort ();
  r *= (a + a);
  if (__real__ r != 4.0f16 || __imag__ r != 6.0f16)
    abort ();
  r -= b;
  if (__real__ r != 2.0f16 || __imag__ r != 3.0f16)
    abort ();
  r *= r;
  if (__real__ r != -5.0f16 || __imag__ r != 12.0f16)
    abort ();

  r /= b;
  r += __builtin_complex (100.0f16, 100.0f16);
  r -= __builtin_complex (100.0f16, 100.0f16);
  if (r != b)
    abort ();
  r = fn (r);
  if (__real__ r != 0.5f16 || __imag__ r != 0.75f16)
    abort ();
  exit (0);
}
# 10 "./torture/float16-complex.c" 2
