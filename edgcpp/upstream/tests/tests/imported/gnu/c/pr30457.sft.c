//type: fp
//options: --c99
# 0 "./pr30457.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr30457.c"
# 11 "./pr30457.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 12 "./pr30457.c" 2


# 13 "./pr30457.c"
void foo(register short paramN, ...)
{
  va_list ap;

  
# 17 "./pr30457.c" 3 4
 __builtin_va_start(
# 17 "./pr30457.c"
 ap
# 17 "./pr30457.c" 3 4
 ,
# 17 "./pr30457.c"
 paramN
# 17 "./pr30457.c" 3 4
 )
# 17 "./pr30457.c"
                     ;


  (void) 
# 20 "./pr30457.c" 3 4
        __builtin_va_arg(
# 20 "./pr30457.c"
        ap
# 20 "./pr30457.c" 3 4
        ,
# 20 "./pr30457.c"
        char
# 20 "./pr30457.c" 3 4
        )
# 20 "./pr30457.c"
                        ;



  
# 24 "./pr30457.c" 3 4
 __builtin_va_end(
# 24 "./pr30457.c"
 ap
# 24 "./pr30457.c" 3 4
 )
# 24 "./pr30457.c"
           ;
}
