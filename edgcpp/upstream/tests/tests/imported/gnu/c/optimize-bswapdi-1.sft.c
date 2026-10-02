//type: fp
//options: 
# 0 "./optimize-bswapdi-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./optimize-bswapdi-1.c"






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
# 8 "./optimize-bswapdi-1.c" 2
# 22 "./optimize-bswapdi-1.c"

# 22 "./optimize-bswapdi-1.c"
uint64_t
swap64 (uint64_t in)
{
  return ((uint64_t)( (((uint64_t)(in) & (uint64_t)0x00000000000000ffULL) << 56) | (((uint64_t)(in) & (uint64_t)0x000000000000ff00ULL) << 40) | (((uint64_t)(in) & (uint64_t)0x0000000000ff0000ULL) << 24) | (((uint64_t)(in) & (uint64_t)0x00000000ff000000ULL) << 8) | (((uint64_t)(in) & (uint64_t)0x000000ff00000000ULL) >> 8) | (((uint64_t)(in) & (uint64_t)0x0000ff0000000000ULL) >> 24) | (((uint64_t)(in) & (uint64_t)0x00ff000000000000ULL) >> 40) | (((uint64_t)(in) & (uint64_t)0xff00000000000000ULL) >> 56)));
}






typedef int DItype __attribute__ ((mode (DI)));
DItype
swap64_b (DItype u)
{
  return ((((u) & 0xff00000000000000ull) >> 56)
   | (((u) & 0x00ff000000000000ull) >> 40)
   | (((u) & 0x0000ff0000000000ull) >> 24)
   | (((u) & 0x000000ff00000000ull) >> 8)
   | (((u) & 0x00000000ff000000ull) << 8)
   | (((u) & 0x0000000000ff0000ull) << 24)
   | (((u) & 0x000000000000ff00ull) << 40)
   | (((u) & 0x00000000000000ffull) << 56));
}



uint64_t
swap64_c (uint64_t x)
{
  uint32_t a = x >> 32;
  uint32_t b = (uint32_t) x;
  return ((uint64_t) ((((((b)) >> (8)) | (((b)) << (32 - (8)))) & 0xff00ff00L)
        | (((((b)) << (8)) | (((b)) >> (32 - (8)))) & 0x00ff00ffL)) << 32)
          | (uint64_t) ((((((a)) >> (8)) | (((a)) << (32 - (8)))) & 0xff00ff00L)
   | (((((a)) << (8)) | (((a)) >> (32 - (8)))) & 0x00ff00ffL));
}
