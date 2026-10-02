//type: rp
//options: --c23
# 0 "./c23-stdarg-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-stdarg-4.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./c23-stdarg-4.c" 2


# 8 "./c23-stdarg-4.c"
extern void abort (void);
extern void exit (int);

double
f (...)
{
  va_list ap;
  
# 15 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 15 "./c23-stdarg-4.c"
 ap
# 15 "./c23-stdarg-4.c" 3 4
 )
# 15 "./c23-stdarg-4.c"
              ;
  double ret = 
# 16 "./c23-stdarg-4.c" 3 4
              __builtin_va_arg(
# 16 "./c23-stdarg-4.c"
              ap
# 16 "./c23-stdarg-4.c" 3 4
              ,
# 16 "./c23-stdarg-4.c"
              int
# 16 "./c23-stdarg-4.c" 3 4
              )
# 16 "./c23-stdarg-4.c"
                              ;
  ret += 
# 17 "./c23-stdarg-4.c" 3 4
        __builtin_va_arg(
# 17 "./c23-stdarg-4.c"
        ap
# 17 "./c23-stdarg-4.c" 3 4
        ,
# 17 "./c23-stdarg-4.c"
        double
# 17 "./c23-stdarg-4.c" 3 4
        )
# 17 "./c23-stdarg-4.c"
                           ;
  ret += 
# 18 "./c23-stdarg-4.c" 3 4
        __builtin_va_arg(
# 18 "./c23-stdarg-4.c"
        ap
# 18 "./c23-stdarg-4.c" 3 4
        ,
# 18 "./c23-stdarg-4.c"
        int
# 18 "./c23-stdarg-4.c" 3 4
        )
# 18 "./c23-stdarg-4.c"
                        ;
  ret += 
# 19 "./c23-stdarg-4.c" 3 4
        __builtin_va_arg(
# 19 "./c23-stdarg-4.c"
        ap
# 19 "./c23-stdarg-4.c" 3 4
        ,
# 19 "./c23-stdarg-4.c"
        double
# 19 "./c23-stdarg-4.c" 3 4
        )
# 19 "./c23-stdarg-4.c"
                           ;
  
# 20 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 20 "./c23-stdarg-4.c"
 ap
# 20 "./c23-stdarg-4.c" 3 4
 )
# 20 "./c23-stdarg-4.c"
            ;
  return ret;
}

void
g (...)
{
  va_list ap;
  
# 28 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 28 "./c23-stdarg-4.c"
 ap, random ! ignored, ignored ** text
# 28 "./c23-stdarg-4.c" 3 4
 )
# 28 "./c23-stdarg-4.c"
                                                 ;
  for (int i = 0; i < 10; i++)
    if (
# 30 "./c23-stdarg-4.c" 3 4
       __builtin_va_arg(
# 30 "./c23-stdarg-4.c"
       ap
# 30 "./c23-stdarg-4.c" 3 4
       ,
# 30 "./c23-stdarg-4.c"
       double
# 30 "./c23-stdarg-4.c" 3 4
       ) 
# 30 "./c23-stdarg-4.c"
                           != i)
      abort ();
  
# 32 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 32 "./c23-stdarg-4.c"
 ap
# 32 "./c23-stdarg-4.c" 3 4
 )
# 32 "./c23-stdarg-4.c"
            ;
}

void
h1 (register int x, ...)
{
  va_list ap;
  
# 39 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 39 "./c23-stdarg-4.c"
 ap
# 39 "./c23-stdarg-4.c" 3 4
 )
# 39 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 42 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 42 "./c23-stdarg-4.c"
         ap
# 42 "./c23-stdarg-4.c" 3 4
         ,
# 42 "./c23-stdarg-4.c"
         double
# 42 "./c23-stdarg-4.c" 3 4
         ) 
# 42 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 45 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 45 "./c23-stdarg-4.c"
         ap
# 45 "./c23-stdarg-4.c" 3 4
         ,
# 45 "./c23-stdarg-4.c"
         int
# 45 "./c23-stdarg-4.c" 3 4
         ) 
# 45 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 48 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 48 "./c23-stdarg-4.c"
 ap
# 48 "./c23-stdarg-4.c" 3 4
 )
# 48 "./c23-stdarg-4.c"
            ;
}

void
h2 (int x(), ...)
{
  va_list ap;
  
# 55 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 55 "./c23-stdarg-4.c"
 ap
# 55 "./c23-stdarg-4.c" 3 4
 )
# 55 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 58 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 58 "./c23-stdarg-4.c"
         ap
# 58 "./c23-stdarg-4.c" 3 4
         ,
# 58 "./c23-stdarg-4.c"
         double
# 58 "./c23-stdarg-4.c" 3 4
         ) 
# 58 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 61 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 61 "./c23-stdarg-4.c"
         ap
# 61 "./c23-stdarg-4.c" 3 4
         ,
# 61 "./c23-stdarg-4.c"
         int
# 61 "./c23-stdarg-4.c" 3 4
         ) 
# 61 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 64 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 64 "./c23-stdarg-4.c"
 ap
# 64 "./c23-stdarg-4.c" 3 4
 )
# 64 "./c23-stdarg-4.c"
            ;
}

void
h3 (int x[10], ...)
{
  va_list ap;
  
# 71 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 71 "./c23-stdarg-4.c"
 ap
# 71 "./c23-stdarg-4.c" 3 4
 )
# 71 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 74 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 74 "./c23-stdarg-4.c"
         ap
# 74 "./c23-stdarg-4.c" 3 4
         ,
# 74 "./c23-stdarg-4.c"
         double
# 74 "./c23-stdarg-4.c" 3 4
         ) 
# 74 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 77 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 77 "./c23-stdarg-4.c"
         ap
# 77 "./c23-stdarg-4.c" 3 4
         ,
# 77 "./c23-stdarg-4.c"
         int
# 77 "./c23-stdarg-4.c" 3 4
         ) 
# 77 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 80 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 80 "./c23-stdarg-4.c"
 ap
# 80 "./c23-stdarg-4.c" 3 4
 )
# 80 "./c23-stdarg-4.c"
            ;
}

void
h4 (char x, ...)
{
  va_list ap;
  
# 87 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 87 "./c23-stdarg-4.c"
 ap
# 87 "./c23-stdarg-4.c" 3 4
 )
# 87 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 90 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 90 "./c23-stdarg-4.c"
         ap
# 90 "./c23-stdarg-4.c" 3 4
         ,
# 90 "./c23-stdarg-4.c"
         double
# 90 "./c23-stdarg-4.c" 3 4
         ) 
# 90 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 93 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 93 "./c23-stdarg-4.c"
         ap
# 93 "./c23-stdarg-4.c" 3 4
         ,
# 93 "./c23-stdarg-4.c"
         int
# 93 "./c23-stdarg-4.c" 3 4
         ) 
# 93 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 96 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 96 "./c23-stdarg-4.c"
 ap
# 96 "./c23-stdarg-4.c" 3 4
 )
# 96 "./c23-stdarg-4.c"
            ;
}

void
h5 (float x, ...)
{
  va_list ap;
  
# 103 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 103 "./c23-stdarg-4.c"
 ap
# 103 "./c23-stdarg-4.c" 3 4
 )
# 103 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 106 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 106 "./c23-stdarg-4.c"
         ap
# 106 "./c23-stdarg-4.c" 3 4
         ,
# 106 "./c23-stdarg-4.c"
         double
# 106 "./c23-stdarg-4.c" 3 4
         ) 
# 106 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 109 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 109 "./c23-stdarg-4.c"
         ap
# 109 "./c23-stdarg-4.c" 3 4
         ,
# 109 "./c23-stdarg-4.c"
         int
# 109 "./c23-stdarg-4.c" 3 4
         ) 
# 109 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 112 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 112 "./c23-stdarg-4.c"
 ap
# 112 "./c23-stdarg-4.c" 3 4
 )
# 112 "./c23-stdarg-4.c"
            ;
}

void
h6 (volatile long x, ...)
{
  va_list ap;
  
# 119 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 119 "./c23-stdarg-4.c"
 ap
# 119 "./c23-stdarg-4.c" 3 4
 )
# 119 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 122 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 122 "./c23-stdarg-4.c"
         ap
# 122 "./c23-stdarg-4.c" 3 4
         ,
# 122 "./c23-stdarg-4.c"
         double
# 122 "./c23-stdarg-4.c" 3 4
         ) 
# 122 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 125 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 125 "./c23-stdarg-4.c"
         ap
# 125 "./c23-stdarg-4.c" 3 4
         ,
# 125 "./c23-stdarg-4.c"
         int
# 125 "./c23-stdarg-4.c" 3 4
         ) 
# 125 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 128 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 128 "./c23-stdarg-4.c"
 ap
# 128 "./c23-stdarg-4.c" 3 4
 )
# 128 "./c23-stdarg-4.c"
            ;
}

struct s { char c[1000]; };

void
h7 (volatile struct s x, ...)
{
  va_list ap;
  
# 137 "./c23-stdarg-4.c" 3 4
 __builtin_c23_va_start(
# 137 "./c23-stdarg-4.c"
 ap
# 137 "./c23-stdarg-4.c" 3 4
 )
# 137 "./c23-stdarg-4.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 140 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 140 "./c23-stdarg-4.c"
         ap
# 140 "./c23-stdarg-4.c" 3 4
         ,
# 140 "./c23-stdarg-4.c"
         double
# 140 "./c23-stdarg-4.c" 3 4
         ) 
# 140 "./c23-stdarg-4.c"
                             != i)
 abort ();
      i++;
      if (
# 143 "./c23-stdarg-4.c" 3 4
         __builtin_va_arg(
# 143 "./c23-stdarg-4.c"
         ap
# 143 "./c23-stdarg-4.c" 3 4
         ,
# 143 "./c23-stdarg-4.c"
         int
# 143 "./c23-stdarg-4.c" 3 4
         ) 
# 143 "./c23-stdarg-4.c"
                          != i)
 abort ();
    }
  
# 146 "./c23-stdarg-4.c" 3 4
 __builtin_va_end(
# 146 "./c23-stdarg-4.c"
 ap
# 146 "./c23-stdarg-4.c" 3 4
 )
# 146 "./c23-stdarg-4.c"
            ;
}

int
main ()
{
  if (f (1, 2.0, 3, 4.0) != 10.0)
    abort ();
  g (0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0);
  g (0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f);
  h1 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h2 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h3 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h4 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h5 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h6 (0, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  h7 ((struct s) {}, 0.0, 1, 2.0, 3, 4.0, 5, 6.0, 7, 8.0, 9);
  exit (0);
}
