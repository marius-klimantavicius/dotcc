//type: rp
//options: 
# 0 "./pr94589-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr94589-6.c"



# 1 "./pr94589-5.c" 1







__attribute__((noipa)) int f3 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c > 0; }
__attribute__((noipa)) int f4 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c < 0; }
__attribute__((noipa)) int f5 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c >= 0; }
__attribute__((noipa)) int f6 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c <= 0; }
__attribute__((noipa)) int f7 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c == -1; }
__attribute__((noipa)) int f8 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c != -1; }
__attribute__((noipa)) int f9 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c > -1; }
__attribute__((noipa)) int f10 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c <= -1; }
__attribute__((noipa)) int f11 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c == 1; }
__attribute__((noipa)) int f12 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c != 1; }
__attribute__((noipa)) int f13 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c < 1; }
__attribute__((noipa)) int f14 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c >= 1; }
__attribute__((noipa)) int f17 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c > 0; }
__attribute__((noipa)) int f18 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c < 0; }
__attribute__((noipa)) int f19 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c >= 0; }
__attribute__((noipa)) int f20 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c <= 0; }
__attribute__((noipa)) int f21 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c == -1; }
__attribute__((noipa)) int f22 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c != -1; }
__attribute__((noipa)) int f23 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c > -1; }
__attribute__((noipa)) int f24 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c <= -1; }
__attribute__((noipa)) int f25 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c == 1; }
__attribute__((noipa)) int f26 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c != 1; }
__attribute__((noipa)) int f27 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c < 1; }
__attribute__((noipa)) int f28 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c >= 1; }
__attribute__((noipa)) int f29 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return (c & ~1) == 0; }
__attribute__((noipa)) int f30 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return (c & ~1) != 0; }
__attribute__((noipa)) int f31 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return (c & ~1) == 0; }
__attribute__((noipa)) int f32 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return (c & ~1) != 0; }
# 5 "./pr94589-6.c" 2

__attribute__((noipa)) int f1 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c == 0; }
__attribute__((noipa)) int f2 (double i, double j) { int c; if (i == j) c = 0; else if (i < j) c = -1; else if (i > j) c = 1; else c = 2; return c != 0; }
__attribute__((noipa)) int f15 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c == 0; }
__attribute__((noipa)) int f16 (double i) { int c; if (i == 5.0) c = 0; else if (i < 5.0) c = -1; else if (i > 5.0) c = 1; else c = 2; return c != 0; }




int
main ()
{
  if (f1 (7.0, 8.0) != 0) __builtin_abort ();
  if (f1 (8.0, 8.0) != 1) __builtin_abort ();
  if (f1 (9.0, 8.0) != 0) __builtin_abort ();
  if (f1 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f2 (7.0, 8.0) != 1) __builtin_abort ();
  if (f2 (8.0, 8.0) != 0) __builtin_abort ();
  if (f2 (9.0, 8.0) != 1) __builtin_abort ();
  if (f2 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f3 (7.0, 8.0) != 0) __builtin_abort ();
  if (f3 (8.0, 8.0) != 0) __builtin_abort ();
  if (f3 (9.0, 8.0) != 1) __builtin_abort ();
  if (f3 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f4 (7.0, 8.0) != 1) __builtin_abort ();
  if (f4 (8.0, 8.0) != 0) __builtin_abort ();
  if (f4 (9.0, 8.0) != 0) __builtin_abort ();
  if (f4 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f5 (7.0, 8.0) != 0) __builtin_abort ();
  if (f5 (8.0, 8.0) != 1) __builtin_abort ();
  if (f5 (9.0, 8.0) != 1) __builtin_abort ();
  if (f5 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f6 (7.0, 8.0) != 1) __builtin_abort ();
  if (f6 (8.0, 8.0) != 1) __builtin_abort ();
  if (f6 (9.0, 8.0) != 0) __builtin_abort ();
  if (f6 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f7 (7.0, 8.0) != 1) __builtin_abort ();
  if (f7 (8.0, 8.0) != 0) __builtin_abort ();
  if (f7 (9.0, 8.0) != 0) __builtin_abort ();
  if (f7 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f8 (7.0, 8.0) != 0) __builtin_abort ();
  if (f8 (8.0, 8.0) != 1) __builtin_abort ();
  if (f8 (9.0, 8.0) != 1) __builtin_abort ();
  if (f8 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f9 (7.0, 8.0) != 0) __builtin_abort ();
  if (f9 (8.0, 8.0) != 1) __builtin_abort ();
  if (f9 (9.0, 8.0) != 1) __builtin_abort ();
  if (f9 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f10 (7.0, 8.0) != 1) __builtin_abort ();
  if (f10 (8.0, 8.0) != 0) __builtin_abort ();
  if (f10 (9.0, 8.0) != 0) __builtin_abort ();
  if (f10 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f11 (7.0, 8.0) != 0) __builtin_abort ();
  if (f11 (8.0, 8.0) != 0) __builtin_abort ();
  if (f11 (9.0, 8.0) != 1) __builtin_abort ();
  if (f11 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f12 (7.0, 8.0) != 1) __builtin_abort ();
  if (f12 (8.0, 8.0) != 1) __builtin_abort ();
  if (f12 (9.0, 8.0) != 0) __builtin_abort ();
  if (f12 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f13 (7.0, 8.0) != 1) __builtin_abort ();
  if (f13 (8.0, 8.0) != 1) __builtin_abort ();
  if (f13 (9.0, 8.0) != 0) __builtin_abort ();
  if (f13 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f14 (7.0, 8.0) != 0) __builtin_abort ();
  if (f14 (8.0, 8.0) != 0) __builtin_abort ();
  if (f14 (9.0, 8.0) != 1) __builtin_abort ();
  if (f14 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f15 (4.0) != 0) __builtin_abort ();
  if (f15 (5.0) != 1) __builtin_abort ();
  if (f15 (6.0) != 0) __builtin_abort ();
  if (f15 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f16 (4.0) != 1) __builtin_abort ();
  if (f16 (5.0) != 0) __builtin_abort ();
  if (f16 (6.0) != 1) __builtin_abort ();
  if (f16 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f17 (4.0) != 0) __builtin_abort ();
  if (f17 (5.0) != 0) __builtin_abort ();
  if (f17 (6.0) != 1) __builtin_abort ();
  if (f17 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f18 (4.0) != 1) __builtin_abort ();
  if (f18 (5.0) != 0) __builtin_abort ();
  if (f18 (6.0) != 0) __builtin_abort ();
  if (f18 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f19 (4.0) != 0) __builtin_abort ();
  if (f19 (5.0) != 1) __builtin_abort ();
  if (f19 (6.0) != 1) __builtin_abort ();
  if (f19 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f20 (4.0) != 1) __builtin_abort ();
  if (f20 (5.0) != 1) __builtin_abort ();
  if (f20 (6.0) != 0) __builtin_abort ();
  if (f20 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f21 (4.0) != 1) __builtin_abort ();
  if (f21 (5.0) != 0) __builtin_abort ();
  if (f21 (6.0) != 0) __builtin_abort ();
  if (f21 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f22 (4.0) != 0) __builtin_abort ();
  if (f22 (5.0) != 1) __builtin_abort ();
  if (f22 (6.0) != 1) __builtin_abort ();
  if (f22 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f23 (4.0) != 0) __builtin_abort ();
  if (f23 (5.0) != 1) __builtin_abort ();
  if (f23 (6.0) != 1) __builtin_abort ();
  if (f23 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f24 (4.0) != 1) __builtin_abort ();
  if (f24 (5.0) != 0) __builtin_abort ();
  if (f24 (6.0) != 0) __builtin_abort ();
  if (f24 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f25 (4.0) != 0) __builtin_abort ();
  if (f25 (5.0) != 0) __builtin_abort ();
  if (f25 (6.0) != 1) __builtin_abort ();
  if (f25 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f26 (4.0) != 1) __builtin_abort ();
  if (f26 (5.0) != 1) __builtin_abort ();
  if (f26 (6.0) != 0) __builtin_abort ();
  if (f26 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f27 (4.0) != 1) __builtin_abort ();
  if (f27 (5.0) != 1) __builtin_abort ();
  if (f27 (6.0) != 0) __builtin_abort ();
  if (f27 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f28 (4.0) != 0) __builtin_abort ();
  if (f28 (5.0) != 0) __builtin_abort ();
  if (f28 (6.0) != 1) __builtin_abort ();
  if (f28 (__builtin_nan ("")) != 1) __builtin_abort ();
  if (f29 (7.0, 8.0) != 0) __builtin_abort ();
  if (f29 (8.0, 8.0) != 1) __builtin_abort ();
  if (f29 (9.0, 8.0) != 1) __builtin_abort ();
  if (f29 (__builtin_nan (""), 8.0) != 0) __builtin_abort ();
  if (f30 (7.0, 8.0) != 1) __builtin_abort ();
  if (f30 (8.0, 8.0) != 0) __builtin_abort ();
  if (f30 (9.0, 8.0) != 0) __builtin_abort ();
  if (f30 (__builtin_nan (""), 8.0) != 1) __builtin_abort ();
  if (f31 (4.0) != 0) __builtin_abort ();
  if (f31 (5.0) != 1) __builtin_abort ();
  if (f31 (6.0) != 1) __builtin_abort ();
  if (f31 (__builtin_nan ("")) != 0) __builtin_abort ();
  if (f32 (4.0) != 1) __builtin_abort ();
  if (f32 (5.0) != 0) __builtin_abort ();
  if (f32 (6.0) != 0) __builtin_abort ();
  if (f32 (__builtin_nan ("")) != 1) __builtin_abort ();
  return 0;
}
