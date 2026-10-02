//type: fp
//options: --c99
# 0 "./overflow-warn-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./overflow-warn-9.c"





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
# 7 "./overflow-warn-9.c" 2


# 8 "./overflow-warn-9.c"
struct Types
{
  signed char sc;
  unsigned char uc;
  signed short ss;
  unsigned short us;
  signed int si;
  unsigned int ui;
  signed long sl;
  unsigned long ul;
  signed long long sll;
  unsigned long long ull;
};

const struct Types t1 = {
# 33 "./overflow-warn-9.c"
  .uc = 
# 33 "./overflow-warn-9.c" 3 4
       (-0x7f - 1)
# 33 "./overflow-warn-9.c"
                ,
  .uc = -1,

  .uc = 
# 36 "./overflow-warn-9.c" 3 4
       (0x7f * 2 + 1) 
# 36 "./overflow-warn-9.c"
                 + 1,
  .uc = 
# 37 "./overflow-warn-9.c" 3 4
       (0x7f * 2 + 1) 
# 37 "./overflow-warn-9.c"
                 * 2,
# 48 "./overflow-warn-9.c"
  .sc = 0x7f 
# 48 "./overflow-warn-9.c"
                 + 1,
  .sc = 0x7f 
# 49 "./overflow-warn-9.c"
                 + 2,
  .sc = 0x7f 
# 50 "./overflow-warn-9.c"
                 * 2,
  .sc = 0x7f 
# 51 "./overflow-warn-9.c"
                 * 2 + 3,
  .sc = 0x7f 
# 52 "./overflow-warn-9.c"
                 * 3 + 3,


  .ss = 0x7fff 
# 55 "./overflow-warn-9.c"
                + 1,
  .us = 
# 56 "./overflow-warn-9.c" 3 4
       (0x7fff * 2 + 1) 
# 56 "./overflow-warn-9.c"
                 + 1,

  .si = 0x7fffffff 
# 58 "./overflow-warn-9.c"
               + 1LU,
  .ui = 
# 59 "./overflow-warn-9.c" 3 4
       (0x7fffffff * 2U + 1U) 
# 59 "./overflow-warn-9.c"
                + 1L,
  .ui = 
# 60 "./overflow-warn-9.c" 3 4
       (0x7fffffff * 2U + 1U) 
# 60 "./overflow-warn-9.c"
                + 1LU,

  .sl = 0x7fffffffffffffffL 
# 62 "./overflow-warn-9.c"
                + 1LU,


  .ul = 
# 65 "./overflow-warn-9.c" 3 4
       (0x7fffffffffffffffL * 2UL + 1UL) 
# 65 "./overflow-warn-9.c"
                 + 1LU
};
