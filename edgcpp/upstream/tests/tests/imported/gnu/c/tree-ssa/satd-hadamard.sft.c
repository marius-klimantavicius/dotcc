//type: fp
//options: 
# 0 "./tree-ssa/satd-hadamard.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/satd-hadamard.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
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
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 6 "./tree-ssa/satd-hadamard.c" 2
# 18 "./tree-ssa/satd-hadamard.c"

# 18 "./tree-ssa/satd-hadamard.c"
int
x264_pixel_satd_8x4_simplified (uint8_t *pix1, int i_pix1, uint8_t *pix2, int i_pix2)
{
  uint32_t tmp[4][4];
  uint32_t a0, a1, a2, a3;
  int sum = 0;
  int i;

  for (i = 0; i < 4; i++, pix1 += i_pix1, pix2 += i_pix2)
    {
      a0 = (pix1[0] - pix2[0]) + ((pix1[4] - pix2[4]) << 16);
      a1 = (pix1[1] - pix2[1]) + ((pix1[5] - pix2[5]) << 16);
      a2 = (pix1[2] - pix2[2]) + ((pix1[6] - pix2[6]) << 16);
      a3 = (pix1[3] - pix2[3]) + ((pix1[7] - pix2[7]) << 16);
      { int t0 = a0 + a1; int t1 = a0 - a1; int t2 = a2 + a3; int t3 = a2 - a3; tmp[i][0] = t0 + t2; tmp[i][1] = t1 + t3; tmp[i][2] = t0 - t2; tmp[i][3] = t1 - t3;};
    }

  for (i = 0; i < 4; i++)
    {
      { int t0 = tmp[0][i] + tmp[1][i]; int t1 = tmp[0][i] - tmp[1][i]; int t2 = tmp[2][i] + tmp[3][i]; int t3 = tmp[2][i] - tmp[3][i]; a0 = t0 + t2; a1 = t1 + t3; a2 = t0 - t2; a3 = t1 - t3;};
      sum += a0 + a1 + a2 + a3;
    }

  return (((uint16_t)sum) + ((uint32_t)sum>>16)) >> 1;
}
