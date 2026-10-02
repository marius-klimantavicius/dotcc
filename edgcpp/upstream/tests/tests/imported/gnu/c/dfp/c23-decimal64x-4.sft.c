//type: rp
//options: --c23
# 0 "./dfp/c23-decimal64x-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/c23-decimal64x-4.c"
# 9 "./dfp/c23-decimal64x-4.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 10 "./dfp/c23-decimal64x-4.c" 2


static_assert (_Generic (9.999999999999999999999999999999999E6144D64x
# 12 "./dfp/c23-decimal64x-4.c"
              , default : 0, _Decimal64x : 1));
static_assert (_Generic (1E-6143D64x
# 13 "./dfp/c23-decimal64x-4.c"
              , default : 0, _Decimal64x : 1));
static_assert (_Generic (1E-33D64x
# 14 "./dfp/c23-decimal64x-4.c"
              , default : 0, _Decimal64x : 1));
static_assert (_Generic (0.000000000000000000000000000000001E-6143D64x
# 15 "./dfp/c23-decimal64x-4.c"
              , default : 0, _Decimal64x : 1));

int
main ()
{


  if (34 
# 22 "./dfp/c23-decimal64x-4.c"
                     != 34)
    __builtin_abort ();

  if ((-6142) 
# 25 "./dfp/c23-decimal64x-4.c"
                    != -6142)
    __builtin_abort ();

  if (6145 
# 28 "./dfp/c23-decimal64x-4.c"
                    != 6145)
    __builtin_abort ();

  if (9.999999999999999999999999999999999E6144D64x 
# 31 "./dfp/c23-decimal64x-4.c"
                != 9.999999999999999999999999999999999E6144D64x)
    __builtin_abort ();

  if (1E-33D64x 
# 34 "./dfp/c23-decimal64x-4.c"
                    != 1E-33D64x)
    __builtin_abort ();

  if (1E-6143D64x 
# 37 "./dfp/c23-decimal64x-4.c"
                != 1E-6143D64x)
    __builtin_abort ();

  if (0.000000000000000000000000000000001E-6143D64x 
# 40 "./dfp/c23-decimal64x-4.c"
                     != 0.000000000000000000000000000000001E-6143D64x)
    __builtin_abort ();

}
