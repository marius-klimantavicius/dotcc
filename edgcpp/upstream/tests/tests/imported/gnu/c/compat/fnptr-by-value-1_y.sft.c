//type: fp
//options: 
# 0 "./compat/fnptr-by-value-1_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/fnptr-by-value-1_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/fnptr-by-value-1_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/fnptr-by-value-1_y.c" 2

typedef void (*fpi)(int);
typedef void (*fpd)(double);

extern int f1_val;
extern void checki (int, int);

void
test1a (fpi f)
{
  (*f)(1);
}

void
test1b (fpi f, int i)
{
  (*f)(i);
}

void
test1c (double x, fpd f)
{
  (*f)(x);
}

void
test2a (fpi f1, fpd f2)
{
  (*f1)(10);
  (*f2)(10.0);
}

void
test2b (fpi f1, fpd f2, int i)
{
  (*f1)(i);
  (*f2)((double)i);
}

void
test2c (fpi f1, int i, fpd f2)
{
  (*f1)(i);
  (*f2)((double)i);
}

void
test2d (int i, fpi f1, fpd f2)
{
  (*f1)(i);
  (*f2)((double)i);
}

void
test2e (fpi f1, fpd f2, int i, double x)
{
  (*f1)(i);
  (*f2)(x);
}

void
test2f (fpi f1, int i, fpd f2, double x)
{
  (*f1)(i);
  (*f2)(x);
}

void
test2g (fpi f1, int i, double x, fpd f2)
{
  (*f1)(i);
  (*f2)(x);
}

void
test2h (double x, fpd f1, fpi f2, int i)
{
  (*f1)(x);
  (*f2)(i);
}

void
test2i (double x, fpd f1, int i, fpi f2)
{
  (*f1)(x);
  (*f2)(i);
}

void
test2j (int i, double x, fpi f1, fpd f2)
{
  (*f1)(i);
  (*f2)(x);
}

void
testva (int n, ...)
{
  int i;
  va_list ap;
  
# 104 "./compat/fnptr-by-value-1_y.c" 3 4
 __builtin_c23_va_start(
# 104 "./compat/fnptr-by-value-1_y.c"
 ap, n
# 104 "./compat/fnptr-by-value-1_y.c" 3 4
 )
# 104 "./compat/fnptr-by-value-1_y.c"
                 ;
  for (i = 0; i < n; i++)
    {
      fpi fp = 
# 107 "./compat/fnptr-by-value-1_y.c" 3 4
              __builtin_va_arg(
# 107 "./compat/fnptr-by-value-1_y.c"
              ap
# 107 "./compat/fnptr-by-value-1_y.c" 3 4
              ,
# 107 "./compat/fnptr-by-value-1_y.c"
              fpi
# 107 "./compat/fnptr-by-value-1_y.c" 3 4
              )
# 107 "./compat/fnptr-by-value-1_y.c"
                              ;
      (*fp)(i);
      checki (f1_val, i);
    }
}
