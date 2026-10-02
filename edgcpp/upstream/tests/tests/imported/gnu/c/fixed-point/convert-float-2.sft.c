//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/convert-float-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/convert-float-2.c"







extern void abort (void);

# 1 "./fixed-point/convert.h" 1
# 11 "./fixed-point/convert-float-2.c" 2

int main ()
{
  { unsigned short _Fract a = 0.5uhr; float b = a; float c = 0.25; unsigned short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25uhr) abort(); } { unsigned short _Fract a = 0.5uhr; double b = a; double c = 0.25; unsigned short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25uhr) abort(); } { _Sat unsigned short _Fract a = 0.5uhr; float b = a; float c = 0.25; _Sat unsigned short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25uhr) abort(); } { _Sat unsigned short _Fract a = 0.5uhr; double b = a; double c = 0.25; _Sat unsigned short _Fract d = c; if (b != 0.5) abort(); if (d != 0.25uhr) abort(); };
  { unsigned _Fract a = 0.5ur; float b = a; float c = 0.25; unsigned _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ur) abort(); } { unsigned _Fract a = 0.5ur; double b = a; double c = 0.25; unsigned _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ur) abort(); } { _Sat unsigned _Fract a = 0.5ur; float b = a; float c = 0.25; _Sat unsigned _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ur) abort(); } { _Sat unsigned _Fract a = 0.5ur; double b = a; double c = 0.25; _Sat unsigned _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ur) abort(); };
  { unsigned long _Fract a = 0.5ulr; float b = a; float c = 0.25; unsigned long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ulr) abort(); } { unsigned long _Fract a = 0.5ulr; double b = a; double c = 0.25; unsigned long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ulr) abort(); } { _Sat unsigned long _Fract a = 0.5ulr; float b = a; float c = 0.25; _Sat unsigned long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ulr) abort(); } { _Sat unsigned long _Fract a = 0.5ulr; double b = a; double c = 0.25; _Sat unsigned long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ulr) abort(); };
  { unsigned long long _Fract a = 0.5ullr; float b = a; float c = 0.25; unsigned long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ullr) abort(); } { unsigned long long _Fract a = 0.5ullr; double b = a; double c = 0.25; unsigned long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ullr) abort(); } { _Sat unsigned long long _Fract a = 0.5ullr; float b = a; float c = 0.25; _Sat unsigned long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ullr) abort(); } { _Sat unsigned long long _Fract a = 0.5ullr; double b = a; double c = 0.25; _Sat unsigned long long _Fract d = c; if (b != 0.5) abort(); if (d != 0.25ullr) abort(); };

  return 0;
}
