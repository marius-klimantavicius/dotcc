//type: fp
//options: 
# 0 "./nonnull-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./nonnull-1.c"





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
# 7 "./nonnull-1.c" 2


# 8 "./nonnull-1.c"
extern void func1 (char *, char *, int) __attribute__((nonnull));

extern void func2 (char *, char *) __attribute__((nonnull(1)));

extern void func3 (char *, int, char *, int)
  __attribute__((nonnull(1,3)));

extern void func4 (char *, char *) __attribute__((nonnull(1)))
  __attribute__((nonnull(2)));

void
foo (int i1, int i2, int i3, char *cp1, char *cp2, char *cp3)
{
  func1(cp1, cp2, i1);

  func1(
# 23 "./nonnull-1.c" 3 4
       ((void *)0)
# 23 "./nonnull-1.c"
           , cp2, i1);
  func1(cp1, 
# 24 "./nonnull-1.c" 3 4
            ((void *)0)
# 24 "./nonnull-1.c"
                , i1);
  func1(cp1, cp2, 0);

  func2(cp1, 
# 27 "./nonnull-1.c" 3 4
            ((void *)0)
# 27 "./nonnull-1.c"
                );
  func2(
# 28 "./nonnull-1.c" 3 4
       ((void *)0)
# 28 "./nonnull-1.c"
           , cp1);

  func3(
# 30 "./nonnull-1.c" 3 4
       ((void *)0)
# 30 "./nonnull-1.c"
           , i2, cp3, i3);
  func3(cp3, i2, 
# 31 "./nonnull-1.c" 3 4
                ((void *)0)
# 31 "./nonnull-1.c"
                    , i3);

  func1(i1 ? cp1 : 
# 33 "./nonnull-1.c" 3 4
                  ((void *)0)
# 33 "./nonnull-1.c"
                      , cp2, i3);
  func1(i1 ? 
# 34 "./nonnull-1.c" 3 4
            ((void *)0) 
# 34 "./nonnull-1.c"
                 : cp1, cp2, i3);
  func1(i1 ? (i2 ? cp1 : 
# 35 "./nonnull-1.c" 3 4
                        ((void *)0)
# 35 "./nonnull-1.c"
                            ) : cp2, cp3, i3);

  func4(
# 37 "./nonnull-1.c" 3 4
       ((void *)0)
# 37 "./nonnull-1.c"
           , cp1);
  func4(cp1, 
# 38 "./nonnull-1.c" 3 4
            ((void *)0)
# 38 "./nonnull-1.c"
                );
}
