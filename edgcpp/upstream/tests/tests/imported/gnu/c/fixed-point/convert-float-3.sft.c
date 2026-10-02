//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/convert-float-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/convert-float-3.c"







extern void abort (void);

# 1 "./fixed-point/convert.h" 1
# 11 "./fixed-point/convert-float-3.c" 2

int main ()
{
  { short _Accum a = 0.5hk; float b = a; float c = 0.25; short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25hk) abort(); } { short _Accum a = 0.5hk; double b = a; double c = 0.25; short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25hk) abort(); } { _Sat short _Accum a = 0.5hk; float b = a; float c = 0.25; _Sat short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25hk) abort(); } { _Sat short _Accum a = 0.5hk; double b = a; double c = 0.25; _Sat short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25hk) abort(); };
  { _Accum a = 0.5k; float b = a; float c = 0.25; _Accum d = c; if (b != 0.5) abort(); if (d != 0.25k) abort(); } { _Accum a = 0.5k; double b = a; double c = 0.25; _Accum d = c; if (b != 0.5) abort(); if (d != 0.25k) abort(); } { _Sat _Accum a = 0.5k; float b = a; float c = 0.25; _Sat _Accum d = c; if (b != 0.5) abort(); if (d != 0.25k) abort(); } { _Sat _Accum a = 0.5k; double b = a; double c = 0.25; _Sat _Accum d = c; if (b != 0.5) abort(); if (d != 0.25k) abort(); };
  { long _Accum a = 0.5lk; float b = a; float c = 0.25; long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25lk) abort(); } { long _Accum a = 0.5lk; double b = a; double c = 0.25; long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25lk) abort(); } { _Sat long _Accum a = 0.5lk; float b = a; float c = 0.25; _Sat long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25lk) abort(); } { _Sat long _Accum a = 0.5lk; double b = a; double c = 0.25; _Sat long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25lk) abort(); };
  { long long _Accum a = 0.5llk; float b = a; float c = 0.25; long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25llk) abort(); } { long long _Accum a = 0.5llk; double b = a; double c = 0.25; long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25llk) abort(); } { _Sat long long _Accum a = 0.5llk; float b = a; float c = 0.25; _Sat long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25llk) abort(); } { _Sat long long _Accum a = 0.5llk; double b = a; double c = 0.25; _Sat long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25llk) abort(); };

  return 0;
}
