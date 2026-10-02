//type: rp
//options: 
# 0 "./torture/stackalign/vararg-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/vararg-1.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./torture/stackalign/vararg-1.c" 2
# 1 "./torture/stackalign/check.h" 1
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
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
# 6 "./torture/stackalign/vararg-1.c" 2





typedef int aligned __attribute__((aligned(64)));

int global;

void
bar (char *p, int size)
{
  __builtin_strncpy (p, "good", size);
}

void
foo (const char *fmt, ...)
{
  va_list arg;
  char *p;
  aligned i;
  int size;
  double x;

  
# 30 "./torture/stackalign/vararg-1.c" 3 4
 __builtin_c23_va_start(
# 30 "./torture/stackalign/vararg-1.c"
 arg, fmt
# 30 "./torture/stackalign/vararg-1.c" 3 4
 )
# 30 "./torture/stackalign/vararg-1.c"
                    ;
  size = 
# 31 "./torture/stackalign/vararg-1.c" 3 4
        __builtin_va_arg(
# 31 "./torture/stackalign/vararg-1.c"
        arg
# 31 "./torture/stackalign/vararg-1.c" 3 4
        ,
# 31 "./torture/stackalign/vararg-1.c"
        int
# 31 "./torture/stackalign/vararg-1.c" 3 4
        )
# 31 "./torture/stackalign/vararg-1.c"
                         ;
  if (size != 5)
    abort ();
  p = __builtin_alloca (size + 1);

  x = 
# 36 "./torture/stackalign/vararg-1.c" 3 4
     __builtin_va_arg(
# 36 "./torture/stackalign/vararg-1.c"
     arg
# 36 "./torture/stackalign/vararg-1.c" 3 4
     ,
# 36 "./torture/stackalign/vararg-1.c"
     double
# 36 "./torture/stackalign/vararg-1.c" 3 4
     )
# 36 "./torture/stackalign/vararg-1.c"
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
  
# 52 "./torture/stackalign/vararg-1.c" 3 4
 __builtin_va_end(
# 52 "./torture/stackalign/vararg-1.c"
 arg
# 52 "./torture/stackalign/vararg-1.c" 3 4
 )
# 52 "./torture/stackalign/vararg-1.c"
             ;
}

int
main()
{
  foo ("foo", 5, 5.0);
  return 0;
}
