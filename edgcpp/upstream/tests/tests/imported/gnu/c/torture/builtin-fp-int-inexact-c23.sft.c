//type: rp
//options: --c23
# 0 "./torture/builtin-fp-int-inexact-c23.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/builtin-fp-int-inexact-c23.c"





# 1 "./torture/builtin-fp-int-inexact.c" 1





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

# 7 "./torture/builtin-fp-int-inexact.c" 2
# 24 "./torture/builtin-fp-int-inexact.c"

# 24 "./torture/builtin-fp-int-inexact.c"
__attribute__ ((noinline, noclone)) double ceil (double x) { return x; } __attribute__ ((noinline, noclone)) float ceilf (float x) { return x; } __attribute__ ((noinline, noclone)) long double ceill (long double x) { return x; }
__attribute__ ((noinline, noclone)) double floor (double x) { return x; } __attribute__ ((noinline, noclone)) float floorf (float x) { return x; } __attribute__ ((noinline, noclone)) long double floorl (long double x) { return x; }
__attribute__ ((noinline, noclone)) double round (double x) { return x; } __attribute__ ((noinline, noclone)) float roundf (float x) { return x; } __attribute__ ((noinline, noclone)) long double roundl (long double x) { return x; }
__attribute__ ((noinline, noclone)) double trunc (double x) { return x; } __attribute__ ((noinline, noclone)) float truncf (float x) { return x; } __attribute__ ((noinline, noclone)) long double truncl (long double x) { return x; }

extern void abort (void);
extern void exit (int);
# 51 "./torture/builtin-fp-int-inexact.c"
static void
main_test (void)
{
  do { do { volatile double a = 1.5, b; b = __builtin_ceil (a); if (fetestexcept (
# 54 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 54 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile float a = 1.5, b; b = __builtin_ceilf (a); if (fetestexcept (
# 54 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 54 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile long double a = 1.5, b; b = __builtin_ceill (a); if (fetestexcept (
# 54 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 54 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); } while (0);
  do { do { volatile double a = 1.5, b; b = __builtin_floor (a); if (fetestexcept (
# 55 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 55 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile float a = 1.5, b; b = __builtin_floorf (a); if (fetestexcept (
# 55 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 55 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile long double a = 1.5, b; b = __builtin_floorl (a); if (fetestexcept (
# 55 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 55 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); } while (0);
  do { do { volatile double a = 1.5, b; b = __builtin_round (a); if (fetestexcept (
# 56 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 56 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile float a = 1.5, b; b = __builtin_roundf (a); if (fetestexcept (
# 56 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 56 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile long double a = 1.5, b; b = __builtin_roundl (a); if (fetestexcept (
# 56 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 56 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); } while (0);
  do { do { volatile double a = 1.5, b; b = __builtin_trunc (a); if (fetestexcept (
# 57 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 57 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile float a = 1.5, b; b = __builtin_truncf (a); if (fetestexcept (
# 57 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 57 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); do { volatile long double a = 1.5, b; b = __builtin_truncl (a); if (fetestexcept (
# 57 "./torture/builtin-fp-int-inexact.c" 3 4
 0x20
# 57 "./torture/builtin-fp-int-inexact.c"
 )) abort (); } while (0); } while (0);
}





int
main (void)
{
  main_test ();
  exit (0);
}
# 7 "./torture/builtin-fp-int-inexact-c23.c" 2
