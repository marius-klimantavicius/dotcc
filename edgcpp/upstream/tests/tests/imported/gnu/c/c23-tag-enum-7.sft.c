//type: fp
//options: --c23
# 0 "./c23-tag-enum-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-tag-enum-7.c"



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
# 5 "./c23-tag-enum-7.c" 2



# 7 "./c23-tag-enum-7.c"
enum E { a = 1ULL, b = _Generic(a, int: 2) };
static_assert(_Generic(a, int: 1));
static_assert(_Generic(b, int: 1));
enum E { a = 1ULL, b = _Generic(a, int: 2) };
static_assert(_Generic(a, int: 1));
static_assert(_Generic(b, int: 1));


enum H { c = 1ULL << (32 
# 15 "./c23-tag-enum-7.c"
                                + 1), d = 2 };
static_assert(_Generic(c, enum H: 1));
static_assert(_Generic(d, enum H: 1));
enum H { c = 1ULL << (32 
# 18 "./c23-tag-enum-7.c"
                                + 1), d = _Generic(c, enum H: 2) };
static_assert(_Generic(c, enum H: 1));
static_assert(_Generic(d, enum H: 1));


enum K { e = 
# 23 "./c23-tag-enum-7.c" 3 4
            (0x7fffffff * 2U + 1U)
# 23 "./c23-tag-enum-7.c"
                    , f, g = _Generic(e, unsigned int: 0) + _Generic(f, unsigned long: 1, unsigned long long: 1) };
static_assert(_Generic(e, enum K: 1));
static_assert(_Generic(f, enum K: 1));
static_assert(_Generic(g, enum K: 1));
enum K { e = 
# 27 "./c23-tag-enum-7.c" 3 4
            (0x7fffffff * 2U + 1U)
# 27 "./c23-tag-enum-7.c"
                    , f, g = _Generic(e, enum K: 0) + _Generic(f, enum K: 1) };
static_assert(_Generic(e, enum K: 1));
static_assert(_Generic(f, enum K: 1));
static_assert(_Generic(g, enum K: 1));


enum U { k = 0x7fffffff
# 33 "./c23-tag-enum-7.c"
                   , l, m = _Generic(k, int: 0) + _Generic(l, long: 1, long long: 1) };
static_assert(_Generic(k, enum U: 1));
static_assert(_Generic(l, enum U: 1));
static_assert(_Generic(m, enum U: 1));
enum U { k = 0x7fffffff
# 37 "./c23-tag-enum-7.c"
                   , l, m = _Generic(k, enum U: 0) + _Generic(l, enum U: 1) };
static_assert(_Generic(k, enum U: 1));
static_assert(_Generic(l, enum U: 1));
static_assert(_Generic(m, enum U: 1));
