//type: fp
//options: 
# 0 "./expr/stdarg3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./expr/stdarg3.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./expr/stdarg3.C" 2


# 6 "./expr/stdarg3.C"
struct A
{
  A (const char *f, ...);
};

A::A (const char *f, ...)
{
  va_list ap;
  
# 14 "./expr/stdarg3.C" 3 4
 __builtin_va_start(
# 14 "./expr/stdarg3.C"
 ap
# 14 "./expr/stdarg3.C" 3 4
 ,
# 14 "./expr/stdarg3.C"
 f
# 14 "./expr/stdarg3.C" 3 4
 )
# 14 "./expr/stdarg3.C"
                 ;
  int i = 
# 15 "./expr/stdarg3.C" 3 4
         __builtin_va_arg(
# 15 "./expr/stdarg3.C"
         ap
# 15 "./expr/stdarg3.C" 3 4
         ,
# 15 "./expr/stdarg3.C"
         int
# 15 "./expr/stdarg3.C" 3 4
         )
# 15 "./expr/stdarg3.C"
                         ;
  int j = 
# 16 "./expr/stdarg3.C" 3 4
         __builtin_va_arg(
# 16 "./expr/stdarg3.C"
         (ap)
# 16 "./expr/stdarg3.C" 3 4
         ,
# 16 "./expr/stdarg3.C"
         int
# 16 "./expr/stdarg3.C" 3 4
         )
# 16 "./expr/stdarg3.C"
                           ;
  
# 17 "./expr/stdarg3.C" 3 4
 __builtin_va_end(
# 17 "./expr/stdarg3.C"
 ap
# 17 "./expr/stdarg3.C" 3 4
 )
# 17 "./expr/stdarg3.C"
            ;
}
