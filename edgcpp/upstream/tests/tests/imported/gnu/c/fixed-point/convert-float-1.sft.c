//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/convert-float-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/convert-float-1.c"







extern void abort (void);

# 1 "./fixed-point/convert.h" 1
# 11 "./fixed-point/convert-float-1.c" 2

int main ()
{
  { short _Fract a = 0.5hr; float b = a; float c = 0.25; short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25hr) abort(); } { short _Fract a = 0.5hr; double b = a; double c = 0.25; short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25hr) abort(); } { _Sat short _Fract a = 0.5hr; float b = a; float c = 0.25; _Sat short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25hr) abort(); } { _Sat short _Fract a = 0.5hr; double b = a; double c = 0.25; _Sat short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25hr) abort(); };
  { _Fract a = 0.5r; float b = a; float c = 0.25; _Fract d = c; if (b != 0.5) abort(); if (d != 0.25r) abort(); } { _Fract a = 0.5r; double b = a; double c = 0.25; _Fract d = c; if (b != 0.5) abort(); if (d != 0.25r) abort(); } { _Sat _Fract a = 0.5r; float b = a; float c = 0.25; _Sat _Fract d = c; if (b != 0.5) abort(); if (d != 0.25r) abort(); } { _Sat _Fract a = 0.5r; double b = a; double c = 0.25; _Sat _Fract d = c; if (b != 0.5) abort(); if (d != 0.25r) abort(); };
  { long _Fract a = 0.5lr; float b = a; float c = 0.25; long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25lr) abort(); } { long _Fract a = 0.5lr; double b = a; double c = 0.25; long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25lr) abort(); } { _Sat long _Fract a = 0.5lr; float b = a; float c = 0.25; _Sat long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25lr) abort(); } { _Sat long _Fract a = 0.5lr; double b = a; double c = 0.25; _Sat long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25lr) abort(); };
  { long long _Fract a = 0.5llr; float b = a; float c = 0.25; long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25llr) abort(); } { long long _Fract a = 0.5llr; double b = a; double c = 0.25; long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25llr) abort(); } { _Sat long long _Fract a = 0.5llr; float b = a; float c = 0.25; _Sat long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25llr) abort(); } { _Sat long long _Fract a = 0.5llr; double b = a; double c = 0.25; _Sat long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25llr) abort(); };

  return 0;
}
