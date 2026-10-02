//type: fn
//options: --c23
# 0 "./c23-tag-enum-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-tag-enum-6.c"



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
# 5 "./c23-tag-enum-6.c" 2


# 6 "./c23-tag-enum-6.c"
enum E : int { a = 1, b = 2 };
enum E : int { b = _Generic(a, enum E: 2), a = 1 };

enum H { x = 1 };
enum H { x = 2ULL + 
# 10 "./c23-tag-enum-6.c" 3 4
                   (0x7fffffff * 2U + 1U) 
# 10 "./c23-tag-enum-6.c"
                            };

enum K : int { z = 1 };
enum K : int { z = 2ULL + 
# 13 "./c23-tag-enum-6.c" 3 4
                         (0x7fffffff * 2U + 1U) 
# 13 "./c23-tag-enum-6.c"
                                  };

enum F { A = 0, B = 
# 15 "./c23-tag-enum-6.c" 3 4
                   (0x7fffffff * 2U + 1U) 
# 15 "./c23-tag-enum-6.c"
                            };
enum F { B = 
# 16 "./c23-tag-enum-6.c" 3 4
            (0x7fffffff * 2U + 1U)
# 16 "./c23-tag-enum-6.c"
                    , A };

enum G : unsigned int { C = 0, D = 
# 18 "./c23-tag-enum-6.c" 3 4
                                  (0x7fffffff * 2U + 1U) 
# 18 "./c23-tag-enum-6.c"
                                           };
enum G : unsigned int { D = 
# 19 "./c23-tag-enum-6.c" 3 4
                           (0x7fffffff * 2U + 1U)
# 19 "./c23-tag-enum-6.c"
                                   , C };
