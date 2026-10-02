//type: fp
//options: 
# 0 "./tree-ssa/pr96789.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr96789.c"
# 11 "./tree-ssa/pr96789.c"
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
# 12 "./tree-ssa/pr96789.c" 2


# 13 "./tree-ssa/pr96789.c"
static inline void
foo (int16_t *diff, int i_size, uint8_t *val1, int i_val1, uint8_t *val2,
     int i_val2)
{
  for (int y = 0; y < i_size; y++)
    {
      for (int x = 0; x < i_size; x++)
 diff[x + y * i_size] = val1[x] - val2[x];
      val1 += i_val1;
      val2 += i_val2;
    }
}

void
bar (int16_t res[16], uint8_t *val1, uint8_t *val2)
{
  int16_t d[16];
  int16_t tmp[16];

  foo (d, 4, val1, 16, val2, 32);

  for (int i = 0; i < 4; i++)
    {
      int s03 = d[i * 4 + 0] + d[i * 4 + 3];
      int s12 = d[i * 4 + 1] + d[i * 4 + 2];
      int d03 = d[i * 4 + 0] - d[i * 4 + 3];
      int d12 = d[i * 4 + 1] - d[i * 4 + 2];

      tmp[0 * 4 + i] = s03 + s12;
      tmp[1 * 4 + i] = 2 * d03 + d12;
      tmp[2 * 4 + i] = s03 - s12;
      tmp[3 * 4 + i] = d03 - 2 * d12;
    }

  for (int i = 0; i < 4; i++)
    {
      int s03 = tmp[i * 4 + 0] + tmp[i * 4 + 3];
      int s12 = tmp[i * 4 + 1] + tmp[i * 4 + 2];
      int d03 = tmp[i * 4 + 0] - tmp[i * 4 + 3];
      int d12 = tmp[i * 4 + 1] - tmp[i * 4 + 2];

      res[i * 4 + 0] = s03 + s12;
      res[i * 4 + 1] = 2 * d03 + d12;
      res[i * 4 + 2] = s03 - s12;
      res[i * 4 + 3] = d03 - 2 * d12;
    }
}
