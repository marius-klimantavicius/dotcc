//type: fp
//options: 
# 0 "./analyzer/data-model-path-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/data-model-path-1.c"
# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 2 "./analyzer/data-model-path-1.c" 2


# 3 "./analyzer/data-model-path-1.c"
static int *__attribute__((noinline))
callee (void)
{
  return 
# 6 "./analyzer/data-model-path-1.c" 3 4
        ((void *)0)
# 6 "./analyzer/data-model-path-1.c"
            ;
}

void test_1 (void)
{
  int *p = callee ();
  *p = 42;
}
