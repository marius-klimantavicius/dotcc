//type: fp
//options: 
# 0 "./expr/stdarg1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./expr/stdarg1.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./expr/stdarg1.C" 2

# 4 "./expr/stdarg1.C"
struct S
{
  int f(int);
};
void f(int i, ...)
{
  va_list ap;
  
# 11 "./expr/stdarg1.C" 3 4
 __builtin_va_start(
# 11 "./expr/stdarg1.C"
 ap
# 11 "./expr/stdarg1.C" 3 4
 ,
# 11 "./expr/stdarg1.C"
 i
# 11 "./expr/stdarg1.C" 3 4
 )
# 11 "./expr/stdarg1.C"
                 ;
  
# 12 "./expr/stdarg1.C" 3 4
 __builtin_va_arg(
# 12 "./expr/stdarg1.C"
 ap
# 12 "./expr/stdarg1.C" 3 4
 ,
# 12 "./expr/stdarg1.C"
 S
# 12 "./expr/stdarg1.C" 3 4
 )
# 12 "./expr/stdarg1.C"
               .f(0);
}
