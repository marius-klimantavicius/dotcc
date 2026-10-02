//type: rp
//options: 
# 0 "./torture/builtin-iseqsig-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/builtin-iseqsig-2.c"






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

# 8 "./torture/builtin-iseqsig-2.c" 2


# 9 "./torture/builtin-iseqsig-2.c"
void
ftrue (double x, double y)
{
  if (!__builtin_iseqsig (x, y))
    __builtin_abort ();
}

void
ffalse (double x, double y)
{
  if (__builtin_iseqsig (x, y))
    __builtin_abort ();
}

int
main ()
{
  volatile double f1, f2;

  f1 = 0.; f2 = 0.;
  ftrue (f1, f2);
  if (fetestexcept (
# 30 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 30 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 0.; f2 = -0.;
  ftrue (f1, f2);
  if (fetestexcept (
# 34 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 34 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 0.; f2 = 1.;
  ffalse (f1, f2);
  if (fetestexcept (
# 38 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 38 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = -0.; f2 = 1.;
  ffalse (f1, f2);
  if (fetestexcept (
# 42 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 42 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 0.; f2 = __builtin_inf();
  ffalse (f1, f2);
  if (fetestexcept (
# 46 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 46 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = -0.; f2 = __builtin_inf();
  ffalse (f1, f2);
  if (fetestexcept (
# 50 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 50 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 0.; f2 = __builtin_nan("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 54 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 54 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 55 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 55 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = -0.; f2 = __builtin_nan("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 59 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 59 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 60 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 60 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = 1.; f2 = 1.;
  ftrue (f1, f2);
  if (fetestexcept (
# 64 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 64 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 1.; f2 = 0.;
  ffalse (f1, f2);
  if (fetestexcept (
# 68 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 68 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 1.; f2 = -0.;
  ffalse (f1, f2);
  if (fetestexcept (
# 72 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 72 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 1.; f2 = __builtin_inf();
  ffalse (f1, f2);
  if (fetestexcept (
# 76 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 76 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = 1.; f2 = __builtin_nan("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 80 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 80 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 81 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 81 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = __builtin_inf(); f2 = __builtin_inf();
  ftrue (f1, f2);
  if (fetestexcept (
# 85 "./torture/builtin-iseqsig-2.c" 3 4
                   0x01
# 85 "./torture/builtin-iseqsig-2.c"
                             )) __builtin_abort ();

  f1 = __builtin_inf(); f2 = __builtin_nan("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 89 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 89 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 90 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 90 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = __builtin_nan(""); f2 = __builtin_nan("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 94 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 94 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 95 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 95 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = __builtin_nans(""); f2 = 1.;
  ffalse (f1, f2);
  if (!fetestexcept (
# 99 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 99 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 100 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 100 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = 1.; f2 = __builtin_nans("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 104 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 104 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 105 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 105 "./torture/builtin-iseqsig-2.c"
                          );

  f1 = __builtin_nans(""); f2 = __builtin_nans("");
  ffalse (f1, f2);
  if (!fetestexcept (
# 109 "./torture/builtin-iseqsig-2.c" 3 4
                    0x01
# 109 "./torture/builtin-iseqsig-2.c"
                              )) __builtin_abort ();
  feclearexcept (
# 110 "./torture/builtin-iseqsig-2.c" 3 4
                0x01
# 110 "./torture/builtin-iseqsig-2.c"
                          );

  return 0;
}
