//type: rp
//options: 
# 0 "./torture/stackalign/inline-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/inline-1.c"



# 1 "./torture/stackalign/check.h" 1
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
# 2 "./torture/stackalign/check.h" 2








# 9 "./torture/stackalign/check.h"
extern void abort (void);


int
check_int (int *i, int align)
{
  *i = 20;
  if ((((ptrdiff_t) i) & (align - 1)) != 0)
    {



      abort ();
    }
  return *i;
}

void
check (void *p, int align)
{
  if ((((ptrdiff_t) p) & (align - 1)) != 0)
    {



      abort ();
    }
}
# 5 "./torture/stackalign/inline-1.c" 2





typedef int aligned __attribute__((aligned(64)));

int global;

static void
inline __attribute__((always_inline))
foo (void)
{
  aligned i;

  if (check_int (&i, __alignof__(i)) != i)
    abort ();
}

int
main()
{
  foo ();
  return 0;
}
