//type: rp
//options: 
# 0 "./torture/float128-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128-complex.c"
# 9 "./torture/float128-complex.c"
# 1 "./torture/floatn-complex.h" 1
# 23 "./torture/floatn-complex.h"
extern void exit (int);
extern void abort (void);

volatile _Float128 a = 1.0f128;
volatile _Complex _Float128 b = 2.0f128 + 3.0if128;
volatile _Complex _Float128 c = 2.0f128 + 3.0F128i;
volatile _Complex _Float128 d = __builtin_complex (2.0f128, 3.0f128);

_Complex _Float128
fn (_Complex _Float128 arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Complex _Float128 r;
  if (b != c)
    abort ();
  if (b != d)
    abort ();
  r = a + b;
  if (__real__ r != 3.0f128 || __imag__ r != 3.0f128)
    abort ();
  r += d;
  if (__real__ r != 5.0f128 || __imag__ r != 6.0f128)
    abort ();
  r -= a;
  if (__real__ r != 4.0f128 || __imag__ r != 6.0f128)
    abort ();
  r /= (a + a);
  if (__real__ r != 2.0f128 || __imag__ r != 3.0f128)
    abort ();
  r *= (a + a);
  if (__real__ r != 4.0f128 || __imag__ r != 6.0f128)
    abort ();
  r -= b;
  if (__real__ r != 2.0f128 || __imag__ r != 3.0f128)
    abort ();
  r *= r;
  if (__real__ r != -5.0f128 || __imag__ r != 12.0f128)
    abort ();

  r /= b;
  r += __builtin_complex (100.0f128, 100.0f128);
  r -= __builtin_complex (100.0f128, 100.0f128);
  if (r != b)
    abort ();
  r = fn (r);
  if (__real__ r != 0.5f128 || __imag__ r != 0.75f128)
    abort ();
  exit (0);
}
# 10 "./torture/float128-complex.c" 2
