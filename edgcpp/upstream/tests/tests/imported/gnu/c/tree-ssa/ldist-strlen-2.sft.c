//type: rp
//options: 
# 0 "./tree-ssa/ldist-strlen-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/ldist-strlen-2.c"




# 1 "/usr/include/assert.h" 1 3 4
# 36 "/usr/include/assert.h" 3 4
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
# 37 "/usr/include/assert.h" 2 3 4
# 65 "/usr/include/assert.h" 3 4




# 68 "/usr/include/assert.h" 3 4
extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



# 6 "./tree-ssa/ldist-strlen-2.c" 2


# 7 "./tree-ssa/ldist-strlen-2.c"
typedef long unsigned int size_t;
extern void* malloc (size_t);
extern void* memset (void*, int, size_t);

__attribute__((noinline))
int test_pos (char *s)
{
  int i;
  for (i=42; s[i]; ++i);
  return i;
}

__attribute__((noinline))
int test_neg (char *s)
{
  int i;
  for (i=-42; s[i]; ++i);
  return i;
}

__attribute__((noinline))
int test_including_null_char (char *s)
{
  int i;
  for (i=1; s[i-1]; ++i);
  return i;
}

int main(void)
{
  void *p = malloc (1024);
  
# 38 "./tree-ssa/ldist-strlen-2.c" 3 4
 ((
# 38 "./tree-ssa/ldist-strlen-2.c"
 p
# 38 "./tree-ssa/ldist-strlen-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 38 "./tree-ssa/ldist-strlen-2.c"
 "p"
# 38 "./tree-ssa/ldist-strlen-2.c" 3 4
 , "./tree-ssa/ldist-strlen-2.c", 38, __PRETTY_FUNCTION__))
# 38 "./tree-ssa/ldist-strlen-2.c"
           ;
  memset (p, 0xf, 1024);
  char *s = (char *)p + 100;

  s[42+13] = 0;
  
# 43 "./tree-ssa/ldist-strlen-2.c" 3 4
 ((
# 43 "./tree-ssa/ldist-strlen-2.c"
 test_pos (s) == 42+13
# 43 "./tree-ssa/ldist-strlen-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 43 "./tree-ssa/ldist-strlen-2.c"
 "test_pos (s) == 42+13"
# 43 "./tree-ssa/ldist-strlen-2.c" 3 4
 , "./tree-ssa/ldist-strlen-2.c", 43, __PRETTY_FUNCTION__))
# 43 "./tree-ssa/ldist-strlen-2.c"
                               ;
  s[42+13] = 0xf;

  s[13] = 0;
  
# 47 "./tree-ssa/ldist-strlen-2.c" 3 4
 ((
# 47 "./tree-ssa/ldist-strlen-2.c"
 test_neg (s) == 13
# 47 "./tree-ssa/ldist-strlen-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 47 "./tree-ssa/ldist-strlen-2.c"
 "test_neg (s) == 13"
# 47 "./tree-ssa/ldist-strlen-2.c" 3 4
 , "./tree-ssa/ldist-strlen-2.c", 47, __PRETTY_FUNCTION__))
# 47 "./tree-ssa/ldist-strlen-2.c"
                            ;
  s[13] = 0xf;

  s[-13] = 0;
  
# 51 "./tree-ssa/ldist-strlen-2.c" 3 4
 ((
# 51 "./tree-ssa/ldist-strlen-2.c"
 test_neg (s) == -13
# 51 "./tree-ssa/ldist-strlen-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 51 "./tree-ssa/ldist-strlen-2.c"
 "test_neg (s) == -13"
# 51 "./tree-ssa/ldist-strlen-2.c" 3 4
 , "./tree-ssa/ldist-strlen-2.c", 51, __PRETTY_FUNCTION__))
# 51 "./tree-ssa/ldist-strlen-2.c"
                             ;
  s[-13] = 0xf;

  s[13] = 0;
  
# 55 "./tree-ssa/ldist-strlen-2.c" 3 4
 ((
# 55 "./tree-ssa/ldist-strlen-2.c"
 test_including_null_char (s) == 13+1
# 55 "./tree-ssa/ldist-strlen-2.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 55 "./tree-ssa/ldist-strlen-2.c"
 "test_including_null_char (s) == 13+1"
# 55 "./tree-ssa/ldist-strlen-2.c" 3 4
 , "./tree-ssa/ldist-strlen-2.c", 55, __PRETTY_FUNCTION__))
# 55 "./tree-ssa/ldist-strlen-2.c"
                                              ;

  return 0;
}
