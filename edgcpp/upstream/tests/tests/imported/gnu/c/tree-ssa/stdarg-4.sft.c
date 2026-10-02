//type: fp
//options: 
# 0 "./tree-ssa/stdarg-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-4.c"







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 9 "./tree-ssa/stdarg-4.c" 2


# 10 "./tree-ssa/stdarg-4.c"
extern void foo (int, va_list);
extern void bar (int);
long x;
double d;



void
f1 (int i, ...)
{
  va_list ap;
  
# 21 "./tree-ssa/stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 21 "./tree-ssa/stdarg-4.c"
 ap, i
# 21 "./tree-ssa/stdarg-4.c" 3 4
 )
# 21 "./tree-ssa/stdarg-4.c"
                 ;
  while (i-- > 0)
    x = 
# 23 "./tree-ssa/stdarg-4.c" 3 4
       __builtin_va_arg(
# 23 "./tree-ssa/stdarg-4.c"
       ap
# 23 "./tree-ssa/stdarg-4.c" 3 4
       ,
# 23 "./tree-ssa/stdarg-4.c"
       long
# 23 "./tree-ssa/stdarg-4.c" 3 4
       )
# 23 "./tree-ssa/stdarg-4.c"
                        ;
  
# 24 "./tree-ssa/stdarg-4.c" 3 4
 __builtin_va_end(
# 24 "./tree-ssa/stdarg-4.c"
 ap
# 24 "./tree-ssa/stdarg-4.c" 3 4
 )
# 24 "./tree-ssa/stdarg-4.c"
            ;
}
# 35 "./tree-ssa/stdarg-4.c"
void
f2 (int i, ...)
{
  va_list ap;
  
# 39 "./tree-ssa/stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 39 "./tree-ssa/stdarg-4.c"
 ap, i
# 39 "./tree-ssa/stdarg-4.c" 3 4
 )
# 39 "./tree-ssa/stdarg-4.c"
                 ;
  while (i-- > 0)
    d = 
# 41 "./tree-ssa/stdarg-4.c" 3 4
       __builtin_va_arg(
# 41 "./tree-ssa/stdarg-4.c"
       ap
# 41 "./tree-ssa/stdarg-4.c" 3 4
       ,
# 41 "./tree-ssa/stdarg-4.c"
       double
# 41 "./tree-ssa/stdarg-4.c" 3 4
       )
# 41 "./tree-ssa/stdarg-4.c"
                          ;
  
# 42 "./tree-ssa/stdarg-4.c" 3 4
 __builtin_va_end(
# 42 "./tree-ssa/stdarg-4.c"
 ap
# 42 "./tree-ssa/stdarg-4.c" 3 4
 )
# 42 "./tree-ssa/stdarg-4.c"
            ;
}
# 55 "./tree-ssa/stdarg-4.c"
void
f3 (int i, ...)
{
  va_list ap;
  int j = i;
  while (j-- > 0)
    {
      
# 62 "./tree-ssa/stdarg-4.c" 3 4
     __builtin_c23_va_start(
# 62 "./tree-ssa/stdarg-4.c"
     ap, i
# 62 "./tree-ssa/stdarg-4.c" 3 4
     )
# 62 "./tree-ssa/stdarg-4.c"
                     ;
      x = 
# 63 "./tree-ssa/stdarg-4.c" 3 4
         __builtin_va_arg(
# 63 "./tree-ssa/stdarg-4.c"
         ap
# 63 "./tree-ssa/stdarg-4.c" 3 4
         ,
# 63 "./tree-ssa/stdarg-4.c"
         long
# 63 "./tree-ssa/stdarg-4.c" 3 4
         )
# 63 "./tree-ssa/stdarg-4.c"
                          ;
      
# 64 "./tree-ssa/stdarg-4.c" 3 4
     __builtin_va_end(
# 64 "./tree-ssa/stdarg-4.c"
     ap
# 64 "./tree-ssa/stdarg-4.c" 3 4
     )
# 64 "./tree-ssa/stdarg-4.c"
                ;
      bar (x);
    }
}
# 77 "./tree-ssa/stdarg-4.c"
void
f4 (int i, ...)
{
  va_list ap;
  int j = i;
  while (j-- > 0)
    {
      
# 84 "./tree-ssa/stdarg-4.c" 3 4
     __builtin_c23_va_start(
# 84 "./tree-ssa/stdarg-4.c"
     ap, i
# 84 "./tree-ssa/stdarg-4.c" 3 4
     )
# 84 "./tree-ssa/stdarg-4.c"
                     ;
      d = 
# 85 "./tree-ssa/stdarg-4.c" 3 4
         __builtin_va_arg(
# 85 "./tree-ssa/stdarg-4.c"
         ap
# 85 "./tree-ssa/stdarg-4.c" 3 4
         ,
# 85 "./tree-ssa/stdarg-4.c"
         double
# 85 "./tree-ssa/stdarg-4.c" 3 4
         )
# 85 "./tree-ssa/stdarg-4.c"
                            ;
      
# 86 "./tree-ssa/stdarg-4.c" 3 4
     __builtin_va_end(
# 86 "./tree-ssa/stdarg-4.c"
     ap
# 86 "./tree-ssa/stdarg-4.c" 3 4
     )
# 86 "./tree-ssa/stdarg-4.c"
                ;
      bar (d + 2.5);
    }
}
