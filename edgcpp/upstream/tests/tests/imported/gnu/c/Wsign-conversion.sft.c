//type: fp
//options: --c99 --signed_chars
# 0 "./Wsign-conversion.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wsign-conversion.c"






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
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 8 "./Wsign-conversion.c" 2


# 9 "./Wsign-conversion.c"
void fsc (signed char sc);
void fuc (unsigned char uc);
unsigned fui (unsigned int ui);
void fsi (signed int ui);
void fsi32 (int ui);

void h (int x)
{
  unsigned int ui = 3;
  int si = 3;
  int si32 = 3;
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
# 46 "./Wsign-conversion.c" 3 4
          (-0x7f - 1) 
# 46 "./Wsign-conversion.c"
                    : 1U;
  uc = x ? 1 : -1;
  uc = x ? 
# 48 "./Wsign-conversion.c" 3 4
          (-0x7f - 1) 
# 48 "./Wsign-conversion.c"
                    : 1;
  ui = x ? 1U : -1;
  ui = x ? 
# 50 "./Wsign-conversion.c" 3 4
          (-0x7fffffff - 1) 
# 50 "./Wsign-conversion.c"
                  : 1U;
  ui = ui ? 
# 51 "./Wsign-conversion.c" 3 4
           (-0x7f - 1) 
# 51 "./Wsign-conversion.c"
                     : 1U;
  ui = 1U * -1;
  ui = ui + 
# 53 "./Wsign-conversion.c" 3 4
           (-0x7fffffff - 1)
# 53 "./Wsign-conversion.c"
                  ;
  ui = x ? 1 : -1;
  ui = ui ? 
# 55 "./Wsign-conversion.c" 3 4
           (-0x7f - 1) 
# 55 "./Wsign-conversion.c"
                     : 1;

  fuc (-1);
  uc = -1;
  fui (-1);
  ui = -1;
  fuc ('\xa0');
  uc = '\xa0';
  fui ('\xa0');
  ui = '\xa0';
  fsi32 (0x80000000);
  si32 = 0x80000000;


  fsi (
# 69 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 69 "./Wsign-conversion.c"
               - 1);
  si = 
# 70 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 70 "./Wsign-conversion.c"
               - 1;
  fsi (
# 71 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 71 "./Wsign-conversion.c"
               - 1U);
  si = 
# 72 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 72 "./Wsign-conversion.c"
               - 1U;
  fsi (
# 73 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U)
# 73 "./Wsign-conversion.c"
              /3U);
  si = 
# 74 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U)
# 74 "./Wsign-conversion.c"
              /3U;
  fsi (
# 75 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U)
# 75 "./Wsign-conversion.c"
              /3);
  si = 
# 76 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U)
# 76 "./Wsign-conversion.c"
              /3;
  fui (
# 77 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 77 "./Wsign-conversion.c"
               - 1);
  ui = 
# 78 "./Wsign-conversion.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 78 "./Wsign-conversion.c"
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
