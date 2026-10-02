//type: rp
//options: --c99 --strict_gnu
# 0 "./vla-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vla-14.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./vla-14.c" 2


# 8 "./vla-14.c"
extern void exit (int);
extern void abort (void);

int a[10];
int i = 9;

void
f (int n, ...)
{
  va_list ap;
  void *p;
  
# 19 "./vla-14.c" 3 4
 __builtin_va_start(
# 19 "./vla-14.c"
 ap
# 19 "./vla-14.c" 3 4
 ,
# 19 "./vla-14.c"
 n
# 19 "./vla-14.c" 3 4
 )
# 19 "./vla-14.c"
                 ;
  p = 
# 20 "./vla-14.c" 3 4
     __builtin_va_arg(
# 20 "./vla-14.c"
     ap
# 20 "./vla-14.c" 3 4
     ,
# 20 "./vla-14.c"
     typeof (int (*)[++i])
# 20 "./vla-14.c" 3 4
     )
# 20 "./vla-14.c"
                                       ;
  if (p != a)
    abort ();
  if (i != n)
    abort ();
  
# 25 "./vla-14.c" 3 4
 __builtin_va_end(
# 25 "./vla-14.c"
 ap
# 25 "./vla-14.c" 3 4
 )
# 25 "./vla-14.c"
            ;
}

int
main (void)
{
  f (10, &a);
  exit (0);
}
