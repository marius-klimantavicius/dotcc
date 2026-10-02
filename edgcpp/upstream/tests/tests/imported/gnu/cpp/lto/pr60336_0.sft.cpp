//type: fp
//options: 
# 0 "./lto/pr60336_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr60336_0.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./lto/pr60336_0.C" 2


# 5 "./lto/pr60336_0.C"
struct dummy { };

void
test (struct dummy a, int m, ...)
{
  va_list va_arglist;
  int i;
  int count = 0;

  if (m == 0)
    count++;
  
# 16 "./lto/pr60336_0.C" 3 4
 __builtin_va_start(
# 16 "./lto/pr60336_0.C"
 va_arglist
# 16 "./lto/pr60336_0.C" 3 4
 ,
# 16 "./lto/pr60336_0.C"
 m
# 16 "./lto/pr60336_0.C" 3 4
 )
# 16 "./lto/pr60336_0.C"
                         ;
  i = 
# 17 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 17 "./lto/pr60336_0.C"
     va_arglist
# 17 "./lto/pr60336_0.C" 3 4
     ,
# 17 "./lto/pr60336_0.C"
     int
# 17 "./lto/pr60336_0.C" 3 4
     )
# 17 "./lto/pr60336_0.C"
                             ;
  if (i == 1)
    count++;
  i = 
# 20 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 20 "./lto/pr60336_0.C"
     va_arglist
# 20 "./lto/pr60336_0.C" 3 4
     ,
# 20 "./lto/pr60336_0.C"
     int
# 20 "./lto/pr60336_0.C" 3 4
     )
# 20 "./lto/pr60336_0.C"
                             ;
  if (i == 2)
  i = 
# 22 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 22 "./lto/pr60336_0.C"
     va_arglist
# 22 "./lto/pr60336_0.C" 3 4
     ,
# 22 "./lto/pr60336_0.C"
     int
# 22 "./lto/pr60336_0.C" 3 4
     )
# 22 "./lto/pr60336_0.C"
                             ;
    count++;
  if (i == 3)
    count++;
  i = 
# 26 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 26 "./lto/pr60336_0.C"
     va_arglist
# 26 "./lto/pr60336_0.C" 3 4
     ,
# 26 "./lto/pr60336_0.C"
     int
# 26 "./lto/pr60336_0.C" 3 4
     )
# 26 "./lto/pr60336_0.C"
                             ;
  if (i == 4)
    count++;
  i = 
# 29 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 29 "./lto/pr60336_0.C"
     va_arglist
# 29 "./lto/pr60336_0.C" 3 4
     ,
# 29 "./lto/pr60336_0.C"
     int
# 29 "./lto/pr60336_0.C" 3 4
     )
# 29 "./lto/pr60336_0.C"
                             ;
  if (i == 5)
    count++;
  i = 
# 32 "./lto/pr60336_0.C" 3 4
     __builtin_va_arg(
# 32 "./lto/pr60336_0.C"
     va_arglist
# 32 "./lto/pr60336_0.C" 3 4
     ,
# 32 "./lto/pr60336_0.C"
     int
# 32 "./lto/pr60336_0.C" 3 4
     )
# 32 "./lto/pr60336_0.C"
                             ;
  if (i == 6)
    count++;
  
# 35 "./lto/pr60336_0.C" 3 4
 __builtin_va_end(
# 35 "./lto/pr60336_0.C"
 va_arglist
# 35 "./lto/pr60336_0.C" 3 4
 )
# 35 "./lto/pr60336_0.C"
                    ;
  if (count != 7)
    __builtin_abort ();
}

struct dummy a0;

int
main ()
{
  test (a0, 0, 1, 2, 3, 4, 5, 6);
  return 0;
}
