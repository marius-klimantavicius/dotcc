//type: rp
//options: 
# 0 "./torture/float128x-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128x-complex.c"
# 9 "./torture/float128x-complex.c"
# 1 "./torture/floatn-complex.h" 1
# 23 "./torture/floatn-complex.h"
extern void exit (int);
extern void abort (void);

volatile _Float128x a = 1.0f128x;
volatile _Complex _Float128x b = 2.0f128x + 3.0if128x;
volatile _Complex _Float128x c = 2.0f128x + 3.0F128xi;
volatile _Complex _Float128x d = __builtin_complex (2.0f128x, 3.0f128x);

_Complex _Float128x
fn (_Complex _Float128x arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Complex _Float128x r;
  if (b != c)
    abort ();
  if (b != d)
    abort ();
  r = a + b;
  if (__real__ r != 3.0f128x || __imag__ r != 3.0f128x)
    abort ();
  r += d;
  if (__real__ r != 5.0f128x || __imag__ r != 6.0f128x)
    abort ();
  r -= a;
  if (__real__ r != 4.0f128x || __imag__ r != 6.0f128x)
    abort ();
  r /= (a + a);
  if (__real__ r != 2.0f128x || __imag__ r != 3.0f128x)
    abort ();
  r *= (a + a);
  if (__real__ r != 4.0f128x || __imag__ r != 6.0f128x)
    abort ();
  r -= b;
  if (__real__ r != 2.0f128x || __imag__ r != 3.0f128x)
    abort ();
  r *= r;
  if (__real__ r != -5.0f128x || __imag__ r != 12.0f128x)
    abort ();

  r /= b;
  r += __builtin_complex (100.0f128x, 100.0f128x);
  r -= __builtin_complex (100.0f128x, 100.0f128x);
  if (r != b)
    abort ();
  r = fn (r);
  if (__real__ r != 0.5f128x || __imag__ r != 0.75f128x)
    abort ();
  exit (0);
}
# 10 "./torture/float128x-complex.c" 2
