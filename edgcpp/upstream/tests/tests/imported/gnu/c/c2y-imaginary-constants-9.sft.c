//type: rp
//options: 
# 0 "./c2y-imaginary-constants-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2y-imaginary-constants-9.c"






# 1 "./c23-imaginary-constants-9.c" 1






_Complex _Float64x a = 1.if64x;
_Complex _Float64x b = 2.F64xj;
_Complex _Float64x c = 3.f64xi;
_Complex _Float64x d = 4.JF64x;
__extension__ _Complex _Float64x e = 1.if64x;
__extension__ _Complex _Float64x f = 2.F64xj;
__extension__ _Complex _Float64x g = 3.f64xi;
__extension__ _Complex _Float64x h = 4.JF64x;

int
main ()
{
  if (a * a != -1.f64x
      || b * b != -4.f64x
      || c * c != -9.f64x
      || d * d != -16.f64x
      || e * e != -1.f64x
      || f * f != -4.f64x
      || g * g != -9.f64x
      || h * h != -16.f64x)
    __builtin_abort ();
}
# 8 "./c2y-imaginary-constants-9.c" 2
