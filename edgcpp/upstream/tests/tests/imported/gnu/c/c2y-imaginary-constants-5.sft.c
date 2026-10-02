//type: rp
//options: 
# 0 "./c2y-imaginary-constants-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2y-imaginary-constants-5.c"






# 1 "./c23-imaginary-constants-5.c" 1






_Complex _Float128 a = 1.if128;
_Complex _Float128 b = 2.F128j;
_Complex _Float128 c = 3.f128i;
_Complex _Float128 d = 4.JF128;
__extension__ _Complex _Float128 e = 1.if128;
__extension__ _Complex _Float128 f = 2.F128j;
__extension__ _Complex _Float128 g = 3.f128i;
__extension__ _Complex _Float128 h = 4.JF128;

int
main ()
{
  if (a * a != -1.f128
      || b * b != -4.f128
      || c * c != -9.f128
      || d * d != -16.f128
      || e * e != -1.f128
      || f * f != -4.f128
      || g * g != -9.f128
      || h * h != -16.f128)
    __builtin_abort ();
}
# 8 "./c2y-imaginary-constants-5.c" 2
