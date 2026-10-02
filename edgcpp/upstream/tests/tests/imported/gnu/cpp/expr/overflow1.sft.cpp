//type: fn
//options: 
# 0 "./expr/overflow1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./expr/overflow1.C"
# 1 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/syslimits.h" 1 3 4






# 1 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 3 4
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
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 2 3 4
# 8 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/syslimits.h" 2 3 4
# 35 "/mds/gnu/build/gcc-14.2.0/lib/gcc/x86_64-pc-linux-gnu/14.2.0/include/limits.h" 2 3 4
# 2 "./expr/overflow1.C" 2

enum E {
  A = (unsigned char)-1,
  B = (signed char)
# 5 "./expr/overflow1.C" 3 4
                  (0x7f * 2 + 1)
# 5 "./expr/overflow1.C"
                           ,
  C = 0x7fffffff 
# 6 "./expr/overflow1.C"
            +1,
  D = 
# 7 "./expr/overflow1.C" 3 4
     (0x7fffffff * 2U + 1U)
# 7 "./expr/overflow1.C"
             +1
};
