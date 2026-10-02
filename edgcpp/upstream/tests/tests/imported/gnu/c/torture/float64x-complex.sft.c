//type: rp
//options: 
# 0 "./torture/float64x-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64x-complex.c"
# 9 "./torture/float64x-complex.c"
# 1 "./torture/floatn-complex.h" 1
# 23 "./torture/floatn-complex.h"
extern void exit (int);
extern void abort (void);

volatile _Float64x a = 1.0f64x;
volatile _Complex _Float64x b = 2.0f64x + 3.0if64x;
volatile _Complex _Float64x c = 2.0f64x + 3.0F64xi;
volatile _Complex _Float64x d = __builtin_complex (2.0f64x, 3.0f64x);

_Complex _Float64x
fn (_Complex _Float64x arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Complex _Float64x r;
  if (b != c)
    abort ();
  if (b != d)
    abort ();
  r = a + b;
  if (__real__ r != 3.0f64x || __imag__ r != 3.0f64x)
    abort ();
  r += d;
  if (__real__ r != 5.0f64x || __imag__ r != 6.0f64x)
    abort ();
  r -= a;
  if (__real__ r != 4.0f64x || __imag__ r != 6.0f64x)
    abort ();
  r /= (a + a);
  if (__real__ r != 2.0f64x || __imag__ r != 3.0f64x)
    abort ();
  r *= (a + a);
  if (__real__ r != 4.0f64x || __imag__ r != 6.0f64x)
    abort ();
  r -= b;
  if (__real__ r != 2.0f64x || __imag__ r != 3.0f64x)
    abort ();
  r *= r;
  if (__real__ r != -5.0f64x || __imag__ r != 12.0f64x)
    abort ();

  r /= b;
  r += __builtin_complex (100.0f64x, 100.0f64x);
  r -= __builtin_complex (100.0f64x, 100.0f64x);
  if (r != b)
    abort ();
  r = fn (r);
  if (__real__ r != 0.5f64x || __imag__ r != 0.75f64x)
    abort ();
  exit (0);
}
# 10 "./torture/float64x-complex.c" 2
