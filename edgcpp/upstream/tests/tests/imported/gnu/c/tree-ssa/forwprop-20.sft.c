//type: fp
//options: 
# 0 "./tree-ssa/forwprop-20.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/forwprop-20.c"




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
# 6 "./tree-ssa/forwprop-20.c" 2







# 12 "./tree-ssa/forwprop-20.c"
typedef double vecf __attribute__ ((vector_size (2 * sizeof (double))));
typedef int64_t veci __attribute__ ((vector_size (2 * sizeof (int64_t))));

void f (double d, vecf* r)
{
  vecf x = { -d, 5 };
  vecf y = { 1, 4 };
  veci m = { 2, 0 };
  *r = __builtin_shuffle (x, y, m);
}

void g (float d, vecf* r)
{
  vecf x = { d, 5 };
  vecf y = { 1, 4 };
  veci m = { 2, 1 };
  *r = __builtin_shuffle (x, y, m);
}

void h (double d, vecf* r)
{
  vecf x = { d + 1, 5 };
  vecf y = { 1 , 4 };
  veci m = { 2 , 0 };
  *r = __builtin_shuffle (y, x, m);
}

void i (float d, vecf* r)
{
  vecf x = { d, 5 };
  veci m = { 1, 0 };
  *r = __builtin_shuffle (x, m);
}

void j (vecf* r)
{
  vecf y = { 1, 2 };
  veci m = { 0, 0 };
  *r = __builtin_shuffle (y, m);
}

void k (vecf* r)
{
  vecf x = { 3, 4 };
  vecf y = { 1, 2 };
  veci m = { 3, 0 };
  *r = __builtin_shuffle (x, y, m);
}

void l (double d, vecf* r)
{
  vecf x = { -d, 5 };
  vecf y = { d, 4 };
  veci m = { 2, 0 };
  *r = __builtin_shuffle (x, y, m);
}
