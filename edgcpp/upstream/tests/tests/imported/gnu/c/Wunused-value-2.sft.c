//type: fp
//options: 
# 0 "./Wunused-value-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wunused-value-2.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./Wunused-value-2.c" 2


# 8 "./Wunused-value-2.c"
int f(int t, ...)
{
  va_list a;
  
# 11 "./Wunused-value-2.c" 3 4
 __builtin_c23_va_start(
# 11 "./Wunused-value-2.c"
 a, t
# 11 "./Wunused-value-2.c" 3 4
 )
# 11 "./Wunused-value-2.c"
                ;
  
# 12 "./Wunused-value-2.c" 3 4
 __builtin_va_arg(
# 12 "./Wunused-value-2.c"
 a
# 12 "./Wunused-value-2.c" 3 4
 ,
# 12 "./Wunused-value-2.c"
 int
# 12 "./Wunused-value-2.c" 3 4
 )
# 12 "./Wunused-value-2.c"
               ;
  int t1 = 
# 13 "./Wunused-value-2.c" 3 4
          __builtin_va_arg(
# 13 "./Wunused-value-2.c"
          a
# 13 "./Wunused-value-2.c" 3 4
          ,
# 13 "./Wunused-value-2.c"
          int
# 13 "./Wunused-value-2.c" 3 4
          )
# 13 "./Wunused-value-2.c"
                        ;
  
# 14 "./Wunused-value-2.c" 3 4
 __builtin_va_end(
# 14 "./Wunused-value-2.c"
 a
# 14 "./Wunused-value-2.c" 3 4
 )
# 14 "./Wunused-value-2.c"
          ;
  return t1;
}
