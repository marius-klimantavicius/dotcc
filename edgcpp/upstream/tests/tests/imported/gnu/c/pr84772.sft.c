//type: fp
//options: 
# 0 "./pr84772.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr84772.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr84772.c" 2


# 7 "./pr84772.c"
void
foo (int *x, int y, va_list ap)
{
  __builtin_memset (x, 0, sizeof (int));
  for (int i = 0; i < y; i++)
    
# 12 "./pr84772.c" 3 4
   __builtin_va_arg(
# 12 "./pr84772.c"
   ap
# 12 "./pr84772.c" 3 4
   ,
# 12 "./pr84772.c"
   long double
# 12 "./pr84772.c" 3 4
   )
# 12 "./pr84772.c"
                           ;
}
