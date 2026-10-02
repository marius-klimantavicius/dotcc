//type: fp
//options: 
# 0 "./tree-ssa/stdarg-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-7.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./tree-ssa/stdarg-7.c" 2


# 7 "./tree-ssa/stdarg-7.c"
void bar (int x, va_list *ap);

void
foo (int x, ...)
{
  va_list ap;
  int n;

  
# 15 "./tree-ssa/stdarg-7.c" 3 4
 __builtin_c23_va_start(
# 15 "./tree-ssa/stdarg-7.c"
 ap, x
# 15 "./tree-ssa/stdarg-7.c" 3 4
 )
# 15 "./tree-ssa/stdarg-7.c"
                 ;
  n = 
# 16 "./tree-ssa/stdarg-7.c" 3 4
     __builtin_va_arg(
# 16 "./tree-ssa/stdarg-7.c"
     ap
# 16 "./tree-ssa/stdarg-7.c" 3 4
     ,
# 16 "./tree-ssa/stdarg-7.c"
     int
# 16 "./tree-ssa/stdarg-7.c" 3 4
     )
# 16 "./tree-ssa/stdarg-7.c"
                     ;
  bar (x, (va_list *) ((n == 0) ? ((void *) 0) : &ap));
  
# 18 "./tree-ssa/stdarg-7.c" 3 4
 __builtin_va_end(
# 18 "./tree-ssa/stdarg-7.c"
 ap
# 18 "./tree-ssa/stdarg-7.c" 3 4
 )
# 18 "./tree-ssa/stdarg-7.c"
            ;
}
