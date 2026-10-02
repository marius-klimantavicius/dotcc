//type: rp
//options: --c23
# 0 "./dfp/c2x-float-dfp-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c2x-float-dfp-8.c"






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

# 8 "./dfp/c2x-float-dfp-8.c" 2
# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 9 "./dfp/c2x-float-dfp-8.c" 2


# 10 "./dfp/c2x-float-dfp-8.c"
volatile _Decimal32 d32 = 
# 10 "./dfp/c2x-float-dfp-8.c" 3 4
                         (__builtin_nansd32 (""))
# 10 "./dfp/c2x-float-dfp-8.c"
                                   ;
volatile _Decimal64 d64 = 
# 11 "./dfp/c2x-float-dfp-8.c" 3 4
                         (__builtin_nansd64 (""))
# 11 "./dfp/c2x-float-dfp-8.c"
                                   ;
volatile _Decimal128 d128 = 
# 12 "./dfp/c2x-float-dfp-8.c" 3 4
                           (__builtin_nansd128 (""))
# 12 "./dfp/c2x-float-dfp-8.c"
                                      ;

extern void abort (void);
extern void exit (int);

int
main (void)
{
  feclearexcept (
# 20 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 20 "./dfp/c2x-float-dfp-8.c"
                             );
  d32 += d32;
  if (!fetestexcept (
# 22 "./dfp/c2x-float-dfp-8.c" 3 4
                    0x01
# 22 "./dfp/c2x-float-dfp-8.c"
                              ))
    abort ();
  feclearexcept (
# 24 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 24 "./dfp/c2x-float-dfp-8.c"
                             );
  d32 += d32;
  if (fetestexcept (
# 26 "./dfp/c2x-float-dfp-8.c" 3 4
                   0x01
# 26 "./dfp/c2x-float-dfp-8.c"
                             ))
    abort ();
  feclearexcept (
# 28 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 28 "./dfp/c2x-float-dfp-8.c"
                             );
  d64 += d64;
  if (!fetestexcept (
# 30 "./dfp/c2x-float-dfp-8.c" 3 4
                    0x01
# 30 "./dfp/c2x-float-dfp-8.c"
                              ))
    abort ();
  feclearexcept (
# 32 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 32 "./dfp/c2x-float-dfp-8.c"
                             );
  d64 += d64;
  if (fetestexcept (
# 34 "./dfp/c2x-float-dfp-8.c" 3 4
                   0x01
# 34 "./dfp/c2x-float-dfp-8.c"
                             ))
    abort ();
  feclearexcept (
# 36 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 36 "./dfp/c2x-float-dfp-8.c"
                             );
  d128 += d128;
  if (!fetestexcept (
# 38 "./dfp/c2x-float-dfp-8.c" 3 4
                    0x01
# 38 "./dfp/c2x-float-dfp-8.c"
                              ))
    abort ();
  feclearexcept (
# 40 "./dfp/c2x-float-dfp-8.c" 3 4
                (0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 40 "./dfp/c2x-float-dfp-8.c"
                             );
  d128 += d128;
  if (fetestexcept (
# 42 "./dfp/c2x-float-dfp-8.c" 3 4
                   0x01
# 42 "./dfp/c2x-float-dfp-8.c"
                             ))
    abort ();
  exit (0);
}
