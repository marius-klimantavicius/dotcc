//type: rp
//options:  --c++17 --c++11
# 0 "./torture/stackalign/eh-vararg-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/eh-vararg-2.C"





# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./torture/stackalign/eh-vararg-2.C" 2
# 1 "./torture/stackalign/check.h" 1
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 2 "./torture/stackalign/check.h" 2






# 7 "./torture/stackalign/check.h"
extern "C" void abort (void);




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
# 8 "./torture/stackalign/eh-vararg-2.C" 2





typedef int aligned __attribute__((aligned(64)));

int global;

void
bar (char *p, int size)
{
  __builtin_strncpy (p, "good", size);
}

class Base {};

struct A : virtual public Base
{
  A() {}
};

struct B {};

void
test (va_list arg)

throw (B,A)

{
  char *p;
  aligned i;
  int size;
  double x;

  size = 
# 43 "./torture/stackalign/eh-vararg-2.C" 3 4
        __builtin_va_arg(
# 43 "./torture/stackalign/eh-vararg-2.C"
        arg
# 43 "./torture/stackalign/eh-vararg-2.C" 3 4
        ,
# 43 "./torture/stackalign/eh-vararg-2.C"
        int
# 43 "./torture/stackalign/eh-vararg-2.C" 3 4
        )
# 43 "./torture/stackalign/eh-vararg-2.C"
                         ;
  if (size != 5)
    abort ();

  p = (char *) __builtin_alloca (size + 1);

  x = 
# 49 "./torture/stackalign/eh-vararg-2.C" 3 4
     __builtin_va_arg(
# 49 "./torture/stackalign/eh-vararg-2.C"
     arg
# 49 "./torture/stackalign/eh-vararg-2.C" 3 4
     ,
# 49 "./torture/stackalign/eh-vararg-2.C"
     double
# 49 "./torture/stackalign/eh-vararg-2.C" 3 4
     )
# 49 "./torture/stackalign/eh-vararg-2.C"
                         ;
  if (x != 5.0)
    abort ();

  bar (p, size);
  if (__builtin_strncmp (p, "good", size) != 0)
    {




      abort ();
    }

  if (check_int (&i, __alignof__(i)) != i)
    abort ();

  throw A();
}

void
foo (const char *fmt, ...)
{
  va_list arg;
  
# 73 "./torture/stackalign/eh-vararg-2.C" 3 4
 __builtin_va_start(
# 73 "./torture/stackalign/eh-vararg-2.C"
 arg
# 73 "./torture/stackalign/eh-vararg-2.C" 3 4
 ,
# 73 "./torture/stackalign/eh-vararg-2.C"
 fmt
# 73 "./torture/stackalign/eh-vararg-2.C" 3 4
 )
# 73 "./torture/stackalign/eh-vararg-2.C"
                    ;
  test (arg);
  
# 75 "./torture/stackalign/eh-vararg-2.C" 3 4
 __builtin_va_end(
# 75 "./torture/stackalign/eh-vararg-2.C"
 arg
# 75 "./torture/stackalign/eh-vararg-2.C" 3 4
 )
# 75 "./torture/stackalign/eh-vararg-2.C"
             ;
}
int
main()
{
  try { foo ("foo", 5, 5.0); }
  catch (A& a) { }
  return 0;
}
