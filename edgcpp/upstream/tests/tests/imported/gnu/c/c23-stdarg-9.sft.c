//type: rp
//options: --c23
# 0 "./c23-stdarg-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-stdarg-9.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./c23-stdarg-9.c" 2






# 12 "./c23-stdarg-9.c"
struct S { int a[1024]; };


int
f1 (...)
{
  int r = 0;
  va_list ap;
  
# 20 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 20 "./c23-stdarg-9.c"
 ap
# 20 "./c23-stdarg-9.c" 3 4
 )
# 20 "./c23-stdarg-9.c"
              ;
  r += 
# 21 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 21 "./c23-stdarg-9.c"
      ap
# 21 "./c23-stdarg-9.c" 3 4
      ,
# 21 "./c23-stdarg-9.c"
      int
# 21 "./c23-stdarg-9.c" 3 4
      )
# 21 "./c23-stdarg-9.c"
                      ;
  
# 22 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 22 "./c23-stdarg-9.c"
 ap
# 22 "./c23-stdarg-9.c" 3 4
 )
# 22 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f2 (...)
{
  int r = 0;
  va_list ap;
  
# 31 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 31 "./c23-stdarg-9.c"
 ap
# 31 "./c23-stdarg-9.c" 3 4
 )
# 31 "./c23-stdarg-9.c"
              ;
  r += 
# 32 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 32 "./c23-stdarg-9.c"
      ap
# 32 "./c23-stdarg-9.c" 3 4
      ,
# 32 "./c23-stdarg-9.c"
      int
# 32 "./c23-stdarg-9.c" 3 4
      )
# 32 "./c23-stdarg-9.c"
                      ;
  r += 
# 33 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 33 "./c23-stdarg-9.c"
      ap
# 33 "./c23-stdarg-9.c" 3 4
      ,
# 33 "./c23-stdarg-9.c"
      int
# 33 "./c23-stdarg-9.c" 3 4
      )
# 33 "./c23-stdarg-9.c"
                      ;
  
# 34 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 34 "./c23-stdarg-9.c"
 ap
# 34 "./c23-stdarg-9.c" 3 4
 )
# 34 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f3 (...)
{
  int r = 0;
  va_list ap;
  
# 43 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 43 "./c23-stdarg-9.c"
 ap
# 43 "./c23-stdarg-9.c" 3 4
 )
# 43 "./c23-stdarg-9.c"
              ;
  r += 
# 44 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 44 "./c23-stdarg-9.c"
      ap
# 44 "./c23-stdarg-9.c" 3 4
      ,
# 44 "./c23-stdarg-9.c"
      int
# 44 "./c23-stdarg-9.c" 3 4
      )
# 44 "./c23-stdarg-9.c"
                      ;
  r += 
# 45 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 45 "./c23-stdarg-9.c"
      ap
# 45 "./c23-stdarg-9.c" 3 4
      ,
# 45 "./c23-stdarg-9.c"
      int
# 45 "./c23-stdarg-9.c" 3 4
      )
# 45 "./c23-stdarg-9.c"
                      ;
  r += 
# 46 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 46 "./c23-stdarg-9.c"
      ap
# 46 "./c23-stdarg-9.c" 3 4
      ,
# 46 "./c23-stdarg-9.c"
      int
# 46 "./c23-stdarg-9.c" 3 4
      )
# 46 "./c23-stdarg-9.c"
                      ;
  
# 47 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 47 "./c23-stdarg-9.c"
 ap
# 47 "./c23-stdarg-9.c" 3 4
 )
# 47 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f4 (...)
{
  int r = 0;
  va_list ap;
  
# 56 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 56 "./c23-stdarg-9.c"
 ap
# 56 "./c23-stdarg-9.c" 3 4
 )
# 56 "./c23-stdarg-9.c"
              ;
  r += 
# 57 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 57 "./c23-stdarg-9.c"
      ap
# 57 "./c23-stdarg-9.c" 3 4
      ,
# 57 "./c23-stdarg-9.c"
      int
# 57 "./c23-stdarg-9.c" 3 4
      )
# 57 "./c23-stdarg-9.c"
                      ;
  r += 
# 58 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 58 "./c23-stdarg-9.c"
      ap
# 58 "./c23-stdarg-9.c" 3 4
      ,
# 58 "./c23-stdarg-9.c"
      int
# 58 "./c23-stdarg-9.c" 3 4
      )
# 58 "./c23-stdarg-9.c"
                      ;
  r += 
# 59 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 59 "./c23-stdarg-9.c"
      ap
# 59 "./c23-stdarg-9.c" 3 4
      ,
# 59 "./c23-stdarg-9.c"
      int
# 59 "./c23-stdarg-9.c" 3 4
      )
# 59 "./c23-stdarg-9.c"
                      ;
  r += 
# 60 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 60 "./c23-stdarg-9.c"
      ap
# 60 "./c23-stdarg-9.c" 3 4
      ,
# 60 "./c23-stdarg-9.c"
      int
# 60 "./c23-stdarg-9.c" 3 4
      )
# 60 "./c23-stdarg-9.c"
                      ;
  
# 61 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 61 "./c23-stdarg-9.c"
 ap
# 61 "./c23-stdarg-9.c" 3 4
 )
# 61 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f5 (...)
{
  int r = 0;
  va_list ap;
  
# 70 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 70 "./c23-stdarg-9.c"
 ap
# 70 "./c23-stdarg-9.c" 3 4
 )
# 70 "./c23-stdarg-9.c"
              ;
  r += 
# 71 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 71 "./c23-stdarg-9.c"
      ap
# 71 "./c23-stdarg-9.c" 3 4
      ,
# 71 "./c23-stdarg-9.c"
      int
# 71 "./c23-stdarg-9.c" 3 4
      )
# 71 "./c23-stdarg-9.c"
                      ;
  r += 
# 72 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 72 "./c23-stdarg-9.c"
      ap
# 72 "./c23-stdarg-9.c" 3 4
      ,
# 72 "./c23-stdarg-9.c"
      int
# 72 "./c23-stdarg-9.c" 3 4
      )
# 72 "./c23-stdarg-9.c"
                      ;
  r += 
# 73 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 73 "./c23-stdarg-9.c"
      ap
# 73 "./c23-stdarg-9.c" 3 4
      ,
# 73 "./c23-stdarg-9.c"
      int
# 73 "./c23-stdarg-9.c" 3 4
      )
# 73 "./c23-stdarg-9.c"
                      ;
  r += 
# 74 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 74 "./c23-stdarg-9.c"
      ap
# 74 "./c23-stdarg-9.c" 3 4
      ,
# 74 "./c23-stdarg-9.c"
      int
# 74 "./c23-stdarg-9.c" 3 4
      )
# 74 "./c23-stdarg-9.c"
                      ;
  r += 
# 75 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 75 "./c23-stdarg-9.c"
      ap
# 75 "./c23-stdarg-9.c" 3 4
      ,
# 75 "./c23-stdarg-9.c"
      int
# 75 "./c23-stdarg-9.c" 3 4
      )
# 75 "./c23-stdarg-9.c"
                      ;
  
# 76 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 76 "./c23-stdarg-9.c"
 ap
# 76 "./c23-stdarg-9.c" 3 4
 )
# 76 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f6 (...)
{
  int r = 0;
  va_list ap;
  
# 85 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 85 "./c23-stdarg-9.c"
 ap
# 85 "./c23-stdarg-9.c" 3 4
 )
# 85 "./c23-stdarg-9.c"
              ;
  r += 
# 86 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 86 "./c23-stdarg-9.c"
      ap
# 86 "./c23-stdarg-9.c" 3 4
      ,
# 86 "./c23-stdarg-9.c"
      int
# 86 "./c23-stdarg-9.c" 3 4
      )
# 86 "./c23-stdarg-9.c"
                      ;
  r += 
# 87 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 87 "./c23-stdarg-9.c"
      ap
# 87 "./c23-stdarg-9.c" 3 4
      ,
# 87 "./c23-stdarg-9.c"
      int
# 87 "./c23-stdarg-9.c" 3 4
      )
# 87 "./c23-stdarg-9.c"
                      ;
  r += 
# 88 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 88 "./c23-stdarg-9.c"
      ap
# 88 "./c23-stdarg-9.c" 3 4
      ,
# 88 "./c23-stdarg-9.c"
      int
# 88 "./c23-stdarg-9.c" 3 4
      )
# 88 "./c23-stdarg-9.c"
                      ;
  r += 
# 89 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 89 "./c23-stdarg-9.c"
      ap
# 89 "./c23-stdarg-9.c" 3 4
      ,
# 89 "./c23-stdarg-9.c"
      int
# 89 "./c23-stdarg-9.c" 3 4
      )
# 89 "./c23-stdarg-9.c"
                      ;
  r += 
# 90 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 90 "./c23-stdarg-9.c"
      ap
# 90 "./c23-stdarg-9.c" 3 4
      ,
# 90 "./c23-stdarg-9.c"
      int
# 90 "./c23-stdarg-9.c" 3 4
      )
# 90 "./c23-stdarg-9.c"
                      ;
  r += 
# 91 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 91 "./c23-stdarg-9.c"
      ap
# 91 "./c23-stdarg-9.c" 3 4
      ,
# 91 "./c23-stdarg-9.c"
      int
# 91 "./c23-stdarg-9.c" 3 4
      )
# 91 "./c23-stdarg-9.c"
                      ;
  
# 92 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 92 "./c23-stdarg-9.c"
 ap
# 92 "./c23-stdarg-9.c" 3 4
 )
# 92 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f7 (...)
{
  int r = 0;
  va_list ap;
  
# 101 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 101 "./c23-stdarg-9.c"
 ap
# 101 "./c23-stdarg-9.c" 3 4
 )
# 101 "./c23-stdarg-9.c"
              ;
  r += 
# 102 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 102 "./c23-stdarg-9.c"
      ap
# 102 "./c23-stdarg-9.c" 3 4
      ,
# 102 "./c23-stdarg-9.c"
      int
# 102 "./c23-stdarg-9.c" 3 4
      )
# 102 "./c23-stdarg-9.c"
                      ;
  r += 
# 103 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 103 "./c23-stdarg-9.c"
      ap
# 103 "./c23-stdarg-9.c" 3 4
      ,
# 103 "./c23-stdarg-9.c"
      int
# 103 "./c23-stdarg-9.c" 3 4
      )
# 103 "./c23-stdarg-9.c"
                      ;
  r += 
# 104 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 104 "./c23-stdarg-9.c"
      ap
# 104 "./c23-stdarg-9.c" 3 4
      ,
# 104 "./c23-stdarg-9.c"
      int
# 104 "./c23-stdarg-9.c" 3 4
      )
# 104 "./c23-stdarg-9.c"
                      ;
  r += 
# 105 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 105 "./c23-stdarg-9.c"
      ap
# 105 "./c23-stdarg-9.c" 3 4
      ,
# 105 "./c23-stdarg-9.c"
      int
# 105 "./c23-stdarg-9.c" 3 4
      )
# 105 "./c23-stdarg-9.c"
                      ;
  r += 
# 106 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 106 "./c23-stdarg-9.c"
      ap
# 106 "./c23-stdarg-9.c" 3 4
      ,
# 106 "./c23-stdarg-9.c"
      int
# 106 "./c23-stdarg-9.c" 3 4
      )
# 106 "./c23-stdarg-9.c"
                      ;
  r += 
# 107 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 107 "./c23-stdarg-9.c"
      ap
# 107 "./c23-stdarg-9.c" 3 4
      ,
# 107 "./c23-stdarg-9.c"
      int
# 107 "./c23-stdarg-9.c" 3 4
      )
# 107 "./c23-stdarg-9.c"
                      ;
  r += 
# 108 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 108 "./c23-stdarg-9.c"
      ap
# 108 "./c23-stdarg-9.c" 3 4
      ,
# 108 "./c23-stdarg-9.c"
      int
# 108 "./c23-stdarg-9.c" 3 4
      )
# 108 "./c23-stdarg-9.c"
                      ;
  
# 109 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 109 "./c23-stdarg-9.c"
 ap
# 109 "./c23-stdarg-9.c" 3 4
 )
# 109 "./c23-stdarg-9.c"
            ;
  return r;
}

int
f8 (...)
{
  int r = 0;
  va_list ap;
  
# 118 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 118 "./c23-stdarg-9.c"
 ap
# 118 "./c23-stdarg-9.c" 3 4
 )
# 118 "./c23-stdarg-9.c"
              ;
  r += 
# 119 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 119 "./c23-stdarg-9.c"
      ap
# 119 "./c23-stdarg-9.c" 3 4
      ,
# 119 "./c23-stdarg-9.c"
      int
# 119 "./c23-stdarg-9.c" 3 4
      )
# 119 "./c23-stdarg-9.c"
                      ;
  r += 
# 120 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 120 "./c23-stdarg-9.c"
      ap
# 120 "./c23-stdarg-9.c" 3 4
      ,
# 120 "./c23-stdarg-9.c"
      int
# 120 "./c23-stdarg-9.c" 3 4
      )
# 120 "./c23-stdarg-9.c"
                      ;
  r += 
# 121 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 121 "./c23-stdarg-9.c"
      ap
# 121 "./c23-stdarg-9.c" 3 4
      ,
# 121 "./c23-stdarg-9.c"
      int
# 121 "./c23-stdarg-9.c" 3 4
      )
# 121 "./c23-stdarg-9.c"
                      ;
  r += 
# 122 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 122 "./c23-stdarg-9.c"
      ap
# 122 "./c23-stdarg-9.c" 3 4
      ,
# 122 "./c23-stdarg-9.c"
      int
# 122 "./c23-stdarg-9.c" 3 4
      )
# 122 "./c23-stdarg-9.c"
                      ;
  r += 
# 123 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 123 "./c23-stdarg-9.c"
      ap
# 123 "./c23-stdarg-9.c" 3 4
      ,
# 123 "./c23-stdarg-9.c"
      int
# 123 "./c23-stdarg-9.c" 3 4
      )
# 123 "./c23-stdarg-9.c"
                      ;
  r += 
# 124 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 124 "./c23-stdarg-9.c"
      ap
# 124 "./c23-stdarg-9.c" 3 4
      ,
# 124 "./c23-stdarg-9.c"
      int
# 124 "./c23-stdarg-9.c" 3 4
      )
# 124 "./c23-stdarg-9.c"
                      ;
  r += 
# 125 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 125 "./c23-stdarg-9.c"
      ap
# 125 "./c23-stdarg-9.c" 3 4
      ,
# 125 "./c23-stdarg-9.c"
      int
# 125 "./c23-stdarg-9.c" 3 4
      )
# 125 "./c23-stdarg-9.c"
                      ;
  r += 
# 126 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 126 "./c23-stdarg-9.c"
      ap
# 126 "./c23-stdarg-9.c" 3 4
      ,
# 126 "./c23-stdarg-9.c"
      int
# 126 "./c23-stdarg-9.c" 3 4
      )
# 126 "./c23-stdarg-9.c"
                      ;
  
# 127 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 127 "./c23-stdarg-9.c"
 ap
# 127 "./c23-stdarg-9.c" 3 4
 )
# 127 "./c23-stdarg-9.c"
            ;
  return r;
}

struct S
s1 (...)
{
  int r = 0;
  va_list ap;
  
# 136 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 136 "./c23-stdarg-9.c"
 ap
# 136 "./c23-stdarg-9.c" 3 4
 )
# 136 "./c23-stdarg-9.c"
              ;
  r += 
# 137 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 137 "./c23-stdarg-9.c"
      ap
# 137 "./c23-stdarg-9.c" 3 4
      ,
# 137 "./c23-stdarg-9.c"
      int
# 137 "./c23-stdarg-9.c" 3 4
      )
# 137 "./c23-stdarg-9.c"
                      ;
  
# 138 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 138 "./c23-stdarg-9.c"
 ap
# 138 "./c23-stdarg-9.c" 3 4
 )
# 138 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s2 (...)
{
  int r = 0;
  va_list ap;
  
# 149 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 149 "./c23-stdarg-9.c"
 ap
# 149 "./c23-stdarg-9.c" 3 4
 )
# 149 "./c23-stdarg-9.c"
              ;
  r += 
# 150 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 150 "./c23-stdarg-9.c"
      ap
# 150 "./c23-stdarg-9.c" 3 4
      ,
# 150 "./c23-stdarg-9.c"
      int
# 150 "./c23-stdarg-9.c" 3 4
      )
# 150 "./c23-stdarg-9.c"
                      ;
  r += 
# 151 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 151 "./c23-stdarg-9.c"
      ap
# 151 "./c23-stdarg-9.c" 3 4
      ,
# 151 "./c23-stdarg-9.c"
      int
# 151 "./c23-stdarg-9.c" 3 4
      )
# 151 "./c23-stdarg-9.c"
                      ;
  
# 152 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 152 "./c23-stdarg-9.c"
 ap
# 152 "./c23-stdarg-9.c" 3 4
 )
# 152 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s3 (...)
{
  int r = 0;
  va_list ap;
  
# 163 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 163 "./c23-stdarg-9.c"
 ap
# 163 "./c23-stdarg-9.c" 3 4
 )
# 163 "./c23-stdarg-9.c"
              ;
  r += 
# 164 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 164 "./c23-stdarg-9.c"
      ap
# 164 "./c23-stdarg-9.c" 3 4
      ,
# 164 "./c23-stdarg-9.c"
      int
# 164 "./c23-stdarg-9.c" 3 4
      )
# 164 "./c23-stdarg-9.c"
                      ;
  r += 
# 165 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 165 "./c23-stdarg-9.c"
      ap
# 165 "./c23-stdarg-9.c" 3 4
      ,
# 165 "./c23-stdarg-9.c"
      int
# 165 "./c23-stdarg-9.c" 3 4
      )
# 165 "./c23-stdarg-9.c"
                      ;
  r += 
# 166 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 166 "./c23-stdarg-9.c"
      ap
# 166 "./c23-stdarg-9.c" 3 4
      ,
# 166 "./c23-stdarg-9.c"
      int
# 166 "./c23-stdarg-9.c" 3 4
      )
# 166 "./c23-stdarg-9.c"
                      ;
  
# 167 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 167 "./c23-stdarg-9.c"
 ap
# 167 "./c23-stdarg-9.c" 3 4
 )
# 167 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s4 (...)
{
  int r = 0;
  va_list ap;
  
# 178 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 178 "./c23-stdarg-9.c"
 ap
# 178 "./c23-stdarg-9.c" 3 4
 )
# 178 "./c23-stdarg-9.c"
              ;
  r += 
# 179 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 179 "./c23-stdarg-9.c"
      ap
# 179 "./c23-stdarg-9.c" 3 4
      ,
# 179 "./c23-stdarg-9.c"
      int
# 179 "./c23-stdarg-9.c" 3 4
      )
# 179 "./c23-stdarg-9.c"
                      ;
  r += 
# 180 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 180 "./c23-stdarg-9.c"
      ap
# 180 "./c23-stdarg-9.c" 3 4
      ,
# 180 "./c23-stdarg-9.c"
      int
# 180 "./c23-stdarg-9.c" 3 4
      )
# 180 "./c23-stdarg-9.c"
                      ;
  r += 
# 181 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 181 "./c23-stdarg-9.c"
      ap
# 181 "./c23-stdarg-9.c" 3 4
      ,
# 181 "./c23-stdarg-9.c"
      int
# 181 "./c23-stdarg-9.c" 3 4
      )
# 181 "./c23-stdarg-9.c"
                      ;
  r += 
# 182 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 182 "./c23-stdarg-9.c"
      ap
# 182 "./c23-stdarg-9.c" 3 4
      ,
# 182 "./c23-stdarg-9.c"
      int
# 182 "./c23-stdarg-9.c" 3 4
      )
# 182 "./c23-stdarg-9.c"
                      ;
  
# 183 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 183 "./c23-stdarg-9.c"
 ap
# 183 "./c23-stdarg-9.c" 3 4
 )
# 183 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s5 (...)
{
  int r = 0;
  va_list ap;
  
# 194 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 194 "./c23-stdarg-9.c"
 ap
# 194 "./c23-stdarg-9.c" 3 4
 )
# 194 "./c23-stdarg-9.c"
              ;
  r += 
# 195 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 195 "./c23-stdarg-9.c"
      ap
# 195 "./c23-stdarg-9.c" 3 4
      ,
# 195 "./c23-stdarg-9.c"
      int
# 195 "./c23-stdarg-9.c" 3 4
      )
# 195 "./c23-stdarg-9.c"
                      ;
  r += 
# 196 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 196 "./c23-stdarg-9.c"
      ap
# 196 "./c23-stdarg-9.c" 3 4
      ,
# 196 "./c23-stdarg-9.c"
      int
# 196 "./c23-stdarg-9.c" 3 4
      )
# 196 "./c23-stdarg-9.c"
                      ;
  r += 
# 197 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 197 "./c23-stdarg-9.c"
      ap
# 197 "./c23-stdarg-9.c" 3 4
      ,
# 197 "./c23-stdarg-9.c"
      int
# 197 "./c23-stdarg-9.c" 3 4
      )
# 197 "./c23-stdarg-9.c"
                      ;
  r += 
# 198 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 198 "./c23-stdarg-9.c"
      ap
# 198 "./c23-stdarg-9.c" 3 4
      ,
# 198 "./c23-stdarg-9.c"
      int
# 198 "./c23-stdarg-9.c" 3 4
      )
# 198 "./c23-stdarg-9.c"
                      ;
  r += 
# 199 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 199 "./c23-stdarg-9.c"
      ap
# 199 "./c23-stdarg-9.c" 3 4
      ,
# 199 "./c23-stdarg-9.c"
      int
# 199 "./c23-stdarg-9.c" 3 4
      )
# 199 "./c23-stdarg-9.c"
                      ;
  
# 200 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 200 "./c23-stdarg-9.c"
 ap
# 200 "./c23-stdarg-9.c" 3 4
 )
# 200 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s6 (...)
{
  int r = 0;
  va_list ap;
  
# 211 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 211 "./c23-stdarg-9.c"
 ap
# 211 "./c23-stdarg-9.c" 3 4
 )
# 211 "./c23-stdarg-9.c"
              ;
  r += 
# 212 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 212 "./c23-stdarg-9.c"
      ap
# 212 "./c23-stdarg-9.c" 3 4
      ,
# 212 "./c23-stdarg-9.c"
      int
# 212 "./c23-stdarg-9.c" 3 4
      )
# 212 "./c23-stdarg-9.c"
                      ;
  r += 
# 213 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 213 "./c23-stdarg-9.c"
      ap
# 213 "./c23-stdarg-9.c" 3 4
      ,
# 213 "./c23-stdarg-9.c"
      int
# 213 "./c23-stdarg-9.c" 3 4
      )
# 213 "./c23-stdarg-9.c"
                      ;
  r += 
# 214 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 214 "./c23-stdarg-9.c"
      ap
# 214 "./c23-stdarg-9.c" 3 4
      ,
# 214 "./c23-stdarg-9.c"
      int
# 214 "./c23-stdarg-9.c" 3 4
      )
# 214 "./c23-stdarg-9.c"
                      ;
  r += 
# 215 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 215 "./c23-stdarg-9.c"
      ap
# 215 "./c23-stdarg-9.c" 3 4
      ,
# 215 "./c23-stdarg-9.c"
      int
# 215 "./c23-stdarg-9.c" 3 4
      )
# 215 "./c23-stdarg-9.c"
                      ;
  r += 
# 216 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 216 "./c23-stdarg-9.c"
      ap
# 216 "./c23-stdarg-9.c" 3 4
      ,
# 216 "./c23-stdarg-9.c"
      int
# 216 "./c23-stdarg-9.c" 3 4
      )
# 216 "./c23-stdarg-9.c"
                      ;
  r += 
# 217 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 217 "./c23-stdarg-9.c"
      ap
# 217 "./c23-stdarg-9.c" 3 4
      ,
# 217 "./c23-stdarg-9.c"
      int
# 217 "./c23-stdarg-9.c" 3 4
      )
# 217 "./c23-stdarg-9.c"
                      ;
  
# 218 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 218 "./c23-stdarg-9.c"
 ap
# 218 "./c23-stdarg-9.c" 3 4
 )
# 218 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s7 (...)
{
  int r = 0;
  va_list ap;
  
# 229 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 229 "./c23-stdarg-9.c"
 ap
# 229 "./c23-stdarg-9.c" 3 4
 )
# 229 "./c23-stdarg-9.c"
              ;
  r += 
# 230 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 230 "./c23-stdarg-9.c"
      ap
# 230 "./c23-stdarg-9.c" 3 4
      ,
# 230 "./c23-stdarg-9.c"
      int
# 230 "./c23-stdarg-9.c" 3 4
      )
# 230 "./c23-stdarg-9.c"
                      ;
  r += 
# 231 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 231 "./c23-stdarg-9.c"
      ap
# 231 "./c23-stdarg-9.c" 3 4
      ,
# 231 "./c23-stdarg-9.c"
      int
# 231 "./c23-stdarg-9.c" 3 4
      )
# 231 "./c23-stdarg-9.c"
                      ;
  r += 
# 232 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 232 "./c23-stdarg-9.c"
      ap
# 232 "./c23-stdarg-9.c" 3 4
      ,
# 232 "./c23-stdarg-9.c"
      int
# 232 "./c23-stdarg-9.c" 3 4
      )
# 232 "./c23-stdarg-9.c"
                      ;
  r += 
# 233 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 233 "./c23-stdarg-9.c"
      ap
# 233 "./c23-stdarg-9.c" 3 4
      ,
# 233 "./c23-stdarg-9.c"
      int
# 233 "./c23-stdarg-9.c" 3 4
      )
# 233 "./c23-stdarg-9.c"
                      ;
  r += 
# 234 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 234 "./c23-stdarg-9.c"
      ap
# 234 "./c23-stdarg-9.c" 3 4
      ,
# 234 "./c23-stdarg-9.c"
      int
# 234 "./c23-stdarg-9.c" 3 4
      )
# 234 "./c23-stdarg-9.c"
                      ;
  r += 
# 235 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 235 "./c23-stdarg-9.c"
      ap
# 235 "./c23-stdarg-9.c" 3 4
      ,
# 235 "./c23-stdarg-9.c"
      int
# 235 "./c23-stdarg-9.c" 3 4
      )
# 235 "./c23-stdarg-9.c"
                      ;
  r += 
# 236 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 236 "./c23-stdarg-9.c"
      ap
# 236 "./c23-stdarg-9.c" 3 4
      ,
# 236 "./c23-stdarg-9.c"
      int
# 236 "./c23-stdarg-9.c" 3 4
      )
# 236 "./c23-stdarg-9.c"
                      ;
  
# 237 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 237 "./c23-stdarg-9.c"
 ap
# 237 "./c23-stdarg-9.c" 3 4
 )
# 237 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

struct S
s8 (...)
{
  int r = 0;
  va_list ap;
  
# 248 "./c23-stdarg-9.c" 3 4
 __builtin_c23_va_start(
# 248 "./c23-stdarg-9.c"
 ap
# 248 "./c23-stdarg-9.c" 3 4
 )
# 248 "./c23-stdarg-9.c"
              ;
  r += 
# 249 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 249 "./c23-stdarg-9.c"
      ap
# 249 "./c23-stdarg-9.c" 3 4
      ,
# 249 "./c23-stdarg-9.c"
      int
# 249 "./c23-stdarg-9.c" 3 4
      )
# 249 "./c23-stdarg-9.c"
                      ;
  r += 
# 250 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 250 "./c23-stdarg-9.c"
      ap
# 250 "./c23-stdarg-9.c" 3 4
      ,
# 250 "./c23-stdarg-9.c"
      int
# 250 "./c23-stdarg-9.c" 3 4
      )
# 250 "./c23-stdarg-9.c"
                      ;
  r += 
# 251 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 251 "./c23-stdarg-9.c"
      ap
# 251 "./c23-stdarg-9.c" 3 4
      ,
# 251 "./c23-stdarg-9.c"
      int
# 251 "./c23-stdarg-9.c" 3 4
      )
# 251 "./c23-stdarg-9.c"
                      ;
  r += 
# 252 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 252 "./c23-stdarg-9.c"
      ap
# 252 "./c23-stdarg-9.c" 3 4
      ,
# 252 "./c23-stdarg-9.c"
      int
# 252 "./c23-stdarg-9.c" 3 4
      )
# 252 "./c23-stdarg-9.c"
                      ;
  r += 
# 253 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 253 "./c23-stdarg-9.c"
      ap
# 253 "./c23-stdarg-9.c" 3 4
      ,
# 253 "./c23-stdarg-9.c"
      int
# 253 "./c23-stdarg-9.c" 3 4
      )
# 253 "./c23-stdarg-9.c"
                      ;
  r += 
# 254 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 254 "./c23-stdarg-9.c"
      ap
# 254 "./c23-stdarg-9.c" 3 4
      ,
# 254 "./c23-stdarg-9.c"
      int
# 254 "./c23-stdarg-9.c" 3 4
      )
# 254 "./c23-stdarg-9.c"
                      ;
  r += 
# 255 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 255 "./c23-stdarg-9.c"
      ap
# 255 "./c23-stdarg-9.c" 3 4
      ,
# 255 "./c23-stdarg-9.c"
      int
# 255 "./c23-stdarg-9.c" 3 4
      )
# 255 "./c23-stdarg-9.c"
                      ;
  r += 
# 256 "./c23-stdarg-9.c" 3 4
      __builtin_va_arg(
# 256 "./c23-stdarg-9.c"
      ap
# 256 "./c23-stdarg-9.c" 3 4
      ,
# 256 "./c23-stdarg-9.c"
      int
# 256 "./c23-stdarg-9.c" 3 4
      )
# 256 "./c23-stdarg-9.c"
                      ;
  
# 257 "./c23-stdarg-9.c" 3 4
 __builtin_va_end(
# 257 "./c23-stdarg-9.c"
 ap
# 257 "./c23-stdarg-9.c" 3 4
 )
# 257 "./c23-stdarg-9.c"
            ;
  struct S s = {};
  s.a[0] = r;
  return s;
}

int
b1 (void)
{
  return f8 (1, 2, 3, 4, 5, 6, 7, 8);
}

int
b2 (void)
{
  return s8 (1, 2, 3, 4, 5, 6, 7, 8).a[0];
}

int
main ()
{
  if (f1 (1) != 1 || f2 (1, 2) != 3 || f3 (1, 2, 3) != 6
      || f4 (1, 2, 3, 4) != 10 || f5 (1, 2, 3, 4, 5) != 15
      || f6 (1, 2, 3, 4, 5, 6) != 21 || f7 (1, 2, 3, 4, 5, 6, 7) != 28
      || f8 (1, 2, 3, 4, 5, 6, 7, 8) != 36)
    __builtin_abort ();
  if (s1 (1).a[0] != 1 || s2 (1, 2).a[0] != 3 || s3 (1, 2, 3).a[0] != 6
      || s4 (1, 2, 3, 4).a[0] != 10 || s5 (1, 2, 3, 4, 5).a[0] != 15
      || s6 (1, 2, 3, 4, 5, 6).a[0] != 21
      || s7 (1, 2, 3, 4, 5, 6, 7).a[0] != 28
      || s8 (1, 2, 3, 4, 5, 6, 7, 8).a[0] != 36)
    __builtin_abort ();
}
