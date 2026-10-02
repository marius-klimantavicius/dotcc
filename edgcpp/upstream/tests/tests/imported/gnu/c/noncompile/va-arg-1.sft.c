//type: fn
//options: 
# 0 "./noncompile/va-arg-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./noncompile/va-arg-1.c"


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./noncompile/va-arg-1.c" 2


# 5 "./noncompile/va-arg-1.c"
void
f (int x, ...)
{
  va_list args;
  
# 9 "./noncompile/va-arg-1.c" 3 4
 __builtin_c23_va_start(
# 9 "./noncompile/va-arg-1.c"
 args, bogus_variable
# 9 "./noncompile/va-arg-1.c" 3 4
 )
# 9 "./noncompile/va-arg-1.c"
                                ;
  
# 10 "./noncompile/va-arg-1.c" 3 4
 __builtin_va_end(
# 10 "./noncompile/va-arg-1.c"
 args
# 10 "./noncompile/va-arg-1.c" 3 4
 )
# 10 "./noncompile/va-arg-1.c"
              ;
}
