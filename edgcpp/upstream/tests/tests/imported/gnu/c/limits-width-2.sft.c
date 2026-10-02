//type: fp
//options: --c23
# 0 "./limits-width-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./limits-width-2.c"




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
# 6 "./limits-width-2.c" 2
# 14 "./limits-width-2.c"

# 14 "./limits-width-2.c"
_Static_assert ((0x7f 
# 14 "./limits-width-2.c"
>> ((char) -1 < 0 ? (8 
# 14 "./limits-width-2.c"
- 2) : (8 
# 14 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7f 
# 18 "./limits-width-2.c"
>> ((signed char) -1 < 0 ? (8 
# 18 "./limits-width-2.c"
- 2) : (8 
# 18 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 22 "./limits-width-2.c" 3 4
(0x7f * 2 + 1) 
# 22 "./limits-width-2.c"
>> ((unsigned char) -1 < 0 ? (8 
# 22 "./limits-width-2.c"
- 2) : (8 
# 22 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fff 
# 26 "./limits-width-2.c"
>> ((signed short) -1 < 0 ? (16 
# 26 "./limits-width-2.c"
- 2) : (16 
# 26 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 30 "./limits-width-2.c" 3 4
(0x7fff * 2 + 1) 
# 30 "./limits-width-2.c"
>> ((unsigned short) -1 < 0 ? (16 
# 30 "./limits-width-2.c"
- 2) : (16 
# 30 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffff 
# 34 "./limits-width-2.c"
>> ((signed int) -1 < 0 ? (32 
# 34 "./limits-width-2.c"
- 2) : (32 
# 34 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 38 "./limits-width-2.c" 3 4
(0x7fffffff * 2U + 1U) 
# 38 "./limits-width-2.c"
>> ((unsigned int) -1 < 0 ? (32 
# 38 "./limits-width-2.c"
- 2) : (32 
# 38 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffffffffffffL 
# 42 "./limits-width-2.c"
>> ((signed long) -1 < 0 ? (64 
# 42 "./limits-width-2.c"
- 2) : (64 
# 42 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 46 "./limits-width-2.c" 3 4
(0x7fffffffffffffffL * 2UL + 1UL) 
# 46 "./limits-width-2.c"
>> ((unsigned long) -1 < 0 ? (64 
# 46 "./limits-width-2.c"
- 2) : (64 
# 46 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((0x7fffffffffffffffLL 
# 50 "./limits-width-2.c"
>> ((signed long long) -1 < 0 ? (64 
# 50 "./limits-width-2.c"
- 2) : (64 
# 50 "./limits-width-2.c"
- 1))) == 1, "width must match type");



_Static_assert ((
# 54 "./limits-width-2.c" 3 4
(0x7fffffffffffffffLL * 2ULL + 1ULL) 
# 54 "./limits-width-2.c"
>> ((unsigned long long) -1 < 0 ? (64 
# 54 "./limits-width-2.c"
- 2) : (64 
# 54 "./limits-width-2.c"
- 1))) == 1, "width must match type");
