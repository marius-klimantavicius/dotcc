//type: rp
//options: 
# 0 "./pr94589-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr94589-3.c"



# 1 "./pr94589-1.c" 1







__attribute__((noipa)) int f1 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c == 0; }
__attribute__((noipa)) int f2 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c != 0; }
__attribute__((noipa)) int f3 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c > 0; }
__attribute__((noipa)) int f4 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c < 0; }
__attribute__((noipa)) int f5 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c >= 0; }
__attribute__((noipa)) int f6 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c <= 0; }
__attribute__((noipa)) int f7 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c == -1; }
__attribute__((noipa)) int f8 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c != -1; }
__attribute__((noipa)) int f9 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c > -1; }
__attribute__((noipa)) int f10 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c <= -1; }
__attribute__((noipa)) int f11 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c == 1; }
__attribute__((noipa)) int f12 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c != 1; }
__attribute__((noipa)) int f13 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c < 1; }
__attribute__((noipa)) int f14 (int i, int j) { int c = i == j ? 0 : i < j ? -1 : 1; return c >= 1; }
__attribute__((noipa)) int f15 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c == 0; }
__attribute__((noipa)) int f16 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c != 0; }
__attribute__((noipa)) int f17 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c > 0; }
__attribute__((noipa)) int f18 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c < 0; }
__attribute__((noipa)) int f19 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c >= 0; }
__attribute__((noipa)) int f20 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c <= 0; }
__attribute__((noipa)) int f21 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c == -1; }
__attribute__((noipa)) int f22 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c != -1; }
__attribute__((noipa)) int f23 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c > -1; }
__attribute__((noipa)) int f24 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c <= -1; }
__attribute__((noipa)) int f25 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c == 1; }
__attribute__((noipa)) int f26 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c != 1; }
__attribute__((noipa)) int f27 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c < 1; }
__attribute__((noipa)) int f28 (int i) { int c = i == 5 ? 0 : i < 5 ? -1 : 1; return c >= 1; }
# 5 "./pr94589-3.c" 2




int
main ()
{
  if (f1 (7, 8) != 0) __builtin_abort ();
  if (f1 (8, 8) != 1) __builtin_abort ();
  if (f1 (9, 8) != 0) __builtin_abort ();
  if (f2 (7, 8) != 1) __builtin_abort ();
  if (f2 (8, 8) != 0) __builtin_abort ();
  if (f2 (9, 8) != 1) __builtin_abort ();
  if (f3 (7, 8) != 0) __builtin_abort ();
  if (f3 (8, 8) != 0) __builtin_abort ();
  if (f3 (9, 8) != 1) __builtin_abort ();
  if (f4 (7, 8) != 1) __builtin_abort ();
  if (f4 (8, 8) != 0) __builtin_abort ();
  if (f4 (9, 8) != 0) __builtin_abort ();
  if (f5 (7, 8) != 0) __builtin_abort ();
  if (f5 (8, 8) != 1) __builtin_abort ();
  if (f5 (9, 8) != 1) __builtin_abort ();
  if (f6 (7, 8) != 1) __builtin_abort ();
  if (f6 (8, 8) != 1) __builtin_abort ();
  if (f6 (9, 8) != 0) __builtin_abort ();
  if (f7 (7, 8) != 1) __builtin_abort ();
  if (f7 (8, 8) != 0) __builtin_abort ();
  if (f7 (9, 8) != 0) __builtin_abort ();
  if (f8 (7, 8) != 0) __builtin_abort ();
  if (f8 (8, 8) != 1) __builtin_abort ();
  if (f8 (9, 8) != 1) __builtin_abort ();
  if (f9 (7, 8) != 0) __builtin_abort ();
  if (f9 (8, 8) != 1) __builtin_abort ();
  if (f9 (9, 8) != 1) __builtin_abort ();
  if (f10 (7, 8) != 1) __builtin_abort ();
  if (f10 (8, 8) != 0) __builtin_abort ();
  if (f10 (9, 8) != 0) __builtin_abort ();
  if (f11 (7, 8) != 0) __builtin_abort ();
  if (f11 (8, 8) != 0) __builtin_abort ();
  if (f11 (9, 8) != 1) __builtin_abort ();
  if (f12 (7, 8) != 1) __builtin_abort ();
  if (f12 (8, 8) != 1) __builtin_abort ();
  if (f12 (9, 8) != 0) __builtin_abort ();
  if (f13 (7, 8) != 1) __builtin_abort ();
  if (f13 (8, 8) != 1) __builtin_abort ();
  if (f13 (9, 8) != 0) __builtin_abort ();
  if (f14 (7, 8) != 0) __builtin_abort ();
  if (f14 (8, 8) != 0) __builtin_abort ();
  if (f14 (9, 8) != 1) __builtin_abort ();
  if (f15 (4) != 0) __builtin_abort ();
  if (f15 (5) != 1) __builtin_abort ();
  if (f15 (6) != 0) __builtin_abort ();
  if (f16 (4) != 1) __builtin_abort ();
  if (f16 (5) != 0) __builtin_abort ();
  if (f16 (6) != 1) __builtin_abort ();
  if (f17 (4) != 0) __builtin_abort ();
  if (f17 (5) != 0) __builtin_abort ();
  if (f17 (6) != 1) __builtin_abort ();
  if (f18 (4) != 1) __builtin_abort ();
  if (f18 (5) != 0) __builtin_abort ();
  if (f18 (6) != 0) __builtin_abort ();
  if (f19 (4) != 0) __builtin_abort ();
  if (f19 (5) != 1) __builtin_abort ();
  if (f19 (6) != 1) __builtin_abort ();
  if (f20 (4) != 1) __builtin_abort ();
  if (f20 (5) != 1) __builtin_abort ();
  if (f20 (6) != 0) __builtin_abort ();
  if (f21 (4) != 1) __builtin_abort ();
  if (f21 (5) != 0) __builtin_abort ();
  if (f21 (6) != 0) __builtin_abort ();
  if (f22 (4) != 0) __builtin_abort ();
  if (f22 (5) != 1) __builtin_abort ();
  if (f22 (6) != 1) __builtin_abort ();
  if (f23 (4) != 0) __builtin_abort ();
  if (f23 (5) != 1) __builtin_abort ();
  if (f23 (6) != 1) __builtin_abort ();
  if (f24 (4) != 1) __builtin_abort ();
  if (f24 (5) != 0) __builtin_abort ();
  if (f24 (6) != 0) __builtin_abort ();
  if (f25 (4) != 0) __builtin_abort ();
  if (f25 (5) != 0) __builtin_abort ();
  if (f25 (6) != 1) __builtin_abort ();
  if (f26 (4) != 1) __builtin_abort ();
  if (f26 (5) != 1) __builtin_abort ();
  if (f26 (6) != 0) __builtin_abort ();
  if (f27 (4) != 1) __builtin_abort ();
  if (f27 (5) != 1) __builtin_abort ();
  if (f27 (6) != 0) __builtin_abort ();
  if (f28 (4) != 0) __builtin_abort ();
  if (f28 (5) != 0) __builtin_abort ();
  if (f28 (6) != 1) __builtin_abort ();
  return 0;
}
