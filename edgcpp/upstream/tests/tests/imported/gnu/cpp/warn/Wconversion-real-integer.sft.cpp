//type: fp
//options: 
# 0 "./warn/Wconversion-real-integer.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wconversion-real-integer.C"







# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 3 4
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



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 2 3 4
# 9 "./warn/Wconversion-real-integer.C" 2


# 10 "./warn/Wconversion-real-integer.C"
void fsi (signed int x);
void fui (unsigned int x);
void ffloat (float x);
void fdouble (double x);

float vfloat;
double vdouble;

void h (void)
{
  unsigned int ui = 3;
  int si = 3;
  unsigned char uc = 3;
  signed char sc = 3;
  float f = 3;
  double d = 3;

  fsi (3.1f);
  si = 3.1f;
  fsi (3.1);
  si = 3.1;
  fsi (d);
  si = d;
  fui (-1.0);
  ui = -1.0;
  ffloat (0x7fffffff
# 35 "./warn/Wconversion-real-integer.C"
                );
  vfloat = 0x7fffffff
# 36 "./warn/Wconversion-real-integer.C"
                 ;
  ffloat (16777217);
  vfloat = 16777217;
  ffloat (si);
  vfloat = si;
  ffloat (ui);
  vfloat = ui;

  fsi (3);
  si = 3;
  fsi (3.0f);
  si = 3.0f;
  fsi (3.0);
  si = 3.0;
  fsi (16777217.0f);
  si = 16777217.0f;
  fsi ((int) 3.1);
  si = (int) 3.1;
  ffloat (3U);
  vfloat = 3U;
  ffloat (3);
  vfloat = 3;
  ffloat (
# 58 "./warn/Wconversion-real-integer.C" 3 4
         (-0x7fffffff - 1)
# 58 "./warn/Wconversion-real-integer.C"
                );
  vfloat = 
# 59 "./warn/Wconversion-real-integer.C" 3 4
          (-0x7fffffff - 1)
# 59 "./warn/Wconversion-real-integer.C"
                 ;
  ffloat (uc);
  vfloat = uc;
  ffloat (sc);
  vfloat = sc;

  fdouble (
# 65 "./warn/Wconversion-real-integer.C" 3 4
          (0x7fffffff * 2U + 1U)
# 65 "./warn/Wconversion-real-integer.C"
                  );
  vdouble = 
# 66 "./warn/Wconversion-real-integer.C" 3 4
           (0x7fffffff * 2U + 1U)
# 66 "./warn/Wconversion-real-integer.C"
                   ;
  fdouble (ui);
  vdouble = ui;
  fdouble (si);
  vdouble = si;
}


void fss (signed short x);
void fus (unsigned short x);
void fsc (signed char x);
void fuc (unsigned char x);

void h2 (void)
{
  unsigned short int us;
  short int ss;
  unsigned char uc;
  signed char sc;

  fss (4294967294.0);
  ss = 4294967294.0;
  fss (-4294967294.0);
  ss = -4294967294.0;
  fus (4294967294.0);
  us = 4294967294.0;
  fus (-4294967294.0);
  us = -4294967294.0;

  fsc (500.0);
  sc = 500.0;
  fsc (-500.0);
  sc = -500.0;
  fuc (500.0);
  uc = 500.0;
  fuc (-500.0);
  uc = -500.0;

  fss (500.0);
  ss = 500.0;
  fss (-500.0);
  ss = -500.0;
  fus (500.0);
  us = 500.0;
  fus (-500.0);
  us = -500.0;
}
