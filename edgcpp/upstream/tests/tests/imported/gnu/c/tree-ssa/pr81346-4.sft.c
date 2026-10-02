//type: rp
//options: 
# 0 "./tree-ssa/pr81346-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr81346-4.c"




# 1 "./tree-ssa/pr81346-3.c" 1





__attribute__((noinline, noclone)) int f00 (int x) { return x / 46340 > -46341; }
__attribute__((noinline, noclone)) int f01 (int x) { int a = 46340, b = -46341; return x / a > b; }
__attribute__((noinline, noclone)) int f02 (int x) { return x / 46340 >= 46341; }
__attribute__((noinline, noclone)) int f03 (int x) { int a = 46340, b = 46341; return x / a >= b; }
__attribute__((noinline, noclone)) int f04 (int x) { return x / 46340 < 46341; }
__attribute__((noinline, noclone)) int f05 (int x) { int a = 46340, b = 46341; return x / a < b; }
__attribute__((noinline, noclone)) int f06 (int x) { return x / 46340 <= -46341; }
__attribute__((noinline, noclone)) int f07 (int x) { int a = 46340, b = -46341; return x / a <= b; }
__attribute__((noinline, noclone)) int f08 (int x) { return x / 46340 == -46341; }
__attribute__((noinline, noclone)) int f09 (int x) { int a = 46340, b = -46341; return x / a == b; }
__attribute__((noinline, noclone)) int f10 (int x) { return x / 46340 == 46341; }
__attribute__((noinline, noclone)) int f11 (int x) { int a = 46340, b = 46341; return x / a == b; }
__attribute__((noinline, noclone)) int f12 (int x) { return x / 46340 != -46341; }
__attribute__((noinline, noclone)) int f13 (int x) { int a = 46340, b = -46341; return x / a != b; }
__attribute__((noinline, noclone)) int f14 (int x) { return x / 46340 != 46341; }
__attribute__((noinline, noclone)) int f15 (int x) { int a = 46340, b = 46341; return x / a != b; }
__attribute__((noinline, noclone)) int f16 (int x) { return x / 15 > -15; }
__attribute__((noinline, noclone)) int f17 (int x) { int a = 15, b = -15; return x / a > b; }
__attribute__((noinline, noclone)) int f18 (int x) { return x / 15 > 15; }
__attribute__((noinline, noclone)) int f19 (int x) { int a = 15, b = 15; return x / a > b; }
__attribute__((noinline, noclone)) int f20 (int x) { return x / 15 >= -15; }
__attribute__((noinline, noclone)) int f21 (int x) { int a = 15, b = -15; return x / a >= b; }
__attribute__((noinline, noclone)) int f22 (int x) { return x / 15 >= 15; }
__attribute__((noinline, noclone)) int f23 (int x) { int a = 15, b = 15; return x / a >= b; }
__attribute__((noinline, noclone)) int f24 (int x) { return x / 15 < -15; }
__attribute__((noinline, noclone)) int f25 (int x) { int a = 15, b = -15; return x / a < b; }
__attribute__((noinline, noclone)) int f26 (int x) { return x / 15 < 15; }
__attribute__((noinline, noclone)) int f27 (int x) { int a = 15, b = 15; return x / a < b; }
__attribute__((noinline, noclone)) int f28 (int x) { return x / 15 <= -15; }
__attribute__((noinline, noclone)) int f29 (int x) { int a = 15, b = -15; return x / a <= b; }
__attribute__((noinline, noclone)) int f30 (int x) { return x / 15 <= 15; }
__attribute__((noinline, noclone)) int f31 (int x) { int a = 15, b = 15; return x / a <= b; }
__attribute__((noinline, noclone)) int f32 (int x) { return x / 15 == -15; }
__attribute__((noinline, noclone)) int f33 (int x) { int a = 15, b = -15; return x / a == b; }
__attribute__((noinline, noclone)) int f34 (int x) { return x / 15 == 15; }
__attribute__((noinline, noclone)) int f35 (int x) { int a = 15, b = 15; return x / a == b; }
__attribute__((noinline, noclone)) int f36 (int x) { return x / 15 != -15; }
__attribute__((noinline, noclone)) int f37 (int x) { int a = 15, b = -15; return x / a != b; }
__attribute__((noinline, noclone)) int f38 (int x) { return x / 15 != 15; }
__attribute__((noinline, noclone)) int f39 (int x) { int a = 15, b = 15; return x / a != b; }
__attribute__((noinline, noclone)) int f40 (int x) { return x / -46340 > -46341; }
__attribute__((noinline, noclone)) int f41 (int x) { int a = -46340, b = -46341; return x / a > b; }
__attribute__((noinline, noclone)) int f42 (int x) { return x / -46340 >= 46341; }
__attribute__((noinline, noclone)) int f43 (int x) { int a = -46340, b = 46341; return x / a >= b; }
__attribute__((noinline, noclone)) int f44 (int x) { return x / -46340 < 46341; }
__attribute__((noinline, noclone)) int f45 (int x) { int a = -46340, b = 46341; return x / a < b; }
__attribute__((noinline, noclone)) int f46 (int x) { return x / -46340 <= -46341; }
__attribute__((noinline, noclone)) int f47 (int x) { int a = -46340, b = -46341; return x / a <= b; }
__attribute__((noinline, noclone)) int f48 (int x) { return x / -46340 == 46341; }
__attribute__((noinline, noclone)) int f49 (int x) { int a = -46340, b = 46341; return x / a == b; }
__attribute__((noinline, noclone)) int f50 (int x) { return x / -46340 == -46341; }
__attribute__((noinline, noclone)) int f51 (int x) { int a = -46340, b = -46341; return x / a == b; }
__attribute__((noinline, noclone)) int f52 (int x) { return x / -46340 != 46341; }
__attribute__((noinline, noclone)) int f53 (int x) { int a = -46340, b = 46341; return x / a != b; }
__attribute__((noinline, noclone)) int f54 (int x) { return x / -46340 != -46341; }
__attribute__((noinline, noclone)) int f55 (int x) { int a = -46340, b = -46341; return x / a != b; }
__attribute__((noinline, noclone)) int f56 (int x) { return x / -15 > 15; }
__attribute__((noinline, noclone)) int f57 (int x) { int a = -15, b = 15; return x / a > b; }
__attribute__((noinline, noclone)) int f58 (int x) { return x / -15 > -15; }
__attribute__((noinline, noclone)) int f59 (int x) { int a = -15, b = -15; return x / a > b; }
__attribute__((noinline, noclone)) int f60 (int x) { return x / -15 >= 15; }
__attribute__((noinline, noclone)) int f61 (int x) { int a = -15, b = 15; return x / a >= b; }
__attribute__((noinline, noclone)) int f62 (int x) { return x / -15 >= -15; }
__attribute__((noinline, noclone)) int f63 (int x) { int a = -15, b = -15; return x / a >= b; }
__attribute__((noinline, noclone)) int f64 (int x) { return x / -15 < 15; }
__attribute__((noinline, noclone)) int f65 (int x) { int a = -15, b = 15; return x / a < b; }
__attribute__((noinline, noclone)) int f66 (int x) { return x / -15 < -15; }
__attribute__((noinline, noclone)) int f67 (int x) { int a = -15, b = -15; return x / a < b; }
__attribute__((noinline, noclone)) int f68 (int x) { return x / -15 <= 15; }
__attribute__((noinline, noclone)) int f69 (int x) { int a = -15, b = 15; return x / a <= b; }
__attribute__((noinline, noclone)) int f70 (int x) { return x / -15 <= -15; }
__attribute__((noinline, noclone)) int f71 (int x) { int a = -15, b = -15; return x / a <= b; }
__attribute__((noinline, noclone)) int f72 (int x) { return x / -15 == 15; }
__attribute__((noinline, noclone)) int f73 (int x) { int a = -15, b = 15; return x / a == b; }
__attribute__((noinline, noclone)) int f74 (int x) { return x / -15 == -15; }
__attribute__((noinline, noclone)) int f75 (int x) { int a = -15, b = -15; return x / a == b; }
__attribute__((noinline, noclone)) int f76 (int x) { return x / -15 != 15; }
__attribute__((noinline, noclone)) int f77 (int x) { int a = -15, b = 15; return x / a != b; }
__attribute__((noinline, noclone)) int f78 (int x) { return x / -15 != -15; }
__attribute__((noinline, noclone)) int f79 (int x) { int a = -15, b = -15; return x / a != b; }
__attribute__((noinline, noclone)) int f80 (int x) { return x / -15 > 0; }
__attribute__((noinline, noclone)) int f81 (int x) { int a = -15, b = 0; return x / a > b; }
__attribute__((noinline, noclone)) int f82 (int x) { return x / 15 > 0; }
__attribute__((noinline, noclone)) int f83 (int x) { int a = 15, b = 0; return x / a > b; }
__attribute__((noinline, noclone)) int f84 (int x) { return x / -15 >= 0; }
__attribute__((noinline, noclone)) int f85 (int x) { int a = -15, b = 0; return x / a >= b; }
__attribute__((noinline, noclone)) int f86 (int x) { return x / 15 >= 0; }
__attribute__((noinline, noclone)) int f87 (int x) { int a = 15, b = 0; return x / a >= b; }
__attribute__((noinline, noclone)) int f88 (int x) { return x / -15 < 0; }
__attribute__((noinline, noclone)) int f89 (int x) { int a = -15, b = 0; return x / a < b; }
__attribute__((noinline, noclone)) int f90 (int x) { return x / 15 < 0; }
__attribute__((noinline, noclone)) int f91 (int x) { int a = 15, b = 0; return x / a < b; }
__attribute__((noinline, noclone)) int f92 (int x) { return x / -15 <= 0; }
__attribute__((noinline, noclone)) int f93 (int x) { int a = -15, b = 0; return x / a <= b; }
__attribute__((noinline, noclone)) int f94 (int x) { return x / 15 <= 0; }
__attribute__((noinline, noclone)) int f95 (int x) { int a = 15, b = 0; return x / a <= b; }
__attribute__((noinline, noclone)) int f96 (int x) { return x / -15 == 0; }
__attribute__((noinline, noclone)) int f97 (int x) { int a = -15, b = 0; return x / a == b; }
__attribute__((noinline, noclone)) int f98 (int x) { return x / 15 == 0; }
__attribute__((noinline, noclone)) int f99 (int x) { int a = 15, b = 0; return x / a == b; }
__attribute__((noinline, noclone)) int f100 (int x) { return x / -15 != 0; }
__attribute__((noinline, noclone)) int f101 (int x) { int a = -15, b = 0; return x / a != b; }
__attribute__((noinline, noclone)) int f102 (int x) { return x / 15 != 0; }
__attribute__((noinline, noclone)) int f103 (int x) { int a = 15, b = 0; return x / a != b; }
# 6 "./tree-ssa/pr81346-4.c" 2




extern void abort (void);

int
main ()
{
  if (8 != 8 || 4 != 4)
    return 0;
# 26 "./tree-ssa/pr81346-4.c"
do { int w1 = -2147441939; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f00 (w1 - 1) != !in || f01 (w1 - 1) != !in) abort (); } if (f00 (w1) != in || f01 (w1) != in) abort (); if (f00 (w2) != in || f01 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f00 (w2 + 1) != !in || f01 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 2147441940; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f02 (w1 - 1) != !in || f03 (w1 - 1) != !in) abort (); } if (f02 (w1) != in || f03 (w1) != in) abort (); if (f02 (w2) != in || f03 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f02 (w2 + 1) != !in || f03 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 2147441939; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f04 (w1 - 1) != !in || f05 (w1 - 1) != !in) abort (); } if (f04 (w1) != in || f05 (w1) != in) abort (); if (f04 (w2) != in || f05 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f04 (w2 + 1) != !in || f05 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -2147441940; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f06 (w1 - 1) != !in || f07 (w1 - 1) != !in) abort (); } if (f06 (w1) != in || f07 (w1) != in) abort (); if (f06 (w2) != in || f07 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f06 (w2 + 1) != !in || f07 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -2147441940; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f08 (w1 - 1) != !in || f09 (w1 - 1) != !in) abort (); } if (f08 (w1) != in || f09 (w1) != in) abort (); if (f08 (w2) != in || f09 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f08 (w2 + 1) != !in || f09 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 2147441940; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f10 (w1 - 1) != !in || f11 (w1 - 1) != !in) abort (); } if (f10 (w1) != in || f11 (w1) != in) abort (); if (f10 (w2) != in || f11 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f10 (w2 + 1) != !in || f11 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -2147441939; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f12 (w1 - 1) != !in || f13 (w1 - 1) != !in) abort (); } if (f12 (w1) != in || f13 (w1) != in) abort (); if (f12 (w2) != in || f13 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f12 (w2 + 1) != !in || f13 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 2147441939; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f14 (w1 - 1) != !in || f15 (w1 - 1) != !in) abort (); } if (f14 (w1) != in || f15 (w1) != in) abort (); if (f14 (w2) != in || f15 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f14 (w2 + 1) != !in || f15 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -224; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f16 (w1 - 1) != !in || f17 (w1 - 1) != !in) abort (); } if (f16 (w1) != in || f17 (w1) != in) abort (); if (f16 (w2) != in || f17 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f16 (w2 + 1) != !in || f17 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 240; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f18 (w1 - 1) != !in || f19 (w1 - 1) != !in) abort (); } if (f18 (w1) != in || f19 (w1) != in) abort (); if (f18 (w2) != in || f19 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f18 (w2 + 1) != !in || f19 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -239; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f20 (w1 - 1) != !in || f21 (w1 - 1) != !in) abort (); } if (f20 (w1) != in || f21 (w1) != in) abort (); if (f20 (w2) != in || f21 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f20 (w2 + 1) != !in || f21 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 225; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f22 (w1 - 1) != !in || f23 (w1 - 1) != !in) abort (); } if (f22 (w1) != in || f23 (w1) != in) abort (); if (f22 (w2) != in || f23 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f22 (w2 + 1) != !in || f23 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -240; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f24 (w1 - 1) != !in || f25 (w1 - 1) != !in) abort (); } if (f24 (w1) != in || f25 (w1) != in) abort (); if (f24 (w2) != in || f25 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f24 (w2 + 1) != !in || f25 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 224; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f26 (w1 - 1) != !in || f27 (w1 - 1) != !in) abort (); } if (f26 (w1) != in || f27 (w1) != in) abort (); if (f26 (w2) != in || f27 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f26 (w2 + 1) != !in || f27 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f28 (w1 - 1) != !in || f29 (w1 - 1) != !in) abort (); } if (f28 (w1) != in || f29 (w1) != in) abort (); if (f28 (w2) != in || f29 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f28 (w2 + 1) != !in || f29 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f30 (w1 - 1) != !in || f31 (w1 - 1) != !in) abort (); } if (f30 (w1) != in || f31 (w1) != in) abort (); if (f30 (w2) != in || f31 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f30 (w2 + 1) != !in || f31 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -239; int w2 = -225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f32 (w1 - 1) != !in || f33 (w1 - 1) != !in) abort (); } if (f32 (w1) != in || f33 (w1) != in) abort (); if (f32 (w2) != in || f33 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f32 (w2 + 1) != !in || f33 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 225; int w2 = 239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f34 (w1 - 1) != !in || f35 (w1 - 1) != !in) abort (); } if (f34 (w1) != in || f35 (w1) != in) abort (); if (f34 (w2) != in || f35 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f34 (w2 + 1) != !in || f35 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -225; int w2 = -239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f36 (w1 - 1) != !in || f37 (w1 - 1) != !in) abort (); } if (f36 (w1) != in || f37 (w1) != in) abort (); if (f36 (w2) != in || f37 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f36 (w2 + 1) != !in || f37 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 239; int w2 = 225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f38 (w1 - 1) != !in || f39 (w1 - 1) != !in) abort (); } if (f38 (w1) != in || f39 (w1) != in) abort (); if (f38 (w2) != in || f39 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f38 (w2 + 1) != !in || f39 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 2147441939; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f40 (w1 - 1) != !in || f41 (w1 - 1) != !in) abort (); } if (f40 (w1) != in || f41 (w1) != in) abort (); if (f40 (w2) != in || f41 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f40 (w2 + 1) != !in || f41 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -2147441940; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f42 (w1 - 1) != !in || f43 (w1 - 1) != !in) abort (); } if (f42 (w1) != in || f43 (w1) != in) abort (); if (f42 (w2) != in || f43 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f42 (w2 + 1) != !in || f43 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -2147441939; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f44 (w1 - 1) != !in || f45 (w1 - 1) != !in) abort (); } if (f44 (w1) != in || f45 (w1) != in) abort (); if (f44 (w2) != in || f45 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f44 (w2 + 1) != !in || f45 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 2147441940; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f46 (w1 - 1) != !in || f47 (w1 - 1) != !in) abort (); } if (f46 (w1) != in || f47 (w1) != in) abort (); if (f46 (w2) != in || f47 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f46 (w2 + 1) != !in || f47 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -2147441940; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f48 (w1 - 1) != !in || f49 (w1 - 1) != !in) abort (); } if (f48 (w1) != in || f49 (w1) != in) abort (); if (f48 (w2) != in || f49 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f48 (w2 + 1) != !in || f49 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 2147441940; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f50 (w1 - 1) != !in || f51 (w1 - 1) != !in) abort (); } if (f50 (w1) != in || f51 (w1) != in) abort (); if (f50 (w2) != in || f51 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f50 (w2 + 1) != !in || f51 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -2147441939; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f52 (w1 - 1) != !in || f53 (w1 - 1) != !in) abort (); } if (f52 (w1) != in || f53 (w1) != in) abort (); if (f52 (w2) != in || f53 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f52 (w2 + 1) != !in || f53 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 2147441939; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f54 (w1 - 1) != !in || f55 (w1 - 1) != !in) abort (); } if (f54 (w1) != in || f55 (w1) != in) abort (); if (f54 (w2) != in || f55 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f54 (w2 + 1) != !in || f55 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -240; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f56 (w1 - 1) != !in || f57 (w1 - 1) != !in) abort (); } if (f56 (w1) != in || f57 (w1) != in) abort (); if (f56 (w2) != in || f57 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f56 (w2 + 1) != !in || f57 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 224; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f58 (w1 - 1) != !in || f59 (w1 - 1) != !in) abort (); } if (f58 (w1) != in || f59 (w1) != in) abort (); if (f58 (w2) != in || f59 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f58 (w2 + 1) != !in || f59 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f60 (w1 - 1) != !in || f61 (w1 - 1) != !in) abort (); } if (f60 (w1) != in || f61 (w1) != in) abort (); if (f60 (w2) != in || f61 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f60 (w2 + 1) != !in || f61 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f62 (w1 - 1) != !in || f63 (w1 - 1) != !in) abort (); } if (f62 (w1) != in || f63 (w1) != in) abort (); if (f62 (w2) != in || f63 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f62 (w2 + 1) != !in || f63 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -224; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f64 (w1 - 1) != !in || f65 (w1 - 1) != !in) abort (); } if (f64 (w1) != in || f65 (w1) != in) abort (); if (f64 (w2) != in || f65 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f64 (w2 + 1) != !in || f65 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 240; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f66 (w1 - 1) != !in || f67 (w1 - 1) != !in) abort (); } if (f66 (w1) != in || f67 (w1) != in) abort (); if (f66 (w2) != in || f67 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f66 (w2 + 1) != !in || f67 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -239; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f68 (w1 - 1) != !in || f69 (w1 - 1) != !in) abort (); } if (f68 (w1) != in || f69 (w1) != in) abort (); if (f68 (w2) != in || f69 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f68 (w2 + 1) != !in || f69 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 225; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f70 (w1 - 1) != !in || f71 (w1 - 1) != !in) abort (); } if (f70 (w1) != in || f71 (w1) != in) abort (); if (f70 (w2) != in || f71 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f70 (w2 + 1) != !in || f71 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -239; int w2 = -225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f72 (w1 - 1) != !in || f73 (w1 - 1) != !in) abort (); } if (f72 (w1) != in || f73 (w1) != in) abort (); if (f72 (w2) != in || f73 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f72 (w2 + 1) != !in || f73 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 225; int w2 = 239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f74 (w1 - 1) != !in || f75 (w1 - 1) != !in) abort (); } if (f74 (w1) != in || f75 (w1) != in) abort (); if (f74 (w2) != in || f75 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f74 (w2 + 1) != !in || f75 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -225; int w2 = -239; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f76 (w1 - 1) != !in || f77 (w1 - 1) != !in) abort (); } if (f76 (w1) != in || f77 (w1) != in) abort (); if (f76 (w2) != in || f77 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f76 (w2 + 1) != !in || f77 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 239; int w2 = 225; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f78 (w1 - 1) != !in || f79 (w1 - 1) != !in) abort (); } if (f78 (w1) != in || f79 (w1) != in) abort (); if (f78 (w2) != in || f79 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f78 (w2 + 1) != !in || f79 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -15; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f80 (w1 - 1) != !in || f81 (w1 - 1) != !in) abort (); } if (f80 (w1) != in || f81 (w1) != in) abort (); if (f80 (w2) != in || f81 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f80 (w2 + 1) != !in || f81 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 15; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f82 (w1 - 1) != !in || f83 (w1 - 1) != !in) abort (); } if (f82 (w1) != in || f83 (w1) != in) abort (); if (f82 (w2) != in || f83 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f82 (w2 + 1) != !in || f83 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f84 (w1 - 1) != !in || f85 (w1 - 1) != !in) abort (); } if (f84 (w1) != in || f85 (w1) != in) abort (); if (f84 (w2) != in || f85 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f84 (w2 + 1) != !in || f85 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -14; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f86 (w1 - 1) != !in || f87 (w1 - 1) != !in) abort (); } if (f86 (w1) != in || f87 (w1) != in) abort (); if (f86 (w2) != in || f87 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f86 (w2 + 1) != !in || f87 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 15; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f88 (w1 - 1) != !in || f89 (w1 - 1) != !in) abort (); } if (f88 (w1) != in || f89 (w1) != in) abort (); if (f88 (w2) != in || f89 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f88 (w2 + 1) != !in || f89 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = -15; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f90 (w1 - 1) != !in || f91 (w1 - 1) != !in) abort (); } if (f90 (w1) != in || f91 (w1) != in) abort (); if (f90 (w2) != in || f91 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f90 (w2 + 1) != !in || f91 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -14; int w2 = 0x7fffffff; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f92 (w1 - 1) != !in || f93 (w1 - 1) != !in) abort (); } if (f92 (w1) != in || f93 (w1) != in) abort (); if (f92 (w2) != in || f93 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f92 (w2 + 1) != !in || f93 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = (-0x7fffffff - 1); int w2 = 14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f94 (w1 - 1) != !in || f95 (w1 - 1) != !in) abort (); } if (f94 (w1) != in || f95 (w1) != in) abort (); if (f94 (w2) != in || f95 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f94 (w2 + 1) != !in || f95 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -14; int w2 = 14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f96 (w1 - 1) != !in || f97 (w1 - 1) != !in) abort (); } if (f96 (w1) != in || f97 (w1) != in) abort (); if (f96 (w2) != in || f97 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f96 (w2 + 1) != !in || f97 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = -14; int w2 = 14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f98 (w1 - 1) != !in || f99 (w1 - 1) != !in) abort (); } if (f98 (w1) != in || f99 (w1) != in) abort (); if (f98 (w2) != in || f99 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f98 (w2 + 1) != !in || f99 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 14; int w2 = -14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f100 (w1 - 1) != !in || f101 (w1 - 1) != !in) abort (); } if (f100 (w1) != in || f101 (w1) != in) abort (); if (f100 (w2) != in || f101 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f100 (w2 + 1) != !in || f101 (w2 + 1) != !in) abort (); } } while (0);
do { int w1 = 14; int w2 = -14; int in = 1; if (w1 > w2) { in = w1; w1 = w2; w2 = in; in = 0; } if (w1 != (-0x7fffffff - 1)) { if (f102 (w1 - 1) != !in || f103 (w1 - 1) != !in) abort (); } if (f102 (w1) != in || f103 (w1) != in) abort (); if (f102 (w2) != in || f103 (w2) != in) abort (); if (w2 != 0x7fffffff) { if (f102 (w2 + 1) != !in || f103 (w2 + 1) != !in) abort (); } } while (0);
  return 0;
}
