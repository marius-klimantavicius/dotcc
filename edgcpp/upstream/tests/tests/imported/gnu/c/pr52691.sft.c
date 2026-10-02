//type: fp
//options: 
# 0 "./pr52691.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr52691.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr52691.c" 2


# 7 "./pr52691.c"
int
foo (int a, ...)
{
  int b = 0, c = 0;
  va_list ap;
  
# 12 "./pr52691.c" 3 4
 __builtin_c23_va_start(
# 12 "./pr52691.c"
 ap, a
# 12 "./pr52691.c" 3 4
 )
# 12 "./pr52691.c"
                 ;
  if (a > 1)
    b = 
# 14 "./pr52691.c" 3 4
       __builtin_va_arg(
# 14 "./pr52691.c"
       ap
# 14 "./pr52691.c" 3 4
       ,
# 14 "./pr52691.c"
       double
# 14 "./pr52691.c" 3 4
       )
# 14 "./pr52691.c"
                          ;
  if (a > 2)
    c = 
# 16 "./pr52691.c" 3 4
       __builtin_va_arg(
# 16 "./pr52691.c"
       ap
# 16 "./pr52691.c" 3 4
       ,
# 16 "./pr52691.c"
       long long
# 16 "./pr52691.c" 3 4
       )
# 16 "./pr52691.c"
                             ;
  
# 17 "./pr52691.c" 3 4
 __builtin_va_end(
# 17 "./pr52691.c"
 ap
# 17 "./pr52691.c" 3 4
 )
# 17 "./pr52691.c"
            ;
  return a + b + c;
}
