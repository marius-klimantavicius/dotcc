//type: fp
//options: 
# 0 "./dfp/float-constant-double.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/float-constant-double.c"
# 9 "./dfp/float-constant-double.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 10 "./dfp/float-constant-double.c" 2

extern double a, b, c, d;

void
foo ()
{
 
# 16 "./dfp/float-constant-double.c"
#pragma STDC FLOAT_CONST_DECIMAL64 ON
# 16 "./dfp/float-constant-double.c"
 
  a = 0.1d * ((double)1.79769313486231570814527423731704357e+308L)
# 17 "./dfp/float-constant-double.c"
                   ;
  b = ((double)2.22044604925031308084726333618164062e-16L) 
# 18 "./dfp/float-constant-double.c"
                 * 10.0d;
  c = ((double)2.22507385850720138309023271733240406e-308L) 
# 19 "./dfp/float-constant-double.c"
             * 200.0d;
}
