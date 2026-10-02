//type: fp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./other/sve_tls_2.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./other/sve_tls_2.C"




# 1 "/home/gnu/gcc-8.2.0/lib/gcc/x86_64-pc-linux-gnu/8.2.0/include/stdint.h" 1 3 4
# 9 "/home/gnu/gcc-8.2.0/lib/gcc/x86_64-pc-linux-gnu/8.2.0/include/stdint.h" 3 4
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
# 10 "/home/gnu/gcc-8.2.0/lib/gcc/x86_64-pc-linux-gnu/8.2.0/include/stdint.h" 2 3 4
# 6 "./other/sve_tls_2.C" 2


# 7 "./other/sve_tls_2.C"
typedef int8_t v32qi __attribute__((vector_size (32)));

extern __thread int z;

void
foo (v32qi *a, int *b)
{
  v32qi x = a[0], y = a[1];
  asm volatile ("" :: "w" ((v32qi) { -1, 0, 0, -1, -1, -1, 0, 0,
         -1, -1, -1, -1, 0, 0, 0, 0,
         -1, -1, -1, -1, -1, -1, -1, -1,
         0, 0, 0, 0, 0, 0, 0, 0 } ? x : y)
  : "memory");
  if (*b)
    {
      x = a[2], y = a[3];
      asm volatile ("" :: "w" ((v32qi) { -1, 0, 0, -1, -1, -1, 0, 0,
      -1, -1, -1, -1, 0, 0, 0, 0,
      -1, -1, -1, -1, -1, -1, -1, -1,
      0, 0, 0, 0, 0, 0, 0, 0 } ? x : y),
      "r" (z));
    }
}
