//type: fp
//options: 
# 0 "./nonnull-8.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./nonnull-8.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 6 "./nonnull-8.c" 2


# 7 "./nonnull-8.c"
extern void func1 (char *, char *, int)
  __attribute__((nonnull_if_nonzero (1, 3), nonnull_if_nonzero (2, 3)));

extern void func2 (char *, char *, unsigned long)
  __attribute__((nonnull_if_nonzero (1, 3)));

enum E { E0 = 0, E1 = 0x7fffffff };
extern void func3 (char *, int, char *, enum E)
  __attribute__((nonnull_if_nonzero (1, 4), nonnull_if_nonzero (3, 2)));

extern void func4 (long, char *, char *, long)
  __attribute__((nonnull_if_nonzero (2, 1)))
  __attribute__((nonnull_if_nonzero (3, 4)));

void
foo (int i1, int i2, int i3, char *cp1, char *cp2, char *cp3)
{
  func1 (cp1, cp2, i1);
  func1 (cp1, cp2, 0);
  func1 (cp1, cp2, 42);
  func1 (
# 27 "./nonnull-8.c" 3 4
        ((void *)0)
# 27 "./nonnull-8.c"
            , 
# 27 "./nonnull-8.c" 3 4
              ((void *)0)
# 27 "./nonnull-8.c"
                  , 0);
  func1 (
# 28 "./nonnull-8.c" 3 4
        ((void *)0)
# 28 "./nonnull-8.c"
            , 
# 28 "./nonnull-8.c" 3 4
              ((void *)0)
# 28 "./nonnull-8.c"
                  , i1);

  func1 (
# 30 "./nonnull-8.c" 3 4
        ((void *)0)
# 30 "./nonnull-8.c"
            , cp2, 42);
  func1 (cp1, 
# 31 "./nonnull-8.c" 3 4
             ((void *)0)
# 31 "./nonnull-8.c"
                 , 1);

  func2 (cp1, 
# 33 "./nonnull-8.c" 3 4
             ((void *)0)
# 33 "./nonnull-8.c"
                 , 17);
  func2 (
# 34 "./nonnull-8.c" 3 4
        ((void *)0)
# 34 "./nonnull-8.c"
            , cp2, 0);
  func2 (
# 35 "./nonnull-8.c" 3 4
        ((void *)0)
# 35 "./nonnull-8.c"
            , cp1, 2);

  func3 (
# 37 "./nonnull-8.c" 3 4
        ((void *)0)
# 37 "./nonnull-8.c"
            , i2, cp3, i3);
  func3 (cp1, i2, 
# 38 "./nonnull-8.c" 3 4
                 ((void *)0)
# 38 "./nonnull-8.c"
                     , i3);
  func3 (
# 39 "./nonnull-8.c" 3 4
        ((void *)0)
# 39 "./nonnull-8.c"
            , i2, cp3, E0);
  func3 (cp1, 0, 
# 40 "./nonnull-8.c" 3 4
                ((void *)0)
# 40 "./nonnull-8.c"
                    , E1);
  func3 (
# 41 "./nonnull-8.c" 3 4
        ((void *)0)
# 41 "./nonnull-8.c"
            , i2, cp3, E1);
  func3 (cp3, 5, 
# 42 "./nonnull-8.c" 3 4
                ((void *)0)
# 42 "./nonnull-8.c"
                    , i3);

  func1 (i2 ? cp1 : 
# 44 "./nonnull-8.c" 3 4
                   ((void *)0)
# 44 "./nonnull-8.c"
                       , cp2, i3);
  func1 (i2 ? 
# 45 "./nonnull-8.c" 3 4
             ((void *)0) 
# 45 "./nonnull-8.c"
                  : cp1, cp2, i3);
  func1 (i2 ? (i3 ? cp1 : 
# 46 "./nonnull-8.c" 3 4
                         ((void *)0)
# 46 "./nonnull-8.c"
                             ) : cp2, cp3, i1);
  func1 (i1 ? cp1 : 
# 47 "./nonnull-8.c" 3 4
                   ((void *)0)
# 47 "./nonnull-8.c"
                       , cp2, 0);
  func1 (i1 ? 
# 48 "./nonnull-8.c" 3 4
             ((void *)0) 
# 48 "./nonnull-8.c"
                  : cp1, cp2, 0);
  func1 (i1 ? (i2 ? cp1 : 
# 49 "./nonnull-8.c" 3 4
                         ((void *)0)
# 49 "./nonnull-8.c"
                             ) : cp2, cp3, 0);
  func1 (i1 ? cp1 : 
# 50 "./nonnull-8.c" 3 4
                   ((void *)0)
# 50 "./nonnull-8.c"
                       , cp2, 1);
  func1 (i1 ? 
# 51 "./nonnull-8.c" 3 4
             ((void *)0) 
# 51 "./nonnull-8.c"
                  : cp1, cp2, 2);
  func1 (i1 ? (i2 ? cp1 : 
# 52 "./nonnull-8.c" 3 4
                         ((void *)0)
# 52 "./nonnull-8.c"
                             ) : cp2, cp3, 3);

  func4 (0, 
# 54 "./nonnull-8.c" 3 4
           ((void *)0)
# 54 "./nonnull-8.c"
               , 
# 54 "./nonnull-8.c" 3 4
                 ((void *)0)
# 54 "./nonnull-8.c"
                     , 0);
  func4 (-1, 
# 55 "./nonnull-8.c" 3 4
            ((void *)0)
# 55 "./nonnull-8.c"
                , cp1, 0);
  func4 (0, cp1, 
# 56 "./nonnull-8.c" 3 4
                ((void *)0)
# 56 "./nonnull-8.c"
                    , 77);
}
