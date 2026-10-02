//type: fp
//options: --c23
# 0 "./torture/c2x-stdarg-split-1b.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/c2x-stdarg-split-1b.c"






# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 8 "./torture/c2x-stdarg-split-1b.c" 2


# 9 "./torture/c2x-stdarg-split-1b.c"
extern void abort (void);

double
f (...)
{
  va_list ap;
  
# 15 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 15 "./torture/c2x-stdarg-split-1b.c"
 ap
# 15 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 15 "./torture/c2x-stdarg-split-1b.c"
              ;
  double ret = 
# 16 "./torture/c2x-stdarg-split-1b.c" 3 4
              __builtin_va_arg(
# 16 "./torture/c2x-stdarg-split-1b.c"
              ap
# 16 "./torture/c2x-stdarg-split-1b.c" 3 4
              ,
# 16 "./torture/c2x-stdarg-split-1b.c"
              int
# 16 "./torture/c2x-stdarg-split-1b.c" 3 4
              )
# 16 "./torture/c2x-stdarg-split-1b.c"
                              ;
  ret += 
# 17 "./torture/c2x-stdarg-split-1b.c" 3 4
        __builtin_va_arg(
# 17 "./torture/c2x-stdarg-split-1b.c"
        ap
# 17 "./torture/c2x-stdarg-split-1b.c" 3 4
        ,
# 17 "./torture/c2x-stdarg-split-1b.c"
        double
# 17 "./torture/c2x-stdarg-split-1b.c" 3 4
        )
# 17 "./torture/c2x-stdarg-split-1b.c"
                           ;
  ret += 
# 18 "./torture/c2x-stdarg-split-1b.c" 3 4
        __builtin_va_arg(
# 18 "./torture/c2x-stdarg-split-1b.c"
        ap
# 18 "./torture/c2x-stdarg-split-1b.c" 3 4
        ,
# 18 "./torture/c2x-stdarg-split-1b.c"
        int
# 18 "./torture/c2x-stdarg-split-1b.c" 3 4
        )
# 18 "./torture/c2x-stdarg-split-1b.c"
                        ;
  ret += 
# 19 "./torture/c2x-stdarg-split-1b.c" 3 4
        __builtin_va_arg(
# 19 "./torture/c2x-stdarg-split-1b.c"
        ap
# 19 "./torture/c2x-stdarg-split-1b.c" 3 4
        ,
# 19 "./torture/c2x-stdarg-split-1b.c"
        double
# 19 "./torture/c2x-stdarg-split-1b.c" 3 4
        )
# 19 "./torture/c2x-stdarg-split-1b.c"
                           ;
  
# 20 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 20 "./torture/c2x-stdarg-split-1b.c"
 ap
# 20 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 20 "./torture/c2x-stdarg-split-1b.c"
            ;
  return ret;
}

void
g (...)
{
  va_list ap;
  
# 28 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 28 "./torture/c2x-stdarg-split-1b.c"
 ap
# 28 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 28 "./torture/c2x-stdarg-split-1b.c"
                                                 ;
  for (int i = 0; i < 10; i++)
    if (
# 30 "./torture/c2x-stdarg-split-1b.c" 3 4
       __builtin_va_arg(
# 30 "./torture/c2x-stdarg-split-1b.c"
       ap
# 30 "./torture/c2x-stdarg-split-1b.c" 3 4
       ,
# 30 "./torture/c2x-stdarg-split-1b.c"
       double
# 30 "./torture/c2x-stdarg-split-1b.c" 3 4
       ) 
# 30 "./torture/c2x-stdarg-split-1b.c"
                           != i)
      abort ();
  
# 32 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 32 "./torture/c2x-stdarg-split-1b.c"
 ap
# 32 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 32 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h1 (register int x, ...)
{
  va_list ap;
  
# 39 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 39 "./torture/c2x-stdarg-split-1b.c"
 ap
# 39 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 39 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 42 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 42 "./torture/c2x-stdarg-split-1b.c"
         ap
# 42 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 42 "./torture/c2x-stdarg-split-1b.c"
         double
# 42 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 42 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 45 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 45 "./torture/c2x-stdarg-split-1b.c"
         ap
# 45 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 45 "./torture/c2x-stdarg-split-1b.c"
         int
# 45 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 45 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 48 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 48 "./torture/c2x-stdarg-split-1b.c"
 ap
# 48 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 48 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h2 (int x(), ...)
{
  va_list ap;
  
# 55 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 55 "./torture/c2x-stdarg-split-1b.c"
 ap
# 55 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 55 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 58 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 58 "./torture/c2x-stdarg-split-1b.c"
         ap
# 58 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 58 "./torture/c2x-stdarg-split-1b.c"
         double
# 58 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 58 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 61 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 61 "./torture/c2x-stdarg-split-1b.c"
         ap
# 61 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 61 "./torture/c2x-stdarg-split-1b.c"
         int
# 61 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 61 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 64 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 64 "./torture/c2x-stdarg-split-1b.c"
 ap
# 64 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 64 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h3 (int x[10], ...)
{
  va_list ap;
  
# 71 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 71 "./torture/c2x-stdarg-split-1b.c"
 ap
# 71 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 71 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 74 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 74 "./torture/c2x-stdarg-split-1b.c"
         ap
# 74 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 74 "./torture/c2x-stdarg-split-1b.c"
         double
# 74 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 74 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 77 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 77 "./torture/c2x-stdarg-split-1b.c"
         ap
# 77 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 77 "./torture/c2x-stdarg-split-1b.c"
         int
# 77 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 77 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 80 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 80 "./torture/c2x-stdarg-split-1b.c"
 ap
# 80 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 80 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h4 (char x, ...)
{
  va_list ap;
  
# 87 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 87 "./torture/c2x-stdarg-split-1b.c"
 ap
# 87 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 87 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 90 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 90 "./torture/c2x-stdarg-split-1b.c"
         ap
# 90 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 90 "./torture/c2x-stdarg-split-1b.c"
         double
# 90 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 90 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 93 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 93 "./torture/c2x-stdarg-split-1b.c"
         ap
# 93 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 93 "./torture/c2x-stdarg-split-1b.c"
         int
# 93 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 93 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 96 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 96 "./torture/c2x-stdarg-split-1b.c"
 ap
# 96 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 96 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h5 (float x, ...)
{
  va_list ap;
  
# 103 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 103 "./torture/c2x-stdarg-split-1b.c"
 ap
# 103 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 103 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 106 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 106 "./torture/c2x-stdarg-split-1b.c"
         ap
# 106 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 106 "./torture/c2x-stdarg-split-1b.c"
         double
# 106 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 106 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 109 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 109 "./torture/c2x-stdarg-split-1b.c"
         ap
# 109 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 109 "./torture/c2x-stdarg-split-1b.c"
         int
# 109 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 109 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 112 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 112 "./torture/c2x-stdarg-split-1b.c"
 ap
# 112 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 112 "./torture/c2x-stdarg-split-1b.c"
            ;
}

void
h6 (volatile long x, ...)
{
  va_list ap;
  
# 119 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 119 "./torture/c2x-stdarg-split-1b.c"
 ap
# 119 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 119 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 122 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 122 "./torture/c2x-stdarg-split-1b.c"
         ap
# 122 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 122 "./torture/c2x-stdarg-split-1b.c"
         double
# 122 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 122 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 125 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 125 "./torture/c2x-stdarg-split-1b.c"
         ap
# 125 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 125 "./torture/c2x-stdarg-split-1b.c"
         int
# 125 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 125 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 128 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 128 "./torture/c2x-stdarg-split-1b.c"
 ap
# 128 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 128 "./torture/c2x-stdarg-split-1b.c"
            ;
}

struct s { char c[1000]; };

void
h7 (volatile struct s x, ...)
{
  va_list ap;
  
# 137 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_start(
# 137 "./torture/c2x-stdarg-split-1b.c"
 ap
# 137 "./torture/c2x-stdarg-split-1b.c" 3 4
 , 0)
# 137 "./torture/c2x-stdarg-split-1b.c"
              ;
  for (int i = 0; i < 10; i++)
    {
      if (
# 140 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 140 "./torture/c2x-stdarg-split-1b.c"
         ap
# 140 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 140 "./torture/c2x-stdarg-split-1b.c"
         double
# 140 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 140 "./torture/c2x-stdarg-split-1b.c"
                             != i)
 abort ();
      i++;
      if (
# 143 "./torture/c2x-stdarg-split-1b.c" 3 4
         __builtin_va_arg(
# 143 "./torture/c2x-stdarg-split-1b.c"
         ap
# 143 "./torture/c2x-stdarg-split-1b.c" 3 4
         ,
# 143 "./torture/c2x-stdarg-split-1b.c"
         int
# 143 "./torture/c2x-stdarg-split-1b.c" 3 4
         ) 
# 143 "./torture/c2x-stdarg-split-1b.c"
                          != i)
 abort ();
    }
  
# 146 "./torture/c2x-stdarg-split-1b.c" 3 4
 __builtin_va_end(
# 146 "./torture/c2x-stdarg-split-1b.c"
 ap
# 146 "./torture/c2x-stdarg-split-1b.c" 3 4
 )
# 146 "./torture/c2x-stdarg-split-1b.c"
            ;
}
