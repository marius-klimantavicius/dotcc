//type: rp
//options: 
# 0 "./guality/pr58791-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr58791-4.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr58791-4.c" 2

__attribute__((noinline, noclone)) double
foo (float a, float b, float c, float d, float l, double u)
{
  float e = a * d;
  float f = d * e;
  double g = (double) f;
  double h = (double) b;
  double i = g * h;
  double i2 = i + 1.0;
  double j = i * 3.25;
  double k = h + j;
  float m = l * 8.75;
  double n = (double) m;
  double o = (double) a;
  double p = n * o;
  double q = h * p;
  double r = q * 2.5;
  double s = k - r;
  double t = (double) c;
  double v = o * u;
  double w = o * v;
  double x = h * w;
  double y = h * x;
  double z = y * 8.5;
  asm volatile ("nop" : : : "memory");
  asm volatile ("nop" : : : "memory");
  return s - z;
}

int
main ()
{
  foo (3.0f, 2.0f, -1.0f, 9.0f, 1.0f, 2.0);
  return 0;
}
