//type: fp
//options: 
# 0 "./dfp/fe-convert-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/fe-convert-1.c"





# 1 "./dfp/fe-check.h" 1




# 1 "./dfp/dfp-dbg.h" 1


int failures;
# 6 "./dfp/fe-check.h" 2
# 1 "./dfp/dfp-except.h" 1







# 1 "/usr/include/fenv.h" 1 3 4
# 25 "/usr/include/fenv.h" 3 4
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
# 26 "/usr/include/fenv.h" 2 3 4
# 57 "/usr/include/fenv.h" 3 4
# 1 "/usr/include/bits/fenv.h" 1 3 4
# 24 "/usr/include/bits/fenv.h" 3 4

# 24 "/usr/include/bits/fenv.h" 3 4
enum
  {
    FE_INVALID =

      0x01,
    __FE_DENORM = 0x02,
    FE_DIVBYZERO =

      0x04,
    FE_OVERFLOW =

      0x08,
    FE_UNDERFLOW =

      0x10,
    FE_INEXACT =

      0x20
  };







enum
  {
    FE_TONEAREST =

      0,
    FE_DOWNWARD =

      0x400,
    FE_UPWARD =

      0x800,
    FE_TOWARDZERO =

      0xc00
  };



typedef unsigned short int fexcept_t;






typedef struct
  {
    unsigned short int __control_word;
    unsigned short int __unused1;
    unsigned short int __status_word;
    unsigned short int __unused2;
    unsigned short int __tags;
    unsigned short int __unused3;
    unsigned int __eip;
    unsigned short int __cs_selector;
    unsigned int __opcode:11;
    unsigned int __unused4:5;
    unsigned int __data_offset;
    unsigned short int __data_selector;
    unsigned short int __unused5;

    unsigned int __mxcsr;

  }
fenv_t;
# 58 "/usr/include/fenv.h" 2 3 4






extern int feclearexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fegetexceptflag (fexcept_t *__flagp, int __excepts) __attribute__ ((__nothrow__ , __leaf__));


extern int feraiseexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fesetexceptflag (const fexcept_t *__flagp, int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fetestexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));





extern int fegetround (void) __attribute__ ((__nothrow__ , __leaf__));


extern int fesetround (int __rounding_direction) __attribute__ ((__nothrow__ , __leaf__));






extern int fegetenv (fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));




extern int feholdexcept (fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));



extern int fesetenv (const fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));




extern int feupdateenv (const fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));
# 133 "/usr/include/fenv.h" 3 4

# 9 "./dfp/dfp-except.h" 2


# 10 "./dfp/dfp-except.h"
extern void __dfp_clear_except (int);

extern int __dfp_test_except (int);
# 7 "./dfp/fe-check.h" 2
# 38 "./dfp/fe-check.h"
void
checkflags (int num, int want)
{
  int got = __dfp_test_except(
# 41 "./dfp/fe-check.h" 3 4
           (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 41 "./dfp/fe-check.h"
           );
  if ((want & 
# 42 "./dfp/fe-check.h" 3 4
 0x01
# 42 "./dfp/fe-check.h"
 ) != (got & 
# 42 "./dfp/fe-check.h" 3 4
 0x01
# 42 "./dfp/fe-check.h"
 )) { if ((want & 
# 42 "./dfp/fe-check.h" 3 4
 0x01
# 42 "./dfp/fe-check.h"
 ) != 0) __builtin_abort (); else __builtin_abort (); } else ;
  if ((want & 
# 43 "./dfp/fe-check.h" 3 4
 0x08
# 43 "./dfp/fe-check.h"
 ) != (got & 
# 43 "./dfp/fe-check.h" 3 4
 0x08
# 43 "./dfp/fe-check.h"
 )) { if ((want & 
# 43 "./dfp/fe-check.h" 3 4
 0x08
# 43 "./dfp/fe-check.h"
 ) != 0) __builtin_abort (); else __builtin_abort (); } else ;
  if ((want & 
# 44 "./dfp/fe-check.h" 3 4
 0x10
# 44 "./dfp/fe-check.h"
 ) != (got & 
# 44 "./dfp/fe-check.h" 3 4
 0x10
# 44 "./dfp/fe-check.h"
 )) { if ((want & 
# 44 "./dfp/fe-check.h" 3 4
 0x10
# 44 "./dfp/fe-check.h"
 ) != 0) __builtin_abort (); else __builtin_abort (); } else ;
  if ((want & 
# 45 "./dfp/fe-check.h" 3 4
 0x04
# 45 "./dfp/fe-check.h"
 ) != (got & 
# 45 "./dfp/fe-check.h" 3 4
 0x04
# 45 "./dfp/fe-check.h"
 )) { if ((want & 
# 45 "./dfp/fe-check.h" 3 4
 0x04
# 45 "./dfp/fe-check.h"
 ) != 0) __builtin_abort (); else __builtin_abort (); } else ;
  if ((want & 
# 46 "./dfp/fe-check.h" 3 4
 0x20
# 46 "./dfp/fe-check.h"
 ) != (got & 
# 46 "./dfp/fe-check.h" 3 4
 0x20
# 46 "./dfp/fe-check.h"
 )) { if ((want & 
# 46 "./dfp/fe-check.h" 3 4
 0x20
# 46 "./dfp/fe-check.h"
 ) != 0) __builtin_abort (); else __builtin_abort (); } else ;
}
# 7 "./dfp/fe-convert-1.c" 2

volatile _Decimal32 d32;
volatile _Decimal64 d64;
volatile _Decimal128 d128;





void convert_100 (void) { d64 = 9.999999e96DD; __dfp_clear_except(
# 16 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 16 "./dfp/fe-convert-1.c"
); d32 = d64; checkflags (100, 0); }
void convert_101 (void) { d64 = 10.000000e96DD; __dfp_clear_except(
# 17 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 17 "./dfp/fe-convert-1.c"
); d32 = d64; checkflags (101, 
# 17 "./dfp/fe-convert-1.c" 3 4
0x20
# 17 "./dfp/fe-convert-1.c"
|
# 17 "./dfp/fe-convert-1.c" 3 4
0x08
# 17 "./dfp/fe-convert-1.c"
); }
void convert_102 (void) { d64 = 1.1111111DD; __dfp_clear_except(
# 18 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 18 "./dfp/fe-convert-1.c"
); d32 = d64; checkflags (102, 
# 18 "./dfp/fe-convert-1.c" 3 4
0x20
# 18 "./dfp/fe-convert-1.c"
); }
void convert_110 (void) { d128 = 9.999999e96DL; __dfp_clear_except(
# 19 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 19 "./dfp/fe-convert-1.c"
); d32 = d128; checkflags (110, 0); }
void convert_111 (void) { d128 = 10.000000e96DL; __dfp_clear_except(
# 20 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 20 "./dfp/fe-convert-1.c"
); d32 = d128; checkflags (111, 
# 20 "./dfp/fe-convert-1.c" 3 4
0x20
# 20 "./dfp/fe-convert-1.c"
|
# 20 "./dfp/fe-convert-1.c" 3 4
0x08
# 20 "./dfp/fe-convert-1.c"
); }
void convert_112 (void) { d128 = 1.1111111DL; __dfp_clear_except(
# 21 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 21 "./dfp/fe-convert-1.c"
); d32 = d128; checkflags (112, 
# 21 "./dfp/fe-convert-1.c" 3 4
0x20
# 21 "./dfp/fe-convert-1.c"
); }
void convert_120 (void) { d128 = 9.999999999999999E384DL; __dfp_clear_except(
# 22 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 22 "./dfp/fe-convert-1.c"
); d64 = d128; checkflags (120, 0); }
void convert_121 (void) { d128 = 10.00000000000000E384DL; __dfp_clear_except(
# 23 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 23 "./dfp/fe-convert-1.c"
); d64 = d128; checkflags (121, 
# 23 "./dfp/fe-convert-1.c" 3 4
0x20
# 23 "./dfp/fe-convert-1.c"
|
# 23 "./dfp/fe-convert-1.c" 3 4
0x08
# 23 "./dfp/fe-convert-1.c"
); }
void convert_122 (void) { d128 = 1.1111111111111111DL; __dfp_clear_except(
# 24 "./dfp/fe-convert-1.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 24 "./dfp/fe-convert-1.c"
); d64 = d128; checkflags (122, 
# 24 "./dfp/fe-convert-1.c" 3 4
0x20
# 24 "./dfp/fe-convert-1.c"
); }

int
main ()
{
  convert_100 ();
  convert_101 ();
  convert_102 ();
  convert_110 ();
  convert_111 ();
  convert_112 ();
  convert_120 ();
  convert_121 ();
  convert_122 ();

  return 0;
}
