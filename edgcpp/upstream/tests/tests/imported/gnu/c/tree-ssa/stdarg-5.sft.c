//type: fp
//options: 
# 0 "./tree-ssa/stdarg-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-5.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./tree-ssa/stdarg-5.c" 2


# 7 "./tree-ssa/stdarg-5.c"
extern void foo (int, va_list);
extern void bar (int);
struct S1 { int i; double d; int j; double e; } s1;
struct S2 { double d; long i; } s2;
int y;
_Complex int ci;
_Complex double cd;


void
f1 (int i, ...)
{
  va_list ap;
  
# 20 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_c23_va_start(
# 20 "./tree-ssa/stdarg-5.c"
 ap, i
# 20 "./tree-ssa/stdarg-5.c" 3 4
 )
# 20 "./tree-ssa/stdarg-5.c"
                 ;
  while (i-- > 0)
    s1 = 
# 22 "./tree-ssa/stdarg-5.c" 3 4
        __builtin_va_arg(
# 22 "./tree-ssa/stdarg-5.c"
        ap
# 22 "./tree-ssa/stdarg-5.c" 3 4
        ,
# 22 "./tree-ssa/stdarg-5.c"
        struct S1
# 22 "./tree-ssa/stdarg-5.c" 3 4
        )
# 22 "./tree-ssa/stdarg-5.c"
                              ;
  
# 23 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_va_end(
# 23 "./tree-ssa/stdarg-5.c"
 ap
# 23 "./tree-ssa/stdarg-5.c" 3 4
 )
# 23 "./tree-ssa/stdarg-5.c"
            ;
}





void
f2 (int i, ...)
{
  va_list ap;
  
# 34 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_c23_va_start(
# 34 "./tree-ssa/stdarg-5.c"
 ap, i
# 34 "./tree-ssa/stdarg-5.c" 3 4
 )
# 34 "./tree-ssa/stdarg-5.c"
                 ;
  while (i-- > 0)
    s2 = 
# 36 "./tree-ssa/stdarg-5.c" 3 4
        __builtin_va_arg(
# 36 "./tree-ssa/stdarg-5.c"
        ap
# 36 "./tree-ssa/stdarg-5.c" 3 4
        ,
# 36 "./tree-ssa/stdarg-5.c"
        struct S2
# 36 "./tree-ssa/stdarg-5.c" 3 4
        )
# 36 "./tree-ssa/stdarg-5.c"
                              ;
  
# 37 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_va_end(
# 37 "./tree-ssa/stdarg-5.c"
 ap
# 37 "./tree-ssa/stdarg-5.c" 3 4
 )
# 37 "./tree-ssa/stdarg-5.c"
            ;
}






void
f3 (int i, ...)
{
  va_list ap;
  int j = i;
  while (j-- > 0)
    {
      
# 52 "./tree-ssa/stdarg-5.c" 3 4
     __builtin_c23_va_start(
# 52 "./tree-ssa/stdarg-5.c"
     ap, i
# 52 "./tree-ssa/stdarg-5.c" 3 4
     )
# 52 "./tree-ssa/stdarg-5.c"
                     ;
      s1 = 
# 53 "./tree-ssa/stdarg-5.c" 3 4
          __builtin_va_arg(
# 53 "./tree-ssa/stdarg-5.c"
          ap
# 53 "./tree-ssa/stdarg-5.c" 3 4
          ,
# 53 "./tree-ssa/stdarg-5.c"
          struct S1
# 53 "./tree-ssa/stdarg-5.c" 3 4
          )
# 53 "./tree-ssa/stdarg-5.c"
                                ;
      
# 54 "./tree-ssa/stdarg-5.c" 3 4
     __builtin_va_end(
# 54 "./tree-ssa/stdarg-5.c"
     ap
# 54 "./tree-ssa/stdarg-5.c" 3 4
     )
# 54 "./tree-ssa/stdarg-5.c"
                ;
      bar (s1.i);
    }
}





void
f4 (int i, ...)
{
  va_list ap;
  int j = i;
  while (j-- > 0)
    {
      
# 70 "./tree-ssa/stdarg-5.c" 3 4
     __builtin_c23_va_start(
# 70 "./tree-ssa/stdarg-5.c"
     ap, i
# 70 "./tree-ssa/stdarg-5.c" 3 4
     )
# 70 "./tree-ssa/stdarg-5.c"
                     ;
      s2 = 
# 71 "./tree-ssa/stdarg-5.c" 3 4
          __builtin_va_arg(
# 71 "./tree-ssa/stdarg-5.c"
          ap
# 71 "./tree-ssa/stdarg-5.c" 3 4
          ,
# 71 "./tree-ssa/stdarg-5.c"
          struct S2
# 71 "./tree-ssa/stdarg-5.c" 3 4
          )
# 71 "./tree-ssa/stdarg-5.c"
                                ;
      y = 
# 72 "./tree-ssa/stdarg-5.c" 3 4
         __builtin_va_arg(
# 72 "./tree-ssa/stdarg-5.c"
         ap
# 72 "./tree-ssa/stdarg-5.c" 3 4
         ,
# 72 "./tree-ssa/stdarg-5.c"
         int
# 72 "./tree-ssa/stdarg-5.c" 3 4
         )
# 72 "./tree-ssa/stdarg-5.c"
                         ;
      
# 73 "./tree-ssa/stdarg-5.c" 3 4
     __builtin_va_end(
# 73 "./tree-ssa/stdarg-5.c"
     ap
# 73 "./tree-ssa/stdarg-5.c" 3 4
     )
# 73 "./tree-ssa/stdarg-5.c"
                ;
      bar (s2.i);
    }
}





void
f5 (int i, ...)
{
  va_list ap;
  
# 86 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_c23_va_start(
# 86 "./tree-ssa/stdarg-5.c"
 ap, i
# 86 "./tree-ssa/stdarg-5.c" 3 4
 )
# 86 "./tree-ssa/stdarg-5.c"
                 ;
  ci = 
# 87 "./tree-ssa/stdarg-5.c" 3 4
      __builtin_va_arg(
# 87 "./tree-ssa/stdarg-5.c"
      ap
# 87 "./tree-ssa/stdarg-5.c" 3 4
      ,
# 87 "./tree-ssa/stdarg-5.c"
      _Complex int
# 87 "./tree-ssa/stdarg-5.c" 3 4
      )
# 87 "./tree-ssa/stdarg-5.c"
                               ;
  ci += 
# 88 "./tree-ssa/stdarg-5.c" 3 4
       __builtin_va_arg(
# 88 "./tree-ssa/stdarg-5.c"
       ap
# 88 "./tree-ssa/stdarg-5.c" 3 4
       ,
# 88 "./tree-ssa/stdarg-5.c"
       _Complex int
# 88 "./tree-ssa/stdarg-5.c" 3 4
       )
# 88 "./tree-ssa/stdarg-5.c"
                                ;
  
# 89 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_va_end(
# 89 "./tree-ssa/stdarg-5.c"
 ap
# 89 "./tree-ssa/stdarg-5.c" 3 4
 )
# 89 "./tree-ssa/stdarg-5.c"
            ;
  bar (__real__ ci + __imag__ ci);
}





void
f6 (int i, ...)
{
  va_list ap;
  
# 101 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_c23_va_start(
# 101 "./tree-ssa/stdarg-5.c"
 ap, i
# 101 "./tree-ssa/stdarg-5.c" 3 4
 )
# 101 "./tree-ssa/stdarg-5.c"
                 ;
  ci = 
# 102 "./tree-ssa/stdarg-5.c" 3 4
      __builtin_va_arg(
# 102 "./tree-ssa/stdarg-5.c"
      ap
# 102 "./tree-ssa/stdarg-5.c" 3 4
      ,
# 102 "./tree-ssa/stdarg-5.c"
      _Complex int
# 102 "./tree-ssa/stdarg-5.c" 3 4
      )
# 102 "./tree-ssa/stdarg-5.c"
                               ;
  cd = 
# 103 "./tree-ssa/stdarg-5.c" 3 4
      __builtin_va_arg(
# 103 "./tree-ssa/stdarg-5.c"
      ap
# 103 "./tree-ssa/stdarg-5.c" 3 4
      ,
# 103 "./tree-ssa/stdarg-5.c"
      _Complex double
# 103 "./tree-ssa/stdarg-5.c" 3 4
      )
# 103 "./tree-ssa/stdarg-5.c"
                                  ;
  
# 104 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_va_end(
# 104 "./tree-ssa/stdarg-5.c"
 ap
# 104 "./tree-ssa/stdarg-5.c" 3 4
 )
# 104 "./tree-ssa/stdarg-5.c"
            ;
  bar (__real__ ci + __imag__ cd);
}





void
f7 (int i, ...)
{
  va_list ap;
  
# 116 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_c23_va_start(
# 116 "./tree-ssa/stdarg-5.c"
 ap, i
# 116 "./tree-ssa/stdarg-5.c" 3 4
 )
# 116 "./tree-ssa/stdarg-5.c"
                 ;
  cd = 
# 117 "./tree-ssa/stdarg-5.c" 3 4
      __builtin_va_arg(
# 117 "./tree-ssa/stdarg-5.c"
      ap
# 117 "./tree-ssa/stdarg-5.c" 3 4
      ,
# 117 "./tree-ssa/stdarg-5.c"
      _Complex double
# 117 "./tree-ssa/stdarg-5.c" 3 4
      )
# 117 "./tree-ssa/stdarg-5.c"
                                  ;
  cd += 
# 118 "./tree-ssa/stdarg-5.c" 3 4
       __builtin_va_arg(
# 118 "./tree-ssa/stdarg-5.c"
       ap
# 118 "./tree-ssa/stdarg-5.c" 3 4
       ,
# 118 "./tree-ssa/stdarg-5.c"
       _Complex double
# 118 "./tree-ssa/stdarg-5.c" 3 4
       )
# 118 "./tree-ssa/stdarg-5.c"
                                   ;
  
# 119 "./tree-ssa/stdarg-5.c" 3 4
 __builtin_va_end(
# 119 "./tree-ssa/stdarg-5.c"
 ap
# 119 "./tree-ssa/stdarg-5.c" 3 4
 )
# 119 "./tree-ssa/stdarg-5.c"
            ;
  bar (__real__ cd + __imag__ cd);
}
