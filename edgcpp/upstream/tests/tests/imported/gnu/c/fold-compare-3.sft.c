//type: fp
//options: 
# 0 "./fold-compare-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fold-compare-3.c"



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
# 5 "./fold-compare-3.c" 2


# 6 "./fold-compare-3.c"
void this_comparison_is_false (void);
void this_comparison_is_true (void);
void this_comparison_is_not_decidable (void);

void bla1eq (int var)
{
  if (var + 10 == 
# 12 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 12 "./fold-compare-3.c"
                         + 9)
    this_comparison_is_false ();
}

void bla2eq (int var)
{
  if (var + 10 == 
# 18 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 18 "./fold-compare-3.c"
                         + 10)
    this_comparison_is_not_decidable ();
}

void bla3eq (int var)
{
  if (var - 10 == 0x7fffffff 
# 24 "./fold-compare-3.c"
                         - 9)
    this_comparison_is_false ();
}

void bla4eq (int var)
{
  if (var - 10 == 0x7fffffff 
# 30 "./fold-compare-3.c"
                         - 10)
    this_comparison_is_not_decidable ();
}

void bla1ne (int var)
{
  if (var + 10 != 
# 36 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 36 "./fold-compare-3.c"
                         + 9)
    this_comparison_is_true ();
}

void bla2ne (int var)
{
  if (var + 10 != 
# 42 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 42 "./fold-compare-3.c"
                         + 10)
    this_comparison_is_not_decidable ();
}

void bla3ne (int var)
{
  if (var - 10 != 0x7fffffff 
# 48 "./fold-compare-3.c"
                         - 9)
    this_comparison_is_true ();
}

void bla4ne (int var)
{
  if (var - 10 != 0x7fffffff 
# 54 "./fold-compare-3.c"
                         - 10)
    this_comparison_is_not_decidable ();
}

void bla1lt (int var)
{
  if (var + 10 < 
# 60 "./fold-compare-3.c" 3 4
                (-0x7fffffff - 1) 
# 60 "./fold-compare-3.c"
                        + 10)
    this_comparison_is_false ();
}

void bla2lt (int var)
{
  if (var + 10 < 
# 66 "./fold-compare-3.c" 3 4
                (-0x7fffffff - 1) 
# 66 "./fold-compare-3.c"
                        + 11)
    this_comparison_is_not_decidable ();
}

void bla3lt (int var)
{
  if (var - 10 < 0x7fffffff 
# 72 "./fold-compare-3.c"
                        - 9)
    this_comparison_is_true ();
}

void bla4lt (int var)
{
  if (var - 10 < 0x7fffffff 
# 78 "./fold-compare-3.c"
                        - 10)
    this_comparison_is_not_decidable ();
}

void bla1le (int var)
{
  if (var + 10 <= 
# 84 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 84 "./fold-compare-3.c"
                         + 9)
    this_comparison_is_false ();
}

void bla2le (int var)
{
  if (var + 10 <= 
# 90 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 90 "./fold-compare-3.c"
                         + 10)
    this_comparison_is_not_decidable ();
}

void bla3le (int var)
{
  if (var - 10 <= 0x7fffffff 
# 96 "./fold-compare-3.c"
                         - 10)
    this_comparison_is_true ();
}

void bla4le (int var)
{
  if (var - 10 <= 0x7fffffff 
# 102 "./fold-compare-3.c"
                         - 11)
    this_comparison_is_not_decidable ();
}

void bla1gt (int var)
{
  if (var + 10 > 
# 108 "./fold-compare-3.c" 3 4
                (-0x7fffffff - 1) 
# 108 "./fold-compare-3.c"
                        + 9)
    this_comparison_is_true ();
}

void bla2gt (int var)
{
  if (var + 10 > 
# 114 "./fold-compare-3.c" 3 4
                (-0x7fffffff - 1) 
# 114 "./fold-compare-3.c"
                        + 10)
    this_comparison_is_not_decidable ();
}

void bla3gt (int var)
{
  if (var - 10 > 0x7fffffff 
# 120 "./fold-compare-3.c"
                        - 10)
    this_comparison_is_false ();
}

void bla4gt (int var)
{
  if (var - 10 > 0x7fffffff 
# 126 "./fold-compare-3.c"
                        - 11)
    this_comparison_is_not_decidable ();
}

void bla1ge (int var)
{
  if (var + 10 >= 
# 132 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 132 "./fold-compare-3.c"
                         + 10)
    this_comparison_is_true ();
}

void bla2ge (int var)
{
  if (var + 10 >= 
# 138 "./fold-compare-3.c" 3 4
                 (-0x7fffffff - 1) 
# 138 "./fold-compare-3.c"
                         + 11)
    this_comparison_is_not_decidable ();
}

void bla3ge (int var)
{
  if (var - 11 >= 0x7fffffff 
# 144 "./fold-compare-3.c"
                         - 10)
    this_comparison_is_false ();
}

void bla4ge (int var)
{
  if (var - 10 >= 0x7fffffff 
# 150 "./fold-compare-3.c"
                         - 10)
    this_comparison_is_not_decidable ();
}
