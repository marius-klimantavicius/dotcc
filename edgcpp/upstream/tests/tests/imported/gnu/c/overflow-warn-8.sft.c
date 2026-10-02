//type: fp
//options: 
# 0 "./overflow-warn-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./overflow-warn-8.c"
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
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 2 "./overflow-warn-8.c" 2



# 4 "./overflow-warn-8.c"
void foo (int j)
{
  int i1 = (int)(double)1.0 + 0x7fffffff
# 6 "./overflow-warn-8.c"
                                    ;
  int i2 = (int)(double)1 + 0x7fffffff
# 7 "./overflow-warn-8.c"
                                  ;
  int i3 = 1 + 0x7fffffff
# 8 "./overflow-warn-8.c"
                     ;
  int i4 = +1 + 0x7fffffff
# 9 "./overflow-warn-8.c"
                      ;
  int i5 = (int)((double)1.0 + 0x7fffffff
# 10 "./overflow-warn-8.c"
                                     );
  int i6 = (double)1.0 + 0x7fffffff
# 11 "./overflow-warn-8.c"
                               ;
  int i7 = 0 ? (int)(double)1.0 + 0x7fffffff 
# 12 "./overflow-warn-8.c"
                                         : 1;
  int i8 = 1 ? 1 : (int)(double)1.0 + 0x7fffffff
# 13 "./overflow-warn-8.c"
                                            ;
  int i9 = j ? (int)(double)1.0 + 0x7fffffff 
# 14 "./overflow-warn-8.c"
                                         : 1;
  unsigned int i10 = 0 ? (int)(double)1.0 + 0x7fffffff 
# 15 "./overflow-warn-8.c"
                                                   : 9U;
  unsigned int i11 = 1 ? 9U : (int)(double)1.0 + 0x7fffffff
# 16 "./overflow-warn-8.c"
                                                       ;
  unsigned int i12 = j ? (int)(double)1.0 + 0x7fffffff 
# 17 "./overflow-warn-8.c"
                                                   : 9U;
  int i13 = 1 || (int)(double)1.0 + 0x7fffffff 
# 18 "./overflow-warn-8.c"
                                           < 0;
  int i14 = 0 && (int)(double)1.0 + 0x7fffffff 
# 19 "./overflow-warn-8.c"
                                           < 0;
  int i15 = 0 || (int)(double)1.0 + 0x7fffffff 
# 20 "./overflow-warn-8.c"
                                           < 0;
  int i16 = 1 && (int)(double)1.0 + 0x7fffffff 
# 21 "./overflow-warn-8.c"
                                           < 0;
  int i17 = j || (int)(double)1.0 + 0x7fffffff 
# 22 "./overflow-warn-8.c"
                                           < 0;
  int i18 = j && (int)(double)1.0 + 0x7fffffff 
# 23 "./overflow-warn-8.c"
                                           < 0;
}
