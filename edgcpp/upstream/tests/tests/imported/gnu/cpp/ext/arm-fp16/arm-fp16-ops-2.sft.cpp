//type: rp
//options: 
# 0 "./ext/arm-fp16/arm-fp16-ops-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/arm-fp16/arm-fp16-ops-2.C"




# 1 "./ext/arm-fp16/arm-fp16-ops.h" 1


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

# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 4 "./ext/arm-fp16/arm-fp16-ops.h" 2







# 10 "./ext/arm-fp16/arm-fp16-ops.h"
__fp16 h0 = 0.0;
__fp16 h1 = 1.0;
__fp16 h42 = 42.0;
__fp16 hm2 = -2.0;
__fp16 temp;

float f0 = 0.0;
float f1 = 1.0;
float f42 = 42.0;
float fm2 = -2.0;

int main (void)
{
  
# 23 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 23 "./ext/arm-fp16/arm-fp16-ops.h"
 h1
# 23 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 23 "./ext/arm-fp16/arm-fp16-ops.h"
 "h1"
# 23 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 23, __PRETTY_FUNCTION__))
# 23 "./ext/arm-fp16/arm-fp16-ops.h"
          ;
  
# 24 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 24 "./ext/arm-fp16/arm-fp16-ops.h"
 !(h0)
# 24 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 24 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(h0)"
# 24 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 24, __PRETTY_FUNCTION__))
# 24 "./ext/arm-fp16/arm-fp16-ops.h"
             ;
  
# 25 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 25 "./ext/arm-fp16/arm-fp16-ops.h"
 !h0
# 25 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 25 "./ext/arm-fp16/arm-fp16-ops.h"
 "!h0"
# 25 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 25, __PRETTY_FUNCTION__))
# 25 "./ext/arm-fp16/arm-fp16-ops.h"
           ;
  
# 26 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 26 "./ext/arm-fp16/arm-fp16-ops.h"
 !(!h1)
# 26 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 26 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(!h1)"
# 26 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 26, __PRETTY_FUNCTION__))
# 26 "./ext/arm-fp16/arm-fp16-ops.h"
              ;

  (
# 28 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 28 "./ext/arm-fp16/arm-fp16-ops.h"
 (-h1) == -1.0
# 28 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 28 "./ext/arm-fp16/arm-fp16-ops.h"
 "(-h1) == -1.0"
# 28 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 28, __PRETTY_FUNCTION__))
# 28 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (-h1), 
# 28 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 28 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -1.0
# 28 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -1.0", "./ext/arm-fp16/arm-fp16-ops.h", 28, __PRETTY_FUNCTION__))
# 28 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 29 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 29 "./ext/arm-fp16/arm-fp16-ops.h"
 (+h1) == 1.0
# 29 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 29 "./ext/arm-fp16/arm-fp16-ops.h"
 "(+h1) == 1.0"
# 29 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 29, __PRETTY_FUNCTION__))
# 29 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (+h1), 
# 29 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 29 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 1.0
# 29 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 1.0", "./ext/arm-fp16/arm-fp16-ops.h", 29, __PRETTY_FUNCTION__))
# 29 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  
# 31 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 31 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1++) == 1.0
# 31 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 31 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1++) == 1.0"
# 31 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 31, __PRETTY_FUNCTION__))
# 31 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 32 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 32 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1) == 2.0
# 32 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 32 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1) == 2.0"
# 32 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 32, __PRETTY_FUNCTION__))
# 32 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 33 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 33 "./ext/arm-fp16/arm-fp16-ops.h"
 (++h1) == 3.0
# 33 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 33 "./ext/arm-fp16/arm-fp16-ops.h"
 "(++h1) == 3.0"
# 33 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 33, __PRETTY_FUNCTION__))
# 33 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 34 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 34 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1) == 3.0
# 34 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 34 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1) == 3.0"
# 34 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 34, __PRETTY_FUNCTION__))
# 34 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  
# 36 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 36 "./ext/arm-fp16/arm-fp16-ops.h"
 (--h1) == 2.0
# 36 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 36 "./ext/arm-fp16/arm-fp16-ops.h"
 "(--h1) == 2.0"
# 36 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 36, __PRETTY_FUNCTION__))
# 36 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 37 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 37 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1) == 2.0
# 37 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 37 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1) == 2.0"
# 37 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 37, __PRETTY_FUNCTION__))
# 37 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 38 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 38 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1--) == 2.0
# 38 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 38 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1--) == 2.0"
# 38 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 38, __PRETTY_FUNCTION__))
# 38 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 39 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 39 "./ext/arm-fp16/arm-fp16-ops.h"
 (h1) == 1.0
# 39 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 39 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h1) == 1.0"
# 39 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 39, __PRETTY_FUNCTION__))
# 39 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  (
# 41 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 41 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 * hm2) == -84.0
# 41 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 41 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 * hm2) == -84.0"
# 41 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 41, __PRETTY_FUNCTION__))
# 41 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 * hm2), 
# 41 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 41 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -84.0
# 41 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -84.0", "./ext/arm-fp16/arm-fp16-ops.h", 41, __PRETTY_FUNCTION__))
# 41 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 42 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 42 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 * (__fp16) -2.0) == -84.0
# 42 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 42 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 * (__fp16) -2.0) == -84.0"
# 42 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 42, __PRETTY_FUNCTION__))
# 42 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 * (__fp16) -2.0), 
# 42 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 42 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -84.0
# 42 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -84.0", "./ext/arm-fp16/arm-fp16-ops.h", 42, __PRETTY_FUNCTION__))
# 42 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 43 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 43 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 * fm2) == -84.0
# 43 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 43 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 * fm2) == -84.0"
# 43 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 43, __PRETTY_FUNCTION__))
# 43 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 * fm2), 
# 43 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 43 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -84.0
# 43 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -84.0", "./ext/arm-fp16/arm-fp16-ops.h", 43, __PRETTY_FUNCTION__))
# 43 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 44 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 44 "./ext/arm-fp16/arm-fp16-ops.h"
 (f42 * hm2) == -84.0
# 44 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 44 "./ext/arm-fp16/arm-fp16-ops.h"
 "(f42 * hm2) == -84.0"
# 44 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 44, __PRETTY_FUNCTION__))
# 44 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (f42 * hm2), 
# 44 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 44 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -84.0
# 44 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -84.0", "./ext/arm-fp16/arm-fp16-ops.h", 44, __PRETTY_FUNCTION__))
# 44 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  (
# 46 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 46 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 / hm2) == -21.0
# 46 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 46 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 / hm2) == -21.0"
# 46 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 46, __PRETTY_FUNCTION__))
# 46 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 / hm2), 
# 46 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 46 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -21.0
# 46 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -21.0", "./ext/arm-fp16/arm-fp16-ops.h", 46, __PRETTY_FUNCTION__))
# 46 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 47 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 47 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 / (__fp16) -2.0) == -21.0
# 47 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 47 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 / (__fp16) -2.0) == -21.0"
# 47 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 47, __PRETTY_FUNCTION__))
# 47 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 / (__fp16) -2.0), 
# 47 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 47 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -21.0
# 47 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -21.0", "./ext/arm-fp16/arm-fp16-ops.h", 47, __PRETTY_FUNCTION__))
# 47 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 48 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 48 "./ext/arm-fp16/arm-fp16-ops.h"
 (h42 / fm2) == -21.0
# 48 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 48 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h42 / fm2) == -21.0"
# 48 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 48, __PRETTY_FUNCTION__))
# 48 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (h42 / fm2), 
# 48 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 48 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -21.0
# 48 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -21.0", "./ext/arm-fp16/arm-fp16-ops.h", 48, __PRETTY_FUNCTION__))
# 48 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 49 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 49 "./ext/arm-fp16/arm-fp16-ops.h"
 (f42 / hm2) == -21.0
# 49 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 49 "./ext/arm-fp16/arm-fp16-ops.h"
 "(f42 / hm2) == -21.0"
# 49 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 49, __PRETTY_FUNCTION__))
# 49 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (f42 / hm2), 
# 49 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 49 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -21.0
# 49 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -21.0", "./ext/arm-fp16/arm-fp16-ops.h", 49, __PRETTY_FUNCTION__))
# 49 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  (
# 51 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 51 "./ext/arm-fp16/arm-fp16-ops.h"
 (hm2 + h42) == 40.0
# 51 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 51 "./ext/arm-fp16/arm-fp16-ops.h"
 "(hm2 + h42) == 40.0"
# 51 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 51, __PRETTY_FUNCTION__))
# 51 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (hm2 + h42), 
# 51 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 51 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 40.0
# 51 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 40.0", "./ext/arm-fp16/arm-fp16-ops.h", 51, __PRETTY_FUNCTION__))
# 51 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 52 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 52 "./ext/arm-fp16/arm-fp16-ops.h"
 ((__fp16)-2.0 + h42) == 40.0
# 52 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 52 "./ext/arm-fp16/arm-fp16-ops.h"
 "((__fp16)-2.0 + h42) == 40.0"
# 52 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 52, __PRETTY_FUNCTION__))
# 52 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = ((__fp16)-2.0 + h42), 
# 52 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 52 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 40.0
# 52 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 40.0", "./ext/arm-fp16/arm-fp16-ops.h", 52, __PRETTY_FUNCTION__))
# 52 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 53 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 53 "./ext/arm-fp16/arm-fp16-ops.h"
 (hm2 + f42) == 40.0
# 53 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 53 "./ext/arm-fp16/arm-fp16-ops.h"
 "(hm2 + f42) == 40.0"
# 53 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 53, __PRETTY_FUNCTION__))
# 53 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (hm2 + f42), 
# 53 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 53 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 40.0
# 53 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 40.0", "./ext/arm-fp16/arm-fp16-ops.h", 53, __PRETTY_FUNCTION__))
# 53 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 54 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 54 "./ext/arm-fp16/arm-fp16-ops.h"
 (fm2 + h42) == 40.0
# 54 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 54 "./ext/arm-fp16/arm-fp16-ops.h"
 "(fm2 + h42) == 40.0"
# 54 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 54, __PRETTY_FUNCTION__))
# 54 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (fm2 + h42), 
# 54 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 54 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 40.0
# 54 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 40.0", "./ext/arm-fp16/arm-fp16-ops.h", 54, __PRETTY_FUNCTION__))
# 54 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  (
# 56 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 56 "./ext/arm-fp16/arm-fp16-ops.h"
 (hm2 - h42) == -44.0
# 56 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 56 "./ext/arm-fp16/arm-fp16-ops.h"
 "(hm2 - h42) == -44.0"
# 56 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 56, __PRETTY_FUNCTION__))
# 56 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (hm2 - h42), 
# 56 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 56 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -44.0
# 56 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -44.0", "./ext/arm-fp16/arm-fp16-ops.h", 56, __PRETTY_FUNCTION__))
# 56 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 57 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 57 "./ext/arm-fp16/arm-fp16-ops.h"
 ((__fp16)-2.0 - h42) == -44.0
# 57 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 57 "./ext/arm-fp16/arm-fp16-ops.h"
 "((__fp16)-2.0 - h42) == -44.0"
# 57 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 57, __PRETTY_FUNCTION__))
# 57 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = ((__fp16)-2.0 - h42), 
# 57 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 57 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -44.0
# 57 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -44.0", "./ext/arm-fp16/arm-fp16-ops.h", 57, __PRETTY_FUNCTION__))
# 57 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 58 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 58 "./ext/arm-fp16/arm-fp16-ops.h"
 (hm2 - f42) == -44.0
# 58 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 58 "./ext/arm-fp16/arm-fp16-ops.h"
 "(hm2 - f42) == -44.0"
# 58 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 58, __PRETTY_FUNCTION__))
# 58 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (hm2 - f42), 
# 58 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 58 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -44.0
# 58 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -44.0", "./ext/arm-fp16/arm-fp16-ops.h", 58, __PRETTY_FUNCTION__))
# 58 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 59 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 59 "./ext/arm-fp16/arm-fp16-ops.h"
 (fm2 - h42) == -44.0
# 59 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 59 "./ext/arm-fp16/arm-fp16-ops.h"
 "(fm2 - h42) == -44.0"
# 59 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 59, __PRETTY_FUNCTION__))
# 59 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = (fm2 - h42), 
# 59 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 59 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -44.0
# 59 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -44.0", "./ext/arm-fp16/arm-fp16-ops.h", 59, __PRETTY_FUNCTION__))
# 59 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  
# 61 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 61 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 < h42
# 61 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 61 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 < h42"
# 61 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 61, __PRETTY_FUNCTION__))
# 61 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 62 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 62 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 < (__fp16)42.0
# 62 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 62 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 < (__fp16)42.0"
# 62 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 62, __PRETTY_FUNCTION__))
# 62 "./ext/arm-fp16/arm-fp16-ops.h"
                          ;
  
# 63 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 63 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 < f42
# 63 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 63 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 < f42"
# 63 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 63, __PRETTY_FUNCTION__))
# 63 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 64 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 64 "./ext/arm-fp16/arm-fp16-ops.h"
 fm2 < h42
# 64 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 64 "./ext/arm-fp16/arm-fp16-ops.h"
 "fm2 < h42"
# 64 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 64, __PRETTY_FUNCTION__))
# 64 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;

  
# 66 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 66 "./ext/arm-fp16/arm-fp16-ops.h"
 h42 > hm2
# 66 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 66 "./ext/arm-fp16/arm-fp16-ops.h"
 "h42 > hm2"
# 66 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 66, __PRETTY_FUNCTION__))
# 66 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 67 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 67 "./ext/arm-fp16/arm-fp16-ops.h"
 (__fp16)42.0 > hm2
# 67 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 67 "./ext/arm-fp16/arm-fp16-ops.h"
 "(__fp16)42.0 > hm2"
# 67 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 67, __PRETTY_FUNCTION__))
# 67 "./ext/arm-fp16/arm-fp16-ops.h"
                          ;
  
# 68 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 68 "./ext/arm-fp16/arm-fp16-ops.h"
 h42 > fm2
# 68 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 68 "./ext/arm-fp16/arm-fp16-ops.h"
 "h42 > fm2"
# 68 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 68, __PRETTY_FUNCTION__))
# 68 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 69 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 69 "./ext/arm-fp16/arm-fp16-ops.h"
 f42 > hm2
# 69 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 69 "./ext/arm-fp16/arm-fp16-ops.h"
 "f42 > hm2"
# 69 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 69, __PRETTY_FUNCTION__))
# 69 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;

  
# 71 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 71 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 <= h42
# 71 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 71 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 <= h42"
# 71 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 71, __PRETTY_FUNCTION__))
# 71 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 72 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 72 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 <= (__fp16)42.0
# 72 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 72 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 <= (__fp16)42.0"
# 72 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 72, __PRETTY_FUNCTION__))
# 72 "./ext/arm-fp16/arm-fp16-ops.h"
                           ;
  
# 73 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 73 "./ext/arm-fp16/arm-fp16-ops.h"
 hm2 <= f42
# 73 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 73 "./ext/arm-fp16/arm-fp16-ops.h"
 "hm2 <= f42"
# 73 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 73, __PRETTY_FUNCTION__))
# 73 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 74 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 74 "./ext/arm-fp16/arm-fp16-ops.h"
 fm2 <= h42
# 74 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 74 "./ext/arm-fp16/arm-fp16-ops.h"
 "fm2 <= h42"
# 74 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 74, __PRETTY_FUNCTION__))
# 74 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;

  
# 76 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 76 "./ext/arm-fp16/arm-fp16-ops.h"
 h42 >= hm2
# 76 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 76 "./ext/arm-fp16/arm-fp16-ops.h"
 "h42 >= hm2"
# 76 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 76, __PRETTY_FUNCTION__))
# 76 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 77 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 77 "./ext/arm-fp16/arm-fp16-ops.h"
 h42 >= (__fp16)-2.0
# 77 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 77 "./ext/arm-fp16/arm-fp16-ops.h"
 "h42 >= (__fp16)-2.0"
# 77 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 77, __PRETTY_FUNCTION__))
# 77 "./ext/arm-fp16/arm-fp16-ops.h"
                           ;
  
# 78 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 78 "./ext/arm-fp16/arm-fp16-ops.h"
 h42 >= fm2
# 78 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 78 "./ext/arm-fp16/arm-fp16-ops.h"
 "h42 >= fm2"
# 78 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 78, __PRETTY_FUNCTION__))
# 78 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;
  
# 79 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 79 "./ext/arm-fp16/arm-fp16-ops.h"
 f42 >= hm2
# 79 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 79 "./ext/arm-fp16/arm-fp16-ops.h"
 "f42 >= hm2"
# 79 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 79, __PRETTY_FUNCTION__))
# 79 "./ext/arm-fp16/arm-fp16-ops.h"
                  ;

  
# 81 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 81 "./ext/arm-fp16/arm-fp16-ops.h"
 !(h1 == hm2)
# 81 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 81 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(h1 == hm2)"
# 81 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 81, __PRETTY_FUNCTION__))
# 81 "./ext/arm-fp16/arm-fp16-ops.h"
                    ;
  
# 82 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 82 "./ext/arm-fp16/arm-fp16-ops.h"
 h1 == h1
# 82 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 82 "./ext/arm-fp16/arm-fp16-ops.h"
 "h1 == h1"
# 82 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 82, __PRETTY_FUNCTION__))
# 82 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 83 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 83 "./ext/arm-fp16/arm-fp16-ops.h"
 h1 == (__fp16)1.0
# 83 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 83 "./ext/arm-fp16/arm-fp16-ops.h"
 "h1 == (__fp16)1.0"
# 83 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 83, __PRETTY_FUNCTION__))
# 83 "./ext/arm-fp16/arm-fp16-ops.h"
                         ;
  
# 84 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 84 "./ext/arm-fp16/arm-fp16-ops.h"
 h1 == f1
# 84 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 84 "./ext/arm-fp16/arm-fp16-ops.h"
 "h1 == f1"
# 84 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 84, __PRETTY_FUNCTION__))
# 84 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 85 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 85 "./ext/arm-fp16/arm-fp16-ops.h"
 f1 == h1
# 85 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 85 "./ext/arm-fp16/arm-fp16-ops.h"
 "f1 == h1"
# 85 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 85, __PRETTY_FUNCTION__))
# 85 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  
# 87 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 87 "./ext/arm-fp16/arm-fp16-ops.h"
 h1 != hm2
# 87 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 87 "./ext/arm-fp16/arm-fp16-ops.h"
 "h1 != hm2"
# 87 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 87, __PRETTY_FUNCTION__))
# 87 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 88 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 88 "./ext/arm-fp16/arm-fp16-ops.h"
 !(h1 != h1)
# 88 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 88 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(h1 != h1)"
# 88 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 88, __PRETTY_FUNCTION__))
# 88 "./ext/arm-fp16/arm-fp16-ops.h"
                   ;
  
# 89 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 89 "./ext/arm-fp16/arm-fp16-ops.h"
 !(h1 != (__fp16)1.0)
# 89 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 89 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(h1 != (__fp16)1.0)"
# 89 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 89, __PRETTY_FUNCTION__))
# 89 "./ext/arm-fp16/arm-fp16-ops.h"
                            ;
  
# 90 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 90 "./ext/arm-fp16/arm-fp16-ops.h"
 !(h1 != f1)
# 90 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 90 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(h1 != f1)"
# 90 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 90, __PRETTY_FUNCTION__))
# 90 "./ext/arm-fp16/arm-fp16-ops.h"
                   ;
  
# 91 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 91 "./ext/arm-fp16/arm-fp16-ops.h"
 !(f1 != h1)
# 91 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 91 "./ext/arm-fp16/arm-fp16-ops.h"
 "!(f1 != h1)"
# 91 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 91, __PRETTY_FUNCTION__))
# 91 "./ext/arm-fp16/arm-fp16-ops.h"
                   ;

  (
# 93 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 93 "./ext/arm-fp16/arm-fp16-ops.h"
 ((h1 ? hm2 : h42)) == -2.0
# 93 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 93 "./ext/arm-fp16/arm-fp16-ops.h"
 "((h1 ? hm2 : h42)) == -2.0"
# 93 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 93, __PRETTY_FUNCTION__))
# 93 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = ((h1 ? hm2 : h42)), 
# 93 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 93 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == -2.0
# 93 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == -2.0", "./ext/arm-fp16/arm-fp16-ops.h", 93, __PRETTY_FUNCTION__))
# 93 "./ext/arm-fp16/arm-fp16-ops.h"
 );
  (
# 94 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 94 "./ext/arm-fp16/arm-fp16-ops.h"
 ((h0 ? hm2 : h42)) == 42.0
# 94 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 94 "./ext/arm-fp16/arm-fp16-ops.h"
 "((h0 ? hm2 : h42)) == 42.0"
# 94 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 94, __PRETTY_FUNCTION__))
# 94 "./ext/arm-fp16/arm-fp16-ops.h"
 , temp = ((h0 ? hm2 : h42)), 
# 94 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 94 "./ext/arm-fp16/arm-fp16-ops.h"
 temp == 42.0
# 94 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail ("temp == 42.0", "./ext/arm-fp16/arm-fp16-ops.h", 94, __PRETTY_FUNCTION__))
# 94 "./ext/arm-fp16/arm-fp16-ops.h"
 );

  
# 96 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 96 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 = h42) == 42.0
# 96 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 96 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 = h42) == 42.0"
# 96 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 96, __PRETTY_FUNCTION__))
# 96 "./ext/arm-fp16/arm-fp16-ops.h"
                       ;
  
# 97 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 97 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 42.0
# 97 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 97 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 42.0"
# 97 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 97, __PRETTY_FUNCTION__))
# 97 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 98 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 98 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 = (__fp16)-2.0) == -2.0
# 98 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 98 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 = (__fp16)-2.0) == -2.0"
# 98 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 98, __PRETTY_FUNCTION__))
# 98 "./ext/arm-fp16/arm-fp16-ops.h"
                                ;
  
# 99 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 99 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -2.0
# 99 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 99 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -2.0"
# 99 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 99, __PRETTY_FUNCTION__))
# 99 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 100 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 100 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 = f0) == 0.0
# 100 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 100 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 = f0) == 0.0"
# 100 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 100, __PRETTY_FUNCTION__))
# 100 "./ext/arm-fp16/arm-fp16-ops.h"
                     ;
  
# 101 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 101 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 0.0
# 101 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 101 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 0.0"
# 101 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 101, __PRETTY_FUNCTION__))
# 101 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  
# 103 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 103 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 += h1) == 1.0
# 103 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 103 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 += h1) == 1.0"
# 103 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 103, __PRETTY_FUNCTION__))
# 103 "./ext/arm-fp16/arm-fp16-ops.h"
                      ;
  
# 104 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 104 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 1.0
# 104 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 104 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 1.0"
# 104 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 104, __PRETTY_FUNCTION__))
# 104 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 105 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 105 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 += (__fp16)1.0) == 2.0
# 105 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 105 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 += (__fp16)1.0) == 2.0"
# 105 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 105, __PRETTY_FUNCTION__))
# 105 "./ext/arm-fp16/arm-fp16-ops.h"
                               ;
  
# 106 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 106 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 2.0
# 106 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 106 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 2.0"
# 106 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 106, __PRETTY_FUNCTION__))
# 106 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 107 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 107 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 += fm2) == 0.0
# 107 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 107 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 += fm2) == 0.0"
# 107 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 107, __PRETTY_FUNCTION__))
# 107 "./ext/arm-fp16/arm-fp16-ops.h"
                       ;
  
# 108 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 108 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 0.0
# 108 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 108 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 0.0"
# 108 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 108, __PRETTY_FUNCTION__))
# 108 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  
# 110 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 110 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 -= h1) == -1.0
# 110 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 110 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 -= h1) == -1.0"
# 110 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 110, __PRETTY_FUNCTION__))
# 110 "./ext/arm-fp16/arm-fp16-ops.h"
                       ;
  
# 111 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 111 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -1.0
# 111 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 111 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -1.0"
# 111 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 111, __PRETTY_FUNCTION__))
# 111 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 112 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 112 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 -= (__fp16)1.0) == -2.0
# 112 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 112 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 -= (__fp16)1.0) == -2.0"
# 112 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 112, __PRETTY_FUNCTION__))
# 112 "./ext/arm-fp16/arm-fp16-ops.h"
                                ;
  
# 113 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 113 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -2.0
# 113 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 113 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -2.0"
# 113 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 113, __PRETTY_FUNCTION__))
# 113 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 114 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 114 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 -= fm2) == 0.0
# 114 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 114 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 -= fm2) == 0.0"
# 114 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 114, __PRETTY_FUNCTION__))
# 114 "./ext/arm-fp16/arm-fp16-ops.h"
                       ;
  
# 115 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 115 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 0.0
# 115 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 115 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 0.0"
# 115 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 115, __PRETTY_FUNCTION__))
# 115 "./ext/arm-fp16/arm-fp16-ops.h"
                ;

  h0 = hm2;
  
# 118 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 118 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 *= hm2) == 4.0
# 118 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 118 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 *= hm2) == 4.0"
# 118 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 118, __PRETTY_FUNCTION__))
# 118 "./ext/arm-fp16/arm-fp16-ops.h"
                       ;
  
# 119 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 119 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 4.0
# 119 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 119 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 4.0"
# 119 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 119, __PRETTY_FUNCTION__))
# 119 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 120 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 120 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 *= (__fp16)-2.0) == -8.0
# 120 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 120 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 *= (__fp16)-2.0) == -8.0"
# 120 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 120, __PRETTY_FUNCTION__))
# 120 "./ext/arm-fp16/arm-fp16-ops.h"
                                 ;
  
# 121 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 121 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -8.0
# 121 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 121 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -8.0"
# 121 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 121, __PRETTY_FUNCTION__))
# 121 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 122 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 122 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 *= fm2) == 16.0
# 122 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 122 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 *= fm2) == 16.0"
# 122 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 122, __PRETTY_FUNCTION__))
# 122 "./ext/arm-fp16/arm-fp16-ops.h"
                        ;
  
# 123 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 123 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 16.0
# 123 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 123 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 16.0"
# 123 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 123, __PRETTY_FUNCTION__))
# 123 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;

  
# 125 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 125 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 /= hm2) == -8.0
# 125 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 125 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 /= hm2) == -8.0"
# 125 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 125, __PRETTY_FUNCTION__))
# 125 "./ext/arm-fp16/arm-fp16-ops.h"
                        ;
  
# 126 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 126 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -8.0
# 126 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 126 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -8.0"
# 126 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 126, __PRETTY_FUNCTION__))
# 126 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;
  
# 127 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 127 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 /= (__fp16)-2.0) == 4.0
# 127 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 127 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 /= (__fp16)-2.0) == 4.0"
# 127 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 127, __PRETTY_FUNCTION__))
# 127 "./ext/arm-fp16/arm-fp16-ops.h"
                                ;
  
# 128 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 128 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == 4.0
# 128 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 128 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == 4.0"
# 128 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 128, __PRETTY_FUNCTION__))
# 128 "./ext/arm-fp16/arm-fp16-ops.h"
                ;
  
# 129 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 129 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0 /= fm2) == -2.0
# 129 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 129 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0 /= fm2) == -2.0"
# 129 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 129, __PRETTY_FUNCTION__))
# 129 "./ext/arm-fp16/arm-fp16-ops.h"
                        ;
  
# 130 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 130 "./ext/arm-fp16/arm-fp16-ops.h"
 (h0) == -2.0
# 130 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 130 "./ext/arm-fp16/arm-fp16-ops.h"
 "(h0) == -2.0"
# 130 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 130, __PRETTY_FUNCTION__))
# 130 "./ext/arm-fp16/arm-fp16-ops.h"
                 ;

  
# 132 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ((
# 132 "./ext/arm-fp16/arm-fp16-ops.h"
 ((h0, h1)) == 1.0
# 132 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 132 "./ext/arm-fp16/arm-fp16-ops.h"
 "((h0, h1)) == 1.0"
# 132 "./ext/arm-fp16/arm-fp16-ops.h" 3 4
 , "./ext/arm-fp16/arm-fp16-ops.h", 132, __PRETTY_FUNCTION__))
# 132 "./ext/arm-fp16/arm-fp16-ops.h"
                      ;

  return 0;
}
# 6 "./ext/arm-fp16/arm-fp16-ops-2.C" 2
