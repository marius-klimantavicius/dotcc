//type: fp
//options: 
# 0 "./pr45461.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr45461.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./pr45461.c" 2


# 6 "./pr45461.c"
int
foo (int i, ...)
{
  short e;
  va_list ap;
  
# 11 "./pr45461.c" 3 4
 __builtin_c23_va_start(
# 11 "./pr45461.c"
 ap, i
# 11 "./pr45461.c" 3 4
 )
# 11 "./pr45461.c"
                 ;

  e = 
# 13 "./pr45461.c" 3 4
     __builtin_va_arg(
# 13 "./pr45461.c"
     ap
# 13 "./pr45461.c" 3 4
     ,
# 13 "./pr45461.c"
     short
# 13 "./pr45461.c" 3 4
     )
# 13 "./pr45461.c"
                       ;



  
# 17 "./pr45461.c" 3 4
 __builtin_va_end(
# 17 "./pr45461.c"
 ap
# 17 "./pr45461.c" 3 4
 )
# 17 "./pr45461.c"
            ;
  return e;
}
