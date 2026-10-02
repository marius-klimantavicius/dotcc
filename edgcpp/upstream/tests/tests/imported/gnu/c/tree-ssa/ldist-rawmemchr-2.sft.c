//type: rp
//options: 
# 0 "./tree-ssa/ldist-rawmemchr-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ldist-rawmemchr-2.c"
# 10 "./tree-ssa/ldist-rawmemchr-2.c"
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
# 11 "./tree-ssa/ldist-rawmemchr-2.c" 2
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



# 12 "./tree-ssa/ldist-rawmemchr-2.c" 2


# 13 "./tree-ssa/ldist-rawmemchr-2.c"
typedef long unsigned int size_t;
extern void* malloc (size_t);
extern void* memset (void*, int, size_t);

uint8_t *p_uint8_t;
uint16_t *p_uint16_t;
uint32_t *p_uint32_t;

int8_t *p_int8_t;
int16_t *p_int16_t;
int32_t *p_int32_t;
# 34 "./tree-ssa/ldist-rawmemchr-2.c"
__attribute__((noinline)) uint8_t *test_uint8_t (void) { while (*p_uint8_t != 0xab) ++p_uint8_t; return p_uint8_t; }
__attribute__((noinline)) uint16_t *test_uint16_t (void) { while (*p_uint16_t != 0xabcd) ++p_uint16_t; return p_uint16_t; }
__attribute__((noinline)) uint32_t *test_uint32_t (void) { while (*p_uint32_t != 0xabcdef15) ++p_uint32_t; return p_uint32_t; }

__attribute__((noinline)) int8_t *test_int8_t (void) { while (*p_int8_t != (int8_t)0xab) ++p_int8_t; return p_int8_t; }
__attribute__((noinline)) int16_t *test_int16_t (void) { while (*p_int16_t != (int16_t)0xabcd) ++p_int16_t; return p_int16_t; }
__attribute__((noinline)) int32_t *test_int32_t (void) { while (*p_int32_t != (int32_t)0xabcdef15) ++p_int32_t; return p_int32_t; }
# 53 "./tree-ssa/ldist-rawmemchr-2.c"
int main(void)
{
  void *p = malloc (1024);
  
# 56 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 56 "./tree-ssa/ldist-rawmemchr-2.c"
 p
# 56 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 56 "./tree-ssa/ldist-rawmemchr-2.c"
 "p"
# 56 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 56, __PRETTY_FUNCTION__))
# 56 "./tree-ssa/ldist-rawmemchr-2.c"
           ;
  memset (p, '\0', 1024);

  { uint8_t *q = p; q[0] = 0xab; p_uint8_t = p; uint8_t *r = test_uint8_t (); 
# 59 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 59 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint8_t
# 59 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 59 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint8_t"
# 59 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 59, __PRETTY_FUNCTION__))
# 59 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 59 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 59 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 59 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 59, __PRETTY_FUNCTION__))
# 59 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { uint8_t *q = p; q[1] = 0xab; p_uint8_t = p; uint8_t *r = test_uint8_t (); 
# 60 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 60 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint8_t
# 60 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 60 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint8_t"
# 60 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 60, __PRETTY_FUNCTION__))
# 60 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 60 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 60 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 60 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 60, __PRETTY_FUNCTION__))
# 60 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { uint8_t *q = p; q[13] = 0xab; p_uint8_t = p; uint8_t *r = test_uint8_t (); 
# 61 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 61 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint8_t
# 61 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 61 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint8_t"
# 61 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 61, __PRETTY_FUNCTION__))
# 61 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 61 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 61 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 61 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 61, __PRETTY_FUNCTION__))
# 61 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  { uint16_t *q = p; q[0] = 0xabcd; p_uint16_t = p; uint16_t *r = test_uint16_t (); 
# 63 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 63 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint16_t
# 63 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 63 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint16_t"
# 63 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 63, __PRETTY_FUNCTION__))
# 63 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 63 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 63 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 63 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 63, __PRETTY_FUNCTION__))
# 63 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { uint16_t *q = p; q[1] = 0xabcd; p_uint16_t = p; uint16_t *r = test_uint16_t (); 
# 64 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 64 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint16_t
# 64 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 64 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint16_t"
# 64 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 64, __PRETTY_FUNCTION__))
# 64 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 64 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 64 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 64 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 64, __PRETTY_FUNCTION__))
# 64 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { uint16_t *q = p; q[13] = 0xabcd; p_uint16_t = p; uint16_t *r = test_uint16_t (); 
# 65 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 65 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint16_t
# 65 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 65 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint16_t"
# 65 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 65, __PRETTY_FUNCTION__))
# 65 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 65 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 65 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 65 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 65, __PRETTY_FUNCTION__))
# 65 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  { uint32_t *q = p; q[0] = 0xabcdef15; p_uint32_t = p; uint32_t *r = test_uint32_t (); 
# 67 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 67 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint32_t
# 67 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 67 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint32_t"
# 67 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 67, __PRETTY_FUNCTION__))
# 67 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 67 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 67 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 67 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 67, __PRETTY_FUNCTION__))
# 67 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { uint32_t *q = p; q[1] = 0xabcdef15; p_uint32_t = p; uint32_t *r = test_uint32_t (); 
# 68 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 68 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint32_t
# 68 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 68 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint32_t"
# 68 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 68, __PRETTY_FUNCTION__))
# 68 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 68 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 68 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 68 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 68, __PRETTY_FUNCTION__))
# 68 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { uint32_t *q = p; q[13] = 0xabcdef15; p_uint32_t = p; uint32_t *r = test_uint32_t (); 
# 69 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 69 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_uint32_t
# 69 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 69 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_uint32_t"
# 69 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 69, __PRETTY_FUNCTION__))
# 69 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 69 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 69 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 69 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 69, __PRETTY_FUNCTION__))
# 69 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  { int8_t *q = p; q[0] = (int8_t)0xab; p_int8_t = p; int8_t *r = test_int8_t (); 
# 71 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 71 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int8_t
# 71 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 71 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int8_t"
# 71 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 71, __PRETTY_FUNCTION__))
# 71 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 71 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 71 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 71 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 71, __PRETTY_FUNCTION__))
# 71 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { int8_t *q = p; q[1] = (int8_t)0xab; p_int8_t = p; int8_t *r = test_int8_t (); 
# 72 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 72 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int8_t
# 72 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 72 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int8_t"
# 72 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 72, __PRETTY_FUNCTION__))
# 72 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 72 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 72 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 72 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 72, __PRETTY_FUNCTION__))
# 72 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { int8_t *q = p; q[13] = (int8_t)0xab; p_int8_t = p; int8_t *r = test_int8_t (); 
# 73 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 73 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int8_t
# 73 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 73 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int8_t"
# 73 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 73, __PRETTY_FUNCTION__))
# 73 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 73 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 73 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 73 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 73, __PRETTY_FUNCTION__))
# 73 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  { int16_t *q = p; q[0] = (int16_t)0xabcd; p_int16_t = p; int16_t *r = test_int16_t (); 
# 75 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 75 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int16_t
# 75 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 75 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int16_t"
# 75 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 75, __PRETTY_FUNCTION__))
# 75 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 75 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 75 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 75 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 75, __PRETTY_FUNCTION__))
# 75 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { int16_t *q = p; q[1] = (int16_t)0xabcd; p_int16_t = p; int16_t *r = test_int16_t (); 
# 76 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 76 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int16_t
# 76 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 76 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int16_t"
# 76 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 76, __PRETTY_FUNCTION__))
# 76 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 76 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 76 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 76 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 76, __PRETTY_FUNCTION__))
# 76 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { int16_t *q = p; q[13] = (int16_t)0xabcd; p_int16_t = p; int16_t *r = test_int16_t (); 
# 77 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 77 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int16_t
# 77 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 77 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int16_t"
# 77 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 77, __PRETTY_FUNCTION__))
# 77 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 77 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 77 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 77 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 77, __PRETTY_FUNCTION__))
# 77 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  { int32_t *q = p; q[0] = (int32_t)0xabcdef15; p_int32_t = p; int32_t *r = test_int32_t (); 
# 79 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 79 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int32_t
# 79 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 79 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int32_t"
# 79 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 79, __PRETTY_FUNCTION__))
# 79 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 79 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 79 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[0]
# 79 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[0]", "./tree-ssa/ldist-rawmemchr-2.c", 79, __PRETTY_FUNCTION__))
# 79 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[0] = 0; };
  { int32_t *q = p; q[1] = (int32_t)0xabcdef15; p_int32_t = p; int32_t *r = test_int32_t (); 
# 80 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 80 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int32_t
# 80 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 80 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int32_t"
# 80 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 80, __PRETTY_FUNCTION__))
# 80 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 80 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 80 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[1]
# 80 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[1]", "./tree-ssa/ldist-rawmemchr-2.c", 80, __PRETTY_FUNCTION__))
# 80 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[1] = 0; };
  { int32_t *q = p; q[13] = (int32_t)0xabcdef15; p_int32_t = p; int32_t *r = test_int32_t (); 
# 81 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 81 "./tree-ssa/ldist-rawmemchr-2.c"
 r == p_int32_t
# 81 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 81 "./tree-ssa/ldist-rawmemchr-2.c"
 "r == p_int32_t"
# 81 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 , "./tree-ssa/ldist-rawmemchr-2.c", 81, __PRETTY_FUNCTION__))
# 81 "./tree-ssa/ldist-rawmemchr-2.c"
 ; 
# 81 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ((
# 81 "./tree-ssa/ldist-rawmemchr-2.c"
 r == &q[13]
# 81 "./tree-ssa/ldist-rawmemchr-2.c" 3 4
 ) ? (void) (0) : __assert_fail ("r == &q[13]", "./tree-ssa/ldist-rawmemchr-2.c", 81, __PRETTY_FUNCTION__))
# 81 "./tree-ssa/ldist-rawmemchr-2.c"
 ; q[13] = 0; };

  return 0;
}
