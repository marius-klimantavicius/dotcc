//type: fp
//options: 
# 0 "./Wcxx-compat-11.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wcxx-compat-11.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./Wcxx-compat-11.c" 2


# 6 "./Wcxx-compat-11.c"
enum E { A };

extern void f2 (int);
void
f1 (int n, ...)
{
  va_list ap;

  
# 14 "./Wcxx-compat-11.c" 3 4
 __builtin_c23_va_start(
# 14 "./Wcxx-compat-11.c"
 ap, n
# 14 "./Wcxx-compat-11.c" 3 4
 )
# 14 "./Wcxx-compat-11.c"
                 ;
  f2 (
# 15 "./Wcxx-compat-11.c" 3 4
     __builtin_va_arg(
# 15 "./Wcxx-compat-11.c"
     ap
# 15 "./Wcxx-compat-11.c" 3 4
     ,
# 15 "./Wcxx-compat-11.c"
     int
# 15 "./Wcxx-compat-11.c" 3 4
     )
# 15 "./Wcxx-compat-11.c"
                     );
  f2 (
# 16 "./Wcxx-compat-11.c" 3 4
     __builtin_va_arg(
# 16 "./Wcxx-compat-11.c"
     ap
# 16 "./Wcxx-compat-11.c" 3 4
     ,
# 16 "./Wcxx-compat-11.c"
     _Bool
# 16 "./Wcxx-compat-11.c" 3 4
     )
# 16 "./Wcxx-compat-11.c"
                       );
  f2 (
# 17 "./Wcxx-compat-11.c" 3 4
     __builtin_va_arg(
# 17 "./Wcxx-compat-11.c"
     ap
# 17 "./Wcxx-compat-11.c" 3 4
     ,
# 17 "./Wcxx-compat-11.c"
     enum E
# 17 "./Wcxx-compat-11.c" 3 4
     )
# 17 "./Wcxx-compat-11.c"
                        );
}
