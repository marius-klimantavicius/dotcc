//type: fp
//options: --c99 --strict_gnu
# 0 "./Wconversion-complex-gnu.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wconversion-complex-gnu.c"
# 12 "./Wconversion-complex-gnu.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 27 "/usr/include/limits.h" 2 3 4
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 13 "./Wconversion-complex-gnu.c" 2


# 14 "./Wconversion-complex-gnu.c"
void fsi (int);
void fui (unsigned);
void ffloat (float);
int vsi;
unsigned int vui;
float vfloat;

void fsic (int _Complex);
void fuic (unsigned _Complex);
void ffloatc (float _Complex);
int _Complex vsic;
unsigned _Complex vuic;
float _Complex vfloatc;



void
var_float_to_int (void)
{
  double _Complex doublec = 0.;

  fsic (doublec);
  fuic (doublec);

  vsic = doublec;
  vuic = doublec;
}



void
var_complex_to_real (void)
{
  int _Complex ic = 0;
  unsigned _Complex uc = 0;
  unsigned long long _Complex ullc = 0;

  fsic (ic);
  fuic (uc);
  vsic = ic;
  vuic = uc;

  fsi (ic);
  vsi = ic;
  fui (uc);
  vui = uc;

  fuic (ullc);
  vuic = ullc;

  fui (ic);
  vui = ic;
}



void
const_float_to_int (void)
{
  fsic (1. - 1.i);
  fuic (1. + 1.i);
  vsic = 1. - 1.i;
  vuic = 1. + 1.i;

  fsic (0.5 + 0.i);
  vsic = 0.5 + 0.i;
  fuic (0.5 + 0.i);
}



void
const_complex_int_to_real_int (void)
{
  fsi (-1 + 0i);
  fui (1 + 0i);
  vsi = -1 + 0i;
  vui = 1 + 0i;

  fui (1 + 1i);
  vui = 1 + 1i;

  fui (
# 96 "./Wconversion-complex-gnu.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 96 "./Wconversion-complex-gnu.c"
               + 1ull + 0i);
  vui = 
# 97 "./Wconversion-complex-gnu.c" 3 4
       (0x7fffffff * 2U + 1U) 
# 97 "./Wconversion-complex-gnu.c"
                + 1ull + 0i;

  ffloat (
# 99 "./Wconversion-complex-gnu.c" 3 4
         (0x7fffffff * 2U + 1U) 
# 99 "./Wconversion-complex-gnu.c"
                  + 0i);
  vfloat = 
# 100 "./Wconversion-complex-gnu.c" 3 4
          (0x7fffffff * 2U + 1U) 
# 100 "./Wconversion-complex-gnu.c"
                   + 0i;
}

void
const_complex_int_narrowing (void)
{
  fsic (1 - 1i);
  fuic (1 + 1i);
  vsic = 1 - 1i;
  vuic = 1 + 1i;

  fuic (
# 111 "./Wconversion-complex-gnu.c" 3 4
       (0x7fffffff * 2U + 1U) 
# 111 "./Wconversion-complex-gnu.c"
                + 1ull + 1i);
  fuic ((
# 112 "./Wconversion-complex-gnu.c" 3 4
        (0x7fffffff * 2U + 1U) 
# 112 "./Wconversion-complex-gnu.c"
                 + 1ull) * 1i);
  fuic ((
# 113 "./Wconversion-complex-gnu.c" 3 4
        (0x7fffffff * 2U + 1U) 
# 113 "./Wconversion-complex-gnu.c"
                 + 1ull) + (
# 113 "./Wconversion-complex-gnu.c" 3 4
                            (0x7fffffff * 2U + 1U) 
# 113 "./Wconversion-complex-gnu.c"
                                     + 1ull) * 1i);

  vuic = (
# 115 "./Wconversion-complex-gnu.c" 3 4
         (0x7fffffff * 2U + 1U) 
# 115 "./Wconversion-complex-gnu.c"
                  + 1ull) * 1i;
  vuic = (
# 116 "./Wconversion-complex-gnu.c" 3 4
         (0x7fffffff * 2U + 1U) 
# 116 "./Wconversion-complex-gnu.c"
                  + 1ull) + 1i;
  vuic = (
# 117 "./Wconversion-complex-gnu.c" 3 4
         (0x7fffffff * 2U + 1U) 
# 117 "./Wconversion-complex-gnu.c"
                  + 1ull) + (
# 117 "./Wconversion-complex-gnu.c" 3 4
                             (0x7fffffff * 2U + 1U) 
# 117 "./Wconversion-complex-gnu.c"
                                      + 1ull) * 1i;

  ffloatc (
# 119 "./Wconversion-complex-gnu.c" 3 4
          (0x7fffffff * 2U + 1U) 
# 119 "./Wconversion-complex-gnu.c"
                   * 1i);
  ffloatc (
# 120 "./Wconversion-complex-gnu.c" 3 4
          (0x7fffffff * 2U + 1U) 
# 120 "./Wconversion-complex-gnu.c"
                   + 1i);
  ffloatc (
# 121 "./Wconversion-complex-gnu.c" 3 4
          (0x7fffffff * 2U + 1U) 
# 121 "./Wconversion-complex-gnu.c"
                   + 
# 121 "./Wconversion-complex-gnu.c" 3 4
                     (0x7fffffff * 2U + 1U) 
# 121 "./Wconversion-complex-gnu.c"
                              * 1i);

  vfloatc = 
# 123 "./Wconversion-complex-gnu.c" 3 4
           (0x7fffffff * 2U + 1U) 
# 123 "./Wconversion-complex-gnu.c"
                    * 1i;
  vfloatc = 
# 124 "./Wconversion-complex-gnu.c" 3 4
           (0x7fffffff * 2U + 1U) 
# 124 "./Wconversion-complex-gnu.c"
                    + 1i;
  vfloatc = 
# 125 "./Wconversion-complex-gnu.c" 3 4
           (0x7fffffff * 2U + 1U) 
# 125 "./Wconversion-complex-gnu.c"
                    + 
# 125 "./Wconversion-complex-gnu.c" 3 4
                      (0x7fffffff * 2U + 1U) 
# 125 "./Wconversion-complex-gnu.c"
                               * 1i;
}
