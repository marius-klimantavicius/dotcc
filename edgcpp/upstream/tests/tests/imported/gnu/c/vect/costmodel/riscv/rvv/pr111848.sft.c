//type: fp
//options: 
# 0 "./vect/costmodel/riscv/rvv/pr111848.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/costmodel/riscv/rvv/pr111848.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint-gcc.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint-gcc.h" 3 4

# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint-gcc.h" 3 4
typedef signed char int8_t;


typedef short int int16_t;


typedef int int32_t;


typedef long int int64_t;


typedef unsigned char uint8_t;


typedef short unsigned int uint16_t;


typedef unsigned int uint32_t;


typedef long unsigned int uint64_t;




typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;
typedef long int int_least64_t;
typedef unsigned char uint_least8_t;
typedef short unsigned int uint_least16_t;
typedef unsigned int uint_least32_t;
typedef long unsigned int uint_least64_t;



typedef signed char int_fast8_t;
typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
typedef unsigned char uint_fast8_t;
typedef long unsigned int uint_fast16_t;
typedef long unsigned int uint_fast32_t;
typedef long unsigned int uint_fast64_t;




typedef long int intptr_t;


typedef long unsigned int uintptr_t;




typedef long int intmax_t;
typedef long unsigned int uintmax_t;
# 5 "./vect/costmodel/riscv/rvv/pr111848.c" 2

# 5 "./vect/costmodel/riscv/rvv/pr111848.c"
void
f3 (uint8_t *restrict a, uint8_t *restrict b,
   uint8_t *restrict c, uint8_t *restrict d,
   int n)
{
  for (int i = 0; i < n; ++i)
    {
      a[i * 8] = c[i * 8] + d[i * 8];
      a[i * 8 + 1] = c[i * 8] + d[i * 8 + 1];
      a[i * 8 + 2] = c[i * 8 + 2] + d[i * 8 + 2];
      a[i * 8 + 3] = c[i * 8 + 2] + d[i * 8 + 3];
      a[i * 8 + 4] = c[i * 8 + 4] + d[i * 8 + 4];
      a[i * 8 + 5] = c[i * 8 + 4] + d[i * 8 + 5];
      a[i * 8 + 6] = c[i * 8 + 6] + d[i * 8 + 6];
      a[i * 8 + 7] = c[i * 8 + 6] + d[i * 8 + 7];
      b[i * 8] = c[i * 8 + 1] + d[i * 8];
      b[i * 8 + 1] = c[i * 8 + 1] + d[i * 8 + 1];
      b[i * 8 + 2] = c[i * 8 + 3] + d[i * 8 + 2];
      b[i * 8 + 3] = c[i * 8 + 3] + d[i * 8 + 3];
      b[i * 8 + 4] = c[i * 8 + 5] + d[i * 8 + 4];
      b[i * 8 + 5] = c[i * 8 + 5] + d[i * 8 + 5];
      b[i * 8 + 6] = c[i * 8 + 7] + d[i * 8 + 6];
      b[i * 8 + 7] = c[i * 8 + 7] + d[i * 8 + 7];
    }
}
