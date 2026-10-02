//type: rp
//options: --c23
# 0 "./c23-stdarg-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-stdarg-6.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./c23-stdarg-6.c" 2


# 8 "./c23-stdarg-6.c"
extern void abort (void);
extern void exit (int);
struct s { char c[1000]; };

struct s
f (...)
{
  va_list ap;
  
# 16 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 16 "./c23-stdarg-6.c"
 ap
# 16 "./c23-stdarg-6.c" 3 4
 )
# 16 "./c23-stdarg-6.c"
              ;
  double r = 
# 17 "./c23-stdarg-6.c" 3 4
            __builtin_va_arg(
# 17 "./c23-stdarg-6.c"
            ap
# 17 "./c23-stdarg-6.c" 3 4
            ,
# 17 "./c23-stdarg-6.c"
            int
# 17 "./c23-stdarg-6.c" 3 4
            )
# 17 "./c23-stdarg-6.c"
                            ;
  r += 
# 18 "./c23-stdarg-6.c" 3 4
      __builtin_va_arg(
# 18 "./c23-stdarg-6.c"
      ap
# 18 "./c23-stdarg-6.c" 3 4
      ,
# 18 "./c23-stdarg-6.c"
      double
# 18 "./c23-stdarg-6.c" 3 4
      )
# 18 "./c23-stdarg-6.c"
                         ;
  r += 
# 19 "./c23-stdarg-6.c" 3 4
      __builtin_va_arg(
# 19 "./c23-stdarg-6.c"
      ap
# 19 "./c23-stdarg-6.c" 3 4
      ,
# 19 "./c23-stdarg-6.c"
      int
# 19 "./c23-stdarg-6.c" 3 4
      )
# 19 "./c23-stdarg-6.c"
                      ;
  r += 
# 20 "./c23-stdarg-6.c" 3 4
      __builtin_va_arg(
# 20 "./c23-stdarg-6.c"
      ap
# 20 "./c23-stdarg-6.c" 3 4
      ,
# 20 "./c23-stdarg-6.c"
      double
# 20 "./c23-stdarg-6.c" 3 4
      )
# 20 "./c23-stdarg-6.c"
                         ;
  
# 21 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 21 "./c23-stdarg-6.c"
 ap
# 21 "./c23-stdarg-6.c" 3 4
 )
# 21 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = r;
  ret.c[999] = 42;
  return ret;
}

struct s
g (...)
{
  va_list ap;
  
# 32 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 32 "./c23-stdarg-6.c"
 ap, random ! ignored, ignored ** text
# 32 "./c23-stdarg-6.c" 3 4
 )
# 32 "./c23-stdarg-6.c"
                                                 ;
  for (int i = 0; i < 10; i++)
    if (
# 34 "./c23-stdarg-6.c" 3 4
       __builtin_va_arg(
# 34 "./c23-stdarg-6.c"
       ap
# 34 "./c23-stdarg-6.c" 3 4
       ,
# 34 "./c23-stdarg-6.c"
       double
# 34 "./c23-stdarg-6.c" 3 4
       ) 
# 34 "./c23-stdarg-6.c"
                           != i)
      abort ();
  
# 36 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 36 "./c23-stdarg-6.c"
 ap
# 36 "./c23-stdarg-6.c" 3 4
 )
# 36 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 17;
  ret.c[999] = 58;
  return ret;
}

struct s
h1 (register int x, ...)
{
  va_list ap;
  
# 47 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 47 "./c23-stdarg-6.c"
 ap
# 47 "./c23-stdarg-6.c" 3 4
 )
# 47 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 50 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 50 "./c23-stdarg-6.c"
         ap
# 50 "./c23-stdarg-6.c" 3 4
         ,
# 50 "./c23-stdarg-6.c"
         double
# 50 "./c23-stdarg-6.c" 3 4
         ) 
# 50 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 53 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 53 "./c23-stdarg-6.c"
         ap
# 53 "./c23-stdarg-6.c" 3 4
         ,
# 53 "./c23-stdarg-6.c"
         int
# 53 "./c23-stdarg-6.c" 3 4
         ) 
# 53 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 56 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 56 "./c23-stdarg-6.c"
 ap
# 56 "./c23-stdarg-6.c" 3 4
 )
# 56 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 32;
  ret.c[999] = 95;
  return ret;
}

struct s
h2 (int x(), ...)
{
  va_list ap;
  
# 67 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 67 "./c23-stdarg-6.c"
 ap
# 67 "./c23-stdarg-6.c" 3 4
 )
# 67 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 70 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 70 "./c23-stdarg-6.c"
         ap
# 70 "./c23-stdarg-6.c" 3 4
         ,
# 70 "./c23-stdarg-6.c"
         double
# 70 "./c23-stdarg-6.c" 3 4
         ) 
# 70 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 73 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 73 "./c23-stdarg-6.c"
         ap
# 73 "./c23-stdarg-6.c" 3 4
         ,
# 73 "./c23-stdarg-6.c"
         int
# 73 "./c23-stdarg-6.c" 3 4
         ) 
# 73 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 76 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 76 "./c23-stdarg-6.c"
 ap
# 76 "./c23-stdarg-6.c" 3 4
 )
# 76 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 5;
  ret.c[999] = 125;
  return ret;
}

struct s
h3 (int x[10], ...)
{
  va_list ap;
  
# 87 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 87 "./c23-stdarg-6.c"
 ap
# 87 "./c23-stdarg-6.c" 3 4
 )
# 87 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 90 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 90 "./c23-stdarg-6.c"
         ap
# 90 "./c23-stdarg-6.c" 3 4
         ,
# 90 "./c23-stdarg-6.c"
         double
# 90 "./c23-stdarg-6.c" 3 4
         ) 
# 90 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 93 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 93 "./c23-stdarg-6.c"
         ap
# 93 "./c23-stdarg-6.c" 3 4
         ,
# 93 "./c23-stdarg-6.c"
         int
# 93 "./c23-stdarg-6.c" 3 4
         ) 
# 93 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 96 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 96 "./c23-stdarg-6.c"
 ap
# 96 "./c23-stdarg-6.c" 3 4
 )
# 96 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 8;
  ret.c[999] = 12;
  return ret;
}

struct s
h4 (char x, ...)
{
  va_list ap;
  
# 107 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 107 "./c23-stdarg-6.c"
 ap
# 107 "./c23-stdarg-6.c" 3 4
 )
# 107 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 110 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 110 "./c23-stdarg-6.c"
         ap
# 110 "./c23-stdarg-6.c" 3 4
         ,
# 110 "./c23-stdarg-6.c"
         double
# 110 "./c23-stdarg-6.c" 3 4
         ) 
# 110 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 113 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 113 "./c23-stdarg-6.c"
         ap
# 113 "./c23-stdarg-6.c" 3 4
         ,
# 113 "./c23-stdarg-6.c"
         int
# 113 "./c23-stdarg-6.c" 3 4
         ) 
# 113 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 116 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 116 "./c23-stdarg-6.c"
 ap
# 116 "./c23-stdarg-6.c" 3 4
 )
# 116 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 18;
  ret.c[999] = 28;
  return ret;
}

struct s
h5 (float x, ...)
{
  va_list ap;
  
# 127 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 127 "./c23-stdarg-6.c"
 ap
# 127 "./c23-stdarg-6.c" 3 4
 )
# 127 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 130 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 130 "./c23-stdarg-6.c"
         ap
# 130 "./c23-stdarg-6.c" 3 4
         ,
# 130 "./c23-stdarg-6.c"
         double
# 130 "./c23-stdarg-6.c" 3 4
         ) 
# 130 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 133 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 133 "./c23-stdarg-6.c"
         ap
# 133 "./c23-stdarg-6.c" 3 4
         ,
# 133 "./c23-stdarg-6.c"
         int
# 133 "./c23-stdarg-6.c" 3 4
         ) 
# 133 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 136 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 136 "./c23-stdarg-6.c"
 ap
# 136 "./c23-stdarg-6.c" 3 4
 )
# 136 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 38;
  ret.c[999] = 48;
  return ret;
}

struct s
h6 (volatile long x, ...)
{
  va_list ap;
  
# 147 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 147 "./c23-stdarg-6.c"
 ap
# 147 "./c23-stdarg-6.c" 3 4
 )
# 147 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 150 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 150 "./c23-stdarg-6.c"
         ap
# 150 "./c23-stdarg-6.c" 3 4
         ,
# 150 "./c23-stdarg-6.c"
         double
# 150 "./c23-stdarg-6.c" 3 4
         ) 
# 150 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 153 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 153 "./c23-stdarg-6.c"
         ap
# 153 "./c23-stdarg-6.c" 3 4
         ,
# 153 "./c23-stdarg-6.c"
         int
# 153 "./c23-stdarg-6.c" 3 4
         ) 
# 153 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 156 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 156 "./c23-stdarg-6.c"
 ap
# 156 "./c23-stdarg-6.c" 3 4
 )
# 156 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 58;
  ret.c[999] = 68;
  return ret;
}

struct s
h7 (volatile struct s x, ...)
{
  va_list ap;
  
# 167 "./c23-stdarg-6.c" 3 4
 __builtin_c23_va_start(
# 167 "./c23-stdarg-6.c"
 ap
# 167 "./c23-stdarg-6.c" 3 4
 )
# 167 "./c23-stdarg-6.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 170 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 170 "./c23-stdarg-6.c"
         ap
# 170 "./c23-stdarg-6.c" 3 4
         ,
# 170 "./c23-stdarg-6.c"
         double
# 170 "./c23-stdarg-6.c" 3 4
         ) 
# 170 "./c23-stdarg-6.c"
                             != i)
 abort ();
      i++;
      if (
# 173 "./c23-stdarg-6.c" 3 4
         __builtin_va_arg(
# 173 "./c23-stdarg-6.c"
         ap
# 173 "./c23-stdarg-6.c" 3 4
         ,
# 173 "./c23-stdarg-6.c"
         int
# 173 "./c23-stdarg-6.c" 3 4
         ) 
# 173 "./c23-stdarg-6.c"
                          != i)
 abort ();
    }
  
# 176 "./c23-stdarg-6.c" 3 4
 __builtin_va_end(
# 176 "./c23-stdarg-6.c"
 ap
# 176 "./c23-stdarg-6.c" 3 4
 )
# 176 "./c23-stdarg-6.c"
            ;
  struct s ret = {};
  ret.c[0] = 78;
  ret.c[999] = 88;
  return ret;
}

int
main ()
{
  struct s x = f (1, 2.0, 3, 4.0);
  if (x.c[0] != 10 || x.c[999] != 42)
    abort ();
  x = g (0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0);
  if (x.c[0] != 17 || x.c[999] != 58)
    abort ();
  x = g (0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f);
  if (x.c[0] != 17 || x.c[999] != 58)
    abort ();
  x = h1 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 32 || x.c[999] != 95)
    abort ();
  x = h2 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 5 || x.c[999] != 125)
    abort ();
  x = h3 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 8 || x.c[999] != 12)
    abort ();
  x = h4 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 18 || x.c[999] != 28)
    abort ();
  x = h5 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 38 || x.c[999] != 48)
    abort ();
  x = h6 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 58 || x.c[999] != 68)
    abort ();
  x = h7 ((struct s) {}, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  if (x.c[0] != 78 || x.c[999] != 88)
    abort ();
  exit (0);
}
