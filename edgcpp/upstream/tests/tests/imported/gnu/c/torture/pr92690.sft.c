//type: rp
//options: 
# 0 "./torture/pr92690.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr92690.c"




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

# 6 "./torture/pr92690.c" 2


# 7 "./torture/pr92690.c"
typedef int v4si __attribute__((vector_size(16)));
typedef float v4sf __attribute__((vector_size(16)));

void __attribute__((noipa))
foo (v4si *dstp, v4sf *srcp)
{
  v4sf src = *srcp;
  *dstp = (v4si) { src[0], src[1], 3, 4 };
}

void __attribute__((noipa))
bar (v4sf *dstp, v4si *srcp)
{
  v4si src = *srcp;
  *dstp = (v4sf) { src[0], src[1], 3.5, 4.5 };
}

int
main()
{
  feenableexcept (
# 27 "./torture/pr92690.c" 3 4
                 0x01
# 27 "./torture/pr92690.c"
                           |
# 27 "./torture/pr92690.c" 3 4
                            0x20
# 27 "./torture/pr92690.c"
                                      );
  v4sf x = (v4sf) { 1, 2, __builtin_nanf (""), 3.5 };
  v4si y;
  foo (&y, &x);
  if (y[0] != 1 || y[1] != 2 || y[2] != 3 || y[3] != 4)
    __builtin_abort ();
  y = (v4si) { 0, 1, 0x7fffffff, -0x7fffffff };
  bar (&x, &y);
  if (x[0] != 0 || x[1] != 1 || x[2] != 3.5 || x[3] != 4.5)
    __builtin_abort ();
  return 0;
}
