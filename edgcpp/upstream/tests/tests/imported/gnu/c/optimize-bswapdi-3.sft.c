//type: fp
//options: 
# 0 "./optimize-bswapdi-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./optimize-bswapdi-3.c"






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
# 8 "./optimize-bswapdi-3.c" 2


# 9 "./optimize-bswapdi-3.c"
unsigned char data[8];

struct uint64_st {
  unsigned char u0, u1, u2, u3, u4, u5, u6, u7;
};

uint64_t read_le64_1 (void)
{
  return (uint64_t) data[0] | ((uint64_t) data[1] << 8)
  | ((uint64_t) data[2] << 16) | ((uint64_t) data[3] << 24)
  | ((uint64_t) data[4] << 32) | ((uint64_t) data[5] << 40)
  | ((uint64_t) data[6] << 48) | ((uint64_t) data[7] << 56);
}

uint64_t read_le64_2 (struct uint64_st data)
{
  return (uint64_t) data.u0 | ((uint64_t) data.u1 << 8)
  | ((uint64_t) data.u2 << 16) | ((uint64_t) data.u3 << 24)
  | ((uint64_t) data.u4 << 32) | ((uint64_t) data.u5 << 40)
  | ((uint64_t) data.u6 << 48) | ((uint64_t) data.u7 << 56);
}

uint64_t read_le64_3 (unsigned char *data)
{
  return (uint64_t) *data | ((uint64_t) *(data + 1) << 8)
  | ((uint64_t) *(data + 2) << 16) | ((uint64_t) *(data + 3) << 24)
  | ((uint64_t) *(data + 4) << 32) | ((uint64_t) *(data + 5) << 40)
  | ((uint64_t) *(data + 6) << 48) | ((uint64_t) *(data + 7) << 56);
}

uint64_t read_be64_1 (void)
{
  return (uint64_t) data[7] | ((uint64_t) data[6] << 8)
  | ((uint64_t) data[5] << 16) | ((uint64_t) data[4] << 24)
  | ((uint64_t) data[3] << 32) | ((uint64_t) data[2] << 40)
  | ((uint64_t) data[1] << 48) | ((uint64_t) data[0] << 56);
}

uint64_t read_be64_2 (struct uint64_st data)
{
  return (uint64_t) data.u7 | ((uint64_t) data.u6 << 8)
  | ((uint64_t) data.u5 << 16) | ((uint64_t) data.u4 << 24)
  | ((uint64_t) data.u3 << 32) | ((uint64_t) data.u2 << 40)
  | ((uint64_t) data.u1 << 48) | ((uint64_t) data.u0 << 56);
}

uint64_t read_be64_3 (unsigned char *data)
{
  return (uint64_t) *(data + 7) | ((uint64_t) *(data + 6) << 8)
  | ((uint64_t) *(data + 5) << 16) | ((uint64_t) *(data + 4) << 24)
  | ((uint64_t) *(data + 3) << 32) | ((uint64_t) *(data + 2) << 40)
  | ((uint64_t) *(data + 1) << 48) | ((uint64_t) *data << 56);
}
