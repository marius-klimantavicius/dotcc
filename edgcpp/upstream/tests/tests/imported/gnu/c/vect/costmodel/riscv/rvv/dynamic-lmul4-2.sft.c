//type: fp
//options: 
# 0 "./vect/costmodel/riscv/rvv/dynamic-lmul4-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/costmodel/riscv/rvv/dynamic-lmul4-2.c"



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
# 5 "./vect/costmodel/riscv/rvv/dynamic-lmul4-2.c" 2


# 6 "./vect/costmodel/riscv/rvv/dynamic-lmul4-2.c"
void
foo (int8_t *__restrict a, int8_t *__restrict b, int8_t *__restrict c,
      int8_t *__restrict a2, int8_t *__restrict b2, int8_t *__restrict c2,
      int8_t *__restrict a3, int8_t *__restrict b3, int8_t *__restrict c3,
      int8_t *__restrict a4, int8_t *__restrict b4, int8_t *__restrict c4,
      int8_t *__restrict a5, int8_t *__restrict b5, int8_t *__restrict c5,
      int8_t *__restrict d, int8_t *__restrict d2, int8_t *__restrict d3,
      int8_t *__restrict d4, int8_t *__restrict d5, int n, int m)
{
  for (int i = 0; i < n; i++)
    {
      a[i] = b[i] + c[i];
      a2[i] = b2[i] + c2[i];
      a3[i] = b3[i] + c3[i];
      a4[i] = b4[i] + c4[i];
      a5[i] = a[i] + a4[i];
      d[i] = a[i] - a2[i];
      d2[i] = a2[i] * a[i];
      d3[i] = a3[i] * a2[i];
      d4[i] = a2[i] * d2[i];
      d5[i] = a[i] * a2[i] * a3[i] * a4[i] * d[i];
    }
}
