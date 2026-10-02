//type: rp
//options: 
# 0 "./absu.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./absu.c"




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
# 6 "./absu.c" 2
# 15 "./absu.c"

# 15 "./absu.c"
void foo_char (signed char x, unsigned char y){ char t = (((x) >= 0) ? (x) : -(x)); if (t != y) __builtin_abort (); };
void foo_short (signed short x, unsigned short y){ short t = (((x) >= 0) ? (x) : -(x)); if (t != y) __builtin_abort (); };
void foo_int (signed int x, unsigned int y){ int t = (((x) >= 0) ? (x) : -(x)); if (t != y) __builtin_abort (); };
void foo_long (signed long x, unsigned long y){ long t = (((x) >= 0) ? (x) : -(x)); if (t != y) __builtin_abort (); };

int main ()
{
  foo_char (
# 22 "./absu.c" 3 4
           (-0x7f - 1) 
# 22 "./absu.c"
                     + 1, 0x7f
# 22 "./absu.c"
                                   );
  foo_char (0, 0);
  foo_char (-1, 1);
  foo_char (1, 1);
  foo_char (0x7f
# 26 "./absu.c"
                    , 0x7f
# 26 "./absu.c"
                               );

  foo_int (-1, 1);
  foo_int (0, 0);
  foo_int (0x7fffffff
# 30 "./absu.c"
                 , 0x7fffffff
# 30 "./absu.c"
                          );
  foo_int (
# 31 "./absu.c" 3 4
          (-0x7fffffff - 1) 
# 31 "./absu.c"
                  + 1, 0x7fffffff
# 31 "./absu.c"
                              );

  foo_short (-1, 1);
  foo_short (0, 0);
  foo_short (0x7fff
# 35 "./absu.c"
                    , 0x7fff
# 35 "./absu.c"
                              );
  foo_short (
# 36 "./absu.c" 3 4
            (-0x7fff - 1) 
# 36 "./absu.c"
                     + 1, 0x7fff
# 36 "./absu.c"
                                  );

  foo_long (-1, 1);
  foo_long (0, 0);
  foo_long (0x7fffffffffffffffL
# 40 "./absu.c"
                   , 0x7fffffffffffffffL
# 40 "./absu.c"
                             );
  foo_long (
# 41 "./absu.c" 3 4
           (-0x7fffffffffffffffL - 1L) 
# 41 "./absu.c"
                    + 1, 0x7fffffffffffffffL
# 41 "./absu.c"
                                 );

  return 0;
}
