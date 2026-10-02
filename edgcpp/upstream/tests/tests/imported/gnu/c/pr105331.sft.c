//type: fp
//options: 
# 0 "./pr105331.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr105331.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr105331.c" 2


# 7 "./pr105331.c"
int
foo (va_list *va)
{
  return 
# 10 "./pr105331.c" 3 4
        __builtin_va_arg(
# 10 "./pr105331.c"
        *va
# 10 "./pr105331.c" 3 4
        ,
# 10 "./pr105331.c"
        double _Complex
# 10 "./pr105331.c" 3 4
        )
# 10 "./pr105331.c"
                                     ;
}
