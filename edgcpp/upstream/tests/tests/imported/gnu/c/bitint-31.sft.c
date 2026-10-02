//type: rp
//options: --c23
# 0 "./bitint-31.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./bitint-31.c"






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

# 8 "./bitint-31.c" 2




# 11 "./bitint-31.c"
__attribute__((noipa)) float
testflt_135 (_BitInt(135) b)
{
  return b;
}

__attribute__((noipa)) float
testfltu_135 (unsigned _BitInt(135) b)
{
  return b;
}



__attribute__((noipa)) float
testflt_192 (_BitInt(192) b)
{
  return b;
}

__attribute__((noipa)) float
testfltu_192 (unsigned _BitInt(192) b)
{
  return b;
}



__attribute__((noipa)) float
testflt_575 (_BitInt(575) b)
{
  return b;
}

__attribute__((noipa)) float
testfltu_575 (unsigned _BitInt(575) b)
{
  return b;
}





__attribute__((noipa)) double
testdbl_135 (_BitInt(135) b)
{
  return b;
}

__attribute__((noipa)) double
testdblu_135 (unsigned _BitInt(135) b)
{
  return b;
}



__attribute__((noipa)) double
testdbl_192 (_BitInt(192) b)
{
  return b;
}

__attribute__((noipa)) double
testdblu_192 (unsigned _BitInt(192) b)
{
  return b;
}



__attribute__((noipa)) double
testdbl_575 (_BitInt(575) b)
{
  return b;
}

__attribute__((noipa)) double
testdblu_575 (unsigned _BitInt(575) b)
{
  return b;
}





__attribute__((noipa)) long double
testldbl_135 (_BitInt(135) b)
{
  return b;
}

__attribute__((noipa)) long double
testldblu_135 (unsigned _BitInt(135) b)
{
  return b;
}



__attribute__((noipa)) long double
testldbl_192 (_BitInt(192) b)
{
  return b;
}

__attribute__((noipa)) long double
testldblu_192 (unsigned _BitInt(192) b)
{
  return b;
}



__attribute__((noipa)) long double
testldbl_575 (_BitInt(575) b)
{
  return b;
}

__attribute__((noipa)) long double
testldblu_575 (unsigned _BitInt(575) b)
{
  return b;
}





__attribute__((noipa)) _Float128
testflt128_135 (_BitInt(135) b)
{
  return b;
}

__attribute__((noipa)) _Float128
testflt128u_135 (unsigned _BitInt(135) b)
{
  return b;
}



__attribute__((noipa)) _Float128
testflt128_192 (_BitInt(192) b)
{
  return b;
}

__attribute__((noipa)) _Float128
testflt128u_192 (unsigned _BitInt(192) b)
{
  return b;
}



__attribute__((noipa)) _Float128
testflt128_575 (_BitInt(575) b)
{
  return b;
}

__attribute__((noipa)) _Float128
testflt128u_575 (unsigned _BitInt(575) b)
{
  return b;
}
# 205 "./bitint-31.c"
int
main ()
{



  do { fesetround (
# 211 "./bitint-31.c" 3 4
 0
# 211 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726783wb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 211 "./bitint-31.c" 3 4
 0x400
# 211 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726783wb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 211 "./bitint-31.c" 3 4
 0x800
# 211 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726783wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 211 "./bitint-31.c" 3 4
 0xc00
# 211 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726783wb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 212 "./bitint-31.c" 3 4
 0
# 212 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726784wb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 212 "./bitint-31.c" 3 4
 0x400
# 212 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726784wb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 212 "./bitint-31.c" 3 4
 0x800
# 212 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726784wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 212 "./bitint-31.c" 3 4
 0xc00
# 212 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726784wb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 213 "./bitint-31.c" 3 4
 0
# 213 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726785wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 213 "./bitint-31.c" 3 4
 0x400
# 213 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726785wb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 213 "./bitint-31.c" 3 4
 0x800
# 213 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726785wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 213 "./bitint-31.c" 3 4
 0xc00
# 213 "./bitint-31.c"
 ); if (testflt_135 (151115713941029764726785wb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 214 "./bitint-31.c" 3 4
 0
# 214 "./bitint-31.c"
 ); if (testflt_135 (151115718444629392097280wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 214 "./bitint-31.c" 3 4
 0x400
# 214 "./bitint-31.c"
 ); if (testflt_135 (151115718444629392097280wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 214 "./bitint-31.c" 3 4
 0x800
# 214 "./bitint-31.c"
 ); if (testflt_135 (151115718444629392097280wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 214 "./bitint-31.c" 3 4
 0xc00
# 214 "./bitint-31.c"
 ); if (testflt_135 (151115718444629392097280wb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 215 "./bitint-31.c" 3 4
 0
# 215 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467775wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 215 "./bitint-31.c" 3 4
 0x400
# 215 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467775wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 215 "./bitint-31.c" 3 4
 0x800
# 215 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467775wb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 215 "./bitint-31.c" 3 4
 0xc00
# 215 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467775wb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 216 "./bitint-31.c" 3 4
 0
# 216 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467776wb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 216 "./bitint-31.c" 3 4
 0x400
# 216 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467776wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 216 "./bitint-31.c" 3 4
 0x800
# 216 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467776wb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 216 "./bitint-31.c" 3 4
 0xc00
# 216 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467776wb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 217 "./bitint-31.c" 3 4
 0
# 217 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467777wb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 217 "./bitint-31.c" 3 4
 0x400
# 217 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467777wb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 217 "./bitint-31.c" 3 4
 0x800
# 217 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467777wb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 217 "./bitint-31.c" 3 4
 0xc00
# 217 "./bitint-31.c"
 ); if (testflt_135 (151115722948229019467777wb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 218 "./bitint-31.c" 3 4
 0
# 218 "./bitint-31.c"
 ); if (testflt_135 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 218 "./bitint-31.c" 3 4
 0x400
# 218 "./bitint-31.c"
 ); if (testflt_135 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 218 "./bitint-31.c" 3 4
 0x800
# 218 "./bitint-31.c"
 ); if (testflt_135 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 218 "./bitint-31.c" 3 4
 0xc00
# 218 "./bitint-31.c"
 ); if (testflt_135 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 219 "./bitint-31.c" 3 4
 0
# 219 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 219 "./bitint-31.c" 3 4
 0x400
# 219 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568447wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 219 "./bitint-31.c" 3 4
 0x800
# 219 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 219 "./bitint-31.c" 3 4
 0xc00
# 219 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 220 "./bitint-31.c" 3 4
 0
# 220 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568448wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 220 "./bitint-31.c" 3 4
 0x400
# 220 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568448wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 220 "./bitint-31.c" 3 4
 0x800
# 220 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568448wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 220 "./bitint-31.c" 3 4
 0xc00
# 220 "./bitint-31.c"
 ); if (testflt_135 (-340282356779733661637539395458142568448wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 221 "./bitint-31.c" 3 4
 0
# 221 "./bitint-31.c"
 ); if (testflt_135 (-21778071482940061661655974875633165533183wb - 1) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 221 "./bitint-31.c" 3 4
 0x400
# 221 "./bitint-31.c"
 ); if (testflt_135 (-21778071482940061661655974875633165533183wb - 1) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 221 "./bitint-31.c" 3 4
 0x800
# 221 "./bitint-31.c"
 ); if (testflt_135 (-21778071482940061661655974875633165533183wb - 1) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 221 "./bitint-31.c" 3 4
 0xc00
# 221 "./bitint-31.c"
 ); if (testflt_135 (-21778071482940061661655974875633165533183wb - 1) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 222 "./bitint-31.c" 3 4
 0
# 222 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726783uwb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 222 "./bitint-31.c" 3 4
 0x400
# 222 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726783uwb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 222 "./bitint-31.c" 3 4
 0x800
# 222 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726783uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 222 "./bitint-31.c" 3 4
 0xc00
# 222 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726783uwb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 223 "./bitint-31.c" 3 4
 0
# 223 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726784uwb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 223 "./bitint-31.c" 3 4
 0x400
# 223 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726784uwb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 223 "./bitint-31.c" 3 4
 0x800
# 223 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726784uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 223 "./bitint-31.c" 3 4
 0xc00
# 223 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726784uwb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 224 "./bitint-31.c" 3 4
 0
# 224 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726785uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 224 "./bitint-31.c" 3 4
 0x400
# 224 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726785uwb) != 0xfffffep+53f) __builtin_abort (); fesetround (
# 224 "./bitint-31.c" 3 4
 0x800
# 224 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726785uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 224 "./bitint-31.c" 3 4
 0xc00
# 224 "./bitint-31.c"
 ); if (testfltu_135 (151115713941029764726785uwb) != 0xfffffep+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 225 "./bitint-31.c" 3 4
 0
# 225 "./bitint-31.c"
 ); if (testfltu_135 (151115718444629392097280uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 225 "./bitint-31.c" 3 4
 0x400
# 225 "./bitint-31.c"
 ); if (testfltu_135 (151115718444629392097280uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 225 "./bitint-31.c" 3 4
 0x800
# 225 "./bitint-31.c"
 ); if (testfltu_135 (151115718444629392097280uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 225 "./bitint-31.c" 3 4
 0xc00
# 225 "./bitint-31.c"
 ); if (testfltu_135 (151115718444629392097280uwb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 226 "./bitint-31.c" 3 4
 0
# 226 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467775uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 226 "./bitint-31.c" 3 4
 0x400
# 226 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467775uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 226 "./bitint-31.c" 3 4
 0x800
# 226 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467775uwb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 226 "./bitint-31.c" 3 4
 0xc00
# 226 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467775uwb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 227 "./bitint-31.c" 3 4
 0
# 227 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467776uwb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 227 "./bitint-31.c" 3 4
 0x400
# 227 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467776uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 227 "./bitint-31.c" 3 4
 0x800
# 227 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467776uwb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 227 "./bitint-31.c" 3 4
 0xc00
# 227 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467776uwb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 228 "./bitint-31.c" 3 4
 0
# 228 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467777uwb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 228 "./bitint-31.c" 3 4
 0x400
# 228 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467777uwb) != 0xffffffp+53f) __builtin_abort (); fesetround (
# 228 "./bitint-31.c" 3 4
 0x800
# 228 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467777uwb) != 0x1000000p+53f) __builtin_abort (); fesetround (
# 228 "./bitint-31.c" 3 4
 0xc00
# 228 "./bitint-31.c"
 ); if (testfltu_135 (151115722948229019467777uwb) != 0xffffffp+53f) __builtin_abort (); } while (0);
  do { fesetround (
# 229 "./bitint-31.c" 3 4
 0
# 229 "./bitint-31.c"
 ); if (testfltu_135 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 229 "./bitint-31.c" 3 4
 0x400
# 229 "./bitint-31.c"
 ); if (testfltu_135 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 229 "./bitint-31.c" 3 4
 0x800
# 229 "./bitint-31.c"
 ); if (testfltu_135 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 229 "./bitint-31.c" 3 4
 0xc00
# 229 "./bitint-31.c"
 ); if (testfltu_135 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 230 "./bitint-31.c" 3 4
 0
# 230 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 230 "./bitint-31.c" 3 4
 0x400
# 230 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 230 "./bitint-31.c" 3 4
 0x800
# 230 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568447uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 230 "./bitint-31.c" 3 4
 0xc00
# 230 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 231 "./bitint-31.c" 3 4
 0
# 231 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 231 "./bitint-31.c" 3 4
 0x400
# 231 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 231 "./bitint-31.c" 3 4
 0x800
# 231 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 231 "./bitint-31.c" 3 4
 0xc00
# 231 "./bitint-31.c"
 ); if (testfltu_135 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 232 "./bitint-31.c" 3 4
 0
# 232 "./bitint-31.c"
 ); if (testfltu_135 (43556142965880123323311949751266331066367uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 232 "./bitint-31.c" 3 4
 0x400
# 232 "./bitint-31.c"
 ); if (testfltu_135 (43556142965880123323311949751266331066367uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 232 "./bitint-31.c" 3 4
 0x800
# 232 "./bitint-31.c"
 ); if (testfltu_135 (43556142965880123323311949751266331066367uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 232 "./bitint-31.c" 3 4
 0xc00
# 232 "./bitint-31.c"
 ); if (testfltu_135 (43556142965880123323311949751266331066367uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);


  do { fesetround (
# 235 "./bitint-31.c" 3 4
 0
# 235 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613375wb) != -0xfffffep+79f) __builtin_abort (); fesetround (
# 235 "./bitint-31.c" 3 4
 0x400
# 235 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613375wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 235 "./bitint-31.c" 3 4
 0x800
# 235 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613375wb) != -0xfffffep+79f) __builtin_abort (); fesetround (
# 235 "./bitint-31.c" 3 4
 0xc00
# 235 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613375wb) != -0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 236 "./bitint-31.c" 3 4
 0
# 236 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613376wb) != -0xfffffep+79f) __builtin_abort (); fesetround (
# 236 "./bitint-31.c" 3 4
 0x400
# 236 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613376wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 236 "./bitint-31.c" 3 4
 0x800
# 236 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613376wb) != -0xfffffep+79f) __builtin_abort (); fesetround (
# 236 "./bitint-31.c" 3 4
 0xc00
# 236 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613376wb) != -0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 237 "./bitint-31.c" 3 4
 0
# 237 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613377wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 237 "./bitint-31.c" 3 4
 0x400
# 237 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613377wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 237 "./bitint-31.c" 3 4
 0x800
# 237 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613377wb) != -0xfffffep+79f) __builtin_abort (); fesetround (
# 237 "./bitint-31.c" 3 4
 0xc00
# 237 "./bitint-31.c"
 ); if (testflt_192 (-10141203895131470501001744613377wb) != -0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 238 "./bitint-31.c" 3 4
 0
# 238 "./bitint-31.c"
 ); if (testflt_192 (-10141204197362925404659038289920wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 238 "./bitint-31.c" 3 4
 0x400
# 238 "./bitint-31.c"
 ); if (testflt_192 (-10141204197362925404659038289920wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 238 "./bitint-31.c" 3 4
 0x800
# 238 "./bitint-31.c"
 ); if (testflt_192 (-10141204197362925404659038289920wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 238 "./bitint-31.c" 3 4
 0xc00
# 238 "./bitint-31.c"
 ); if (testflt_192 (-10141204197362925404659038289920wb) != -0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 239 "./bitint-31.c" 3 4
 0
# 239 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966463wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 239 "./bitint-31.c" 3 4
 0x400
# 239 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966463wb) != -0x1000000p+79f) __builtin_abort (); fesetround (
# 239 "./bitint-31.c" 3 4
 0x800
# 239 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966463wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 239 "./bitint-31.c" 3 4
 0xc00
# 239 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966463wb) != -0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 240 "./bitint-31.c" 3 4
 0
# 240 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966464wb) != -0x1000000p+79f) __builtin_abort (); fesetround (
# 240 "./bitint-31.c" 3 4
 0x400
# 240 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966464wb) != -0x1000000p+79f) __builtin_abort (); fesetround (
# 240 "./bitint-31.c" 3 4
 0x800
# 240 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966464wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 240 "./bitint-31.c" 3 4
 0xc00
# 240 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966464wb) != -0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 241 "./bitint-31.c" 3 4
 0
# 241 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966465wb) != -0x1000000p+79f) __builtin_abort (); fesetround (
# 241 "./bitint-31.c" 3 4
 0x400
# 241 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966465wb) != -0x1000000p+79f) __builtin_abort (); fesetround (
# 241 "./bitint-31.c" 3 4
 0x800
# 241 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966465wb) != -0xffffffp+79f) __builtin_abort (); fesetround (
# 241 "./bitint-31.c" 3 4
 0xc00
# 241 "./bitint-31.c"
 ); if (testflt_192 (-10141204499594380308316331966465wb) != -0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 242 "./bitint-31.c" 3 4
 0
# 242 "./bitint-31.c"
 ); if (testflt_192 (340282346638528859811704183484516925440wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 242 "./bitint-31.c" 3 4
 0x400
# 242 "./bitint-31.c"
 ); if (testflt_192 (340282346638528859811704183484516925440wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 242 "./bitint-31.c" 3 4
 0x800
# 242 "./bitint-31.c"
 ); if (testflt_192 (340282346638528859811704183484516925440wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 242 "./bitint-31.c" 3 4
 0xc00
# 242 "./bitint-31.c"
 ); if (testflt_192 (340282346638528859811704183484516925440wb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 243 "./bitint-31.c" 3 4
 0
# 243 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568447wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 243 "./bitint-31.c" 3 4
 0x400
# 243 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568447wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 243 "./bitint-31.c" 3 4
 0x800
# 243 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568447wb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 243 "./bitint-31.c" 3 4
 0xc00
# 243 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568447wb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 244 "./bitint-31.c" 3 4
 0
# 244 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568448wb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 244 "./bitint-31.c" 3 4
 0x400
# 244 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568448wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 244 "./bitint-31.c" 3 4
 0x800
# 244 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568448wb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 244 "./bitint-31.c" 3 4
 0xc00
# 244 "./bitint-31.c"
 ); if (testflt_192 (340282356779733661637539395458142568448wb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 245 "./bitint-31.c" 3 4
 0
# 245 "./bitint-31.c"
 ); if (testflt_192 (3138550867693340381917894711603833208051177722232017256447wb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 245 "./bitint-31.c" 3 4
 0x400
# 245 "./bitint-31.c"
 ); if (testflt_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 245 "./bitint-31.c" 3 4
 0x800
# 245 "./bitint-31.c"
 ); if (testflt_192 (3138550867693340381917894711603833208051177722232017256447wb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 245 "./bitint-31.c" 3 4
 0xc00
# 245 "./bitint-31.c"
 ); if (testflt_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 246 "./bitint-31.c" 3 4
 0
# 246 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613375uwb) != 0xfffffep+79f) __builtin_abort (); fesetround (
# 246 "./bitint-31.c" 3 4
 0x400
# 246 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613375uwb) != 0xfffffep+79f) __builtin_abort (); fesetround (
# 246 "./bitint-31.c" 3 4
 0x800
# 246 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613375uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 246 "./bitint-31.c" 3 4
 0xc00
# 246 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613375uwb) != 0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 247 "./bitint-31.c" 3 4
 0
# 247 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613376uwb) != 0xfffffep+79f) __builtin_abort (); fesetround (
# 247 "./bitint-31.c" 3 4
 0x400
# 247 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613376uwb) != 0xfffffep+79f) __builtin_abort (); fesetround (
# 247 "./bitint-31.c" 3 4
 0x800
# 247 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613376uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 247 "./bitint-31.c" 3 4
 0xc00
# 247 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613376uwb) != 0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 248 "./bitint-31.c" 3 4
 0
# 248 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613377uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 248 "./bitint-31.c" 3 4
 0x400
# 248 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613377uwb) != 0xfffffep+79f) __builtin_abort (); fesetround (
# 248 "./bitint-31.c" 3 4
 0x800
# 248 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613377uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 248 "./bitint-31.c" 3 4
 0xc00
# 248 "./bitint-31.c"
 ); if (testfltu_192 (10141203895131470501001744613377uwb) != 0xfffffep+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 249 "./bitint-31.c" 3 4
 0
# 249 "./bitint-31.c"
 ); if (testfltu_192 (10141204197362925404659038289920uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 249 "./bitint-31.c" 3 4
 0x400
# 249 "./bitint-31.c"
 ); if (testfltu_192 (10141204197362925404659038289920uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 249 "./bitint-31.c" 3 4
 0x800
# 249 "./bitint-31.c"
 ); if (testfltu_192 (10141204197362925404659038289920uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 249 "./bitint-31.c" 3 4
 0xc00
# 249 "./bitint-31.c"
 ); if (testfltu_192 (10141204197362925404659038289920uwb) != 0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 250 "./bitint-31.c" 3 4
 0
# 250 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966463uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 250 "./bitint-31.c" 3 4
 0x400
# 250 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966463uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 250 "./bitint-31.c" 3 4
 0x800
# 250 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966463uwb) != 0x1000000p+79f) __builtin_abort (); fesetround (
# 250 "./bitint-31.c" 3 4
 0xc00
# 250 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966463uwb) != 0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 251 "./bitint-31.c" 3 4
 0
# 251 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966464uwb) != 0x1000000p+79f) __builtin_abort (); fesetround (
# 251 "./bitint-31.c" 3 4
 0x400
# 251 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966464uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 251 "./bitint-31.c" 3 4
 0x800
# 251 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966464uwb) != 0x1000000p+79f) __builtin_abort (); fesetround (
# 251 "./bitint-31.c" 3 4
 0xc00
# 251 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966464uwb) != 0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 252 "./bitint-31.c" 3 4
 0
# 252 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966465uwb) != 0x1000000p+79f) __builtin_abort (); fesetround (
# 252 "./bitint-31.c" 3 4
 0x400
# 252 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966465uwb) != 0xffffffp+79f) __builtin_abort (); fesetround (
# 252 "./bitint-31.c" 3 4
 0x800
# 252 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966465uwb) != 0x1000000p+79f) __builtin_abort (); fesetround (
# 252 "./bitint-31.c" 3 4
 0xc00
# 252 "./bitint-31.c"
 ); if (testfltu_192 (10141204499594380308316331966465uwb) != 0xffffffp+79f) __builtin_abort (); } while (0);
  do { fesetround (
# 253 "./bitint-31.c" 3 4
 0
# 253 "./bitint-31.c"
 ); if (testfltu_192 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 253 "./bitint-31.c" 3 4
 0x400
# 253 "./bitint-31.c"
 ); if (testfltu_192 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 253 "./bitint-31.c" 3 4
 0x800
# 253 "./bitint-31.c"
 ); if (testfltu_192 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 253 "./bitint-31.c" 3 4
 0xc00
# 253 "./bitint-31.c"
 ); if (testfltu_192 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 254 "./bitint-31.c" 3 4
 0
# 254 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 254 "./bitint-31.c" 3 4
 0x400
# 254 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 254 "./bitint-31.c" 3 4
 0x800
# 254 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568447uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 254 "./bitint-31.c" 3 4
 0xc00
# 254 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 255 "./bitint-31.c" 3 4
 0
# 255 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 255 "./bitint-31.c" 3 4
 0x400
# 255 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 255 "./bitint-31.c" 3 4
 0x800
# 255 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 255 "./bitint-31.c" 3 4
 0xc00
# 255 "./bitint-31.c"
 ); if (testfltu_192 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 256 "./bitint-31.c" 3 4
 0
# 256 "./bitint-31.c"
 ); if (testfltu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 256 "./bitint-31.c" 3 4
 0x400
# 256 "./bitint-31.c"
 ); if (testfltu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 256 "./bitint-31.c" 3 4
 0x800
# 256 "./bitint-31.c"
 ); if (testfltu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 256 "./bitint-31.c" 3 4
 0xc00
# 256 "./bitint-31.c"
 ); if (testfltu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);


  do { fesetround (
# 259 "./bitint-31.c" 3 4
 0
# 259 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352575wb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 259 "./bitint-31.c" 3 4
 0x400
# 259 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352575wb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 259 "./bitint-31.c" 3 4
 0x800
# 259 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352575wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 259 "./bitint-31.c" 3 4
 0xc00
# 259 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352575wb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 260 "./bitint-31.c" 3 4
 0
# 260 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352576wb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 260 "./bitint-31.c" 3 4
 0x400
# 260 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352576wb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 260 "./bitint-31.c" 3 4
 0x800
# 260 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352576wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 260 "./bitint-31.c" 3 4
 0xc00
# 260 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352576wb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 261 "./bitint-31.c" 3 4
 0
# 261 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352577wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 261 "./bitint-31.c" 3 4
 0x400
# 261 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352577wb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 261 "./bitint-31.c" 3 4
 0x800
# 261 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352577wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 261 "./bitint-31.c" 3 4
 0xc00
# 261 "./bitint-31.c"
 ); if (testflt_575 (10633823015541376812058405359715352577wb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 262 "./bitint-31.c" 3 4
 0
# 262 "./bitint-31.c"
 ); if (testflt_575 (10633823332454026869115755733891153920wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 262 "./bitint-31.c" 3 4
 0x400
# 262 "./bitint-31.c"
 ); if (testflt_575 (10633823332454026869115755733891153920wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 262 "./bitint-31.c" 3 4
 0x800
# 262 "./bitint-31.c"
 ); if (testflt_575 (10633823332454026869115755733891153920wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 262 "./bitint-31.c" 3 4
 0xc00
# 262 "./bitint-31.c"
 ); if (testflt_575 (10633823332454026869115755733891153920wb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 263 "./bitint-31.c" 3 4
 0
# 263 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955263wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 263 "./bitint-31.c" 3 4
 0x400
# 263 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955263wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 263 "./bitint-31.c" 3 4
 0x800
# 263 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955263wb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 263 "./bitint-31.c" 3 4
 0xc00
# 263 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955263wb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 264 "./bitint-31.c" 3 4
 0
# 264 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955264wb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 264 "./bitint-31.c" 3 4
 0x400
# 264 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955264wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 264 "./bitint-31.c" 3 4
 0x800
# 264 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955264wb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 264 "./bitint-31.c" 3 4
 0xc00
# 264 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955264wb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 265 "./bitint-31.c" 3 4
 0
# 265 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955265wb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 265 "./bitint-31.c" 3 4
 0x400
# 265 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955265wb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 265 "./bitint-31.c" 3 4
 0x800
# 265 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955265wb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 265 "./bitint-31.c" 3 4
 0xc00
# 265 "./bitint-31.c"
 ); if (testflt_575 (10633823649366676926173106108066955265wb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 266 "./bitint-31.c" 3 4
 0
# 266 "./bitint-31.c"
 ); if (testflt_575 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 266 "./bitint-31.c" 3 4
 0x400
# 266 "./bitint-31.c"
 ); if (testflt_575 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 266 "./bitint-31.c" 3 4
 0x800
# 266 "./bitint-31.c"
 ); if (testflt_575 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 266 "./bitint-31.c" 3 4
 0xc00
# 266 "./bitint-31.c"
 ); if (testflt_575 (-340282346638528859811704183484516925440wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 267 "./bitint-31.c" 3 4
 0
# 267 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 267 "./bitint-31.c" 3 4
 0x400
# 267 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568447wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 267 "./bitint-31.c" 3 4
 0x800
# 267 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 267 "./bitint-31.c" 3 4
 0xc00
# 267 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568447wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 268 "./bitint-31.c" 3 4
 0
# 268 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568448wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 268 "./bitint-31.c" 3 4
 0x400
# 268 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568448wb) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 268 "./bitint-31.c" 3 4
 0x800
# 268 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568448wb) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 268 "./bitint-31.c" 3 4
 0xc00
# 268 "./bitint-31.c"
 ); if (testflt_575 (-340282356779733661637539395458142568448wb) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 269 "./bitint-31.c" 3 4
 0
# 269 "./bitint-31.c"
 ); if (testflt_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 269 "./bitint-31.c" 3 4
 0x400
# 269 "./bitint-31.c"
 ); if (testflt_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -__builtin_inff ()) __builtin_abort (); fesetround (
# 269 "./bitint-31.c" 3 4
 0x800
# 269 "./bitint-31.c"
 ); if (testflt_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0xffffffp+104f) __builtin_abort (); fesetround (
# 269 "./bitint-31.c" 3 4
 0xc00
# 269 "./bitint-31.c"
 ); if (testflt_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 270 "./bitint-31.c" 3 4
 0
# 270 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352575uwb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 270 "./bitint-31.c" 3 4
 0x400
# 270 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352575uwb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 270 "./bitint-31.c" 3 4
 0x800
# 270 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352575uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 270 "./bitint-31.c" 3 4
 0xc00
# 270 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352575uwb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 271 "./bitint-31.c" 3 4
 0
# 271 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352576uwb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 271 "./bitint-31.c" 3 4
 0x400
# 271 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352576uwb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 271 "./bitint-31.c" 3 4
 0x800
# 271 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352576uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 271 "./bitint-31.c" 3 4
 0xc00
# 271 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352576uwb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 272 "./bitint-31.c" 3 4
 0
# 272 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352577uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 272 "./bitint-31.c" 3 4
 0x400
# 272 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352577uwb) != 0xfffffep+99f) __builtin_abort (); fesetround (
# 272 "./bitint-31.c" 3 4
 0x800
# 272 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352577uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 272 "./bitint-31.c" 3 4
 0xc00
# 272 "./bitint-31.c"
 ); if (testfltu_575 (10633823015541376812058405359715352577uwb) != 0xfffffep+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 273 "./bitint-31.c" 3 4
 0
# 273 "./bitint-31.c"
 ); if (testfltu_575 (10633823332454026869115755733891153920uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 273 "./bitint-31.c" 3 4
 0x400
# 273 "./bitint-31.c"
 ); if (testfltu_575 (10633823332454026869115755733891153920uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 273 "./bitint-31.c" 3 4
 0x800
# 273 "./bitint-31.c"
 ); if (testfltu_575 (10633823332454026869115755733891153920uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 273 "./bitint-31.c" 3 4
 0xc00
# 273 "./bitint-31.c"
 ); if (testfltu_575 (10633823332454026869115755733891153920uwb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 274 "./bitint-31.c" 3 4
 0
# 274 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955263uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 274 "./bitint-31.c" 3 4
 0x400
# 274 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955263uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 274 "./bitint-31.c" 3 4
 0x800
# 274 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955263uwb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 274 "./bitint-31.c" 3 4
 0xc00
# 274 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955263uwb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 275 "./bitint-31.c" 3 4
 0
# 275 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955264uwb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 275 "./bitint-31.c" 3 4
 0x400
# 275 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955264uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 275 "./bitint-31.c" 3 4
 0x800
# 275 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955264uwb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 275 "./bitint-31.c" 3 4
 0xc00
# 275 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955264uwb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 276 "./bitint-31.c" 3 4
 0
# 276 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955265uwb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 276 "./bitint-31.c" 3 4
 0x400
# 276 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955265uwb) != 0xffffffp+99f) __builtin_abort (); fesetround (
# 276 "./bitint-31.c" 3 4
 0x800
# 276 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955265uwb) != 0x1000000p+99f) __builtin_abort (); fesetround (
# 276 "./bitint-31.c" 3 4
 0xc00
# 276 "./bitint-31.c"
 ); if (testfltu_575 (10633823649366676926173106108066955265uwb) != 0xffffffp+99f) __builtin_abort (); } while (0);
  do { fesetround (
# 277 "./bitint-31.c" 3 4
 0
# 277 "./bitint-31.c"
 ); if (testfltu_575 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 277 "./bitint-31.c" 3 4
 0x400
# 277 "./bitint-31.c"
 ); if (testfltu_575 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 277 "./bitint-31.c" 3 4
 0x800
# 277 "./bitint-31.c"
 ); if (testfltu_575 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 277 "./bitint-31.c" 3 4
 0xc00
# 277 "./bitint-31.c"
 ); if (testfltu_575 (340282346638528859811704183484516925440uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 278 "./bitint-31.c" 3 4
 0
# 278 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 278 "./bitint-31.c" 3 4
 0x400
# 278 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 278 "./bitint-31.c" 3 4
 0x800
# 278 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568447uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 278 "./bitint-31.c" 3 4
 0xc00
# 278 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568447uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 279 "./bitint-31.c" 3 4
 0
# 279 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 279 "./bitint-31.c" 3 4
 0x400
# 279 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 279 "./bitint-31.c" 3 4
 0x800
# 279 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568448uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 279 "./bitint-31.c" 3 4
 0xc00
# 279 "./bitint-31.c"
 ); if (testfltu_575 (340282356779733661637539395458142568448uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);
  do { fesetround (
# 280 "./bitint-31.c" 3 4
 0
# 280 "./bitint-31.c"
 ); if (testfltu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 280 "./bitint-31.c" 3 4
 0x400
# 280 "./bitint-31.c"
 ); if (testfltu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0xffffffp+104f) __builtin_abort (); fesetround (
# 280 "./bitint-31.c" 3 4
 0x800
# 280 "./bitint-31.c"
 ); if (testfltu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != __builtin_inff ()) __builtin_abort (); fesetround (
# 280 "./bitint-31.c" 3 4
 0xc00
# 280 "./bitint-31.c"
 ); if (testfltu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0xffffffp+104f) __builtin_abort (); } while (0);




  do { fesetround (
# 285 "./bitint-31.c" 3 4
 0
# 285 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602943wb) != -0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 285 "./bitint-31.c" 3 4
 0x400
# 285 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602943wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 285 "./bitint-31.c" 3 4
 0x800
# 285 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602943wb) != -0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 285 "./bitint-31.c" 3 4
 0xc00
# 285 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602943wb) != -0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 286 "./bitint-31.c" 3 4
 0
# 286 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602944wb) != -0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 286 "./bitint-31.c" 3 4
 0x400
# 286 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602944wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 286 "./bitint-31.c" 3 4
 0x800
# 286 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602944wb) != -0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 286 "./bitint-31.c" 3 4
 0xc00
# 286 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602944wb) != -0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 287 "./bitint-31.c" 3 4
 0
# 287 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602945wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 287 "./bitint-31.c" 3 4
 0x400
# 287 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602945wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 287 "./bitint-31.c" 3 4
 0x800
# 287 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602945wb) != -0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 287 "./bitint-31.c" 3 4
 0xc00
# 287 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558650424686050812251602945wb) != -0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 288 "./bitint-31.c" 3 4
 0
# 288 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558651605277671529662906368wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 288 "./bitint-31.c" 3 4
 0x400
# 288 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558651605277671529662906368wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 288 "./bitint-31.c" 3 4
 0x800
# 288 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558651605277671529662906368wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 288 "./bitint-31.c" 3 4
 0xc00
# 288 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558651605277671529662906368wb) != -0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 289 "./bitint-31.c" 3 4
 0
# 289 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209791wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 289 "./bitint-31.c" 3 4
 0x400
# 289 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209791wb) != -0x20000000000000p+71) __builtin_abort (); fesetround (
# 289 "./bitint-31.c" 3 4
 0x800
# 289 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209791wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 289 "./bitint-31.c" 3 4
 0xc00
# 289 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209791wb) != -0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 290 "./bitint-31.c" 3 4
 0
# 290 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209792wb) != -0x20000000000000p+71) __builtin_abort (); fesetround (
# 290 "./bitint-31.c" 3 4
 0x400
# 290 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209792wb) != -0x20000000000000p+71) __builtin_abort (); fesetround (
# 290 "./bitint-31.c" 3 4
 0x800
# 290 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209792wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 290 "./bitint-31.c" 3 4
 0xc00
# 290 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209792wb) != -0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 291 "./bitint-31.c" 3 4
 0
# 291 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209793wb) != -0x20000000000000p+71) __builtin_abort (); fesetround (
# 291 "./bitint-31.c" 3 4
 0x400
# 291 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209793wb) != -0x20000000000000p+71) __builtin_abort (); fesetround (
# 291 "./bitint-31.c" 3 4
 0x800
# 291 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209793wb) != -0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 291 "./bitint-31.c" 3 4
 0xc00
# 291 "./bitint-31.c"
 ); if (testdbl_135 (-21267647932558652785869292247074209793wb) != -0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 292 "./bitint-31.c" 3 4
 0
# 292 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940059243804335646374816120832wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 292 "./bitint-31.c" 3 4
 0x400
# 292 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940059243804335646374816120832wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 292 "./bitint-31.c" 3 4
 0x800
# 292 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940059243804335646374816120832wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 292 "./bitint-31.c" 3 4
 0xc00
# 292 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940059243804335646374816120832wb) != 0x1fffffffffffffp+81) __builtin_abort (); } while (0);
  do { fesetround (
# 293 "./bitint-31.c" 3 4
 0
# 293 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827007wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 293 "./bitint-31.c" 3 4
 0x400
# 293 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827007wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 293 "./bitint-31.c" 3 4
 0x800
# 293 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827007wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 293 "./bitint-31.c" 3 4
 0xc00
# 293 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827007wb) != 0x1fffffffffffffp+81) __builtin_abort (); } while (0);
  do { fesetround (
# 294 "./bitint-31.c" 3 4
 0
# 294 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827008wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 294 "./bitint-31.c" 3 4
 0x400
# 294 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827008wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 294 "./bitint-31.c" 3 4
 0x800
# 294 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827008wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 294 "./bitint-31.c" 3 4
 0xc00
# 294 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827008wb) != 0x1fffffffffffffp+81) __builtin_abort (); } while (0);
  do { fesetround (
# 295 "./bitint-31.c" 3 4
 0
# 295 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827009wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 295 "./bitint-31.c" 3 4
 0x400
# 295 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827009wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 295 "./bitint-31.c" 3 4
 0x800
# 295 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827009wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 295 "./bitint-31.c" 3 4
 0xc00
# 295 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940060452730155261003990827009wb) != 0x1fffffffffffffp+81) __builtin_abort (); } while (0);
  do { fesetround (
# 296 "./bitint-31.c" 3 4
 0
# 296 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940061661655974875633165533183wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 296 "./bitint-31.c" 3 4
 0x400
# 296 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940061661655974875633165533183wb) != 0x1fffffffffffffp+81) __builtin_abort (); fesetround (
# 296 "./bitint-31.c" 3 4
 0x800
# 296 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940061661655974875633165533183wb) != 0x20000000000000p+81) __builtin_abort (); fesetround (
# 296 "./bitint-31.c" 3 4
 0xc00
# 296 "./bitint-31.c"
 ); if (testdbl_135 (21778071482940061661655974875633165533183wb) != 0x1fffffffffffffp+81) __builtin_abort (); } while (0);
  do { fesetround (
# 297 "./bitint-31.c" 3 4
 0
# 297 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602943uwb) != 0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 297 "./bitint-31.c" 3 4
 0x400
# 297 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602943uwb) != 0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 297 "./bitint-31.c" 3 4
 0x800
# 297 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602943uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 297 "./bitint-31.c" 3 4
 0xc00
# 297 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602943uwb) != 0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 298 "./bitint-31.c" 3 4
 0
# 298 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602944uwb) != 0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 298 "./bitint-31.c" 3 4
 0x400
# 298 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602944uwb) != 0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 298 "./bitint-31.c" 3 4
 0x800
# 298 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602944uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 298 "./bitint-31.c" 3 4
 0xc00
# 298 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602944uwb) != 0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 299 "./bitint-31.c" 3 4
 0
# 299 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602945uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 299 "./bitint-31.c" 3 4
 0x400
# 299 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602945uwb) != 0x1ffffffffffffep+71) __builtin_abort (); fesetround (
# 299 "./bitint-31.c" 3 4
 0x800
# 299 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602945uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 299 "./bitint-31.c" 3 4
 0xc00
# 299 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558650424686050812251602945uwb) != 0x1ffffffffffffep+71) __builtin_abort (); } while (0);
  do { fesetround (
# 300 "./bitint-31.c" 3 4
 0
# 300 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558651605277671529662906368uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 300 "./bitint-31.c" 3 4
 0x400
# 300 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558651605277671529662906368uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 300 "./bitint-31.c" 3 4
 0x800
# 300 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558651605277671529662906368uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 300 "./bitint-31.c" 3 4
 0xc00
# 300 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558651605277671529662906368uwb) != 0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 301 "./bitint-31.c" 3 4
 0
# 301 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209791uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 301 "./bitint-31.c" 3 4
 0x400
# 301 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209791uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 301 "./bitint-31.c" 3 4
 0x800
# 301 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209791uwb) != 0x20000000000000p+71) __builtin_abort (); fesetround (
# 301 "./bitint-31.c" 3 4
 0xc00
# 301 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209791uwb) != 0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 302 "./bitint-31.c" 3 4
 0
# 302 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209792uwb) != 0x20000000000000p+71) __builtin_abort (); fesetround (
# 302 "./bitint-31.c" 3 4
 0x400
# 302 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209792uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 302 "./bitint-31.c" 3 4
 0x800
# 302 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209792uwb) != 0x20000000000000p+71) __builtin_abort (); fesetround (
# 302 "./bitint-31.c" 3 4
 0xc00
# 302 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209792uwb) != 0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 303 "./bitint-31.c" 3 4
 0
# 303 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209793uwb) != 0x20000000000000p+71) __builtin_abort (); fesetround (
# 303 "./bitint-31.c" 3 4
 0x400
# 303 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209793uwb) != 0x1fffffffffffffp+71) __builtin_abort (); fesetround (
# 303 "./bitint-31.c" 3 4
 0x800
# 303 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209793uwb) != 0x20000000000000p+71) __builtin_abort (); fesetround (
# 303 "./bitint-31.c" 3 4
 0xc00
# 303 "./bitint-31.c"
 ); if (testdblu_135 (21267647932558652785869292247074209793uwb) != 0x1fffffffffffffp+71) __builtin_abort (); } while (0);
  do { fesetround (
# 304 "./bitint-31.c" 3 4
 0
# 304 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880118487608671292749632241664uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 304 "./bitint-31.c" 3 4
 0x400
# 304 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880118487608671292749632241664uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 304 "./bitint-31.c" 3 4
 0x800
# 304 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880118487608671292749632241664uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 304 "./bitint-31.c" 3 4
 0xc00
# 304 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880118487608671292749632241664uwb) != 0x1fffffffffffffp+82) __builtin_abort (); } while (0);
  do { fesetround (
# 305 "./bitint-31.c" 3 4
 0
# 305 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654015uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 305 "./bitint-31.c" 3 4
 0x400
# 305 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654015uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 305 "./bitint-31.c" 3 4
 0x800
# 305 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654015uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 305 "./bitint-31.c" 3 4
 0xc00
# 305 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654015uwb) != 0x1fffffffffffffp+82) __builtin_abort (); } while (0);
  do { fesetround (
# 306 "./bitint-31.c" 3 4
 0
# 306 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654016uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 306 "./bitint-31.c" 3 4
 0x400
# 306 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654016uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 306 "./bitint-31.c" 3 4
 0x800
# 306 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654016uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 306 "./bitint-31.c" 3 4
 0xc00
# 306 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654016uwb) != 0x1fffffffffffffp+82) __builtin_abort (); } while (0);
  do { fesetround (
# 307 "./bitint-31.c" 3 4
 0
# 307 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654017uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 307 "./bitint-31.c" 3 4
 0x400
# 307 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654017uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 307 "./bitint-31.c" 3 4
 0x800
# 307 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654017uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 307 "./bitint-31.c" 3 4
 0xc00
# 307 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880120905460310522007981654017uwb) != 0x1fffffffffffffp+82) __builtin_abort (); } while (0);
  do { fesetround (
# 308 "./bitint-31.c" 3 4
 0
# 308 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880123323311949751266331066367uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 308 "./bitint-31.c" 3 4
 0x400
# 308 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880123323311949751266331066367uwb) != 0x1fffffffffffffp+82) __builtin_abort (); fesetround (
# 308 "./bitint-31.c" 3 4
 0x800
# 308 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880123323311949751266331066367uwb) != 0x20000000000000p+82) __builtin_abort (); fesetround (
# 308 "./bitint-31.c" 3 4
 0xc00
# 308 "./bitint-31.c"
 ); if (testdblu_135 (43556142965880123323311949751266331066367uwb) != 0x1fffffffffffffp+82) __builtin_abort (); } while (0);


  do { fesetround (
# 311 "./bitint-31.c" 3 4
 0
# 311 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430975wb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 311 "./bitint-31.c" 3 4
 0x400
# 311 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430975wb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 311 "./bitint-31.c" 3 4
 0x800
# 311 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430975wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 311 "./bitint-31.c" 3 4
 0xc00
# 311 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430975wb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 312 "./bitint-31.c" 3 4
 0
# 312 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430976wb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 312 "./bitint-31.c" 3 4
 0x400
# 312 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430976wb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 312 "./bitint-31.c" 3 4
 0x800
# 312 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430976wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 312 "./bitint-31.c" 3 4
 0xc00
# 312 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430976wb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 313 "./bitint-31.c" 3 4
 0
# 313 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430977wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 313 "./bitint-31.c" 3 4
 0x400
# 313 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430977wb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 313 "./bitint-31.c" 3 4
 0x800
# 313 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430977wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 313 "./bitint-31.c" 3 4
 0xc00
# 313 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122477710862401666030147234430977wb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 314 "./bitint-31.c" 3 4
 0
# 314 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122482662622558807551246830927872wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 314 "./bitint-31.c" 3 4
 0x400
# 314 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122482662622558807551246830927872wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 314 "./bitint-31.c" 3 4
 0x800
# 314 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122482662622558807551246830927872wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 314 "./bitint-31.c" 3 4
 0xc00
# 314 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122482662622558807551246830927872wb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 315 "./bitint-31.c" 3 4
 0
# 315 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424767wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 315 "./bitint-31.c" 3 4
 0x400
# 315 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424767wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 315 "./bitint-31.c" 3 4
 0x800
# 315 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424767wb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 315 "./bitint-31.c" 3 4
 0xc00
# 315 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424767wb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 316 "./bitint-31.c" 3 4
 0
# 316 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424768wb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 316 "./bitint-31.c" 3 4
 0x400
# 316 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424768wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 316 "./bitint-31.c" 3 4
 0x800
# 316 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424768wb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 316 "./bitint-31.c" 3 4
 0xc00
# 316 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424768wb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 317 "./bitint-31.c" 3 4
 0
# 317 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424769wb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 317 "./bitint-31.c" 3 4
 0x400
# 317 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424769wb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 317 "./bitint-31.c" 3 4
 0x800
# 317 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424769wb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 317 "./bitint-31.c" 3 4
 0xc00
# 317 "./bitint-31.c"
 ); if (testdbl_192 (89202980794122487614382715949072346427424769wb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 318 "./bitint-31.c" 3 4
 0
# 318 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340033468750984562846621555579712101368725504wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 318 "./bitint-31.c" 3 4
 0x400
# 318 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340033468750984562846621555579712101368725504wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 318 "./bitint-31.c" 3 4
 0x800
# 318 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340033468750984562846621555579712101368725504wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 318 "./bitint-31.c" 3 4
 0xc00
# 318 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340033468750984562846621555579712101368725504wb) != -0x1fffffffffffffp+138) __builtin_abort (); } while (0);
  do { fesetround (
# 319 "./bitint-31.c" 3 4
 0
# 319 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990975wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 319 "./bitint-31.c" 3 4
 0x400
# 319 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990975wb) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 319 "./bitint-31.c" 3 4
 0x800
# 319 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990975wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 319 "./bitint-31.c" 3 4
 0xc00
# 319 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990975wb) != -0x1fffffffffffffp+138) __builtin_abort (); } while (0);
  do { fesetround (
# 320 "./bitint-31.c" 3 4
 0
# 320 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990976wb) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 320 "./bitint-31.c" 3 4
 0x400
# 320 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990976wb) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 320 "./bitint-31.c" 3 4
 0x800
# 320 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990976wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 320 "./bitint-31.c" 3 4
 0xc00
# 320 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990976wb) != -0x1fffffffffffffp+138) __builtin_abort (); } while (0);
  do { fesetround (
# 321 "./bitint-31.c" 3 4
 0
# 321 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990977wb) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 321 "./bitint-31.c" 3 4
 0x400
# 321 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990977wb) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 321 "./bitint-31.c" 3 4
 0x800
# 321 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990977wb) != -0x1fffffffffffffp+138) __builtin_abort (); fesetround (
# 321 "./bitint-31.c" 3 4
 0xc00
# 321 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340207693322848083339914803378717166692990977wb) != -0x1fffffffffffffp+138) __builtin_abort (); } while (0);
  do { fesetround (
# 322 "./bitint-31.c" 3 4
 0
# 322 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 322 "./bitint-31.c" 3 4
 0x400
# 322 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 322 "./bitint-31.c" 3 4
 0x800
# 322 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1) != -0x20000000000000p+138) __builtin_abort (); fesetround (
# 322 "./bitint-31.c" 3 4
 0xc00
# 322 "./bitint-31.c"
 ); if (testdbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1) != -0x20000000000000p+138) __builtin_abort (); } while (0);
  do { fesetround (
# 323 "./bitint-31.c" 3 4
 0
# 323 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430975uwb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 323 "./bitint-31.c" 3 4
 0x400
# 323 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430975uwb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 323 "./bitint-31.c" 3 4
 0x800
# 323 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430975uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 323 "./bitint-31.c" 3 4
 0xc00
# 323 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430975uwb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 324 "./bitint-31.c" 3 4
 0
# 324 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430976uwb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 324 "./bitint-31.c" 3 4
 0x400
# 324 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430976uwb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 324 "./bitint-31.c" 3 4
 0x800
# 324 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430976uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 324 "./bitint-31.c" 3 4
 0xc00
# 324 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430976uwb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 325 "./bitint-31.c" 3 4
 0
# 325 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430977uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 325 "./bitint-31.c" 3 4
 0x400
# 325 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430977uwb) != 0x1ffffffffffffep+93) __builtin_abort (); fesetround (
# 325 "./bitint-31.c" 3 4
 0x800
# 325 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430977uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 325 "./bitint-31.c" 3 4
 0xc00
# 325 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122477710862401666030147234430977uwb) != 0x1ffffffffffffep+93) __builtin_abort (); } while (0);
  do { fesetround (
# 326 "./bitint-31.c" 3 4
 0
# 326 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122482662622558807551246830927872uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 326 "./bitint-31.c" 3 4
 0x400
# 326 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122482662622558807551246830927872uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 326 "./bitint-31.c" 3 4
 0x800
# 326 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122482662622558807551246830927872uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 326 "./bitint-31.c" 3 4
 0xc00
# 326 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122482662622558807551246830927872uwb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 327 "./bitint-31.c" 3 4
 0
# 327 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424767uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 327 "./bitint-31.c" 3 4
 0x400
# 327 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424767uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 327 "./bitint-31.c" 3 4
 0x800
# 327 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424767uwb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 327 "./bitint-31.c" 3 4
 0xc00
# 327 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424767uwb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 328 "./bitint-31.c" 3 4
 0
# 328 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424768uwb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 328 "./bitint-31.c" 3 4
 0x400
# 328 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424768uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 328 "./bitint-31.c" 3 4
 0x800
# 328 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424768uwb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 328 "./bitint-31.c" 3 4
 0xc00
# 328 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424768uwb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 329 "./bitint-31.c" 3 4
 0
# 329 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424769uwb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 329 "./bitint-31.c" 3 4
 0x400
# 329 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424769uwb) != 0x1fffffffffffffp+93) __builtin_abort (); fesetround (
# 329 "./bitint-31.c" 3 4
 0x800
# 329 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424769uwb) != 0x20000000000000p+93) __builtin_abort (); fesetround (
# 329 "./bitint-31.c" 3 4
 0xc00
# 329 "./bitint-31.c"
 ); if (testdblu_192 (89202980794122487614382715949072346427424769uwb) != 0x1fffffffffffffp+93) __builtin_abort (); } while (0);
  do { fesetround (
# 330 "./bitint-31.c" 3 4
 0
# 330 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680066937501969125693243111159424202737451008uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 330 "./bitint-31.c" 3 4
 0x400
# 330 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680066937501969125693243111159424202737451008uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 330 "./bitint-31.c" 3 4
 0x800
# 330 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680066937501969125693243111159424202737451008uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 330 "./bitint-31.c" 3 4
 0xc00
# 330 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680066937501969125693243111159424202737451008uwb) != 0x1fffffffffffffp+139) __builtin_abort (); } while (0);
  do { fesetround (
# 331 "./bitint-31.c" 3 4
 0
# 331 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981951uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 331 "./bitint-31.c" 3 4
 0x400
# 331 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981951uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 331 "./bitint-31.c" 3 4
 0x800
# 331 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981951uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 331 "./bitint-31.c" 3 4
 0xc00
# 331 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981951uwb) != 0x1fffffffffffffp+139) __builtin_abort (); } while (0);
  do { fesetround (
# 332 "./bitint-31.c" 3 4
 0
# 332 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981952uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 332 "./bitint-31.c" 3 4
 0x400
# 332 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981952uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 332 "./bitint-31.c" 3 4
 0x800
# 332 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981952uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 332 "./bitint-31.c" 3 4
 0xc00
# 332 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981952uwb) != 0x1fffffffffffffp+139) __builtin_abort (); } while (0);
  do { fesetround (
# 333 "./bitint-31.c" 3 4
 0
# 333 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981953uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 333 "./bitint-31.c" 3 4
 0x400
# 333 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981953uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 333 "./bitint-31.c" 3 4
 0x800
# 333 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981953uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 333 "./bitint-31.c" 3 4
 0xc00
# 333 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680415386645696166679829606757434333385981953uwb) != 0x1fffffffffffffp+139) __builtin_abort (); } while (0);
  do { fesetround (
# 334 "./bitint-31.c" 3 4
 0
# 334 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 334 "./bitint-31.c" 3 4
 0x400
# 334 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x1fffffffffffffp+139) __builtin_abort (); fesetround (
# 334 "./bitint-31.c" 3 4
 0x800
# 334 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x20000000000000p+139) __builtin_abort (); fesetround (
# 334 "./bitint-31.c" 3 4
 0xc00
# 334 "./bitint-31.c"
 ); if (testdblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x1fffffffffffffp+139) __builtin_abort (); } while (0);


  do { fesetround (
# 337 "./bitint-31.c" 3 4
 0
# 337 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895wb) != -0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 337 "./bitint-31.c" 3 4
 0x400
# 337 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 337 "./bitint-31.c" 3 4
 0x800
# 337 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895wb) != -0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 337 "./bitint-31.c" 3 4
 0xc00
# 337 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895wb) != -0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 338 "./bitint-31.c" 3 4
 0
# 338 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896wb) != -0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 338 "./bitint-31.c" 3 4
 0x400
# 338 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 338 "./bitint-31.c" 3 4
 0x800
# 338 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896wb) != -0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 338 "./bitint-31.c" 3 4
 0xc00
# 338 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896wb) != -0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 339 "./bitint-31.c" 3 4
 0
# 339 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 339 "./bitint-31.c" 3 4
 0x400
# 339 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 339 "./bitint-31.c" 3 4
 0x800
# 339 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897wb) != -0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 339 "./bitint-31.c" 3 4
 0xc00
# 339 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897wb) != -0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 340 "./bitint-31.c" 3 4
 0
# 340 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 340 "./bitint-31.c" 3 4
 0x400
# 340 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 340 "./bitint-31.c" 3 4
 0x800
# 340 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 340 "./bitint-31.c" 3 4
 0xc00
# 340 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112wb) != -0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 341 "./bitint-31.c" 3 4
 0
# 341 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 341 "./bitint-31.c" 3 4
 0x400
# 341 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327wb) != -0x20000000000000p+325) __builtin_abort (); fesetround (
# 341 "./bitint-31.c" 3 4
 0x800
# 341 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 341 "./bitint-31.c" 3 4
 0xc00
# 341 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327wb) != -0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 342 "./bitint-31.c" 3 4
 0
# 342 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328wb) != -0x20000000000000p+325) __builtin_abort (); fesetround (
# 342 "./bitint-31.c" 3 4
 0x400
# 342 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328wb) != -0x20000000000000p+325) __builtin_abort (); fesetround (
# 342 "./bitint-31.c" 3 4
 0x800
# 342 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 342 "./bitint-31.c" 3 4
 0xc00
# 342 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328wb) != -0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 343 "./bitint-31.c" 3 4
 0
# 343 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329wb) != -0x20000000000000p+325) __builtin_abort (); fesetround (
# 343 "./bitint-31.c" 3 4
 0x400
# 343 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329wb) != -0x20000000000000p+325) __builtin_abort (); fesetround (
# 343 "./bitint-31.c" 3 4
 0x800
# 343 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329wb) != -0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 343 "./bitint-31.c" 3 4
 0xc00
# 343 "./bitint-31.c"
 ); if (testdbl_575 (-615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329wb) != -0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 344 "./bitint-31.c" 3 4
 0
# 344 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276126650327970124302082526882038193909742709080463879918896882169507607035916867654709124839777195049479857541529867095829765369898539058829479405123401922117632wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 344 "./bitint-31.c" 3 4
 0x400
# 344 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276126650327970124302082526882038193909742709080463879918896882169507607035916867654709124839777195049479857541529867095829765369898539058829479405123401922117632wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 344 "./bitint-31.c" 3 4
 0x800
# 344 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276126650327970124302082526882038193909742709080463879918896882169507607035916867654709124839777195049479857541529867095829765369898539058829479405123401922117632wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 344 "./bitint-31.c" 3 4
 0xc00
# 344 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276126650327970124302082526882038193909742709080463879918896882169507607035916867654709124839777195049479857541529867095829765369898539058829479405123401922117632wb) != 0x1fffffffffffffp+521) __builtin_abort (); } while (0);
  do { fesetround (
# 345 "./bitint-31.c" 3 4
 0
# 345 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646207wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 345 "./bitint-31.c" 3 4
 0x400
# 345 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646207wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 345 "./bitint-31.c" 3 4
 0x800
# 345 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646207wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 345 "./bitint-31.c" 3 4
 0xc00
# 345 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646207wb) != 0x1fffffffffffffp+521) __builtin_abort (); } while (0);
  do { fesetround (
# 346 "./bitint-31.c" 3 4
 0
# 346 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646208wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 346 "./bitint-31.c" 3 4
 0x400
# 346 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646208wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 346 "./bitint-31.c" 3 4
 0x800
# 346 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646208wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 346 "./bitint-31.c" 3 4
 0xc00
# 346 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646208wb) != 0x1fffffffffffffp+521) __builtin_abort (); } while (0);
  do { fesetround (
# 347 "./bitint-31.c" 3 4
 0
# 347 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646209wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 347 "./bitint-31.c" 3 4
 0x400
# 347 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646209wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 347 "./bitint-31.c" 3 4
 0x800
# 347 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646209wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 347 "./bitint-31.c" 3 4
 0xc00
# 347 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276130082726800189606940017832437734606351343798113951571601579401237199807508566482735186119597525776757346189685562836258783930892538917151385692137547479646209wb) != 0x1fffffffffffffp+521) __builtin_abort (); } while (0);
  do { fesetround (
# 348 "./bitint-31.c" 3 4
 0
# 348 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 348 "./bitint-31.c" 3 4
 0x400
# 348 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb) != 0x1fffffffffffffp+521) __builtin_abort (); fesetround (
# 348 "./bitint-31.c" 3 4
 0x800
# 348 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb) != 0x20000000000000p+521) __builtin_abort (); fesetround (
# 348 "./bitint-31.c" 3 4
 0xc00
# 348 "./bitint-31.c"
 ); if (testdbl_575 (61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb) != 0x1fffffffffffffp+521) __builtin_abort (); } while (0);
  do { fesetround (
# 349 "./bitint-31.c" 3 4
 0
# 349 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895uwb) != 0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 349 "./bitint-31.c" 3 4
 0x400
# 349 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895uwb) != 0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 349 "./bitint-31.c" 3 4
 0x800
# 349 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 349 "./bitint-31.c" 3 4
 0xc00
# 349 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392895uwb) != 0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 350 "./bitint-31.c" 3 4
 0
# 350 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896uwb) != 0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 350 "./bitint-31.c" 3 4
 0x400
# 350 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896uwb) != 0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 350 "./bitint-31.c" 3 4
 0x800
# 350 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 350 "./bitint-31.c" 3 4
 0xc00
# 350 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392896uwb) != 0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 351 "./bitint-31.c" 3 4
 0
# 351 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 351 "./bitint-31.c" 3 4
 0x400
# 351 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897uwb) != 0x1ffffffffffffep+325) __builtin_abort (); fesetround (
# 351 "./bitint-31.c" 3 4
 0x800
# 351 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 351 "./bitint-31.c" 3 4
 0xc00
# 351 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663635164482277361060010743329029962521103256875011322006445221646740336801072761830405785423389392897uwb) != 0x1ffffffffffffep+325) __builtin_abort (); } while (0);
  do { fesetround (
# 352 "./bitint-31.c" 3 4
 0
# 352 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 352 "./bitint-31.c" 3 4
 0x400
# 352 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 352 "./bitint-31.c" 3 4
 0x800
# 352 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 352 "./bitint-31.c" 3 4
 0xc00
# 352 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663669340274852095621329063676328675354936900147369028450764374312465492316685252079206152816780378112uwb) != 0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 353 "./bitint-31.c" 3 4
 0
# 353 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 353 "./bitint-31.c" 3 4
 0x400
# 353 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 353 "./bitint-31.c" 3 4
 0x800
# 353 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327uwb) != 0x20000000000000p+325) __builtin_abort (); fesetround (
# 353 "./bitint-31.c" 3 4
 0xc00
# 353 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363327uwb) != 0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 354 "./bitint-31.c" 3 4
 0
# 354 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328uwb) != 0x20000000000000p+325) __builtin_abort (); fesetround (
# 354 "./bitint-31.c" 3 4
 0x400
# 354 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 354 "./bitint-31.c" 3 4
 0x800
# 354 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328uwb) != 0x20000000000000p+325) __builtin_abort (); fesetround (
# 354 "./bitint-31.c" 3 4
 0xc00
# 354 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363328uwb) != 0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 355 "./bitint-31.c" 3 4
 0
# 355 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329uwb) != 0x20000000000000p+325) __builtin_abort (); fesetround (
# 355 "./bitint-31.c" 3 4
 0x400
# 355 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329uwb) != 0x1fffffffffffffp+325) __builtin_abort (); fesetround (
# 355 "./bitint-31.c" 3 4
 0x800
# 355 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329uwb) != 0x20000000000000p+325) __builtin_abort (); fesetround (
# 355 "./bitint-31.c" 3 4
 0xc00
# 355 "./bitint-31.c"
 ); if (testdblu_575 (615656346818663703516067426830182647384023627388188770543419726734895083526978190647832297742328006520210171363329uwb) != 0x1fffffffffffffp+325) __builtin_abort (); } while (0);
  do { fesetround (
# 356 "./bitint-31.c" 3 4
 0
# 356 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552253300655940248604165053764076387819485418160927759837793764339015214071833735309418249679554390098959715083059734191659530739797078117658958810246803844235264uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 356 "./bitint-31.c" 3 4
 0x400
# 356 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552253300655940248604165053764076387819485418160927759837793764339015214071833735309418249679554390098959715083059734191659530739797078117658958810246803844235264uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 356 "./bitint-31.c" 3 4
 0x800
# 356 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552253300655940248604165053764076387819485418160927759837793764339015214071833735309418249679554390098959715083059734191659530739797078117658958810246803844235264uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 356 "./bitint-31.c" 3 4
 0xc00
# 356 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552253300655940248604165053764076387819485418160927759837793764339015214071833735309418249679554390098959715083059734191659530739797078117658958810246803844235264uwb) != 0x1fffffffffffffp+522) __builtin_abort (); } while (0);
  do { fesetround (
# 357 "./bitint-31.c" 3 4
 0
# 357 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292415uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 357 "./bitint-31.c" 3 4
 0x400
# 357 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292415uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 357 "./bitint-31.c" 3 4
 0x800
# 357 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292415uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 357 "./bitint-31.c" 3 4
 0xc00
# 357 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292415uwb) != 0x1fffffffffffffp+522) __builtin_abort (); } while (0);
  do { fesetround (
# 358 "./bitint-31.c" 3 4
 0
# 358 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292416uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 358 "./bitint-31.c" 3 4
 0x400
# 358 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292416uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 358 "./bitint-31.c" 3 4
 0x800
# 358 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292416uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 358 "./bitint-31.c" 3 4
 0xc00
# 358 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292416uwb) != 0x1fffffffffffffp+522) __builtin_abort (); } while (0);
  do { fesetround (
# 359 "./bitint-31.c" 3 4
 0
# 359 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292417uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 359 "./bitint-31.c" 3 4
 0x400
# 359 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292417uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 359 "./bitint-31.c" 3 4
 0x800
# 359 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292417uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 359 "./bitint-31.c" 3 4
 0xc00
# 359 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552260165453600379213880035664875469212702687596227903143203158802474399615017132965470372239195051553514692379371125672517567861785077834302771384275094959292417uwb) != 0x1fffffffffffffp+522) __builtin_abort (); } while (0);
  do { fesetround (
# 360 "./bitint-31.c" 3 4
 0
# 360 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 360 "./bitint-31.c" 3 4
 0x400
# 360 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x1fffffffffffffp+522) __builtin_abort (); fesetround (
# 360 "./bitint-31.c" 3 4
 0x800
# 360 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x20000000000000p+522) __builtin_abort (); fesetround (
# 360 "./bitint-31.c" 3 4
 0xc00
# 360 "./bitint-31.c"
 ); if (testdblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x1fffffffffffffp+522) __builtin_abort (); } while (0);




  do { fesetround (
# 365 "./bitint-31.c" 3 4
 0
# 365 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994111wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 365 "./bitint-31.c" 3 4
 0x400
# 365 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994111wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 365 "./bitint-31.c" 3 4
 0x800
# 365 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994111wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 365 "./bitint-31.c" 3 4
 0xc00
# 365 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994111wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 366 "./bitint-31.c" 3 4
 0
# 366 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994112wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 366 "./bitint-31.c" 3 4
 0x400
# 366 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994112wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 366 "./bitint-31.c" 3 4
 0x800
# 366 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994112wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 366 "./bitint-31.c" 3 4
 0xc00
# 366 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994112wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 367 "./bitint-31.c" 3 4
 0
# 367 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994113wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 367 "./bitint-31.c" 3 4
 0x400
# 367 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994113wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 367 "./bitint-31.c" 3 4
 0x800
# 367 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994113wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 367 "./bitint-31.c" 3 4
 0xc00
# 367 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071616947187835994113wb) != -0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 368 "./bitint-31.c" 3 4
 0
# 368 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071618073087742836736wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 368 "./bitint-31.c" 3 4
 0x400
# 368 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071618073087742836736wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 368 "./bitint-31.c" 3 4
 0x800
# 368 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071618073087742836736wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 368 "./bitint-31.c" 3 4
 0xc00
# 368 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071618073087742836736wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 369 "./bitint-31.c" 3 4
 0
# 369 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679359wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 369 "./bitint-31.c" 3 4
 0x400
# 369 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679359wb) != -0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 369 "./bitint-31.c" 3 4
 0x800
# 369 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679359wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 369 "./bitint-31.c" 3 4
 0xc00
# 369 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679359wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 370 "./bitint-31.c" 3 4
 0
# 370 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679360wb) != -0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 370 "./bitint-31.c" 3 4
 0x400
# 370 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679360wb) != -0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 370 "./bitint-31.c" 3 4
 0x800
# 370 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679360wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 370 "./bitint-31.c" 3 4
 0xc00
# 370 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679360wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 371 "./bitint-31.c" 3 4
 0
# 371 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679361wb) != -0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 371 "./bitint-31.c" 3 4
 0x400
# 371 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679361wb) != -0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 371 "./bitint-31.c" 3 4
 0x800
# 371 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679361wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 371 "./bitint-31.c" 3 4
 0xc00
# 371 "./bitint-31.c"
 ); if (testldbl_135 (-27577662721237071619198987649679361wb) != -0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 372 "./bitint-31.c" 3 4
 0
# 372 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061660475383254915754229760wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 372 "./bitint-31.c" 3 4
 0x400
# 372 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061660475383254915754229760wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 372 "./bitint-31.c" 3 4
 0x800
# 372 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061660475383254915754229760wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 372 "./bitint-31.c" 3 4
 0xc00
# 372 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061660475383254915754229760wb) != -0xffffffffffffffffp+70L) __builtin_abort (); } while (0);
  do { fesetround (
# 373 "./bitint-31.c" 3 4
 0
# 373 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881471wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 373 "./bitint-31.c" 3 4
 0x400
# 373 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881471wb) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 373 "./bitint-31.c" 3 4
 0x800
# 373 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881471wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 373 "./bitint-31.c" 3 4
 0xc00
# 373 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881471wb) != -0xffffffffffffffffp+70L) __builtin_abort (); } while (0);
  do { fesetround (
# 374 "./bitint-31.c" 3 4
 0
# 374 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881472wb) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 374 "./bitint-31.c" 3 4
 0x400
# 374 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881472wb) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 374 "./bitint-31.c" 3 4
 0x800
# 374 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881472wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 374 "./bitint-31.c" 3 4
 0xc00
# 374 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881472wb) != -0xffffffffffffffffp+70L) __builtin_abort (); } while (0);
  do { fesetround (
# 375 "./bitint-31.c" 3 4
 0
# 375 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881473wb) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 375 "./bitint-31.c" 3 4
 0x400
# 375 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881473wb) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 375 "./bitint-31.c" 3 4
 0x800
# 375 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881473wb) != -0xffffffffffffffffp+70L) __builtin_abort (); fesetround (
# 375 "./bitint-31.c" 3 4
 0xc00
# 375 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661065679065274459881473wb) != -0xffffffffffffffffp+70L) __builtin_abort (); } while (0);
  do { fesetround (
# 376 "./bitint-31.c" 3 4
 0
# 376 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661655974875633165533183wb - 1) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 376 "./bitint-31.c" 3 4
 0x400
# 376 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661655974875633165533183wb - 1) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 376 "./bitint-31.c" 3 4
 0x800
# 376 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661655974875633165533183wb - 1) != -0x10000000000000000p+70L) __builtin_abort (); fesetround (
# 376 "./bitint-31.c" 3 4
 0xc00
# 376 "./bitint-31.c"
 ); if (testldbl_135 (-21778071482940061661655974875633165533183wb - 1) != -0x10000000000000000p+70L) __builtin_abort (); } while (0);
  do { fesetround (
# 377 "./bitint-31.c" 3 4
 0
# 377 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994111uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 377 "./bitint-31.c" 3 4
 0x400
# 377 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994111uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 377 "./bitint-31.c" 3 4
 0x800
# 377 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994111uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 377 "./bitint-31.c" 3 4
 0xc00
# 377 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994111uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 378 "./bitint-31.c" 3 4
 0
# 378 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994112uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 378 "./bitint-31.c" 3 4
 0x400
# 378 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994112uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 378 "./bitint-31.c" 3 4
 0x800
# 378 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994112uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 378 "./bitint-31.c" 3 4
 0xc00
# 378 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994112uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 379 "./bitint-31.c" 3 4
 0
# 379 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994113uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 379 "./bitint-31.c" 3 4
 0x400
# 379 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994113uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); fesetround (
# 379 "./bitint-31.c" 3 4
 0x800
# 379 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994113uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 379 "./bitint-31.c" 3 4
 0xc00
# 379 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071616947187835994113uwb) != 0xa9f5e144d113e1c4p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 380 "./bitint-31.c" 3 4
 0
# 380 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071618073087742836736uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 380 "./bitint-31.c" 3 4
 0x400
# 380 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071618073087742836736uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 380 "./bitint-31.c" 3 4
 0x800
# 380 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071618073087742836736uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 380 "./bitint-31.c" 3 4
 0xc00
# 380 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071618073087742836736uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 381 "./bitint-31.c" 3 4
 0
# 381 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679359uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 381 "./bitint-31.c" 3 4
 0x400
# 381 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679359uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 381 "./bitint-31.c" 3 4
 0x800
# 381 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679359uwb) != 0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 381 "./bitint-31.c" 3 4
 0xc00
# 381 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679359uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 382 "./bitint-31.c" 3 4
 0
# 382 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679360uwb) != 0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 382 "./bitint-31.c" 3 4
 0x400
# 382 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679360uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 382 "./bitint-31.c" 3 4
 0x800
# 382 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679360uwb) != 0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 382 "./bitint-31.c" 3 4
 0xc00
# 382 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679360uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 383 "./bitint-31.c" 3 4
 0
# 383 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679361uwb) != 0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 383 "./bitint-31.c" 3 4
 0x400
# 383 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679361uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); fesetround (
# 383 "./bitint-31.c" 3 4
 0x800
# 383 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679361uwb) != 0xa9f5e144d113e1c6p+51L) __builtin_abort (); fesetround (
# 383 "./bitint-31.c" 3 4
 0xc00
# 383 "./bitint-31.c"
 ); if (testldblu_135 (27577662721237071619198987649679361uwb) != 0xa9f5e144d113e1c5p+51L) __builtin_abort (); } while (0);
  do { fesetround (
# 384 "./bitint-31.c" 3 4
 0
# 384 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123320950766509831508459520uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 384 "./bitint-31.c" 3 4
 0x400
# 384 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123320950766509831508459520uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 384 "./bitint-31.c" 3 4
 0x800
# 384 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123320950766509831508459520uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 384 "./bitint-31.c" 3 4
 0xc00
# 384 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123320950766509831508459520uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); } while (0);
  do { fesetround (
# 385 "./bitint-31.c" 3 4
 0
# 385 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762943uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 385 "./bitint-31.c" 3 4
 0x400
# 385 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762943uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 385 "./bitint-31.c" 3 4
 0x800
# 385 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762943uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 385 "./bitint-31.c" 3 4
 0xc00
# 385 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762943uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); } while (0);
  do { fesetround (
# 386 "./bitint-31.c" 3 4
 0
# 386 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762944uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 386 "./bitint-31.c" 3 4
 0x400
# 386 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762944uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 386 "./bitint-31.c" 3 4
 0x800
# 386 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762944uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 386 "./bitint-31.c" 3 4
 0xc00
# 386 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762944uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); } while (0);
  do { fesetround (
# 387 "./bitint-31.c" 3 4
 0
# 387 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762945uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 387 "./bitint-31.c" 3 4
 0x400
# 387 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762945uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 387 "./bitint-31.c" 3 4
 0x800
# 387 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762945uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 387 "./bitint-31.c" 3 4
 0xc00
# 387 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123322131358130548919762945uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); } while (0);
  do { fesetround (
# 388 "./bitint-31.c" 3 4
 0
# 388 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123323311949751266331066367uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 388 "./bitint-31.c" 3 4
 0x400
# 388 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123323311949751266331066367uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); fesetround (
# 388 "./bitint-31.c" 3 4
 0x800
# 388 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123323311949751266331066367uwb) != 0x10000000000000000p+71L) __builtin_abort (); fesetround (
# 388 "./bitint-31.c" 3 4
 0xc00
# 388 "./bitint-31.c"
 ); if (testldblu_135 (43556142965880123323311949751266331066367uwb) != 0xffffffffffffffffp+71L) __builtin_abort (); } while (0);


  do { fesetround (
# 391 "./bitint-31.c" 3 4
 0
# 391 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596351wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 391 "./bitint-31.c" 3 4
 0x400
# 391 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596351wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 391 "./bitint-31.c" 3 4
 0x800
# 391 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596351wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 391 "./bitint-31.c" 3 4
 0xc00
# 391 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596351wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 392 "./bitint-31.c" 3 4
 0
# 392 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596352wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 392 "./bitint-31.c" 3 4
 0x400
# 392 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596352wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 392 "./bitint-31.c" 3 4
 0x800
# 392 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596352wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 392 "./bitint-31.c" 3 4
 0xc00
# 392 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596352wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 393 "./bitint-31.c" 3 4
 0
# 393 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596353wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 393 "./bitint-31.c" 3 4
 0x400
# 393 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596353wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 393 "./bitint-31.c" 3 4
 0x800
# 393 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596353wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 393 "./bitint-31.c" 3 4
 0xc00
# 393 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743653878219701497927252918090596353wb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 394 "./bitint-31.c" 3 4
 0
# 394 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743658948822102410844858904903417856wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 394 "./bitint-31.c" 3 4
 0x400
# 394 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743658948822102410844858904903417856wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 394 "./bitint-31.c" 3 4
 0x800
# 394 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743658948822102410844858904903417856wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 394 "./bitint-31.c" 3 4
 0xc00
# 394 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743658948822102410844858904903417856wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 395 "./bitint-31.c" 3 4
 0
# 395 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239359wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 395 "./bitint-31.c" 3 4
 0x400
# 395 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239359wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 395 "./bitint-31.c" 3 4
 0x800
# 395 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239359wb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 395 "./bitint-31.c" 3 4
 0xc00
# 395 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239359wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 396 "./bitint-31.c" 3 4
 0
# 396 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239360wb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 396 "./bitint-31.c" 3 4
 0x400
# 396 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239360wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 396 "./bitint-31.c" 3 4
 0x800
# 396 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239360wb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 396 "./bitint-31.c" 3 4
 0xc00
# 396 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239360wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 397 "./bitint-31.c" 3 4
 0
# 397 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239361wb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 397 "./bitint-31.c" 3 4
 0x400
# 397 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239361wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 397 "./bitint-31.c" 3 4
 0x800
# 397 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239361wb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 397 "./bitint-31.c" 3 4
 0xc00
# 397 "./bitint-31.c"
 ); if (testldbl_192 (96388802158769743664019424503323762464891716239361wb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 398 "./bitint-31.c" 3 4
 0
# 398 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381747753528143363976319490418516133150720wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 398 "./bitint-31.c" 3 4
 0x400
# 398 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381747753528143363976319490418516133150720wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 398 "./bitint-31.c" 3 4
 0x800
# 398 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381747753528143363976319490418516133150720wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 398 "./bitint-31.c" 3 4
 0xc00
# 398 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381747753528143363976319490418516133150720wb) != -0xffffffffffffffffp+127L) __builtin_abort (); } while (0);
  do { fesetround (
# 399 "./bitint-31.c" 3 4
 0
# 399 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203583wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 399 "./bitint-31.c" 3 4
 0x400
# 399 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203583wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 399 "./bitint-31.c" 3 4
 0x800
# 399 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203583wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 399 "./bitint-31.c" 3 4
 0xc00
# 399 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203583wb) != -0xffffffffffffffffp+127L) __builtin_abort (); } while (0);
  do { fesetround (
# 400 "./bitint-31.c" 3 4
 0
# 400 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203584wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 400 "./bitint-31.c" 3 4
 0x400
# 400 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203584wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 400 "./bitint-31.c" 3 4
 0x800
# 400 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203584wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 400 "./bitint-31.c" 3 4
 0xc00
# 400 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203584wb) != -0xffffffffffffffffp+127L) __builtin_abort (); } while (0);
  do { fesetround (
# 401 "./bitint-31.c" 3 4
 0
# 401 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203585wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 401 "./bitint-31.c" 3 4
 0x400
# 401 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203585wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 401 "./bitint-31.c" 3 4
 0x800
# 401 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203585wb) != -0xffffffffffffffffp+127L) __builtin_abort (); fesetround (
# 401 "./bitint-31.c" 3 4
 0xc00
# 401 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381832824119873598592185334070374075203585wb) != -0xffffffffffffffffp+127L) __builtin_abort (); } while (0);
  do { fesetround (
# 402 "./bitint-31.c" 3 4
 0
# 402 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 402 "./bitint-31.c" 3 4
 0x400
# 402 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 402 "./bitint-31.c" 3 4
 0x800
# 402 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1wb) != -0x10000000000000000p+127L) __builtin_abort (); fesetround (
# 402 "./bitint-31.c" 3 4
 0xc00
# 402 "./bitint-31.c"
 ); if (testldbl_192 (-3138550867693340381917894711603833208051177722232017256447wb - 1wb) != -0x10000000000000000p+127L) __builtin_abort (); } while (0);
  do { fesetround (
# 403 "./bitint-31.c" 3 4
 0
# 403 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596351uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 403 "./bitint-31.c" 3 4
 0x400
# 403 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596351uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 403 "./bitint-31.c" 3 4
 0x800
# 403 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596351uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 403 "./bitint-31.c" 3 4
 0xc00
# 403 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596351uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 404 "./bitint-31.c" 3 4
 0
# 404 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596352uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 404 "./bitint-31.c" 3 4
 0x400
# 404 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596352uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 404 "./bitint-31.c" 3 4
 0x800
# 404 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596352uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 404 "./bitint-31.c" 3 4
 0xc00
# 404 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596352uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 405 "./bitint-31.c" 3 4
 0
# 405 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596353uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 405 "./bitint-31.c" 3 4
 0x400
# 405 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596353uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); fesetround (
# 405 "./bitint-31.c" 3 4
 0x800
# 405 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596353uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 405 "./bitint-31.c" 3 4
 0xc00
# 405 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743653878219701497927252918090596353uwb) != 0x83e75ebf94ce024ep+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 406 "./bitint-31.c" 3 4
 0
# 406 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743658948822102410844858904903417856uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 406 "./bitint-31.c" 3 4
 0x400
# 406 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743658948822102410844858904903417856uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 406 "./bitint-31.c" 3 4
 0x800
# 406 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743658948822102410844858904903417856uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 406 "./bitint-31.c" 3 4
 0xc00
# 406 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743658948822102410844858904903417856uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 407 "./bitint-31.c" 3 4
 0
# 407 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239359uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 407 "./bitint-31.c" 3 4
 0x400
# 407 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239359uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 407 "./bitint-31.c" 3 4
 0x800
# 407 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239359uwb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 407 "./bitint-31.c" 3 4
 0xc00
# 407 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239359uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 408 "./bitint-31.c" 3 4
 0
# 408 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239360uwb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 408 "./bitint-31.c" 3 4
 0x400
# 408 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239360uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 408 "./bitint-31.c" 3 4
 0x800
# 408 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239360uwb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 408 "./bitint-31.c" 3 4
 0xc00
# 408 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239360uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 409 "./bitint-31.c" 3 4
 0
# 409 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239361uwb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 409 "./bitint-31.c" 3 4
 0x400
# 409 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239361uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); fesetround (
# 409 "./bitint-31.c" 3 4
 0x800
# 409 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239361uwb) != 0x83e75ebf94ce0250p+103L) __builtin_abort (); fesetround (
# 409 "./bitint-31.c" 3 4
 0xc00
# 409 "./bitint-31.c"
 ); if (testldblu_192 (96388802158769743664019424503323762464891716239361uwb) != 0x83e75ebf94ce024fp+103L) __builtin_abort (); } while (0);
  do { fesetround (
# 410 "./bitint-31.c" 3 4
 0
# 410 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763495507056286727952638980837032266301440uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 410 "./bitint-31.c" 3 4
 0x400
# 410 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763495507056286727952638980837032266301440uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 410 "./bitint-31.c" 3 4
 0x800
# 410 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763495507056286727952638980837032266301440uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 410 "./bitint-31.c" 3 4
 0xc00
# 410 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763495507056286727952638980837032266301440uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); } while (0);
  do { fesetround (
# 411 "./bitint-31.c" 3 4
 0
# 411 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407167uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 411 "./bitint-31.c" 3 4
 0x400
# 411 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407167uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 411 "./bitint-31.c" 3 4
 0x800
# 411 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407167uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 411 "./bitint-31.c" 3 4
 0xc00
# 411 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407167uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); } while (0);
  do { fesetround (
# 412 "./bitint-31.c" 3 4
 0
# 412 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407168uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 412 "./bitint-31.c" 3 4
 0x400
# 412 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407168uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 412 "./bitint-31.c" 3 4
 0x800
# 412 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407168uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 412 "./bitint-31.c" 3 4
 0xc00
# 412 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407168uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); } while (0);
  do { fesetround (
# 413 "./bitint-31.c" 3 4
 0
# 413 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407169uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 413 "./bitint-31.c" 3 4
 0x400
# 413 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407169uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 413 "./bitint-31.c" 3 4
 0x800
# 413 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407169uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 413 "./bitint-31.c" 3 4
 0xc00
# 413 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763665648239747197184370668140748150407169uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); } while (0);
  do { fesetround (
# 414 "./bitint-31.c" 3 4
 0
# 414 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 414 "./bitint-31.c" 3 4
 0x400
# 414 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); fesetround (
# 414 "./bitint-31.c" 3 4
 0x800
# 414 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x10000000000000000p+128L) __builtin_abort (); fesetround (
# 414 "./bitint-31.c" 3 4
 0xc00
# 414 "./bitint-31.c"
 ); if (testldblu_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0xffffffffffffffffp+128L) __builtin_abort (); } while (0);


  do { fesetround (
# 417 "./bitint-31.c" 3 4
 0
# 417 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133511773678272426148233889331025751498446645922568076207932202076431648659257792374503198949281962308977915333294030066289778448068072486649492543280785653760wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 417 "./bitint-31.c" 3 4
 0x400
# 417 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133511773678272426148233889331025751498446645922568076207932202076431648659257792374503198949281962308977915333294030066289778448068072486649492543280785653760wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 417 "./bitint-31.c" 3 4
 0x800
# 417 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133511773678272426148233889331025751498446645922568076207932202076431648659257792374503198949281962308977915333294030066289778448068072486649492543280785653760wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 417 "./bitint-31.c" 3 4
 0xc00
# 417 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133511773678272426148233889331025751498446645922568076207932202076431648659257792374503198949281962308977915333294030066289778448068072486649492543280785653760wb) != -0xffffffffffffffffp+510L) __builtin_abort (); } while (0);
  do { fesetround (
# 418 "./bitint-31.c" 3 4
 0
# 418 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414271wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 418 "./bitint-31.c" 3 4
 0x400
# 418 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414271wb) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 418 "./bitint-31.c" 3 4
 0x800
# 418 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414271wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 418 "./bitint-31.c" 3 4
 0xc00
# 418 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414271wb) != -0xffffffffffffffffp+510L) __builtin_abort (); } while (0);
  do { fesetround (
# 419 "./bitint-31.c" 3 4
 0
# 419 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414272wb) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 419 "./bitint-31.c" 3 4
 0x400
# 419 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414272wb) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 419 "./bitint-31.c" 3 4
 0x800
# 419 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414272wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 419 "./bitint-31.c" 3 4
 0xc00
# 419 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414272wb) != -0xffffffffffffffffp+510L) __builtin_abort (); } while (0);
  do { fesetround (
# 420 "./bitint-31.c" 3 4
 0
# 420 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414273wb) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 420 "./bitint-31.c" 3 4
 0x400
# 420 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414273wb) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 420 "./bitint-31.c" 3 4
 0x800
# 420 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414273wb) != -0xffffffffffffffffp+510L) __builtin_abort (); fesetround (
# 420 "./bitint-31.c" 3 4
 0xc00
# 420 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133513449654263668972871336084150527229212580843295650257104417521612113879761551567875299183569233171906376587276303377046135167303423979970735847486911414273wb) != -0xffffffffffffffffp+510L) __builtin_abort (); } while (0);
  do { fesetround (
# 421 "./bitint-31.c" 3 4
 0
# 421 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 421 "./bitint-31.c" 3 4
 0x400
# 421 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 421 "./bitint-31.c" 3 4
 0x800
# 421 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x10000000000000000p+510L) __builtin_abort (); fesetround (
# 421 "./bitint-31.c" 3 4
 0xc00
# 421 "./bitint-31.c"
 ); if (testldbl_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x10000000000000000p+510L) __builtin_abort (); } while (0);
  do { fesetround (
# 422 "./bitint-31.c" 3 4
 0
# 422 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267023547356544852296467778662051502996893291845136152415864404152863297318515584749006397898563924617955830666588060132579556896136144973298985086561571307520uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 422 "./bitint-31.c" 3 4
 0x400
# 422 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267023547356544852296467778662051502996893291845136152415864404152863297318515584749006397898563924617955830666588060132579556896136144973298985086561571307520uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 422 "./bitint-31.c" 3 4
 0x800
# 422 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267023547356544852296467778662051502996893291845136152415864404152863297318515584749006397898563924617955830666588060132579556896136144973298985086561571307520uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 422 "./bitint-31.c" 3 4
 0xc00
# 422 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267023547356544852296467778662051502996893291845136152415864404152863297318515584749006397898563924617955830666588060132579556896136144973298985086561571307520uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); } while (0);
  do { fesetround (
# 423 "./bitint-31.c" 3 4
 0
# 423 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828543uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 423 "./bitint-31.c" 3 4
 0x400
# 423 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828543uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 423 "./bitint-31.c" 3 4
 0x800
# 423 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828543uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 423 "./bitint-31.c" 3 4
 0xc00
# 423 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828543uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); } while (0);
  do { fesetround (
# 424 "./bitint-31.c" 3 4
 0
# 424 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828544uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 424 "./bitint-31.c" 3 4
 0x400
# 424 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828544uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 424 "./bitint-31.c" 3 4
 0x800
# 424 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828544uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 424 "./bitint-31.c" 3 4
 0xc00
# 424 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828544uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); } while (0);
  do { fesetround (
# 425 "./bitint-31.c" 3 4
 0
# 425 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828545uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 425 "./bitint-31.c" 3 4
 0x400
# 425 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828545uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 425 "./bitint-31.c" 3 4
 0x800
# 425 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828545uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 425 "./bitint-31.c" 3 4
 0xc00
# 425 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267026899308527337945742672168301054458425161686591300514208835043224227759523103135750598367138466343812753174552606754092270334606847959941471694973822828545uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); } while (0);
  do { fesetround (
# 426 "./bitint-31.c" 3 4
 0
# 426 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 426 "./bitint-31.c" 3 4
 0x400
# 426 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); fesetround (
# 426 "./bitint-31.c" 3 4
 0x800
# 426 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x10000000000000000p+511L) __builtin_abort (); fesetround (
# 426 "./bitint-31.c" 3 4
 0xc00
# 426 "./bitint-31.c"
 ); if (testldblu_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0xffffffffffffffffp+511L) __builtin_abort (); } while (0);




  do { fesetround (
# 431 "./bitint-31.c" 3 4
 0
# 431 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917055wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 431 "./bitint-31.c" 3 4
 0x400
# 431 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917055wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 431 "./bitint-31.c" 3 4
 0x800
# 431 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917055wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 431 "./bitint-31.c" 3 4
 0xc00
# 431 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917055wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 432 "./bitint-31.c" 3 4
 0
# 432 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917056wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 432 "./bitint-31.c" 3 4
 0x400
# 432 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917056wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 432 "./bitint-31.c" 3 4
 0x800
# 432 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917056wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 432 "./bitint-31.c" 3 4
 0xc00
# 432 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917056wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 433 "./bitint-31.c" 3 4
 0
# 433 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917057wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 433 "./bitint-31.c" 3 4
 0x400
# 433 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917057wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 433 "./bitint-31.c" 3 4
 0x800
# 433 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917057wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 433 "./bitint-31.c" 3 4
 0xc00
# 433 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488783917057wb) != -0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 434 "./bitint-31.c" 3 4
 0
# 434 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488784965632wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 434 "./bitint-31.c" 3 4
 0x400
# 434 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488784965632wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 434 "./bitint-31.c" 3 4
 0x800
# 434 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488784965632wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 434 "./bitint-31.c" 3 4
 0xc00
# 434 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488784965632wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 435 "./bitint-31.c" 3 4
 0
# 435 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014207wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 435 "./bitint-31.c" 3 4
 0x400
# 435 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014207wb) != -0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 435 "./bitint-31.c" 3 4
 0x800
# 435 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014207wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 435 "./bitint-31.c" 3 4
 0xc00
# 435 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014207wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 436 "./bitint-31.c" 3 4
 0
# 436 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014208wb) != -0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 436 "./bitint-31.c" 3 4
 0x400
# 436 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014208wb) != -0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 436 "./bitint-31.c" 3 4
 0x800
# 436 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014208wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 436 "./bitint-31.c" 3 4
 0xc00
# 436 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014208wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 437 "./bitint-31.c" 3 4
 0
# 437 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014209wb) != -0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 437 "./bitint-31.c" 3 4
 0x400
# 437 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014209wb) != -0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 437 "./bitint-31.c" 3 4
 0x800
# 437 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014209wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 437 "./bitint-31.c" 3 4
 0xc00
# 437 "./bitint-31.c"
 ); if (testflt128_135 (-21646332438261169091754659013488786014209wb) != -0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 438 "./bitint-31.c" 3 4
 0
# 438 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917055uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 438 "./bitint-31.c" 3 4
 0x400
# 438 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917055uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 438 "./bitint-31.c" 3 4
 0x800
# 438 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917055uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 438 "./bitint-31.c" 3 4
 0xc00
# 438 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917055uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 439 "./bitint-31.c" 3 4
 0
# 439 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917056uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 439 "./bitint-31.c" 3 4
 0x400
# 439 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917056uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 439 "./bitint-31.c" 3 4
 0x800
# 439 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917056uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 439 "./bitint-31.c" 3 4
 0xc00
# 439 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917056uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 440 "./bitint-31.c" 3 4
 0
# 440 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917057uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 440 "./bitint-31.c" 3 4
 0x400
# 440 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917057uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); fesetround (
# 440 "./bitint-31.c" 3 4
 0x800
# 440 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917057uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 440 "./bitint-31.c" 3 4
 0xc00
# 440 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488783917057uwb) != 0x1fce71fdcfb1797b42dede66ac9ecp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 441 "./bitint-31.c" 3 4
 0
# 441 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488784965632uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 441 "./bitint-31.c" 3 4
 0x400
# 441 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488784965632uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 441 "./bitint-31.c" 3 4
 0x800
# 441 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488784965632uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 441 "./bitint-31.c" 3 4
 0xc00
# 441 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488784965632uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 442 "./bitint-31.c" 3 4
 0
# 442 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014207uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 442 "./bitint-31.c" 3 4
 0x400
# 442 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014207uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 442 "./bitint-31.c" 3 4
 0x800
# 442 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014207uwb) != 0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 442 "./bitint-31.c" 3 4
 0xc00
# 442 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014207uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 443 "./bitint-31.c" 3 4
 0
# 443 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014208uwb) != 0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 443 "./bitint-31.c" 3 4
 0x400
# 443 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014208uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 443 "./bitint-31.c" 3 4
 0x800
# 443 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014208uwb) != 0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 443 "./bitint-31.c" 3 4
 0xc00
# 443 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014208uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);
  do { fesetround (
# 444 "./bitint-31.c" 3 4
 0
# 444 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014209uwb) != 0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 444 "./bitint-31.c" 3 4
 0x400
# 444 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014209uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); fesetround (
# 444 "./bitint-31.c" 3 4
 0x800
# 444 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014209uwb) != 0x1fce71fdcfb1797b42dede66ac9eep+21F128) __builtin_abort (); fesetround (
# 444 "./bitint-31.c" 3 4
 0xc00
# 444 "./bitint-31.c"
 ); if (testflt128u_135 (21646332438261169091754659013488786014209uwb) != 0x1fce71fdcfb1797b42dede66ac9edp+21F128) __builtin_abort (); } while (0);


  do { fesetround (
# 447 "./bitint-31.c" 3 4
 0
# 447 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603832905819722818574723579904wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 447 "./bitint-31.c" 3 4
 0x400
# 447 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603832905819722818574723579904wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 447 "./bitint-31.c" 3 4
 0x800
# 447 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603832905819722818574723579904wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 447 "./bitint-31.c" 3 4
 0xc00
# 447 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603832905819722818574723579904wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); } while (0);
  do { fesetround (
# 448 "./bitint-31.c" 3 4
 0
# 448 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418175wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 448 "./bitint-31.c" 3 4
 0x400
# 448 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418175wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 448 "./bitint-31.c" 3 4
 0x800
# 448 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418175wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 448 "./bitint-31.c" 3 4
 0xc00
# 448 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418175wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); } while (0);
  do { fesetround (
# 449 "./bitint-31.c" 3 4
 0
# 449 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418176wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 449 "./bitint-31.c" 3 4
 0x400
# 449 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418176wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 449 "./bitint-31.c" 3 4
 0x800
# 449 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418176wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 449 "./bitint-31.c" 3 4
 0xc00
# 449 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418176wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); } while (0);
  do { fesetround (
# 450 "./bitint-31.c" 3 4
 0
# 450 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418177wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 450 "./bitint-31.c" 3 4
 0x400
# 450 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418177wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 450 "./bitint-31.c" 3 4
 0x800
# 450 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418177wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 450 "./bitint-31.c" 3 4
 0xc00
# 450 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833056935450270403370418177wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); } while (0);
  do { fesetround (
# 451 "./bitint-31.c" 3 4
 0
# 451 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 451 "./bitint-31.c" 3 4
 0x400
# 451 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); fesetround (
# 451 "./bitint-31.c" 3 4
 0x800
# 451 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0x20000000000000000000000000000p+78F128) __builtin_abort (); fesetround (
# 451 "./bitint-31.c" 3 4
 0xc00
# 451 "./bitint-31.c"
 ); if (testflt128_192 (3138550867693340381917894711603833208051177722232017256447wb) != 0x1ffffffffffffffffffffffffffffp+78F128) __builtin_abort (); } while (0);
  do { fesetround (
# 452 "./bitint-31.c" 3 4
 0
# 452 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207665811639445637149447159808uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 452 "./bitint-31.c" 3 4
 0x400
# 452 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207665811639445637149447159808uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 452 "./bitint-31.c" 3 4
 0x800
# 452 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207665811639445637149447159808uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 452 "./bitint-31.c" 3 4
 0xc00
# 452 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207665811639445637149447159808uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); } while (0);
  do { fesetround (
# 453 "./bitint-31.c" 3 4
 0
# 453 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836351uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 453 "./bitint-31.c" 3 4
 0x400
# 453 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836351uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 453 "./bitint-31.c" 3 4
 0x800
# 453 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836351uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 453 "./bitint-31.c" 3 4
 0xc00
# 453 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836351uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); } while (0);
  do { fesetround (
# 454 "./bitint-31.c" 3 4
 0
# 454 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836352uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 454 "./bitint-31.c" 3 4
 0x400
# 454 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836352uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 454 "./bitint-31.c" 3 4
 0x800
# 454 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836352uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 454 "./bitint-31.c" 3 4
 0xc00
# 454 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836352uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); } while (0);
  do { fesetround (
# 455 "./bitint-31.c" 3 4
 0
# 455 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836353uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 455 "./bitint-31.c" 3 4
 0x400
# 455 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836353uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 455 "./bitint-31.c" 3 4
 0x800
# 455 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836353uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 455 "./bitint-31.c" 3 4
 0xc00
# 455 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666113870900540806740836353uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); } while (0);
  do { fesetround (
# 456 "./bitint-31.c" 3 4
 0
# 456 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 456 "./bitint-31.c" 3 4
 0x400
# 456 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); fesetround (
# 456 "./bitint-31.c" 3 4
 0x800
# 456 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x20000000000000000000000000000p+79F128) __builtin_abort (); fesetround (
# 456 "./bitint-31.c" 3 4
 0xc00
# 456 "./bitint-31.c"
 ); if (testflt128u_192 (6277101735386680763835789423207666416102355444464034512895uwb) != 0x1ffffffffffffffffffffffffffffp+79F128) __builtin_abort (); } while (0);


  do { fesetround (
# 459 "./bitint-31.c" 3 4
 0
# 459 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787846289740055435765388813682045155135192382154626611682813571487190641804615256990296246545713518740501887218789991403746059512699763279527936wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 459 "./bitint-31.c" 3 4
 0x400
# 459 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787846289740055435765388813682045155135192382154626611682813571487190641804615256990296246545713518740501887218789991403746059512699763279527936wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 459 "./bitint-31.c" 3 4
 0x800
# 459 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787846289740055435765388813682045155135192382154626611682813571487190641804615256990296246545713518740501887218789991403746059512699763279527936wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 459 "./bitint-31.c" 3 4
 0xc00
# 459 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787846289740055435765388813682045155135192382154626611682813571487190641804615256990296246545713518740501887218789991403746059512699763279527936wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 460 "./bitint-31.c" 3 4
 0
# 460 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158911wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 460 "./bitint-31.c" 3 4
 0x400
# 460 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158911wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 460 "./bitint-31.c" 3 4
 0x800
# 460 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158911wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 460 "./bitint-31.c" 3 4
 0xc00
# 460 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158911wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 461 "./bitint-31.c" 3 4
 0
# 461 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158912wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 461 "./bitint-31.c" 3 4
 0x400
# 461 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158912wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 461 "./bitint-31.c" 3 4
 0x800
# 461 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158912wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 461 "./bitint-31.c" 3 4
 0xc00
# 461 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158912wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 462 "./bitint-31.c" 3 4
 0
# 462 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158913wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 462 "./bitint-31.c" 3 4
 0x400
# 462 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158913wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 462 "./bitint-31.c" 3 4
 0x800
# 462 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158913wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); fesetround (
# 462 "./bitint-31.c" 3 4
 0xc00
# 462 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787849266871470150571212503712362264401765094669639986937588484471046485703139369468190190624257242316066424102078490670010875270428034085158913wb) != -0x148b25ce53790ddc343a80e5af6bap+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 463 "./bitint-31.c" 3 4
 0
# 463 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787852244002884865377036193742679373668337807184653362192363397454902329601663481946084134702800965891630960985366989936275691028156304890789888wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 463 "./bitint-31.c" 3 4
 0x400
# 463 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787852244002884865377036193742679373668337807184653362192363397454902329601663481946084134702800965891630960985366989936275691028156304890789888wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 463 "./bitint-31.c" 3 4
 0x800
# 463 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787852244002884865377036193742679373668337807184653362192363397454902329601663481946084134702800965891630960985366989936275691028156304890789888wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 463 "./bitint-31.c" 3 4
 0xc00
# 463 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787852244002884865377036193742679373668337807184653362192363397454902329601663481946084134702800965891630960985366989936275691028156304890789888wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 464 "./bitint-31.c" 3 4
 0
# 464 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420863wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 464 "./bitint-31.c" 3 4
 0x400
# 464 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420863wb) != -0x148b25ce53790ddc343a80e5af6bcp+461F128) __builtin_abort (); fesetround (
# 464 "./bitint-31.c" 3 4
 0x800
# 464 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420863wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 464 "./bitint-31.c" 3 4
 0xc00
# 464 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420863wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 465 "./bitint-31.c" 3 4
 0
# 465 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420864wb) != -0x148b25ce53790ddc343a80e5af6bcp+461F128) __builtin_abort (); fesetround (
# 465 "./bitint-31.c" 3 4
 0x400
# 465 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420864wb) != -0x148b25ce53790ddc343a80e5af6bcp+461F128) __builtin_abort (); fesetround (
# 465 "./bitint-31.c" 3 4
 0x800
# 465 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420864wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 465 "./bitint-31.c" 3 4
 0xc00
# 465 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420864wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 466 "./bitint-31.c" 3 4
 0
# 466 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420865wb) != -0x148b25ce53790ddc343a80e5af6bcp+461F128) __builtin_abort (); fesetround (
# 466 "./bitint-31.c" 3 4
 0x400
# 466 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420865wb) != -0x148b25ce53790ddc343a80e5af6bcp+461F128) __builtin_abort (); fesetround (
# 466 "./bitint-31.c" 3 4
 0x800
# 466 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420865wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); fesetround (
# 466 "./bitint-31.c" 3 4
 0xc00
# 466 "./bitint-31.c"
 ); if (testflt128_575 (-39695651458311907436978914487787855221134299580182859883772996482934910519699666737447138310438758173500187594423978078781344689467195497868655489202540506785884575696420865wb) != -0x148b25ce53790ddc343a80e5af6bbp+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 467 "./bitint-31.c" 3 4
 0
# 467 "./bitint-31.c"
 ); if (testflt128_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x20000000000000000000000000000p+461F128) __builtin_abort (); fesetround (
# 467 "./bitint-31.c" 3 4
 0x400
# 467 "./bitint-31.c"
 ); if (testflt128_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x20000000000000000000000000000p+461F128) __builtin_abort (); fesetround (
# 467 "./bitint-31.c" 3 4
 0x800
# 467 "./bitint-31.c"
 ); if (testflt128_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x20000000000000000000000000000p+461F128) __builtin_abort (); fesetround (
# 467 "./bitint-31.c" 3 4
 0xc00
# 467 "./bitint-31.c"
 ); if (testflt128_575 (-61832600368276133515125630254911797508782837275302959978515764023224306276632966792579100265310761247399417856504034834837841258576687802491886538775473291979151693037174783wb - 1) != -0x20000000000000000000000000000p+461F128) __builtin_abort (); } while (0);
  do { fesetround (
# 468 "./bitint-31.c" 3 4
 0
# 468 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575692579480110871530777627364090310270384764309253223365627142974381283609230513980592493091427037481003774437579982807492119025399526559055872uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 468 "./bitint-31.c" 3 4
 0x400
# 468 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575692579480110871530777627364090310270384764309253223365627142974381283609230513980592493091427037481003774437579982807492119025399526559055872uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 468 "./bitint-31.c" 3 4
 0x800
# 468 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575692579480110871530777627364090310270384764309253223365627142974381283609230513980592493091427037481003774437579982807492119025399526559055872uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 468 "./bitint-31.c" 3 4
 0xc00
# 468 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575692579480110871530777627364090310270384764309253223365627142974381283609230513980592493091427037481003774437579982807492119025399526559055872uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 469 "./bitint-31.c" 3 4
 0
# 469 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317823uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 469 "./bitint-31.c" 3 4
 0x400
# 469 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317823uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 469 "./bitint-31.c" 3 4
 0x800
# 469 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317823uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 469 "./bitint-31.c" 3 4
 0xc00
# 469 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317823uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 470 "./bitint-31.c" 3 4
 0
# 470 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317824uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 470 "./bitint-31.c" 3 4
 0x400
# 470 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317824uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 470 "./bitint-31.c" 3 4
 0x800
# 470 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317824uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 470 "./bitint-31.c" 3 4
 0xc00
# 470 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317824uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 471 "./bitint-31.c" 3 4
 0
# 471 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317825uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 471 "./bitint-31.c" 3 4
 0x400
# 471 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317825uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); fesetround (
# 471 "./bitint-31.c" 3 4
 0x800
# 471 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317825uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 471 "./bitint-31.c" 3 4
 0xc00
# 471 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575698533742940301142425007424724528803530189339279973875176968942092971406278738936380381248514484632132848204156981340021750540856068170317825uwb) != 0x148b25ce53790ddc343a80e5af6bap+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 472 "./bitint-31.c" 3 4
 0
# 472 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575704488005769730754072387485358747336675614369306724384726794909804659203326963892168269405601931783261921970733979872551382056312609781579776uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 472 "./bitint-31.c" 3 4
 0x400
# 472 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575704488005769730754072387485358747336675614369306724384726794909804659203326963892168269405601931783261921970733979872551382056312609781579776uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 472 "./bitint-31.c" 3 4
 0x800
# 472 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575704488005769730754072387485358747336675614369306724384726794909804659203326963892168269405601931783261921970733979872551382056312609781579776uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 472 "./bitint-31.c" 3 4
 0xc00
# 472 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575704488005769730754072387485358747336675614369306724384726794909804659203326963892168269405601931783261921970733979872551382056312609781579776uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 473 "./bitint-31.c" 3 4
 0
# 473 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841727uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 473 "./bitint-31.c" 3 4
 0x400
# 473 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841727uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 473 "./bitint-31.c" 3 4
 0x800
# 473 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841727uwb) != 0x148b25ce53790ddc343a80e5af6bcp+462F128) __builtin_abort (); fesetround (
# 473 "./bitint-31.c" 3 4
 0xc00
# 473 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841727uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 474 "./bitint-31.c" 3 4
 0
# 474 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841728uwb) != 0x148b25ce53790ddc343a80e5af6bcp+462F128) __builtin_abort (); fesetround (
# 474 "./bitint-31.c" 3 4
 0x400
# 474 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841728uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 474 "./bitint-31.c" 3 4
 0x800
# 474 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841728uwb) != 0x148b25ce53790ddc343a80e5af6bcp+462F128) __builtin_abort (); fesetround (
# 474 "./bitint-31.c" 3 4
 0xc00
# 474 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841728uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 475 "./bitint-31.c" 3 4
 0
# 475 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841729uwb) != 0x148b25ce53790ddc343a80e5af6bcp+462F128) __builtin_abort (); fesetround (
# 475 "./bitint-31.c" 3 4
 0x400
# 475 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841729uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); fesetround (
# 475 "./bitint-31.c" 3 4
 0x800
# 475 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841729uwb) != 0x148b25ce53790ddc343a80e5af6bcp+462F128) __builtin_abort (); fesetround (
# 475 "./bitint-31.c" 3 4
 0xc00
# 475 "./bitint-31.c"
 ); if (testflt128u_575 (79391302916623814873957828975575710442268599160365719767545992965869821039399333474894276620877516347000375188847956157562689378934390995737310978405081013571769151392841729uwb) != 0x148b25ce53790ddc343a80e5af6bbp+462F128) __builtin_abort (); } while (0);
  do { fesetround (
# 476 "./bitint-31.c" 3 4
 0
# 476 "./bitint-31.c"
 ); if (testflt128u_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x20000000000000000000000000000p+462F128) __builtin_abort (); fesetround (
# 476 "./bitint-31.c" 3 4
 0x400
# 476 "./bitint-31.c"
 ); if (testflt128u_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x1ffffffffffffffffffffffffffffp+462F128) __builtin_abort (); fesetround (
# 476 "./bitint-31.c" 3 4
 0x800
# 476 "./bitint-31.c"
 ); if (testflt128u_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x20000000000000000000000000000p+462F128) __builtin_abort (); fesetround (
# 476 "./bitint-31.c" 3 4
 0xc00
# 476 "./bitint-31.c"
 ); if (testflt128u_575 (123665200736552267030251260509823595017565674550605919957031528046448612553265933585158200530621522494798835713008069669675682517153375604983773077550946583958303386074349567uwb) != 0x1ffffffffffffffffffffffffffffp+462F128) __builtin_abort (); } while (0);



}
