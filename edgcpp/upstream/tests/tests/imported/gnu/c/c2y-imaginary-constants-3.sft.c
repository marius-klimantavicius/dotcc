//type: rp
//options: 
# 0 "./c2y-imaginary-constants-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2y-imaginary-constants-3.c"
# 11 "./c2y-imaginary-constants-3.c"
# 1 "./c23-imaginary-constants-3.c" 1
# 11 "./c23-imaginary-constants-3.c"
_Complex _Float32 a = 1.if32;
_Complex _Float32 b = 2.F32j;
_Complex _Float32 c = 3.f32i;
_Complex _Float32 d = 4.JF32;
_Complex _Float64 e = 1.if64;
_Complex _Float64 f = 2.F64j;
_Complex _Float64 g = 3.f64i;
_Complex _Float64 h = 4.JF64;
_Complex _Float32x i = 1.if32x;
_Complex _Float32x j = 2.F32xj;
_Complex _Float32x k = 3.f32xI;
_Complex _Float32x l = 4.JF32x;
__extension__ _Complex _Float32 m = 1.if32;
__extension__ _Complex _Float32 n = 2.F32j;
__extension__ _Complex _Float32 o = 3.f32i;
__extension__ _Complex _Float32 p = 4.JF32;
__extension__ _Complex _Float64 q = 1.if64;
__extension__ _Complex _Float64 r = 2.F64j;
__extension__ _Complex _Float64 s = 3.f64i;
__extension__ _Complex _Float64 t = 4.JF64;
__extension__ _Complex _Float32x u = 1.if32x;
__extension__ _Complex _Float32x v = 2.F32xj;
__extension__ _Complex _Float32x w = 3.f32xI;
__extension__ _Complex _Float32x x = 4.JF32x;

int
main ()
{
  if (a * a != -1.f32
      || b * b != -4.f32
      || c * c != -9.f32
      || d * d != -16.f32
      || e * e != -1.f64
      || f * f != -4.f64
      || g * g != -9.f64
      || h * h != -16.f64
      || i * i != -1.f32x
      || j * j != -4.f32x
      || k * k != -9.f32x
      || l * l != -16.f32x
      || m * m != -1.f32
      || n * n != -4.f32
      || o * o != -9.f32
      || p * p != -16.f32
      || q * q != -1.f64
      || r * r != -4.f64
      || s * s != -9.f64
      || t * t != -16.f64
      || u * u != -1.f32x
      || v * v != -4.f32x
      || w * w != -9.f32x
      || x * x != -16.f32x)
    __builtin_abort ();
}
# 12 "./c2y-imaginary-constants-3.c" 2
