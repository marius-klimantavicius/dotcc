//type: fp
//options: 
# 0 "./tree-ssa/pr116024.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr116024.c"




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
# 6 "./tree-ssa/pr116024.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
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
# 7 "./tree-ssa/pr116024.c" 2


# 8 "./tree-ssa/pr116024.c"
uint32_t f(void);

int32_t i1(void)
{
  int32_t l = 10 - (int32_t)f();
  return l <= 9;
}

int32_t i1a(void)
{
  int32_t l = 20 - (int32_t)f();
  return l <= 
# 19 "./tree-ssa/pr116024.c" 3 4
             (-2147483647-1)
# 19 "./tree-ssa/pr116024.c"
                      ;
}

int32_t i1b(void)
{
  int32_t l = 30 - (int32_t)f();
  return l <= 
# 25 "./tree-ssa/pr116024.c" 3 4
             (-2147483647-1) 
# 25 "./tree-ssa/pr116024.c"
                       + 31;
}

int32_t i1c(void)
{
  int32_t l = 
# 30 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 30 "./tree-ssa/pr116024.c"
                       - 40 - (int32_t)f();
  return l <= -38;
}

int32_t i1d(void)
{
  int32_t l = 
# 36 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 36 "./tree-ssa/pr116024.c"
                       - 50 - (int32_t)f();
  return l <= 
# 37 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 37 "./tree-ssa/pr116024.c"
                       - 1;
}

int32_t i1e(void)
{
  int32_t l = 
# 42 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 42 "./tree-ssa/pr116024.c"
                       - 60 - (int32_t)f();
  return l != 
# 43 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 43 "./tree-ssa/pr116024.c"
                       - 90;
}

int32_t i1f(void)
{
  int32_t l = 
# 48 "./tree-ssa/pr116024.c" 3 4
             (-2147483647-1) 
# 48 "./tree-ssa/pr116024.c"
                       + 70 - (int32_t)f();
  return l <= 
# 49 "./tree-ssa/pr116024.c" 3 4
             (2147483647) 
# 49 "./tree-ssa/pr116024.c"
                       - 2;
}

int32_t i1g(void)
{
  int32_t l = 
# 54 "./tree-ssa/pr116024.c" 3 4
             (2147483647)
# 54 "./tree-ssa/pr116024.c"
                      /2 + 30 - (int32_t)f();
  return l <= 
# 55 "./tree-ssa/pr116024.c" 3 4
             (-2147483647-1)
# 55 "./tree-ssa/pr116024.c"
                      /2 - 30;
}
