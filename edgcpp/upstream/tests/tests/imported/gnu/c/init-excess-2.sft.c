//type: fp
//options: 
# 0 "./init-excess-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./init-excess-2.c"







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 9 "./init-excess-2.c" 2


# 10 "./init-excess-2.c"
int* a[1] = {
  0,
  
# 12 "./init-excess-2.c" 3 4
 ((void *)0)

# 13 "./init-excess-2.c"
};

const char str[1] = {
  0,
  
# 17 "./init-excess-2.c" 3 4
 ((void *)0)

# 18 "./init-excess-2.c"
};

struct S {
  int *a;
} s = {
  0,
  
# 24 "./init-excess-2.c" 3 4
 ((void *)0)

# 25 "./init-excess-2.c"
};

struct __attribute__ ((designated_init)) S2 {
  int *a;
} s2 = {
  
# 30 "./init-excess-2.c" 3 4
 ((void *)0)

# 31 "./init-excess-2.c"
};

union U {
  int *a;
} u = {
  0,
  
# 37 "./init-excess-2.c" 3 4
 ((void *)0)

# 38 "./init-excess-2.c"
};

int __attribute__ ((vector_size (16))) ivec = {
  0, 0, 0, 0,
  
# 42 "./init-excess-2.c" 3 4
 ((void *)0)

# 43 "./init-excess-2.c"
};

int* scal = {
  0,
  
# 47 "./init-excess-2.c" 3 4
 ((void *)0)

# 48 "./init-excess-2.c"
};
