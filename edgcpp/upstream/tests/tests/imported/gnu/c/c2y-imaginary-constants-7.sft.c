//type: rp
//options: 
# 0 "./c2y-imaginary-constants-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2y-imaginary-constants-7.c"






# 1 "./c23-imaginary-constants-7.c" 1






_Complex _Float16 a = 1.if16;
_Complex _Float16 b = 2.F16j;
_Complex _Float16 c = 3.f16i;
_Complex _Float16 d = 4.JF16;
__extension__ _Complex _Float16 e = 1.if16;
__extension__ _Complex _Float16 f = 2.F16j;
__extension__ _Complex _Float16 g = 3.f16i;
__extension__ _Complex _Float16 h = 4.JF16;

int
main ()
{
  if (a * a != -1.f16
      || b * b != -4.f16
      || c * c != -9.f16
      || d * d != -16.f16
      || e * e != -1.f16
      || f * f != -4.f16
      || g * g != -9.f16
      || h * h != -16.f16)
    __builtin_abort ();
}
# 8 "./c2y-imaginary-constants-7.c" 2
