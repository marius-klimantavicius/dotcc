//type: fp
//options: 
# 0 "./tree-prof/va-arg-pack-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-prof/va-arg-pack-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./tree-prof/va-arg-pack-1.c" 2


# 7 "./tree-prof/va-arg-pack-1.c"
extern void abort (void);

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

  
# 27 "./tree-prof/va-arg-pack-1.c" 3 4
 __builtin_c23_va_start(
# 27 "./tree-prof/va-arg-pack-1.c"
 ap, y
# 27 "./tree-prof/va-arg-pack-1.c" 3 4
 )
# 27 "./tree-prof/va-arg-pack-1.c"
                 ;
  if (x < 0 || x >= 20 || seen[x])
    abort ();
  seen[x] = ++cnt;
  if (y != 6)
    abort ();
  i = 
# 33 "./tree-prof/va-arg-pack-1.c" 3 4
     __builtin_va_arg(
# 33 "./tree-prof/va-arg-pack-1.c"
     ap
# 33 "./tree-prof/va-arg-pack-1.c" 3 4
     ,
# 33 "./tree-prof/va-arg-pack-1.c"
     int
# 33 "./tree-prof/va-arg-pack-1.c" 3 4
     )
# 33 "./tree-prof/va-arg-pack-1.c"
                     ;
  if (i != 5)
    abort ();
  switch (x)
    {
    case 0:
      i = 
# 39 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 39 "./tree-prof/va-arg-pack-1.c"
         ap
# 39 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 39 "./tree-prof/va-arg-pack-1.c"
         int
# 39 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 39 "./tree-prof/va-arg-pack-1.c"
                         ;
      if (i != 9 || v1 != 9)
 abort ();
      a = 
# 42 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 42 "./tree-prof/va-arg-pack-1.c"
         ap
# 42 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 42 "./tree-prof/va-arg-pack-1.c"
         struct A
# 42 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 42 "./tree-prof/va-arg-pack-1.c"
                              ;
      if (__builtin_memcmp (a.c, v4.c, sizeof (a.c)) != 0)
 abort ();
      v = (void *) 
# 45 "./tree-prof/va-arg-pack-1.c" 3 4
                  __builtin_va_arg(
# 45 "./tree-prof/va-arg-pack-1.c"
                  ap
# 45 "./tree-prof/va-arg-pack-1.c" 3 4
                  ,
# 45 "./tree-prof/va-arg-pack-1.c"
                  struct A *
# 45 "./tree-prof/va-arg-pack-1.c" 3 4
                  )
# 45 "./tree-prof/va-arg-pack-1.c"
                                         ;
      if (v != (void *) &v4)
 abort ();
      l = 
# 48 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 48 "./tree-prof/va-arg-pack-1.c"
         ap
# 48 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 48 "./tree-prof/va-arg-pack-1.c"
         long int
# 48 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 48 "./tree-prof/va-arg-pack-1.c"
                              ;
      if (l != 3 || v2 != 4)
 abort ();
      break;
    case 1:
      ld = 
# 53 "./tree-prof/va-arg-pack-1.c" 3 4
          __builtin_va_arg(
# 53 "./tree-prof/va-arg-pack-1.c"
          ap
# 53 "./tree-prof/va-arg-pack-1.c" 3 4
          ,
# 53 "./tree-prof/va-arg-pack-1.c"
          long double
# 53 "./tree-prof/va-arg-pack-1.c" 3 4
          )
# 53 "./tree-prof/va-arg-pack-1.c"
                                  ;
      if (ld != 41 || v5 != ld)
 abort ();
      i = 
# 56 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 56 "./tree-prof/va-arg-pack-1.c"
         ap
# 56 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 56 "./tree-prof/va-arg-pack-1.c"
         int
# 56 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 56 "./tree-prof/va-arg-pack-1.c"
                         ;
      if (i != 8)
 abort ();
      v = 
# 59 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 59 "./tree-prof/va-arg-pack-1.c"
         ap
# 59 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 59 "./tree-prof/va-arg-pack-1.c"
         void *
# 59 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 59 "./tree-prof/va-arg-pack-1.c"
                            ;
      if (v != &v2)
 abort ();
      break;
    case 2:
      break;
    default:
      abort ();
    }
  
# 68 "./tree-prof/va-arg-pack-1.c" 3 4
 __builtin_va_end(
# 68 "./tree-prof/va-arg-pack-1.c"
 ap
# 68 "./tree-prof/va-arg-pack-1.c" 3 4
 )
# 68 "./tree-prof/va-arg-pack-1.c"
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

  
# 81 "./tree-prof/va-arg-pack-1.c" 3 4
 __builtin_c23_va_start(
# 81 "./tree-prof/va-arg-pack-1.c"
 ap, y
# 81 "./tree-prof/va-arg-pack-1.c" 3 4
 )
# 81 "./tree-prof/va-arg-pack-1.c"
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
# 92 "./tree-prof/va-arg-pack-1.c" 3 4
          __builtin_va_arg(
# 92 "./tree-prof/va-arg-pack-1.c"
          ap
# 92 "./tree-prof/va-arg-pack-1.c" 3 4
          ,
# 92 "./tree-prof/va-arg-pack-1.c"
          long double
# 92 "./tree-prof/va-arg-pack-1.c" 3 4
          )
# 92 "./tree-prof/va-arg-pack-1.c"
                                  ;
      if (ld != 41 || v5 != 40)
 abort ();
      a = 
# 95 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 95 "./tree-prof/va-arg-pack-1.c"
         ap
# 95 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 95 "./tree-prof/va-arg-pack-1.c"
         struct A
# 95 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 95 "./tree-prof/va-arg-pack-1.c"
                              ;
      if (__builtin_memcmp (a.c, v4.c, sizeof (a.c)) != 0)
 abort ();
      b = 
# 98 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 98 "./tree-prof/va-arg-pack-1.c"
         ap
# 98 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 98 "./tree-prof/va-arg-pack-1.c"
         struct A
# 98 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 98 "./tree-prof/va-arg-pack-1.c"
                              ;
      if (__builtin_memcmp (b.c, v4.c, sizeof (b.c)) != 0)
 abort ();
      v = 
# 101 "./tree-prof/va-arg-pack-1.c" 3 4
         __builtin_va_arg(
# 101 "./tree-prof/va-arg-pack-1.c"
         ap
# 101 "./tree-prof/va-arg-pack-1.c" 3 4
         ,
# 101 "./tree-prof/va-arg-pack-1.c"
         void *
# 101 "./tree-prof/va-arg-pack-1.c" 3 4
         )
# 101 "./tree-prof/va-arg-pack-1.c"
                            ;
      if (v != &v2)
 abort ();
      ll = 
# 104 "./tree-prof/va-arg-pack-1.c" 3 4
          __builtin_va_arg(
# 104 "./tree-prof/va-arg-pack-1.c"
          ap
# 104 "./tree-prof/va-arg-pack-1.c" 3 4
          ,
# 104 "./tree-prof/va-arg-pack-1.c"
          long long int
# 104 "./tree-prof/va-arg-pack-1.c" 3 4
          )
# 104 "./tree-prof/va-arg-pack-1.c"
                                    ;
      if (ll != 16LL)
 abort ();
      break;
    case 2:
      break;
    default:
      abort ();
    }
  
# 113 "./tree-prof/va-arg-pack-1.c" 3 4
 __builtin_va_end(
# 113 "./tree-prof/va-arg-pack-1.c"
 ap
# 113 "./tree-prof/va-arg-pack-1.c" 3 4
 )
# 113 "./tree-prof/va-arg-pack-1.c"
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
