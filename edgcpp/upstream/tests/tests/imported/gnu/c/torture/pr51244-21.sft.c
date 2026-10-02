//type: rp
//options: 
# 0 "./torture/pr51244-21.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr51244-21.c"

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



# 3 "./torture/pr51244-21.c" 2


# 4 "./torture/pr51244-21.c"
static inline int
blk_oversized_queue (int* q)
{
  if (q[2])
    return q[1] != 0;
  return q[0] == 0;
}

int __attribute__ ((noinline))
get_request (int* q, int rw)
{
  if (blk_oversized_queue (q))
    {
      if ((rw == 1) || (rw == 0))
 return -33;

      return 0;
    }

  return -100;
}

int main (void)
{
  int x[3];
  int r;

  x[0] = 0; x[1] = 1; x[2] = 1;
  r = get_request (x, 0);
  
# 33 "./torture/pr51244-21.c" 3 4
 ((
# 33 "./torture/pr51244-21.c"
 r == -33
# 33 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 33 "./torture/pr51244-21.c"
 "r == -33"
# 33 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 33, __PRETTY_FUNCTION__))
# 33 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 1);
  
# 36 "./torture/pr51244-21.c" 3 4
 ((
# 36 "./torture/pr51244-21.c"
 r == -33
# 36 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 36 "./torture/pr51244-21.c"
 "r == -33"
# 36 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 36, __PRETTY_FUNCTION__))
# 36 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 2);
  
# 39 "./torture/pr51244-21.c" 3 4
 ((
# 39 "./torture/pr51244-21.c"
 r == 0
# 39 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 39 "./torture/pr51244-21.c"
 "r == 0"
# 39 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 39, __PRETTY_FUNCTION__))
# 39 "./torture/pr51244-21.c"
                ;


  x[0] = 0; x[1] = 0; x[2] = 1;
  r = get_request (x, 0);
  
# 44 "./torture/pr51244-21.c" 3 4
 ((
# 44 "./torture/pr51244-21.c"
 r == -100
# 44 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 44 "./torture/pr51244-21.c"
 "r == -100"
# 44 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 44, __PRETTY_FUNCTION__))
# 44 "./torture/pr51244-21.c"
                   ;

  r = get_request (x, 1);
  
# 47 "./torture/pr51244-21.c" 3 4
 ((
# 47 "./torture/pr51244-21.c"
 r == -100
# 47 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 47 "./torture/pr51244-21.c"
 "r == -100"
# 47 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 47, __PRETTY_FUNCTION__))
# 47 "./torture/pr51244-21.c"
                   ;

  r = get_request (x, 2);
  
# 50 "./torture/pr51244-21.c" 3 4
 ((
# 50 "./torture/pr51244-21.c"
 r == -100
# 50 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 50 "./torture/pr51244-21.c"
 "r == -100"
# 50 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 50, __PRETTY_FUNCTION__))
# 50 "./torture/pr51244-21.c"
                   ;


  x[0] = 0; x[1] = 0; x[2] = 0;
  r = get_request (x, 0);
  
# 55 "./torture/pr51244-21.c" 3 4
 ((
# 55 "./torture/pr51244-21.c"
 r == -33
# 55 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 55 "./torture/pr51244-21.c"
 "r == -33"
# 55 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 55, __PRETTY_FUNCTION__))
# 55 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 1);
  
# 58 "./torture/pr51244-21.c" 3 4
 ((
# 58 "./torture/pr51244-21.c"
 r == -33
# 58 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 58 "./torture/pr51244-21.c"
 "r == -33"
# 58 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 58, __PRETTY_FUNCTION__))
# 58 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 2);
  
# 61 "./torture/pr51244-21.c" 3 4
 ((
# 61 "./torture/pr51244-21.c"
 r == 0
# 61 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 61 "./torture/pr51244-21.c"
 "r == 0"
# 61 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 61, __PRETTY_FUNCTION__))
# 61 "./torture/pr51244-21.c"
                ;


  x[0] = 0; x[1] = 0; x[2] = 0;
  r = get_request (x, 0);
  
# 66 "./torture/pr51244-21.c" 3 4
 ((
# 66 "./torture/pr51244-21.c"
 r == -33
# 66 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 66 "./torture/pr51244-21.c"
 "r == -33"
# 66 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 66, __PRETTY_FUNCTION__))
# 66 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 1);
  
# 69 "./torture/pr51244-21.c" 3 4
 ((
# 69 "./torture/pr51244-21.c"
 r == -33
# 69 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 69 "./torture/pr51244-21.c"
 "r == -33"
# 69 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 69, __PRETTY_FUNCTION__))
# 69 "./torture/pr51244-21.c"
                  ;

  r = get_request (x, 2);
  
# 72 "./torture/pr51244-21.c" 3 4
 ((
# 72 "./torture/pr51244-21.c"
 r == 0
# 72 "./torture/pr51244-21.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 72 "./torture/pr51244-21.c"
 "r == 0"
# 72 "./torture/pr51244-21.c" 3 4
 , "./torture/pr51244-21.c", 72, __PRETTY_FUNCTION__))
# 72 "./torture/pr51244-21.c"
                ;

  return 0;
}
