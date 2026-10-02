//type: fp
//options: 
# 0 "./pr108757-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr108757-2.c"




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
# 6 "./pr108757-2.c" 2




# 9 "./pr108757-2.c"
typedef unsigned int UINT;
typedef int INT;



# 1 "./pr108757.h" 1

UINT __attribute__ ((noinline))
opt_u1 (UINT x)
{
  if (x < (3 * 4) - 2)
    return 0;
  UINT a = x - (3 * 4);
  UINT b = a / 4;
  return b + 3;
}

UINT __attribute__ ((noinline))
opt_u2 (UINT x)
{
  if (x > (
# 15 "./pr108757.h" 3 4
          (0x7fffffff * 2U + 1U) 
# 15 "./pr108757.h"
               - (3 * 4) + 2))
    return 0;
  UINT a = x + (3 * 4);
  UINT b = a / 4;
  return b - 3;
}

INT __attribute__ ((noinline))
opt_s1 (INT x)
{
  if (x < (3 * 4) - 2)
    return 0;
  INT a = x - (3 * 4);
  INT b = a / 4;
  return b + 3;
}

INT __attribute__ ((noinline))
opt_s2 (INT x)
{
  if (x < 
# 35 "./pr108757.h" 3 4
         (-0x7fffffff - 1) 
# 35 "./pr108757.h"
              + (3 * 4) - 2 || x > 0)
    return 0;
  INT a = x - (3 * 4);
  INT b = a / 4;
  return b + 3;
}

INT __attribute__ ((noinline))
opt_s3 (INT x)
{
  if (x < (3 * 4) - 2)
    return 0;
  INT a = x - (3 * 4);
  INT b = a / -4;
  return b + -3;
}

INT __attribute__ ((noinline))
opt_s4 (INT x)
{
  if (x < 
# 55 "./pr108757.h" 3 4
         (-0x7fffffff - 1) 
# 55 "./pr108757.h"
              + (3 * 4) - 2 || x > 0)
    return 0;
  INT a = x - (3 * 4);
  INT b = a / -4;
  return b + -3;
}

INT __attribute__ ((noinline))
opt_s5 (INT x)
{
  if (x > (-3 * 4) + 2)
    return 0;
  INT a = x - (-3 * 4);
  INT b = a / 4;
  return b + -3;
}

INT __attribute__ ((noinline))
opt_s6 (INT x)
{
  if (x > 0x7fffffff 
# 75 "./pr108757.h"
              - (3 * 4) + 2 || x < 0)
    return 0;
  INT a = x - (-3 * 4);
  INT b = a / 4;
  return b + -3;
}

INT __attribute__ ((noinline))
opt_s7 (INT x)
{
  if (x > (3 * -4) + 2)
    return 0;
  INT a = x - (3 * -4);
  INT b = a / -4;
  return b + 3;
}

INT __attribute__ ((noinline))
opt_s8 (INT x)
{
  if (x > 0x7fffffff 
# 95 "./pr108757.h"
              - (3 * 4) + 2 || x < 0)
    return 0;
  INT a = x - (3 * -4);
  INT b = a / -4;
  return b + 3;
}

UINT __attribute__ ((noinline))
opt_u3 (UINT x)
{
  if (x < (3 << 4) - 2)
    return 0;
  UINT a = x - (3 << 4);
  UINT b = a >> 4;
  return b + 3;
}

UINT __attribute__ ((noinline))
opt_u4 (UINT x)
{
  if (x > (
# 115 "./pr108757.h" 3 4
          (0x7fffffff * 2U + 1U) 
# 115 "./pr108757.h"
               - (3 << 4)) + 2)
    return 0;
  UINT a = x + (3 << 4);
  UINT b = a >> 4;
  return b - 3;
}

INT __attribute__ ((noinline))
opt_s9 (INT x)
{
  if (x < (3 << 4) - 2)
    return 0;
  INT a = x - (3 << 4);
  INT b = a >> 4;
  return b + 3;
}

INT __attribute__ ((noinline))
opt_s10 (INT x)
{
  if (x < 
# 135 "./pr108757.h" 3 4
         (-0x7fffffff - 1) 
# 135 "./pr108757.h"
              + (3 << 4) - 2 || x > 0)
    return 0;
  INT a = x - (3 << 4);
  INT b = a >> 4;
  return b + 3;
}

INT __attribute__ ((noinline))
opt_s11 (INT x)
{
  if (x > (-3 << 4) + 2)
    return 0;
  INT a = x - (-3 << 4);
  INT b = a >> 4;
  return b + -3;
}

INT __attribute__ ((noinline))
opt_s12 (INT x)
{
  if (x > 0x7fffffff 
# 155 "./pr108757.h"
              - (3 << 4) + 2 || x < 0)
    return 0;
  INT a = x - (-3 << 4);
  INT b = a >> 4;
  return b + -3;
}

UINT __attribute__ ((noinline))
opt_u5 (UINT x, UINT n, UINT m)
{
  if (n > 4 || m > 3)
    return 0;
  if (x < (3*4) - 2)
    return 0;
  UINT a = x - (m * n);
  UINT b = a / n;
  return b + m;
}

UINT __attribute__ ((noinline))
opt_u6 (UINT x, UINT n, UINT m)
{
  if (n > 4 || m > 3)
    return 0;
  if (x > (
# 179 "./pr108757.h" 3 4
          (0x7fffffff * 2U + 1U) 
# 179 "./pr108757.h"
               - 3*4) + 2)
    return 0;
  UINT a = x + (m * n);
  UINT b = a / n;
  return b - m;
}

INT __attribute__ ((noinline))
opt_s13 (INT x, INT n, INT m)
{
  if (n > 4 || m > 3 || n < 0 || m < 0)
    return 0;
  if (x < (3*4) - 2)
    return 0;
  INT a = x - (m * n);
  INT b = a / n;
  return b + m;
}

INT __attribute__ ((noinline))
opt_s14 (INT x, INT n, INT m)
{
  if (n > 4 || m > 3 || n < 0 || m < 0)
    return 0;
  if (x > -3*4 + 2)
    return 0;
  INT a = x + (m * n);
  INT b = a / n;
  return b - m;
}

INT
opt_s15 (INT x, INT n, INT m)
{
  if (n > 0 || m > 0 || n < -4 || m < -3)
    return 0;
  if (x < (3*4) - 2)
    return 0;
  INT a = x - (m * n);
  INT b = a / n;
  return b + m;
}

INT __attribute__ ((noinline))
opt_s16 (INT x, INT n, INT m)
{
  if (n > 0 || m > 0 || n < -4 || m < -3)
    return 0;
  if (x < 0 || x > (0x7fffffff 
# 227 "./pr108757.h"
                        - 3*4) + 2)
    return 0;
  INT a = x + (m * n);
  INT b = a / n;
  return b - m;
}
# 15 "./pr108757-2.c" 2
