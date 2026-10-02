//type: fn
//options: --c99 -w
# 0 "./init-string-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./init-string-2.c"
# 11 "./init-string-2.c"
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
# 12 "./init-string-2.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 13 "./init-string-2.c" 2


# 14 "./init-string-2.c"
typedef enum { schar_min = 
# 14 "./init-string-2.c" 3 4
                          (-0x7f - 1)
# 14 "./init-string-2.c"
                                   , schar_max = 0x7f 
# 14 "./init-string-2.c"
                                                           } schar;
typedef enum { uchar_max = 
# 15 "./init-string-2.c" 3 4
                          (0x7f * 2 + 1) 
# 15 "./init-string-2.c"
                                    } uchar;
typedef enum { shrt_min = 
# 16 "./init-string-2.c" 3 4
                         (-0x7fff - 1)
# 16 "./init-string-2.c"
                                 , shrt_max = 0x7fff 
# 16 "./init-string-2.c"
                                                       } sshrt;
typedef enum { ushrt_max = 
# 17 "./init-string-2.c" 3 4
                          (0x7fff * 2 + 1) 
# 17 "./init-string-2.c"
                                    } ushrt;

char a0[] = "foo";
const signed char a2[4] = "foo";
volatile unsigned char a3[3] = "foo";
wchar_t a4[] = L"foo";
const wchar_t a5[3] = L"foo";
volatile ushrt a6[] = L"foo";

schar a7[] = "foo";
uchar a8[] = "foo";
const schar a9[] = "foo";
short a10[] = L"foo";
const sshrt a11[] = L"foo";
char a12[] = L"foo";
wchar_t a13[] = "foo";

char b0[] = { "foo" };
const signed char b2[4] = { "foo" };
volatile unsigned char b3[3] = { "foo" };
wchar_t b4[] = { L"foo" };
const wchar_t b5[3] = { L"foo" };
volatile ushrt b6[] = { L"foo" };

schar b7[] = { "foo" };
uchar b8[] = { "foo" };
const schar b9[] = { "foo" };
short b10[] = { L"foo" };
const sshrt b11[] = { L"foo" };
char b12[] = { L"foo" };
wchar_t b13[] = { "foo" };

struct s { signed char a[10]; int b; ushrt c[10]; };

struct s c = { "foo", 0, L"bar" };
struct s d = { .c = L"bar", .a = "foo" };

ushrt *e = (ushrt [7]){ L"bar" };

wchar_t f[5][5] = { L"foo", L"bar" };
ushrt g[5][5] = { L"foo", L"bar" };
