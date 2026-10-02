//type: rp
//options: 
# 0 "./tree-ssa/ldist-rawmemchr-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ldist-rawmemchr-1.c"
# 10 "./tree-ssa/ldist-rawmemchr-1.c"
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
# 11 "./tree-ssa/ldist-rawmemchr-1.c" 2
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



# 12 "./tree-ssa/ldist-rawmemchr-1.c" 2


# 13 "./tree-ssa/ldist-rawmemchr-1.c"
typedef long unsigned int size_t;
extern void* malloc (size_t);
extern void* memset (void*, int, size_t);
# 26 "./tree-ssa/ldist-rawmemchr-1.c"
__attribute__((noinline)) uint8_t *test_uint8_t (uint8_t *p) { while (*p != (uint8_t)0xab) ++p; return p; }
__attribute__((noinline)) uint16_t *test_uint16_t (uint16_t *p) { while (*p != (uint16_t)0xabcd) ++p; return p; }
__attribute__((noinline)) uint32_t *test_uint32_t (uint32_t *p) { while (*p != (uint32_t)0xabcdef15) ++p; return p; }

__attribute__((noinline)) int8_t *test_int8_t (int8_t *p) { while (*p != (int8_t)0xab) ++p; return p; }
__attribute__((noinline)) int16_t *test_int16_t (int16_t *p) { while (*p != (int16_t)0xabcd) ++p; return p; }
__attribute__((noinline)) int32_t *test_int32_t (int32_t *p) { while (*p != (int32_t)0xabcdef15) ++p; return p; }
# 42 "./tree-ssa/ldist-rawmemchr-1.c"
int main(void)
{
  void *p = malloc (1024);
  
# 45 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 45 "./tree-ssa/ldist-rawmemchr-1.c"
 p
# 45 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 45 "./tree-ssa/ldist-rawmemchr-1.c"
 "p"
# 45 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 45, __PRETTY_FUNCTION__))
# 45 "./tree-ssa/ldist-rawmemchr-1.c"
           ;
  memset (p, 0, 1024);

  { uint8_t *q = p; q[0] = (uint8_t)0xab; 
# 48 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 48 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint8_t (p) == &q[0]
# 48 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 48 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint8_t (p) == &q[0]"
# 48 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 48, __PRETTY_FUNCTION__))
# 48 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { uint8_t *q = p; q[1] = (uint8_t)0xab; 
# 49 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 49 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint8_t (p) == &q[1]
# 49 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 49 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint8_t (p) == &q[1]"
# 49 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 49, __PRETTY_FUNCTION__))
# 49 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { uint8_t *q = p; q[13] = (uint8_t)0xab; 
# 50 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 50 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint8_t (p) == &q[13]
# 50 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 50 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint8_t (p) == &q[13]"
# 50 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 50, __PRETTY_FUNCTION__))
# 50 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  { uint16_t *q = p; q[0] = (uint16_t)0xabcd; 
# 52 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 52 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint16_t (p) == &q[0]
# 52 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 52 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint16_t (p) == &q[0]"
# 52 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 52, __PRETTY_FUNCTION__))
# 52 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { uint16_t *q = p; q[1] = (uint16_t)0xabcd; 
# 53 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 53 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint16_t (p) == &q[1]
# 53 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 53 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint16_t (p) == &q[1]"
# 53 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 53, __PRETTY_FUNCTION__))
# 53 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { uint16_t *q = p; q[13] = (uint16_t)0xabcd; 
# 54 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 54 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint16_t (p) == &q[13]
# 54 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 54 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint16_t (p) == &q[13]"
# 54 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 54, __PRETTY_FUNCTION__))
# 54 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  { uint32_t *q = p; q[0] = (uint32_t)0xabcdef15; 
# 56 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 56 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint32_t (p) == &q[0]
# 56 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 56 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint32_t (p) == &q[0]"
# 56 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 56, __PRETTY_FUNCTION__))
# 56 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { uint32_t *q = p; q[1] = (uint32_t)0xabcdef15; 
# 57 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 57 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint32_t (p) == &q[1]
# 57 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 57 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint32_t (p) == &q[1]"
# 57 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 57, __PRETTY_FUNCTION__))
# 57 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { uint32_t *q = p; q[13] = (uint32_t)0xabcdef15; 
# 58 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 58 "./tree-ssa/ldist-rawmemchr-1.c"
 test_uint32_t (p) == &q[13]
# 58 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 58 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_uint32_t (p) == &q[13]"
# 58 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 58, __PRETTY_FUNCTION__))
# 58 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  { int8_t *q = p; q[0] = (int8_t)0xab; 
# 60 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 60 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int8_t (p) == &q[0]
# 60 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 60 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int8_t (p) == &q[0]"
# 60 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 60, __PRETTY_FUNCTION__))
# 60 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { int8_t *q = p; q[1] = (int8_t)0xab; 
# 61 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 61 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int8_t (p) == &q[1]
# 61 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 61 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int8_t (p) == &q[1]"
# 61 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 61, __PRETTY_FUNCTION__))
# 61 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { int8_t *q = p; q[13] = (int8_t)0xab; 
# 62 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 62 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int8_t (p) == &q[13]
# 62 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 62 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int8_t (p) == &q[13]"
# 62 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 62, __PRETTY_FUNCTION__))
# 62 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  { int16_t *q = p; q[0] = (int16_t)0xabcd; 
# 64 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 64 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int16_t (p) == &q[0]
# 64 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 64 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int16_t (p) == &q[0]"
# 64 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 64, __PRETTY_FUNCTION__))
# 64 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { int16_t *q = p; q[1] = (int16_t)0xabcd; 
# 65 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 65 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int16_t (p) == &q[1]
# 65 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 65 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int16_t (p) == &q[1]"
# 65 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 65, __PRETTY_FUNCTION__))
# 65 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { int16_t *q = p; q[13] = (int16_t)0xabcd; 
# 66 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 66 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int16_t (p) == &q[13]
# 66 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 66 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int16_t (p) == &q[13]"
# 66 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 66, __PRETTY_FUNCTION__))
# 66 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  { int32_t *q = p; q[0] = (int32_t)0xabcdef15; 
# 68 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 68 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int32_t (p) == &q[0]
# 68 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 68 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int32_t (p) == &q[0]"
# 68 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 68, __PRETTY_FUNCTION__))
# 68 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[0] = 0; };
  { int32_t *q = p; q[1] = (int32_t)0xabcdef15; 
# 69 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 69 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int32_t (p) == &q[1]
# 69 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 69 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int32_t (p) == &q[1]"
# 69 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 69, __PRETTY_FUNCTION__))
# 69 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[1] = 0; };
  { int32_t *q = p; q[13] = (int32_t)0xabcdef15; 
# 70 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ((
# 70 "./tree-ssa/ldist-rawmemchr-1.c"
 test_int32_t (p) == &q[13]
# 70 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 70 "./tree-ssa/ldist-rawmemchr-1.c"
 "test_int32_t (p) == &q[13]"
# 70 "./tree-ssa/ldist-rawmemchr-1.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-1.c", 70, __PRETTY_FUNCTION__))
# 70 "./tree-ssa/ldist-rawmemchr-1.c"
 ; q[13] = 0; };

  return 0;
}
