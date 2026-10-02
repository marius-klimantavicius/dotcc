//type: fp
//options: --c23
# 0 "./dfp/int128-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/int128-4.c"






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

# 8 "./dfp/int128-4.c" 2







# 14 "./dfp/int128-4.c"
__attribute__((noipa)) __int128
tests_32 (_Decimal32 d)
{
  return d;
}

__attribute__((noipa)) unsigned __int128
testu_32 (_Decimal32 d)
{
  return d;
}

__attribute__((noipa)) __int128
tests_64 (_Decimal64 d)
{
  return d;
}

__attribute__((noipa)) unsigned __int128
testu_64 (_Decimal64 d)
{
  return d;
}

__attribute__((noipa)) __int128
tests_128 (_Decimal128 d)
{
  return d;
}

__attribute__((noipa)) unsigned __int128
testu_128 (_Decimal128 d)
{
  return d;
}

__attribute__((noipa)) void
check_invalid (int test, int inv)
{
  if (!test)
    __builtin_abort ();
  if ((!fetestexcept (
# 55 "./dfp/int128-4.c" 3 4
                     0x01
# 55 "./dfp/int128-4.c"
                               )) != (!inv))
    __builtin_abort ();
  feclearexcept (
# 57 "./dfp/int128-4.c" 3 4
                0x01
# 57 "./dfp/int128-4.c"
                          );
}

int
main ()
{
  check_invalid (tests_32 (__builtin_infd32 ()) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_32 (-__builtin_infd32 ()) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_32 (__builtin_nand32 ("")) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_32 (-1701411.0e+32DF) == -((((__int128) (0x7ffffbe294adefdaULL)) << 64) | (0xd863b4a300000000ULL)), 0);
  check_invalid (tests_32 (-1701412.0e+32DF) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_32 (1701411.0e+32DF) == ((((__int128) (0x7ffffbe294adefdaULL)) << 64) | (0xd863b4a300000000ULL)), 0);
  check_invalid (tests_32 (1701412.0e+32DF) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (testu_32 (__builtin_infd32 ()) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_32 (-__builtin_infd32 ()) == 0U, 1);
  check_invalid (testu_32 (__builtin_nand32 ("")) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_32 (-0.9999999DF) == 0U, 0);
  check_invalid (testu_32 (-1.0DF) == 0U, 1);
  check_invalid (testu_32 (3402823.0e+32DF) == ((((unsigned __int128) (0xfffffcb356c92111ULL)) << 64) | (0x367458c700000000ULL)), 0);
  check_invalid (testu_32 (3402824.0e+32DF) == (~(unsigned __int128) 0), 1);
  check_invalid (tests_64 (__builtin_infd64 ()) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_64 (-__builtin_infd64 ()) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_64 (__builtin_nand64 ("")) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_64 (-170141183460469.2e+24DD) == -((((__int128) (0x7ffffffffffff947ULL)) << 64) | (0xd26076f482000000ULL)), 0);
  check_invalid (tests_64 (-170141183460469.3e+24DD) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_64 (170141183460469.2e+24DD) == ((((__int128) (0x7ffffffffffff947ULL)) << 64) | (0xd26076f482000000ULL)), 0);
  check_invalid (tests_64 (170141183460469.3e+24DD) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (testu_64 (__builtin_infd64 ()) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_64 (-__builtin_infd64 ()) == 0, 1);
  check_invalid (testu_64 (__builtin_nand64 ("")) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_64 (-0.9999999999999999DD) == 0U, 0);
  check_invalid (testu_64 (-1.0DD) == 0U, 1);
  check_invalid (testu_64 (340282366920938.4e+24DD) == ((((unsigned __int128) (0xfffffffffffff28fULL)) << 64) | (0xa4c0ede904000000ULL)), 0);
  check_invalid (testu_64 (340282366920938.5e+24DD) == (~(unsigned __int128) 0), 1);
  check_invalid (tests_128 (__builtin_infd128 ()) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_128 (-__builtin_infd128 ()) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_128 (__builtin_nand128 ("")) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (tests_128 (-1701411834604692317316873037158841.0e+5DL) == -((((__int128) (0x7fffffffffffffffULL)) << 64) | (0xffffffffffffe9a0ULL)), 0);
  check_invalid (tests_128 (-1701411834604692317316873037158842.0e+5DL) == -((__int128) ((((unsigned __int128) 1) << 127) - 1)) - 1, 1);
  check_invalid (tests_128 (1701411834604692317316873037158841.0e+5DL) == ((((__int128) (0x7fffffffffffffffULL)) << 64) | (0xffffffffffffe9a0ULL)), 0);
  check_invalid (tests_128 (1701411834604692317316873037158842.0e+5DL) == ((__int128) ((((unsigned __int128) 1) << 127) - 1)), 1);
  check_invalid (testu_128 (__builtin_infd128 ()) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_128 (-__builtin_infd128 ()) == 0, 1);
  check_invalid (testu_128 (__builtin_nand128 ("")) == (~(unsigned __int128) 0), 1);
  check_invalid (testu_128 (-0.9999999999999999999999999999999999DL) == 0U, 0);
  check_invalid (testu_128 (-1.0DL) == 0U, 1);
  check_invalid (testu_128 (3402823669209384634633746074317682.0e+5DL) == ((((unsigned __int128) (0xffffffffffffffffULL)) << 64) | (0xffffffffffffd340ULL)), 0);
  check_invalid (testu_128 (3402823669209384634633746074317683.0e+5DL) == (~(unsigned __int128) 0), 1);
}
