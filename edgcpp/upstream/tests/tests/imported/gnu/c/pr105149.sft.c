//type: fn
//options: 
# 0 "./pr105149.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr105149.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./pr105149.c" 2


# 7 "./pr105149.c"
void
foo (int s, ...)
{
  int e;
  va_list ap;

  
# 13 "./pr105149.c" 3 4
 __builtin_c23_va_start(
# 13 "./pr105149.c"
 ap, s
# 13 "./pr105149.c" 3 4
 )
# 13 "./pr105149.c"
                 ;
  e = 
# 14 "./pr105149.c" 3 4
     __builtin_va_arg(
# 14 "./pr105149.c"
     ap
# 14 "./pr105149.c" 3 4
     ,
# 14 "./pr105149.c"
     int (void)
# 14 "./pr105149.c" 3 4
     ) 
# 14 "./pr105149.c"
                             ();
  
# 15 "./pr105149.c" 3 4
 __builtin_va_end(
# 15 "./pr105149.c"
 ap
# 15 "./pr105149.c" 3 4
 )
# 15 "./pr105149.c"
            ;
}
