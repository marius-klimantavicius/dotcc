//type: fp
//options: --c23
# 0 "./c23-stdarg-10.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-stdarg-10.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./c23-stdarg-10.c" 2


# 7 "./c23-stdarg-10.c"
int i;

void
f0 (...)
{
  va_list ap;
  
# 13 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 13 "./c23-stdarg-10.c"
 ap
# 13 "./c23-stdarg-10.c" 3 4
 )
# 13 "./c23-stdarg-10.c"
              ;
  
# 14 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 14 "./c23-stdarg-10.c"
 ap
# 14 "./c23-stdarg-10.c" 3 4
 )
# 14 "./c23-stdarg-10.c"
            ;
}

void
f1 (...)
{
  va_list ap;
  
# 21 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 21 "./c23-stdarg-10.c"
 ap, i
# 21 "./c23-stdarg-10.c" 3 4
 )
# 21 "./c23-stdarg-10.c"
                 ;
  
# 22 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 22 "./c23-stdarg-10.c"
 ap
# 22 "./c23-stdarg-10.c" 3 4
 )
# 22 "./c23-stdarg-10.c"
            ;
}

void
f2 (...)
{
  int j = 0;
  va_list ap;
  
# 30 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 30 "./c23-stdarg-10.c"
 ap, j
# 30 "./c23-stdarg-10.c" 3 4
 )
# 30 "./c23-stdarg-10.c"
                 ;
  
# 31 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 31 "./c23-stdarg-10.c"
 ap
# 31 "./c23-stdarg-10.c" 3 4
 )
# 31 "./c23-stdarg-10.c"
            ;
}

void
f3 (int k, int l, ...)
{
  va_list ap;
  
# 38 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 38 "./c23-stdarg-10.c"
 ap, k
# 38 "./c23-stdarg-10.c" 3 4
 )
# 38 "./c23-stdarg-10.c"
                 ;
  
# 39 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 39 "./c23-stdarg-10.c"
 ap
# 39 "./c23-stdarg-10.c" 3 4
 )
# 39 "./c23-stdarg-10.c"
            ;
}

void
f4 (int k, int l, ...)
{
  va_list ap;
  
# 46 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 46 "./c23-stdarg-10.c"
 ap, l
# 46 "./c23-stdarg-10.c" 3 4
 )
# 46 "./c23-stdarg-10.c"
                 ;
  
# 47 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 47 "./c23-stdarg-10.c"
 ap
# 47 "./c23-stdarg-10.c" 3 4
 )
# 47 "./c23-stdarg-10.c"
            ;
}

void
f5 (int k, int l, ...)
{
  va_list ap;
  
# 54 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 54 "./c23-stdarg-10.c"
 ap, (int) l
# 54 "./c23-stdarg-10.c" 3 4
 )
# 54 "./c23-stdarg-10.c"
                       ;
  
# 55 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 55 "./c23-stdarg-10.c"
 ap
# 55 "./c23-stdarg-10.c" 3 4
 )
# 55 "./c23-stdarg-10.c"
            ;
}

void
f6 (int k, int l, ...)
{
  va_list ap;
  
# 62 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 62 "./c23-stdarg-10.c"
 ap, l + 0
# 62 "./c23-stdarg-10.c" 3 4
 )
# 62 "./c23-stdarg-10.c"
                     ;
  
# 63 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 63 "./c23-stdarg-10.c"
 ap
# 63 "./c23-stdarg-10.c" 3 4
 )
# 63 "./c23-stdarg-10.c"
            ;
}

void
f7 (int k, int l, ...)
{
  va_list ap;
  
# 70 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 70 "./c23-stdarg-10.c"
 ap, ()()(), [][][], {}{}{}, *+-/1({[_*_]})%&&!?!?
# 70 "./c23-stdarg-10.c" 3 4
 )
# 70 "./c23-stdarg-10.c"
                                                             ;
  
# 71 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 71 "./c23-stdarg-10.c"
 ap
# 71 "./c23-stdarg-10.c" 3 4
 )
# 71 "./c23-stdarg-10.c"
            ;
}

void
f8 (...)
{
  va_list ap;
  
# 78 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 78 "./c23-stdarg-10.c"
 ap,
# 78 "./c23-stdarg-10.c" 3 4
 )
# 78 "./c23-stdarg-10.c"
               ;
  
# 79 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 79 "./c23-stdarg-10.c"
 ap
# 79 "./c23-stdarg-10.c" 3 4
 )
# 79 "./c23-stdarg-10.c"
            ;
}

void
f9 (int k, int l, ...)
{
  va_list ap;
  
# 86 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 86 "./c23-stdarg-10.c"
 ap, k+l+****2
# 86 "./c23-stdarg-10.c" 3 4
 )
# 86 "./c23-stdarg-10.c"
                         ;
  
# 87 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 87 "./c23-stdarg-10.c"
 ap
# 87 "./c23-stdarg-10.c" 3 4
 )
# 87 "./c23-stdarg-10.c"
            ;
}

void
f10 (register int m, ...)
{
  va_list ap;
  
# 94 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 94 "./c23-stdarg-10.c"
 ap, m
# 94 "./c23-stdarg-10.c" 3 4
 )
# 94 "./c23-stdarg-10.c"
                 ;
  
# 95 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 95 "./c23-stdarg-10.c"
 ap
# 95 "./c23-stdarg-10.c" 3 4
 )
# 95 "./c23-stdarg-10.c"
            ;
}

void
f11 (int k, int l, ...)
{
  va_list ap;
  
# 102 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 102 "./c23-stdarg-10.c"
 ap, ()()()[[[}}}
# 102 "./c23-stdarg-10.c" 3 4
 )
# 102 "./c23-stdarg-10.c"
                            ;
  
# 103 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 103 "./c23-stdarg-10.c"
 ap
# 103 "./c23-stdarg-10.c" 3 4
 )
# 103 "./c23-stdarg-10.c"
            ;
}

void
f12 (int k, int l, ...)
{
  va_list ap;
  
# 110 "./c23-stdarg-10.c" 3 4
 __builtin_c23_va_start(
# 110 "./c23-stdarg-10.c"
 ap, ]]]]]]{{{{{{
# 110 "./c23-stdarg-10.c" 3 4
 )
# 110 "./c23-stdarg-10.c"
                            ;
  
# 111 "./c23-stdarg-10.c" 3 4
 __builtin_va_end(
# 111 "./c23-stdarg-10.c"
 ap
# 111 "./c23-stdarg-10.c" 3 4
 )
# 111 "./c23-stdarg-10.c"
            ;
}
