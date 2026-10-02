//type: rp
//options: 
# 0 "./ext/va-arg-pack-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/va-arg-pack-1.C"




# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./ext/va-arg-pack-1.C" 2


# 7 "./ext/va-arg-pack-1.C"
extern "C" void abort (void);

int v1 = 8;
long int v2 = 3;
void *v3 = (void *) &v2;
struct A { char c[16]; } v4 = { "foo" };
long double v5 = 40;
char seen[20];
int cnt;

__attribute__ ((noinline)) int
foo1 (int x, int y, ...)
{
  int i;
  long int l;
  void *v;
  struct A a;
  long double ld;
  va_list ap;

  
# 27 "./ext/va-arg-pack-1.C" 3 4
 __builtin_va_start(
# 27 "./ext/va-arg-pack-1.C"
 ap
# 27 "./ext/va-arg-pack-1.C" 3 4
 ,
# 27 "./ext/va-arg-pack-1.C"
 y
# 27 "./ext/va-arg-pack-1.C" 3 4
 )
# 27 "./ext/va-arg-pack-1.C"
                 ;
  if (x < 0 || x >= 20 || seen[x])
    abort ();
  seen[x] = ++cnt;
  if (y != 6)
    abort ();
  i = 
# 33 "./ext/va-arg-pack-1.C" 3 4
     __builtin_va_arg(
# 33 "./ext/va-arg-pack-1.C"
     ap
# 33 "./ext/va-arg-pack-1.C" 3 4
     ,
# 33 "./ext/va-arg-pack-1.C"
     int
# 33 "./ext/va-arg-pack-1.C" 3 4
     )
# 33 "./ext/va-arg-pack-1.C"
                     ;
  if (i != 5)
    abort ();
  switch (x)
    {
    case 0:
      i = 
# 39 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 39 "./ext/va-arg-pack-1.C"
         ap
# 39 "./ext/va-arg-pack-1.C" 3 4
         ,
# 39 "./ext/va-arg-pack-1.C"
         int
# 39 "./ext/va-arg-pack-1.C" 3 4
         )
# 39 "./ext/va-arg-pack-1.C"
                         ;
      if (i != 9 || v1 != 9)
 abort ();
      a = 
# 42 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 42 "./ext/va-arg-pack-1.C"
         ap
# 42 "./ext/va-arg-pack-1.C" 3 4
         ,
# 42 "./ext/va-arg-pack-1.C"
         struct A
# 42 "./ext/va-arg-pack-1.C" 3 4
         )
# 42 "./ext/va-arg-pack-1.C"
                              ;
      if (__builtin_memcmp (a.c, v4.c, sizeof (a.c)) != 0)
 abort ();
      v = (void *) 
# 45 "./ext/va-arg-pack-1.C" 3 4
                  __builtin_va_arg(
# 45 "./ext/va-arg-pack-1.C"
                  ap
# 45 "./ext/va-arg-pack-1.C" 3 4
                  ,
# 45 "./ext/va-arg-pack-1.C"
                  struct A *
# 45 "./ext/va-arg-pack-1.C" 3 4
                  )
# 45 "./ext/va-arg-pack-1.C"
                                         ;
      if (v != (void *) &v4)
 abort ();
      l = 
# 48 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 48 "./ext/va-arg-pack-1.C"
         ap
# 48 "./ext/va-arg-pack-1.C" 3 4
         ,
# 48 "./ext/va-arg-pack-1.C"
         long int
# 48 "./ext/va-arg-pack-1.C" 3 4
         )
# 48 "./ext/va-arg-pack-1.C"
                              ;
      if (l != 3 || v2 != 4)
 abort ();
      break;
    case 1:
      ld = 
# 53 "./ext/va-arg-pack-1.C" 3 4
          __builtin_va_arg(
# 53 "./ext/va-arg-pack-1.C"
          ap
# 53 "./ext/va-arg-pack-1.C" 3 4
          ,
# 53 "./ext/va-arg-pack-1.C"
          long double
# 53 "./ext/va-arg-pack-1.C" 3 4
          )
# 53 "./ext/va-arg-pack-1.C"
                                  ;
      if (ld != 41 || v5 != ld)
 abort ();
      i = 
# 56 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 56 "./ext/va-arg-pack-1.C"
         ap
# 56 "./ext/va-arg-pack-1.C" 3 4
         ,
# 56 "./ext/va-arg-pack-1.C"
         int
# 56 "./ext/va-arg-pack-1.C" 3 4
         )
# 56 "./ext/va-arg-pack-1.C"
                         ;
      if (i != 8)
 abort ();
      v = 
# 59 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 59 "./ext/va-arg-pack-1.C"
         ap
# 59 "./ext/va-arg-pack-1.C" 3 4
         ,
# 59 "./ext/va-arg-pack-1.C"
         void *
# 59 "./ext/va-arg-pack-1.C" 3 4
         )
# 59 "./ext/va-arg-pack-1.C"
                            ;
      if (v != &v2)
 abort ();
      break;
    case 2:
      break;
    default:
      abort ();
    }
  
# 68 "./ext/va-arg-pack-1.C" 3 4
 __builtin_va_end(
# 68 "./ext/va-arg-pack-1.C"
 ap
# 68 "./ext/va-arg-pack-1.C" 3 4
 )
# 68 "./ext/va-arg-pack-1.C"
            ;
  return x;
}

__attribute__ ((noinline)) int
foo2 (int x, int y, ...)
{
  long long int ll;
  void *v;
  struct A a, b;
  long double ld;
  va_list ap;

  
# 81 "./ext/va-arg-pack-1.C" 3 4
 __builtin_va_start(
# 81 "./ext/va-arg-pack-1.C"
 ap
# 81 "./ext/va-arg-pack-1.C" 3 4
 ,
# 81 "./ext/va-arg-pack-1.C"
 y
# 81 "./ext/va-arg-pack-1.C" 3 4
 )
# 81 "./ext/va-arg-pack-1.C"
                 ;
  if (x < 0 || x >= 20 || seen[x])
    abort ();
  seen[x] = ++cnt | 64;
  if (y != 10)
    abort ();
  switch (x)
    {
    case 11:
      break;
    case 12:
      ld = 
# 92 "./ext/va-arg-pack-1.C" 3 4
          __builtin_va_arg(
# 92 "./ext/va-arg-pack-1.C"
          ap
# 92 "./ext/va-arg-pack-1.C" 3 4
          ,
# 92 "./ext/va-arg-pack-1.C"
          long double
# 92 "./ext/va-arg-pack-1.C" 3 4
          )
# 92 "./ext/va-arg-pack-1.C"
                                  ;
      if (ld != 41 || v5 != 40)
 abort ();
      a = 
# 95 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 95 "./ext/va-arg-pack-1.C"
         ap
# 95 "./ext/va-arg-pack-1.C" 3 4
         ,
# 95 "./ext/va-arg-pack-1.C"
         struct A
# 95 "./ext/va-arg-pack-1.C" 3 4
         )
# 95 "./ext/va-arg-pack-1.C"
                              ;
      if (__builtin_memcmp (a.c, v4.c, sizeof (a.c)) != 0)
 abort ();
      b = 
# 98 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 98 "./ext/va-arg-pack-1.C"
         ap
# 98 "./ext/va-arg-pack-1.C" 3 4
         ,
# 98 "./ext/va-arg-pack-1.C"
         struct A
# 98 "./ext/va-arg-pack-1.C" 3 4
         )
# 98 "./ext/va-arg-pack-1.C"
                              ;
      if (__builtin_memcmp (b.c, v4.c, sizeof (b.c)) != 0)
 abort ();
      v = 
# 101 "./ext/va-arg-pack-1.C" 3 4
         __builtin_va_arg(
# 101 "./ext/va-arg-pack-1.C"
         ap
# 101 "./ext/va-arg-pack-1.C" 3 4
         ,
# 101 "./ext/va-arg-pack-1.C"
         void *
# 101 "./ext/va-arg-pack-1.C" 3 4
         )
# 101 "./ext/va-arg-pack-1.C"
                            ;
      if (v != &v2)
 abort ();
      ll = 
# 104 "./ext/va-arg-pack-1.C" 3 4
          __builtin_va_arg(
# 104 "./ext/va-arg-pack-1.C"
          ap
# 104 "./ext/va-arg-pack-1.C" 3 4
          ,
# 104 "./ext/va-arg-pack-1.C"
          long long int
# 104 "./ext/va-arg-pack-1.C" 3 4
          )
# 104 "./ext/va-arg-pack-1.C"
                                    ;
      if (ll != 16LL)
 abort ();
      break;
    case 2:
      break;
    default:
      abort ();
    }
  
# 113 "./ext/va-arg-pack-1.C" 3 4
 __builtin_va_end(
# 113 "./ext/va-arg-pack-1.C"
 ap
# 113 "./ext/va-arg-pack-1.C" 3 4
 )
# 113 "./ext/va-arg-pack-1.C"
            ;
  return x + 8;
}

__attribute__ ((noinline)) int
foo3 (void)
{
  return 6;
}

extern inline __attribute__ ((always_inline, gnu_inline)) int
bar (int x, ...)
{
  if (x < 10)
    return foo1 (x, foo3 (), 5, __builtin_va_arg_pack ());
  return foo2 (x, foo3 () + 4, __builtin_va_arg_pack ());
}

int
main (void)
{
  if (bar (0, ++v1, v4, &v4, v2++) != 0)
    abort ();
  if (bar (1, ++v5, 8, v3) != 1)
    abort ();
  if (bar (2) != 2)
    abort ();
  if (bar (v1 + 2) != 19)
    abort ();
  if (bar (v1 + 3, v5--, v4, v4, v3, 16LL) != 20)
    abort ();
  return 0;
}
