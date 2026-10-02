//type: fp
//options: 
# 0 "./vect/costmodel/riscv/rvv/no-dynamic-lmul-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/costmodel/riscv/rvv/no-dynamic-lmul-1.c"



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
# 5 "./vect/costmodel/riscv/rvv/no-dynamic-lmul-1.c" 2


# 6 "./vect/costmodel/riscv/rvv/no-dynamic-lmul-1.c"
void
foo (int8_t *restrict a)
{
  for (int i = 0; i < 4096; ++i)
    a[i] = a[i]-16;
}

void
foo2 (int16_t *restrict a)
{
  for (int i = 0; i < 2048; ++i)
    a[i] = a[i]-16;
}

void
foo3 (int32_t *restrict a)
{
  for (int i = 0; i < 1024; ++i)
    a[i] = a[i]-16;
}

void
foo4 (int64_t *restrict a)
{
  for (int i = 0; i < 512; ++i)
    a[i] = a[i]-16;
}

void
foo5 (int8_t *restrict a)
{
  for (int i = 0; i < 16; ++i)
    a[i] = a[i]-16;
}

void
foo6 (int16_t *restrict a)
{
  for (int i = 0; i < 16; ++i)
    a[i] = a[i]-16;
}

void
foo7 (int32_t *restrict a)
{
  for (int i = 0; i < 16; ++i)
    a[i] = a[i]-16;
}

void
foo8 (int64_t *restrict a)
{
  for (int i = 0; i < 16; ++i)
    a[i] = a[i]-16;
}
