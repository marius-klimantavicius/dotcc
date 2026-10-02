//type: fp
//options: 
# 0 "./vect/complex/vect-complex-add-pattern-unsigned-long.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/complex/vect-complex-add-pattern-unsigned-long.c"
# 9 "./vect/complex/vect-complex-add-pattern-unsigned-long.c"
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
# 10 "./vect/complex/vect-complex-add-pattern-unsigned-long.c" 2
# 1 "./vect/complex/complex-add-pattern-template.c" 1

# 1 "./vect/complex/complex-add-pattern-template.c"
void add90 (uint64_t a[restrict 200], uint64_t b[restrict 200], uint64_t c[restrict 200])
{

#pragma GCC unroll 16

  for (int i=0; i < 200; i+=2)
    {
      c[i] = a[i] - b[i+1];
      c[i+1] = a[i+1] + b[i];
    }
}



void add270 (uint64_t a[restrict 200], uint64_t b[restrict 200], uint64_t c[restrict 200])
{

#pragma GCC unroll 16

  for (int i=0; i < 200; i+=2)
    {
      c[i] = a[i] + b[i+1];
      c[i+1] = a[i+1] - b[i];
    }
}



void addMixed (uint64_t a[restrict 200], uint64_t b[restrict 200], uint64_t c[restrict 200])
{

#pragma GCC unroll 16

  for (int i=0; i < 200; i+=4)
    {
      c[i] = a[i] - b[i+1];
      c[i+1] = a[i+1] + b[i];
      c[i+2] = a[i+2] + b[i+3];
      c[i+3] = a[i+3] - b[i+2];
    }
}

void add90HandUnrolled (uint64_t a[restrict 200], uint64_t b[restrict 200],
   uint64_t c[restrict 200])
{

#pragma GCC unroll 16

  for (int i=0; i < (200 /2); i+=4)
    {
      c[i] = a[i] - b[i+1];
      c[i+2] = a[i+2] - b[i+3];
      c[i+1] = a[i+1] + b[i];
      c[i+3] = a[i+3] + b[i+2];
    }
}



void add90Hybrid (uint64_t a[restrict 200], uint64_t b[restrict 200], uint64_t c[restrict 200],
    uint64_t d[restrict 200])
{

#pragma GCC unroll 16

  for (int i=0; i < 200; i+=2)
    {
      c[i] = a[i] - b[i+1];
      c[i+1] = a[i+1] + b[i];
      d[i] = a[i] - b[i];
      d[i+1] = a[i+1] - b[i+1];
    }
}
# 11 "./vect/complex/vect-complex-add-pattern-unsigned-long.c" 2
