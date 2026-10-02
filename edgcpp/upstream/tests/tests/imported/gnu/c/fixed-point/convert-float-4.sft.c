//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/convert-float-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/convert-float-4.c"
# 9 "./fixed-point/convert-float-4.c"
extern void abort (void);

# 1 "./fixed-point/convert.h" 1
# 12 "./fixed-point/convert-float-4.c" 2

int main ()
{
  { unsigned short _Accum a = 0.5uhk; float b = a; float c = 0.25; unsigned short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uhk) abort(); } { unsigned short _Accum a = 0.5uhk; double b = a; double c = 0.25; unsigned short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uhk) abort(); } { _Sat unsigned short _Accum a = 0.5uhk; float b = a; float c = 0.25; _Sat unsigned short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uhk) abort(); } { _Sat unsigned short _Accum a = 0.5uhk; double b = a; double c = 0.25; _Sat unsigned short _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uhk) abort(); };
  { unsigned _Accum a = 0.5uk; float b = a; float c = 0.25; unsigned _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uk) abort(); } { unsigned _Accum a = 0.5uk; double b = a; double c = 0.25; unsigned _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uk) abort(); } { _Sat unsigned _Accum a = 0.5uk; float b = a; float c = 0.25; _Sat unsigned _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uk) abort(); } { _Sat unsigned _Accum a = 0.5uk; double b = a; double c = 0.25; _Sat unsigned _Accum d = c; if (b != 0.5) abort(); if (d != 0.25uk) abort(); };
  { unsigned long _Accum a = 0.5ulk; float b = a; float c = 0.25; unsigned long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ulk) abort(); } { unsigned long _Accum a = 0.5ulk; double b = a; double c = 0.25; unsigned long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ulk) abort(); } { _Sat unsigned long _Accum a = 0.5ulk; float b = a; float c = 0.25; _Sat unsigned long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ulk) abort(); } { _Sat unsigned long _Accum a = 0.5ulk; double b = a; double c = 0.25; _Sat unsigned long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ulk) abort(); };
  { unsigned long long _Accum a = 0.5ullk; float b = a; float c = 0.25; unsigned long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ullk) abort(); } { unsigned long long _Accum a = 0.5ullk; double b = a; double c = 0.25; unsigned long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ullk) abort(); } { _Sat unsigned long long _Accum a = 0.5ullk; float b = a; float c = 0.25; _Sat unsigned long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ullk) abort(); } { _Sat unsigned long long _Accum a = 0.5ullk; double b = a; double c = 0.25; _Sat unsigned long long _Accum d = c; if (b != 0.5) abort(); if (d != 0.25ullk) abort(); };

  return 0;
}
