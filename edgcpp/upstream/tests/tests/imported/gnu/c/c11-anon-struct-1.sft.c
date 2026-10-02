//type: fp
//options: --c11
# 0 "./c11-anon-struct-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-anon-struct-1.c"




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
# 6 "./c11-anon-struct-1.c" 2


# 7 "./c11-anon-struct-1.c"
struct s1
{
  int a;
  union
  {
    int i;
  };
  struct
  {
    int b;
  };
};

union u1
{
  int b;
  struct
  {
    int i;
  };
  union
  {
    int c;
  };
};

struct s2
{
  struct
  {
    int a;
  };
};

struct s3
{
  union
  {
    int i;
  };
};

struct s4
{
  struct
  {
    int i;
  };
  int a[];
};

struct s1 x =
  {
    .b = 1,
    .i = 2,
    .a = 3
  };

int o = 
# 65 "./c11-anon-struct-1.c" 3 4
       __builtin_offsetof (
# 65 "./c11-anon-struct-1.c"
       struct s1
# 65 "./c11-anon-struct-1.c" 3 4
       , 
# 65 "./c11-anon-struct-1.c"
       i
# 65 "./c11-anon-struct-1.c" 3 4
       )
# 65 "./c11-anon-struct-1.c"
                              ;

void
f (void)
{
  x.i = 3;
  (&x)->i = 4;
}
