//type: fp
//options:  --signed_chars
# 0 "./warn/Wsign-conversion.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wsign-conversion.C"






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
# 8 "./warn/Wsign-conversion.C" 2


# 9 "./warn/Wsign-conversion.C"
void fsc (signed char sc);
void fuc (unsigned char uc);
unsigned fui (unsigned int ui);
void fsi (signed int ui);

void h (int x)
{
  unsigned int ui = 3;
  int si = 3;
  unsigned char uc = 3;
  signed char sc = 3;

  uc = ui;
  uc = si;
  sc = ui;
  sc = si;
  fuc (ui);
  fuc (si);
  fsc (ui);
  fsc (si);

  fsi (si);
  fui (ui);
  fsi (uc);
  si = uc;
  fui (uc);
  ui = uc;
  fui ('A');
  ui = 'A';
  fsi ('A');
  si = 'A';
  fuc ('A');
  uc = 'A';

  uc = x ? 1U : -1;
  uc = x ? 
# 44 "./warn/Wsign-conversion.C" 3 4
          (-0x7f - 1) 
# 44 "./warn/Wsign-conversion.C"
                    : 1U;
  uc = x ? 1 : -1;
  uc = x ? 
# 46 "./warn/Wsign-conversion.C" 3 4
          (-0x7f - 1) 
# 46 "./warn/Wsign-conversion.C"
                    : 1;
  ui = x ? 1U : -1;
  ui = x ? 
# 48 "./warn/Wsign-conversion.C" 3 4
          (-0x7fffffff - 1) 
# 48 "./warn/Wsign-conversion.C"
                  : 1U;
  ui = ui ? 
# 49 "./warn/Wsign-conversion.C" 3 4
           (-0x7f - 1) 
# 49 "./warn/Wsign-conversion.C"
                     : 1U;
  ui = 1U * -1;
  ui = ui + 
# 51 "./warn/Wsign-conversion.C" 3 4
           (-0x7fffffff - 1)
# 51 "./warn/Wsign-conversion.C"
                  ;
  ui = x ? 1 : -1;
  ui = ui ? 
# 53 "./warn/Wsign-conversion.C" 3 4
           (-0x7f - 1) 
# 53 "./warn/Wsign-conversion.C"
                     : 1;

  fuc (-1);
  uc = -1;
  fui (-1);
  ui = -1;
  fuc ('\xa0');
  uc = '\xa0';
  fui ('\xa0');
  ui = '\xa0';
  fsi (0x80000000);
  si = 0x80000000;


  fsi (
# 67 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 67 "./warn/Wsign-conversion.C"
               - 1);
  si = 
# 68 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 68 "./warn/Wsign-conversion.C"
               - 1;
  fsi (
# 69 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 69 "./warn/Wsign-conversion.C"
               - 1U);
  si = 
# 70 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 70 "./warn/Wsign-conversion.C"
               - 1U;
  fsi (
# 71 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U)
# 71 "./warn/Wsign-conversion.C"
              /3U);
  si = 
# 72 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U)
# 72 "./warn/Wsign-conversion.C"
              /3U;
  fsi (
# 73 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U)
# 73 "./warn/Wsign-conversion.C"
              /3);
  si = 
# 74 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U)
# 74 "./warn/Wsign-conversion.C"
              /3;
  fui (
# 75 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 75 "./warn/Wsign-conversion.C"
               - 1);
  ui = 
# 76 "./warn/Wsign-conversion.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 76 "./warn/Wsign-conversion.C"
               - 1;

  uc = (unsigned char) -1;
  ui = -1 * (1 * -1);
  ui = (unsigned) -1;

  fsc (uc);
  sc = uc;
  fuc (sc);
  uc = sc;
  fsi (ui);
  si = ui;
  fui (si);
  ui = si;
  fui (sc);
  ui = sc;
}

unsigned fui (unsigned a) { return a + -1; }
