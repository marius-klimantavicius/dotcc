//type: fp
//options: 
# 0 "./tree-ssa/stdarg-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-3.c"







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 9 "./tree-ssa/stdarg-3.c" 2


# 10 "./tree-ssa/stdarg-3.c"
extern void foo (int, va_list);
extern void bar (int);
long x;
va_list gap;


void
f1 (int i, ...)
{
  
# 19 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 19 "./tree-ssa/stdarg-3.c"
 gap, i
# 19 "./tree-ssa/stdarg-3.c" 3 4
 )
# 19 "./tree-ssa/stdarg-3.c"
                  ;
  x = 
# 20 "./tree-ssa/stdarg-3.c" 3 4
     __builtin_va_arg(
# 20 "./tree-ssa/stdarg-3.c"
     gap
# 20 "./tree-ssa/stdarg-3.c" 3 4
     ,
# 20 "./tree-ssa/stdarg-3.c"
     long
# 20 "./tree-ssa/stdarg-3.c" 3 4
     )
# 20 "./tree-ssa/stdarg-3.c"
                       ;
  
# 21 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 21 "./tree-ssa/stdarg-3.c"
 gap
# 21 "./tree-ssa/stdarg-3.c" 3 4
 )
# 21 "./tree-ssa/stdarg-3.c"
             ;
}
# 32 "./tree-ssa/stdarg-3.c"
void
f2 (int i, ...)
{
  
# 35 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 35 "./tree-ssa/stdarg-3.c"
 gap, i
# 35 "./tree-ssa/stdarg-3.c" 3 4
 )
# 35 "./tree-ssa/stdarg-3.c"
                  ;
  bar (i);
  
# 37 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 37 "./tree-ssa/stdarg-3.c"
 gap
# 37 "./tree-ssa/stdarg-3.c" 3 4
 )
# 37 "./tree-ssa/stdarg-3.c"
             ;
}
# 50 "./tree-ssa/stdarg-3.c"
void
f3 (int i, ...)
{
  va_list aps[10];
  
# 54 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 54 "./tree-ssa/stdarg-3.c"
 aps[4], i
# 54 "./tree-ssa/stdarg-3.c" 3 4
 )
# 54 "./tree-ssa/stdarg-3.c"
                     ;
  x = 
# 55 "./tree-ssa/stdarg-3.c" 3 4
     __builtin_va_arg(
# 55 "./tree-ssa/stdarg-3.c"
     aps[4]
# 55 "./tree-ssa/stdarg-3.c" 3 4
     ,
# 55 "./tree-ssa/stdarg-3.c"
     long
# 55 "./tree-ssa/stdarg-3.c" 3 4
     )
# 55 "./tree-ssa/stdarg-3.c"
                          ;
  
# 56 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 56 "./tree-ssa/stdarg-3.c"
 aps[4]
# 56 "./tree-ssa/stdarg-3.c" 3 4
 )
# 56 "./tree-ssa/stdarg-3.c"
                ;
}
# 67 "./tree-ssa/stdarg-3.c"
void
f4 (int i, ...)
{
  va_list aps[10];
  
# 71 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 71 "./tree-ssa/stdarg-3.c"
 aps[4], i
# 71 "./tree-ssa/stdarg-3.c" 3 4
 )
# 71 "./tree-ssa/stdarg-3.c"
                     ;
  bar (i);
  
# 73 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 73 "./tree-ssa/stdarg-3.c"
 aps[4]
# 73 "./tree-ssa/stdarg-3.c" 3 4
 )
# 73 "./tree-ssa/stdarg-3.c"
                ;
}
# 84 "./tree-ssa/stdarg-3.c"
void
f5 (int i, ...)
{
  va_list aps[10];
  
# 88 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 88 "./tree-ssa/stdarg-3.c"
 aps[4], i
# 88 "./tree-ssa/stdarg-3.c" 3 4
 )
# 88 "./tree-ssa/stdarg-3.c"
                     ;
  foo (i, aps[4]);
  
# 90 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 90 "./tree-ssa/stdarg-3.c"
 aps[4]
# 90 "./tree-ssa/stdarg-3.c" 3 4
 )
# 90 "./tree-ssa/stdarg-3.c"
                ;
}
# 101 "./tree-ssa/stdarg-3.c"
struct A { int i; va_list g; va_list h[2]; };

void
f6 (int i, ...)
{
  struct A a;
  
# 107 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 107 "./tree-ssa/stdarg-3.c"
 a.g, i
# 107 "./tree-ssa/stdarg-3.c" 3 4
 )
# 107 "./tree-ssa/stdarg-3.c"
                  ;
  x = 
# 108 "./tree-ssa/stdarg-3.c" 3 4
     __builtin_va_arg(
# 108 "./tree-ssa/stdarg-3.c"
     a.g
# 108 "./tree-ssa/stdarg-3.c" 3 4
     ,
# 108 "./tree-ssa/stdarg-3.c"
     long
# 108 "./tree-ssa/stdarg-3.c" 3 4
     )
# 108 "./tree-ssa/stdarg-3.c"
                       ;
  
# 109 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 109 "./tree-ssa/stdarg-3.c"
 a.g
# 109 "./tree-ssa/stdarg-3.c" 3 4
 )
# 109 "./tree-ssa/stdarg-3.c"
             ;
}
# 120 "./tree-ssa/stdarg-3.c"
void
f7 (int i, ...)
{
  struct A a;
  
# 124 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 124 "./tree-ssa/stdarg-3.c"
 a.g, i
# 124 "./tree-ssa/stdarg-3.c" 3 4
 )
# 124 "./tree-ssa/stdarg-3.c"
                  ;
  bar (i);
  
# 126 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 126 "./tree-ssa/stdarg-3.c"
 a.g
# 126 "./tree-ssa/stdarg-3.c" 3 4
 )
# 126 "./tree-ssa/stdarg-3.c"
             ;
}
# 137 "./tree-ssa/stdarg-3.c"
void
f8 (int i, ...)
{
  struct A a;
  
# 141 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 141 "./tree-ssa/stdarg-3.c"
 a.g, i
# 141 "./tree-ssa/stdarg-3.c" 3 4
 )
# 141 "./tree-ssa/stdarg-3.c"
                  ;
  foo (i, a.g);
  
# 143 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 143 "./tree-ssa/stdarg-3.c"
 a.g
# 143 "./tree-ssa/stdarg-3.c" 3 4
 )
# 143 "./tree-ssa/stdarg-3.c"
             ;
}
# 154 "./tree-ssa/stdarg-3.c"
void
f10 (int i, ...)
{
  struct A a;
  
# 158 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 158 "./tree-ssa/stdarg-3.c"
 a.h[1], i
# 158 "./tree-ssa/stdarg-3.c" 3 4
 )
# 158 "./tree-ssa/stdarg-3.c"
                     ;
  x = 
# 159 "./tree-ssa/stdarg-3.c" 3 4
     __builtin_va_arg(
# 159 "./tree-ssa/stdarg-3.c"
     a.h[1]
# 159 "./tree-ssa/stdarg-3.c" 3 4
     ,
# 159 "./tree-ssa/stdarg-3.c"
     long
# 159 "./tree-ssa/stdarg-3.c" 3 4
     )
# 159 "./tree-ssa/stdarg-3.c"
                          ;
  
# 160 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 160 "./tree-ssa/stdarg-3.c"
 a.h[1]
# 160 "./tree-ssa/stdarg-3.c" 3 4
 )
# 160 "./tree-ssa/stdarg-3.c"
                ;
}
# 171 "./tree-ssa/stdarg-3.c"
void
f11 (int i, ...)
{
  struct A a;
  
# 175 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 175 "./tree-ssa/stdarg-3.c"
 a.h[1], i
# 175 "./tree-ssa/stdarg-3.c" 3 4
 )
# 175 "./tree-ssa/stdarg-3.c"
                     ;
  bar (i);
  
# 177 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 177 "./tree-ssa/stdarg-3.c"
 a.h[1]
# 177 "./tree-ssa/stdarg-3.c" 3 4
 )
# 177 "./tree-ssa/stdarg-3.c"
                ;
}
# 188 "./tree-ssa/stdarg-3.c"
void
f12 (int i, ...)
{
  struct A a;
  
# 192 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_c23_va_start(
# 192 "./tree-ssa/stdarg-3.c"
 a.h[1], i
# 192 "./tree-ssa/stdarg-3.c" 3 4
 )
# 192 "./tree-ssa/stdarg-3.c"
                     ;
  foo (i, a.h[1]);
  
# 194 "./tree-ssa/stdarg-3.c" 3 4
 __builtin_va_end(
# 194 "./tree-ssa/stdarg-3.c"
 a.h[1]
# 194 "./tree-ssa/stdarg-3.c" 3 4
 )
# 194 "./tree-ssa/stdarg-3.c"
                ;
}
