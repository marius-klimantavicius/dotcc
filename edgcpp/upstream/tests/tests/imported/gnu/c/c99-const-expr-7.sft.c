//type: fn
//options: --c99 --strict_gnu
# 0 "./c99-const-expr-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c99-const-expr-7.c"






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 8 "./c99-const-expr-7.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
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
# 27 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 9 "./c99-const-expr-7.c" 2


# 10 "./c99-const-expr-7.c"
int a = ((double)1.79769313486231570814527423731704357e+308L)
# 10 "./c99-const-expr-7.c"
              ;

int b = (int) ((double)1.79769313486231570814527423731704357e+308L)
# 12 "./c99-const-expr-7.c"
                    ;
unsigned int c = -1.0;

unsigned int d = (unsigned)-1.0;

int e = 0 << 1000;

int f = 0 << -1;

int g = 0 >> 1000;

int h = 0 >> -1;


int b1 = (0 ? (int) ((double)1.79769313486231570814527423731704357e+308L) 
# 26 "./c99-const-expr-7.c"
                           : 0);
unsigned int d1 = (0 ? (unsigned int)-1.0 : 0);
int e1 = (0 ? 0 << 1000 : 0);
int f1 = (0 ? 0 << -1 : 0);
int g1 = (0 ? 0 >> 1000 : 0);
int h1 = (0 ? 0 >> -1: 0);

int i = -1 << 0;


int j[1] = { ((double)1.79769313486231570814527423731704357e+308L) 
# 36 "./c99-const-expr-7.c"
                    };


int array[2] = { [0 * (0x7fffffff 
# 39 "./c99-const-expr-7.c"
                              + 1)] = 0 };


_Bool k = 0x7fffffff 
# 42 "./c99-const-expr-7.c"
                 + 1;
