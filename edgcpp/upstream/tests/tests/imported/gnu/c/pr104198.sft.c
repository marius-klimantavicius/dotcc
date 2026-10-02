//type: rp
//options: --c99
# 0 "./pr104198.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr104198.c"







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
# 9 "./pr104198.c" 2
# 1 "/usr/include/assert.h" 1 3 4
# 65 "/usr/include/assert.h" 3 4



extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



# 10 "./pr104198.c" 2


# 11 "./pr104198.c"
__attribute__ ((noinline))
int foo (int *a, int n)
{
  int min = 999999;
  int bla = 0;
  for (int i = 0; i < n; i++)
    {
      if (a[i] < min)
 {
   min = a[i];
   bla = 1;
 }
    }

  if (bla)
    min += 1;
  return min;
}

int main()
{
  int a[] = {2, 1, -13, 0x7fffffff
# 32 "./pr104198.c"
                              , 
# 32 "./pr104198.c" 3 4
                                (-0x7fffffff - 1)
# 32 "./pr104198.c"
                                       , 0};

  int res = foo (a, sizeof (a) / sizeof (a[0]));

  
# 36 "./pr104198.c" 3 4
 ((
# 36 "./pr104198.c"
 res == (
# 36 "./pr104198.c" 3 4
 (-0x7fffffff - 1) 
# 36 "./pr104198.c"
 + 1)
# 36 "./pr104198.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 36 "./pr104198.c"
 "res == ((-0x7fffffff - 1) + 1)"
# 36 "./pr104198.c" 3 4
 , "./pr104198.c", 36, __PRETTY_FUNCTION__))
# 36 "./pr104198.c"
                              ;
}
