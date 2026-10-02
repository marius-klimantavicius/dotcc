//type: fn
//options: 
# 0 "./warn/var-args1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/var-args1.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./warn/var-args1.C" 2


# 5 "./warn/var-args1.C"
void foo(int, ...)
{
    va_list va;
    int i;
    i = 
# 9 "./warn/var-args1.C" 3 4
       __builtin_va_arg(
# 9 "./warn/var-args1.C"
       va
# 9 "./warn/var-args1.C" 3 4
       ,
# 9 "./warn/var-args1.C"
       int&
# 9 "./warn/var-args1.C" 3 4
       )
# 9 "./warn/var-args1.C"
                       ;
}
