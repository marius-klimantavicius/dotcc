//type: fp
//options: 
# 0 "./Walloca-larger-than-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Walloca-larger-than-3.c"





# 1 "./Walloca-larger-than-3.h" 1

# 1 "/usr/include/alloca.h" 1 3 4
# 21 "/usr/include/alloca.h" 3 4
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
# 22 "/usr/include/alloca.h" 2 3 4


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 25 "/usr/include/alloca.h" 2 3 4







extern void *alloca (size_t __size) __attribute__ ((__nothrow__ , __leaf__));






# 3 "./Walloca-larger-than-3.h" 2
# 7 "./Walloca-larger-than-3.c" 2


# 8 "./Walloca-larger-than-3.c"
void sink (void*, ...);

void call_builtin_alloca (int n)
{
  if (n < 9)
    n = 9;
  void *p = __builtin_alloca (n);
  sink (p, 0);
}

void call_alloca_sys_hdr (int n)
{
  if (n < 9)
    n = 9;
  void *p = 
# 22 "./Walloca-larger-than-3.c" 3 4
           __builtin_alloca (
# 22 "./Walloca-larger-than-3.c"
           n
# 22 "./Walloca-larger-than-3.c" 3 4
           )
# 22 "./Walloca-larger-than-3.c"
                     ;
  sink (p, 1);
}

static inline void inline_call_alloca (int n)
{
  if (n > 9)
    n = 9;
  void *p = 
# 30 "./Walloca-larger-than-3.c" 3 4
           __builtin_alloca (
# 30 "./Walloca-larger-than-3.c"
           n
# 30 "./Walloca-larger-than-3.c" 3 4
           )
# 30 "./Walloca-larger-than-3.c"
                     ;
  sink (p, 2);
}

void make_inlined_call (void)
{
  inline_call_alloca (10);
}
