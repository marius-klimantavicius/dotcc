//type: fp
//options: 
# 0 "./dfp/convert-complex.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/convert-complex.c"





# 1 "./dfp/dfp-dbg.h" 1


int failures;
# 7 "./dfp/convert-complex.c" 2

int
main ()
{
  _Complex float cf;
  _Complex double cd;
  _Complex long double cld;

  _Decimal32 d32;
  _Decimal64 d64;
  _Decimal128 d128;

  cf = 2.0f * __extension__ 1i + 3.0f;
  cd = 2.0 * __extension__ 1i + 3.0;
  cld = 2.0l * __extension__ 1i + 3.0l;


  d32 = cf;
  d64 = cd;
  d128 = cld;

  if (d32 != 3.0DF)
    __builtin_abort ();
  if (d64 != 3.0dd)
    __builtin_abort ();
  if (d128 != 3.0dl)
    __builtin_abort ();


  d32 = 2.5DF;
  d64 = 1.5DD;
  d128 = 2.5DL;

  cf = d32;
  cd = d64;
  cld = d128;







  if (__real__ cf != 2.5f)
    __builtin_abort ();
  if (__real__ cd !=1.5)
    __builtin_abort ();
  if (__real__ cld != 2.5)
    __builtin_abort ();
  if (__imag__ cf != 0.0f)
    __builtin_abort ();
  if (__imag__ cd != 0.0)
    __builtin_abort ();
  if (__imag__ cld != 0.0l)
    __builtin_abort ();





  d64 = 0.125dl;
  cf = d64;
  if (__real__ cf != 0.125f)
    __builtin_abort ();

  d128 = 1.25E-7dl;
  cd = d128;
  if (__real__ cd != 1.25E-7)
    __builtin_abort ();






  cf = 2.0f * __extension__ 1i + 2.25f;
  d32 = cf;
  d64 = cf;
  d128 = cf;
  if (d32 != 2.25DF)
    __builtin_abort ();
  if (d64 != 2.25DD)
    __builtin_abort ();
  if (d128 != 2.25DL)
    __builtin_abort ();


  cd = 2.0 * __extension__ 1i + 1.25;
  d32 = cd;
  d64 = cd;
  d128 = cd;
  if (d32 != 1.25DF)
    __builtin_abort ();
  if (d64 != 1.25DD)
    __builtin_abort ();
  if (d128 != 1.25DL)
    __builtin_abort ();


  cld = 2.0l * __extension__ 1i + 0.0625l;
  d32 = cld;
  d64 = cld;
  d128 = cld;
  if (d32 != 0.0625DF)
    __builtin_abort ();
  if (d64 != 0.0625DD)
    __builtin_abort ();
  if (d128 != 0.0625DL)
    __builtin_abort ();

  return 0;
}
