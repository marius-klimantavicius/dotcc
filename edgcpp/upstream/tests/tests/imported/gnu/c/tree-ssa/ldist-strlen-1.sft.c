//type: rp
//options: 
# 0 "./tree-ssa/ldist-strlen-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ldist-strlen-1.c"







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
# 9 "./tree-ssa/ldist-strlen-1.c" 2
# 1 "/usr/include/assert.h" 1 3 4
# 65 "/usr/include/assert.h" 3 4



extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



# 10 "./tree-ssa/ldist-strlen-1.c" 2


# 11 "./tree-ssa/ldist-strlen-1.c"
typedef long unsigned int size_t;
extern void* malloc (size_t);
extern void* memset (void*, int, size_t);
# 24 "./tree-ssa/ldist-strlen-1.c"
__attribute__((noinline)) size_t test_uint8_tsize_t (uint8_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) size_t test_uint16_tsize_t (uint16_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) size_t test_uint32_tsize_t (uint32_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_uint8_tint (uint8_t *s) { int i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_uint16_tint (uint16_t *s) { int i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_uint32_tint (uint32_t *s) { int i; for (i=0; s[i]; ++i); return i; }

__attribute__((noinline)) size_t test_int8_tsize_t (int8_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) size_t test_int16_tsize_t (int16_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) size_t test_int32_tsize_t (int32_t *s) { size_t i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_int8_tint (int8_t *s) { int i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_int16_tint (int16_t *s) { int i; for (i=0; s[i]; ++i); return i; }
__attribute__((noinline)) int test_int32_tint (int32_t *s) { int i; for (i=0; s[i]; ++i); return i; }
# 46 "./tree-ssa/ldist-strlen-1.c"
int main(void)
{
  void *p = malloc (1024);
  
# 49 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 49 "./tree-ssa/ldist-strlen-1.c"
 p
# 49 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 49 "./tree-ssa/ldist-strlen-1.c"
 "p"
# 49 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 49, __PRETTY_FUNCTION__))
# 49 "./tree-ssa/ldist-strlen-1.c"
           ;
  memset (p, 0xf, 1024);

  { uint8_t *q = p; q[0] = 0; 
# 52 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 52 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tsize_t (p) == 0
# 52 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 52 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tsize_t (p) == 0"
# 52 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 52, __PRETTY_FUNCTION__))
# 52 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint8_t)); };
  { uint8_t *q = p; q[1] = 0; 
# 53 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 53 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tsize_t (p) == 1
# 53 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 53 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tsize_t (p) == 1"
# 53 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 53, __PRETTY_FUNCTION__))
# 53 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint8_t)); };
  { uint8_t *q = p; q[13] = 0; 
# 54 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 54 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tsize_t (p) == 13
# 54 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 54 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tsize_t (p) == 13"
# 54 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 54, __PRETTY_FUNCTION__))
# 54 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint8_t)); };

  { int8_t *q = p; q[0] = 0; 
# 56 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 56 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tsize_t (p) == 0
# 56 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 56 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tsize_t (p) == 0"
# 56 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 56, __PRETTY_FUNCTION__))
# 56 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int8_t)); };
  { int8_t *q = p; q[1] = 0; 
# 57 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 57 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tsize_t (p) == 1
# 57 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 57 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tsize_t (p) == 1"
# 57 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 57, __PRETTY_FUNCTION__))
# 57 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int8_t)); };
  { int8_t *q = p; q[13] = 0; 
# 58 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 58 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tsize_t (p) == 13
# 58 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 58 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tsize_t (p) == 13"
# 58 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 58, __PRETTY_FUNCTION__))
# 58 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int8_t)); };

  { uint8_t *q = p; q[0] = 0; 
# 60 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 60 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tint (p) == 0
# 60 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 60 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tint (p) == 0"
# 60 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 60, __PRETTY_FUNCTION__))
# 60 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint8_t)); };
  { uint8_t *q = p; q[1] = 0; 
# 61 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 61 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tint (p) == 1
# 61 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 61 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tint (p) == 1"
# 61 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 61, __PRETTY_FUNCTION__))
# 61 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint8_t)); };
  { uint8_t *q = p; q[13] = 0; 
# 62 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 62 "./tree-ssa/ldist-strlen-1.c"
 test_uint8_tint (p) == 13
# 62 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 62 "./tree-ssa/ldist-strlen-1.c"
 "test_uint8_tint (p) == 13"
# 62 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 62, __PRETTY_FUNCTION__))
# 62 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint8_t)); };

  { int8_t *q = p; q[0] = 0; 
# 64 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 64 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tint (p) == 0
# 64 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 64 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tint (p) == 0"
# 64 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 64, __PRETTY_FUNCTION__))
# 64 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int8_t)); };
  { int8_t *q = p; q[1] = 0; 
# 65 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 65 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tint (p) == 1
# 65 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 65 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tint (p) == 1"
# 65 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 65, __PRETTY_FUNCTION__))
# 65 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int8_t)); };
  { int8_t *q = p; q[13] = 0; 
# 66 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 66 "./tree-ssa/ldist-strlen-1.c"
 test_int8_tint (p) == 13
# 66 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 66 "./tree-ssa/ldist-strlen-1.c"
 "test_int8_tint (p) == 13"
# 66 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 66, __PRETTY_FUNCTION__))
# 66 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int8_t)); };

  { uint16_t *q = p; q[0] = 0; 
# 68 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 68 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tsize_t (p) == 0
# 68 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 68 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tsize_t (p) == 0"
# 68 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 68, __PRETTY_FUNCTION__))
# 68 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint16_t)); };
  { uint16_t *q = p; q[1] = 0; 
# 69 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 69 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tsize_t (p) == 1
# 69 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 69 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tsize_t (p) == 1"
# 69 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 69, __PRETTY_FUNCTION__))
# 69 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint16_t)); };
  { uint16_t *q = p; q[13] = 0; 
# 70 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 70 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tsize_t (p) == 13
# 70 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 70 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tsize_t (p) == 13"
# 70 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 70, __PRETTY_FUNCTION__))
# 70 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint16_t)); };

  { int16_t *q = p; q[0] = 0; 
# 72 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 72 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tsize_t (p) == 0
# 72 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 72 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tsize_t (p) == 0"
# 72 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 72, __PRETTY_FUNCTION__))
# 72 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int16_t)); };
  { int16_t *q = p; q[1] = 0; 
# 73 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 73 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tsize_t (p) == 1
# 73 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 73 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tsize_t (p) == 1"
# 73 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 73, __PRETTY_FUNCTION__))
# 73 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int16_t)); };
  { int16_t *q = p; q[13] = 0; 
# 74 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 74 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tsize_t (p) == 13
# 74 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 74 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tsize_t (p) == 13"
# 74 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 74, __PRETTY_FUNCTION__))
# 74 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int16_t)); };

  { uint16_t *q = p; q[0] = 0; 
# 76 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 76 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tint (p) == 0
# 76 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 76 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tint (p) == 0"
# 76 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 76, __PRETTY_FUNCTION__))
# 76 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint16_t)); };
  { uint16_t *q = p; q[1] = 0; 
# 77 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 77 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tint (p) == 1
# 77 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 77 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tint (p) == 1"
# 77 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 77, __PRETTY_FUNCTION__))
# 77 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint16_t)); };
  { uint16_t *q = p; q[13] = 0; 
# 78 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 78 "./tree-ssa/ldist-strlen-1.c"
 test_uint16_tint (p) == 13
# 78 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 78 "./tree-ssa/ldist-strlen-1.c"
 "test_uint16_tint (p) == 13"
# 78 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 78, __PRETTY_FUNCTION__))
# 78 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint16_t)); };

  { int16_t *q = p; q[0] = 0; 
# 80 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 80 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tint (p) == 0
# 80 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 80 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tint (p) == 0"
# 80 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 80, __PRETTY_FUNCTION__))
# 80 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int16_t)); };
  { int16_t *q = p; q[1] = 0; 
# 81 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 81 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tint (p) == 1
# 81 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 81 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tint (p) == 1"
# 81 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 81, __PRETTY_FUNCTION__))
# 81 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int16_t)); };
  { int16_t *q = p; q[13] = 0; 
# 82 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 82 "./tree-ssa/ldist-strlen-1.c"
 test_int16_tint (p) == 13
# 82 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 82 "./tree-ssa/ldist-strlen-1.c"
 "test_int16_tint (p) == 13"
# 82 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 82, __PRETTY_FUNCTION__))
# 82 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int16_t)); };

  { uint32_t *q = p; q[0] = 0; 
# 84 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 84 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tsize_t (p) == 0
# 84 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 84 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tsize_t (p) == 0"
# 84 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 84, __PRETTY_FUNCTION__))
# 84 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint32_t)); };
  { uint32_t *q = p; q[1] = 0; 
# 85 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 85 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tsize_t (p) == 1
# 85 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 85 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tsize_t (p) == 1"
# 85 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 85, __PRETTY_FUNCTION__))
# 85 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint32_t)); };
  { uint32_t *q = p; q[13] = 0; 
# 86 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 86 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tsize_t (p) == 13
# 86 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 86 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tsize_t (p) == 13"
# 86 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 86, __PRETTY_FUNCTION__))
# 86 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint32_t)); };

  { int32_t *q = p; q[0] = 0; 
# 88 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 88 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tsize_t (p) == 0
# 88 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 88 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tsize_t (p) == 0"
# 88 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 88, __PRETTY_FUNCTION__))
# 88 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int32_t)); };
  { int32_t *q = p; q[1] = 0; 
# 89 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 89 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tsize_t (p) == 1
# 89 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 89 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tsize_t (p) == 1"
# 89 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 89, __PRETTY_FUNCTION__))
# 89 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int32_t)); };
  { int32_t *q = p; q[13] = 0; 
# 90 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 90 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tsize_t (p) == 13
# 90 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 90 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tsize_t (p) == 13"
# 90 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 90, __PRETTY_FUNCTION__))
# 90 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int32_t)); };

  { uint32_t *q = p; q[0] = 0; 
# 92 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 92 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tint (p) == 0
# 92 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 92 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tint (p) == 0"
# 92 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 92, __PRETTY_FUNCTION__))
# 92 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (uint32_t)); };
  { uint32_t *q = p; q[1] = 0; 
# 93 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 93 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tint (p) == 1
# 93 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 93 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tint (p) == 1"
# 93 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 93, __PRETTY_FUNCTION__))
# 93 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (uint32_t)); };
  { uint32_t *q = p; q[13] = 0; 
# 94 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 94 "./tree-ssa/ldist-strlen-1.c"
 test_uint32_tint (p) == 13
# 94 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 94 "./tree-ssa/ldist-strlen-1.c"
 "test_uint32_tint (p) == 13"
# 94 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 94, __PRETTY_FUNCTION__))
# 94 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (uint32_t)); };

  { int32_t *q = p; q[0] = 0; 
# 96 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 96 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tint (p) == 0
# 96 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 96 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tint (p) == 0"
# 96 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 96, __PRETTY_FUNCTION__))
# 96 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[0], 0xf, sizeof (int32_t)); };
  { int32_t *q = p; q[1] = 0; 
# 97 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 97 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tint (p) == 1
# 97 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 97 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tint (p) == 1"
# 97 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 97, __PRETTY_FUNCTION__))
# 97 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[1], 0xf, sizeof (int32_t)); };
  { int32_t *q = p; q[13] = 0; 
# 98 "./tree-ssa/ldist-strlen-1.c" 3 4
 ((
# 98 "./tree-ssa/ldist-strlen-1.c"
 test_int32_tint (p) == 13
# 98 "./tree-ssa/ldist-strlen-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 98 "./tree-ssa/ldist-strlen-1.c"
 "test_int32_tint (p) == 13"
# 98 "./tree-ssa/ldist-strlen-1.c" 3 4
 , "./tree-ssa/ldist-strlen-1.c", 98, __PRETTY_FUNCTION__))
# 98 "./tree-ssa/ldist-strlen-1.c"
 ; memset (&q[13], 0xf, sizeof (int32_t)); };

  return 0;
}
