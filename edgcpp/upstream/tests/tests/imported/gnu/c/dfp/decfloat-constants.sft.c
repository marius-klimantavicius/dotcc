//type: fp
//options: 
# 0 "./dfp/decfloat-constants.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/decfloat-constants.c"
# 12 "./dfp/decfloat-constants.c"
# 1 "./dfp/dfp-dbg.h" 1


int failures;
# 13 "./dfp/decfloat-constants.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 14 "./dfp/decfloat-constants.c" 2

int main ()
{
  if (7 
# 17 "./dfp/decfloat-constants.c"
                    != 7) __builtin_abort ();
  if (16 
# 18 "./dfp/decfloat-constants.c"
                    != 16) __builtin_abort ();
  if (34 
# 19 "./dfp/decfloat-constants.c"
                     != 34) __builtin_abort ();

  if ((-94) 
# 21 "./dfp/decfloat-constants.c"
                   != -94) __builtin_abort ();
  if ((-382) 
# 22 "./dfp/decfloat-constants.c"
                   != -382) __builtin_abort ();
  if ((-6142) 
# 23 "./dfp/decfloat-constants.c"
                    != -6142) __builtin_abort ();

  if (97 
# 25 "./dfp/decfloat-constants.c"
                   != 97) __builtin_abort ();
  if (385 
# 26 "./dfp/decfloat-constants.c"
                   != 385) __builtin_abort ();
  if (6145 
# 27 "./dfp/decfloat-constants.c"
                    != 6145) __builtin_abort ();

  if (9.999999E96DF 
# 29 "./dfp/decfloat-constants.c"
               != 9.999999E96DF) __builtin_abort ();
  if (9.999999999999999E384DD 
# 30 "./dfp/decfloat-constants.c"
               != 9.999999999999999E384DD) __builtin_abort ();
  if (9.999999999999999999999999999999999E6144DL 
# 31 "./dfp/decfloat-constants.c"
                != 9.999999999999999999999999999999999E6144DL) __builtin_abort ();

  if (1E-6DF 
# 33 "./dfp/decfloat-constants.c"
                   != 1E-6DF) __builtin_abort ();
  if (1E-15DD 
# 34 "./dfp/decfloat-constants.c"
                   != 1E-15DD) __builtin_abort ();
  if (1E-33DL 
# 35 "./dfp/decfloat-constants.c"
                    != 1E-33DL) __builtin_abort ();

  if (1E-95DF 
# 37 "./dfp/decfloat-constants.c"
               != 1E-95DF) __builtin_abort ();
  if (1E-383DD 
# 38 "./dfp/decfloat-constants.c"
               != 1E-383DD) __builtin_abort ();
  if (1E-6143DL 
# 39 "./dfp/decfloat-constants.c"
                != 1E-6143DL) __builtin_abort ();

  if (0.000001E-95DF 
# 41 "./dfp/decfloat-constants.c"
                         != 0.000001E-95DF) __builtin_abort ();
  if (0.000000000000001E-383DD 
# 42 "./dfp/decfloat-constants.c"
                         != 0.000000000000001E-383DD) __builtin_abort ();
  if (0.000000000000000000000000000000001E-6143DL 
# 43 "./dfp/decfloat-constants.c"
                          != 0.000000000000000000000000000000001E-6143DL)
    __builtin_abort ();

  return 0;
}
