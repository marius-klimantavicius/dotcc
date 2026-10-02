//type: rp
//options: 
# 0 "./torture/stackalign/regparm-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/regparm-1.c"


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
# 4 "./torture/stackalign/regparm-1.c" 2





typedef int aligned __attribute__((aligned(64)));

int test_nested (int i)
{
  aligned y;

  int __attribute__ ((__noinline__, __regparm__(2))) foo (int j, int k, int l)
  {
    aligned x;

    if (check_int (&x, __alignof__(x)) != x)
      abort ();

    if (x != 20)
      abort ();

    return i + j + k + l;
  }

  if (check_int (&y, __alignof__(y)) != y)
    abort ();

  if (y != 20)
    abort ();

  return foo(i, i+1, i+2) * i;
}

int __attribute__ ((__noinline__, __regparm__(3), __force_align_arg_pointer__))
test_realigned (int j, int k, int l)
{
  aligned y;

  if (check_int (&y, __alignof__(y)) != y)
    abort ();

  if (y != 20)
    abort ();

  return j + k + l;
}

int main ()
{
  if (test_nested(10) != 430)
    abort ();

  if (test_realigned(10, 11, 12) != 33)
    abort ();

  return 0;
}
