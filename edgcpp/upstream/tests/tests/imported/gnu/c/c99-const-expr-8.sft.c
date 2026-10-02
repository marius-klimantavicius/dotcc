//type: fn
//options: --c99 --strict_gnu
# 0 "./c99-const-expr-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c99-const-expr-8.c"
# 9 "./c99-const-expr-8.c"
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
# 10 "./c99-const-expr-8.c" 2


# 11 "./c99-const-expr-8.c"
enum e {
  E0 = 0 * (0x7fffffff 
# 12 "./c99-const-expr-8.c"
                   + 1),

  E1 = 0 * (
# 14 "./c99-const-expr-8.c" 3 4
           (-0x7fffffff - 1) 
# 14 "./c99-const-expr-8.c"
                   / -1),

  E2 = 0 * (0x7fffffff 
# 16 "./c99-const-expr-8.c"
                   * 0x7fffffff
# 16 "./c99-const-expr-8.c"
                            ),

  E3 = 0 * (
# 18 "./c99-const-expr-8.c" 3 4
           (-0x7fffffff - 1) 
# 18 "./c99-const-expr-8.c"
                   - 1),

  E4 = 0 * (unsigned)(
# 20 "./c99-const-expr-8.c" 3 4
                     (-0x7fffffff - 1) 
# 20 "./c99-const-expr-8.c"
                             - 1),

  E5 = 0 * -
# 22 "./c99-const-expr-8.c" 3 4
           (-0x7fffffff - 1)
# 22 "./c99-const-expr-8.c"
                  ,

  E6 = 0 * !-
# 24 "./c99-const-expr-8.c" 3 4
            (-0x7fffffff - 1)
# 24 "./c99-const-expr-8.c"
                   ,

  E7 = 
# 26 "./c99-const-expr-8.c" 3 4
      (-0x7fffffff - 1) 
# 26 "./c99-const-expr-8.c"
              % -1

};
