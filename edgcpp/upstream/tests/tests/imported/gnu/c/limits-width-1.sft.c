//type: fp
//options: --c11
# 0 "./limits-width-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./limits-width-1.c"





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
# 7 "./limits-width-1.c" 2
# 15 "./limits-width-1.c"

# 15 "./limits-width-1.c"
_Static_assert ((0x7f 
# 15 "./limits-width-1.c"
>> ((char) -1 < 0 ? (8 
# 15 "./limits-width-1.c"
- 2) : (8 
# 15 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7f 
# 19 "./limits-width-1.c"
>> ((signed char) -1 < 0 ? (8 
# 19 "./limits-width-1.c"
- 2) : (8 
# 19 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 23 "./limits-width-1.c" 3 4
(0x7f * 2 + 1) 
# 23 "./limits-width-1.c"
>> ((unsigned char) -1 < 0 ? (8 
# 23 "./limits-width-1.c"
- 2) : (8 
# 23 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fff 
# 27 "./limits-width-1.c"
>> ((signed short) -1 < 0 ? (16 
# 27 "./limits-width-1.c"
- 2) : (16 
# 27 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 31 "./limits-width-1.c" 3 4
(0x7fff * 2 + 1) 
# 31 "./limits-width-1.c"
>> ((unsigned short) -1 < 0 ? (16 
# 31 "./limits-width-1.c"
- 2) : (16 
# 31 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffff 
# 35 "./limits-width-1.c"
>> ((signed int) -1 < 0 ? (32 
# 35 "./limits-width-1.c"
- 2) : (32 
# 35 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 39 "./limits-width-1.c" 3 4
(0x7fffffff * 2U + 1U) 
# 39 "./limits-width-1.c"
>> ((unsigned int) -1 < 0 ? (32 
# 39 "./limits-width-1.c"
- 2) : (32 
# 39 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffffffffffffL 
# 43 "./limits-width-1.c"
>> ((signed long) -1 < 0 ? (64 
# 43 "./limits-width-1.c"
- 2) : (64 
# 43 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 47 "./limits-width-1.c" 3 4
(0x7fffffffffffffffL * 2UL + 1UL) 
# 47 "./limits-width-1.c"
>> ((unsigned long) -1 < 0 ? (64 
# 47 "./limits-width-1.c"
- 2) : (64 
# 47 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffffffffffffLL 
# 51 "./limits-width-1.c"
>> ((signed long long) -1 < 0 ? (64 
# 51 "./limits-width-1.c"
- 2) : (64 
# 51 "./limits-width-1.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 55 "./limits-width-1.c" 3 4
(0x7fffffffffffffffLL * 2ULL + 1ULL) 
# 55 "./limits-width-1.c"
>> ((unsigned long long) -1 < 0 ? (64 
# 55 "./limits-width-1.c"
- 2) : (64 
# 55 "./limits-width-1.c"
- 1))) == 1, "width must match type");
