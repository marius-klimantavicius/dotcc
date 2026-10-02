//type: rp
//options: 
# 0 "./torture/float32x-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float32x-complex.c"
# 9 "./torture/float32x-complex.c"
# 1 "./torture/floatn-complex.h" 1
# 23 "./torture/floatn-complex.h"
extern void exit (int);
extern void abort (void);

volatile _Float32x a = 1.0f32x;
volatile _Complex _Float32x b = 2.0f32x + 3.0if32x;
volatile _Complex _Float32x c = 2.0f32x + 3.0F32xi;
volatile _Complex _Float32x d = __builtin_complex (2.0f32x, 3.0f32x);

_Complex _Float32x
fn (_Complex _Float32x arg)
{
  return arg / 4;
}

int
main (void)
{
  volatile _Complex _Float32x r;
  if (b != c)
    abort ();
  if (b != d)
    abort ();
  r = a + b;
  if (__real__ r != 3.0f32x || __imag__ r != 3.0f32x)
    abort ();
  r += d;
  if (__real__ r != 5.0f32x || __imag__ r != 6.0f32x)
    abort ();
  r -= a;
  if (__real__ r != 4.0f32x || __imag__ r != 6.0f32x)
    abort ();
  r /= (a + a);
  if (__real__ r != 2.0f32x || __imag__ r != 3.0f32x)
    abort ();
  r *= (a + a);
  if (__real__ r != 4.0f32x || __imag__ r != 6.0f32x)
    abort ();
  r -= b;
  if (__real__ r != 2.0f32x || __imag__ r != 3.0f32x)
    abort ();
  r *= r;
  if (__real__ r != -5.0f32x || __imag__ r != 12.0f32x)
    abort ();

  r /= b;
  r += __builtin_complex (100.0f32x, 100.0f32x);
  r -= __builtin_complex (100.0f32x, 100.0f32x);
  if (r != b)
    abort ();
  r = fn (r);
  if (__real__ r != 0.5f32x || __imag__ r != 0.75f32x)
    abort ();
  exit (0);
}
# 10 "./torture/float32x-complex.c" 2
