//type: rp
//options: 
# 0 "./torture/floatn-convert.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/floatn-convert.c"
# 15 "./torture/floatn-convert.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 16 "./torture/floatn-convert.c" 2
# 62 "./torture/floatn-convert.c"
extern void exit (int);
extern void abort (void);
# 93 "./torture/floatn-convert.c"
int
main (void)
{
  do { do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float16 a = (_Float16) 1 + 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 96 "./torture/floatn-convert.c"
 < 11
# 96 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 9.76562500000000000000000000000000000e-4F16
# 96 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 97 "./torture/floatn-convert.c"
 < 24
# 97 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.19209289550781250000000000000000000e-7F32
# 97 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64 a = (_Float64) 1 + 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 98 "./torture/floatn-convert.c"
 < 53
# 98 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 2.22044604925031308084726333618164062e-16F64
# 98 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float128 a = (_Float128) 1 + 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 99 "./torture/floatn-convert.c"
 < 113
# 99 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.92592994438723585305597794258492732e-34F128
# 99 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32x a = (_Float32x) 1 + 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 100 "./torture/floatn-convert.c"
 < 53
# 100 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 2.22044604925031308084726333618164062e-16F32x
# 100 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float64x a = (_Float64x) 1 + 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 101 "./torture/floatn-convert.c"
 < 64
# 101 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.08420217248550443400745280086994171e-19F64x
# 101 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  do { do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float16 b = (_Float16) a; volatile _Float16 expected; if (11 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float16) 1; else expected = (_Float16) 1 + (_Float16) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float64 b = (_Float64) a; volatile _Float64 expected; if (53 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float64) 1; else expected = (_Float64) 1 + (_Float64) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float128 b = (_Float128) a; volatile _Float128 expected; if (113 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float128) 1; else expected = (_Float128) 1 + (_Float128) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float32x b = (_Float32x) a; volatile _Float32x expected; if (53 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float32x) 1; else expected = (_Float32x) 1 + (_Float32x) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float64x b = (_Float64x) a; volatile _Float64x expected; if (64 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float64x) 1; else expected = (_Float64x) 1 + (_Float64x) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); do { volatile _Float32 a = (_Float32) 1 + 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; volatile _Float32 b = (_Float32) a; volatile _Float32 expected; if (24 
# 102 "./torture/floatn-convert.c"
 < 24
# 102 "./torture/floatn-convert.c"
 ) expected = (_Float32) 1; else expected = (_Float32) 1 + (_Float32) 1.19209289550781250000000000000000000e-7F32
# 102 "./torture/floatn-convert.c"
 ; if (b != expected) abort (); } while (0); } while (0);
  exit (0);
}
