//type: rp
//options: 
# 0 "./torture/type-generic-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/type-generic-1.C"







# 1 "./torture/../../gcc.dg/tg-tests.h" 1
# 10 "./torture/../../gcc.dg/tg-tests.h"
void __attribute__ ((__noinline__))
foo_1 (float f, double d, long double ld,
       int res_unord, int res_isnan, int res_isinf,
       int res_isinf_sign, int res_isfin, int res_isnorm,
       int res_signbit, int classification)
{
  if (__builtin_isunordered (f, 0) != res_unord)
    __builtin_abort ();
  if (__builtin_isunordered (0, f) != res_unord)
    __builtin_abort ();
  if (__builtin_isunordered (d, 0) != res_unord)
    __builtin_abort ();
  if (__builtin_isunordered (0, d) != res_unord)
    __builtin_abort ();
  if (__builtin_isunordered (ld, 0) != res_unord)
    __builtin_abort ();
  if (__builtin_isunordered (0, ld) != res_unord)
    __builtin_abort ();

  if (__builtin_isnan (f) != res_isnan)
    __builtin_abort ();
  if (__builtin_isnan (d) != res_isnan)
    __builtin_abort ();
  if (__builtin_isnan (ld) != res_isnan)
    __builtin_abort ();
  if (__builtin_isnanf (f) != res_isnan)
    __builtin_abort ();
  if (__builtin_isnanl (ld) != res_isnan)
    __builtin_abort ();

  if (__builtin_isinf (f) != res_isinf)
    __builtin_abort ();
  if (__builtin_isinf (d) != res_isinf)
    __builtin_abort ();
  if (__builtin_isinf (ld) != res_isinf)
    __builtin_abort ();
  if (__builtin_isinff (f) != res_isinf)
    __builtin_abort ();
  if (__builtin_isinfl (ld) != res_isinf)
    __builtin_abort ();

  if (__builtin_isinf_sign (f) != res_isinf_sign)
    __builtin_abort ();
  if (__builtin_isinf_sign (d) != res_isinf_sign)
    __builtin_abort ();
  if (__builtin_isinf_sign (ld) != res_isinf_sign)
    __builtin_abort ();

  if (__builtin_isnormal (f) != res_isnorm)
    __builtin_abort ();
  if (__builtin_isnormal (d) != res_isnorm)
    __builtin_abort ();
  if (__builtin_isnormal (ld) != res_isnorm)
    __builtin_abort ();

  if (__builtin_isfinite (f) != res_isfin)
    __builtin_abort ();
  if (__builtin_isfinite (d) != res_isfin)
    __builtin_abort ();
  if (__builtin_isfinite (ld) != res_isfin)
    __builtin_abort ();

  if (__builtin_finitef (f) != res_isfin)
    __builtin_abort ();
  if (__builtin_finite (f) != res_isfin)
    __builtin_abort ();
  if (__builtin_finite (d) != res_isfin)
    __builtin_abort ();
  if (__builtin_finitel (d) != res_isfin)
    __builtin_abort ();
  if (__builtin_finitel (ld) != res_isfin)
    __builtin_abort ();





    {
      if ((__builtin_signbit (f) ? 1 : 0) != res_signbit)
 __builtin_abort ();
      if ((__builtin_signbit (d) ? 1 : 0) != res_signbit)
 __builtin_abort ();
      if ((__builtin_signbit (ld) ? 1 : 0) != res_signbit)
 __builtin_abort ();
      if ((__builtin_signbitf (f) ? 1 : 0) != res_signbit)
 __builtin_abort ();
      if ((__builtin_signbitl (ld) ? 1 : 0) != res_signbit)
 __builtin_abort ();
    }






    {
      if (__builtin_fpclassify(1, 2, 3, 4, 5, (f)) != classification)
 __builtin_abort ();
      if (__builtin_fpclassify(1, 2, 3, 4, 5, (d)) != classification)
 __builtin_abort ();
      if (__builtin_fpclassify(1, 2, 3, 4, 5, (ld)) != classification)
 __builtin_abort ();
    }
}

void __attribute__ ((__noinline__))
foo (float f, double d, long double ld,
     int res_unord, int res_isnan, int res_isinf,
     int res_isfin, int res_isnorm, int classification)
{
  foo_1 (f, d, ld, res_unord, res_isnan, res_isinf, res_isinf, res_isfin, res_isnorm, 0, classification);


  foo_1 (-f, -d, -ld, res_unord, res_isnan, res_isinf, -res_isinf, res_isfin, res_isnorm, 1, classification);
}

int __attribute__ ((__noinline__))
main_tests (void)
{
  volatile float f;
  volatile double d;
  volatile long double ld;


  f = __builtin_nanf(""); d = __builtin_nan(""); ld = __builtin_nanl("");
  foo(f, d, ld, 1, 1, 0, 0, 0, 1);


  f = __builtin_inff(); d = __builtin_inf(); ld = __builtin_infl();
  foo(f, d, ld, 0, 0, 1, 0, 0, 2);


  f = 0; d = 0; ld = 0;
  foo(f, d, ld, 0, 0, 0, 1, 0, 5);


  f = 1; d = 1; ld = 1;
  foo(f, d, ld, 0, 0, 0, 1, 1, 3);


  f = 1.17549435082228750796873653722224568e-38F; d = double(2.22507385850720138309023271733240406e-308L); ld = 3.36210314311209350626267781732175260e-4932L;
  foo(f, d, ld, 0, 0, 0, 1, 1, 3);


  f = 1.17549435082228750796873653722224568e-38F/2; d = double(2.22507385850720138309023271733240406e-308L)/2; ld = 3.36210314311209350626267781732175260e-4932L/2;
  foo(f, d, ld, 0, 0, 0, 1, 0, 4);


  f = 3.40282346638528859811704183484516925e+38F; d = double(1.79769313486231570814527423731704357e+308L); ld = 1.18973149535723176502126385303097021e+4932L;
  foo(f, d, ld, 0, 0, 0, 1, 1, 3);


  f = 3.40282346638528859811704183484516925e+38F*2; d = double(1.79769313486231570814527423731704357e+308L)*2; ld = 1.18973149535723176502126385303097021e+4932L*2;
  foo(f, d, ld, 0, 0, 1, 0, 0, 2);

  return 0;
}
# 9 "./torture/type-generic-1.C" 2

int main(void)
{
  return main_tests ();
}
