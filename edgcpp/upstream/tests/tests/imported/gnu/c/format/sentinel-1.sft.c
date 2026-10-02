//type: fn
//options: --c17 --strict_gnu
# 0 "./format/sentinel-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/sentinel-1.c"





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
# 7 "./format/sentinel-1.c" 2


# 8 "./format/sentinel-1.c"
extern int execl (const char *, const char *, ...);
extern int execlp (const char *, const char *, ...);
extern int execle (const char *, const char *, ...);
extern char *envp[];



extern int a __attribute__ ((__sentinel__));

extern void foo1 (const char *, ...) __attribute__ ((__sentinel__));
extern void foo2 (...) __attribute__ ((__sentinel__));
extern void foo3 () __attribute__ ((__sentinel__));
extern void foo4 (const char *, int) __attribute__ ((__sentinel__));
extern void foo5 (const char *, ...) __attribute__ ((__sentinel__(1)));
extern void foo6 (const char *, ...) __attribute__ ((__sentinel__(5)));
extern void foo7 (const char *, ...) __attribute__ ((__sentinel__(0)));
extern void foo8 (const char *, ...) __attribute__ ((__sentinel__("a")));
extern void foo9 (const char *, ...) __attribute__ ((__sentinel__(-1)));
extern void foo10 (const char *, ...) __attribute__ ((__sentinel__(1,3)));

extern void bar(void)
{
  foo1 ();
  foo1 (
# 31 "./format/sentinel-1.c" 3 4
       ((void *)0)
# 31 "./format/sentinel-1.c"
           );
  foo1 ("a");
  foo1 ("a", 1);
  foo1 ("a", 0);
  foo1 ("a", (void*)1);
  foo1 ("a", 
# 36 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 36 "./format/sentinel-1.c"
                , 1);
  foo1 ("a", 
# 37 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 37 "./format/sentinel-1.c"
                );

  foo5 (
# 39 "./format/sentinel-1.c" 3 4
       ((void *)0)
# 39 "./format/sentinel-1.c"
           );
  foo5 (
# 40 "./format/sentinel-1.c" 3 4
       ((void *)0)
# 40 "./format/sentinel-1.c"
           , 1);
  foo5 ("a", 
# 41 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 41 "./format/sentinel-1.c"
                );
  foo5 ("a", 
# 42 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 42 "./format/sentinel-1.c"
                , 1);
  foo5 ("a", 1, 2, 3, 
# 43 "./format/sentinel-1.c" 3 4
                     ((void *)0)
# 43 "./format/sentinel-1.c"
                         );
  foo5 ("a", 1, 2, 
# 44 "./format/sentinel-1.c" 3 4
                  ((void *)0)
# 44 "./format/sentinel-1.c"
                      , 3);
  foo5 ("a", 1, 
# 45 "./format/sentinel-1.c" 3 4
               ((void *)0)
# 45 "./format/sentinel-1.c"
                   , 2, 3);
  foo5 ("a", 
# 46 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 46 "./format/sentinel-1.c"
                , 1, 2, 3);
  foo5 ("a", 0, 1, 2, 3);

  foo6 ("a", 1, 
# 49 "./format/sentinel-1.c" 3 4
               ((void *)0)
# 49 "./format/sentinel-1.c"
                   );
  foo6 ("a", 1, 
# 50 "./format/sentinel-1.c" 3 4
               ((void *)0)
# 50 "./format/sentinel-1.c"
                   , 2);
  foo6 ("a", 1, 
# 51 "./format/sentinel-1.c" 3 4
               ((void *)0)
# 51 "./format/sentinel-1.c"
                   , 2, 3);
  foo6 ("a", 
# 52 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 52 "./format/sentinel-1.c"
                , 1, 2, 3);
  foo6 ("a", 
# 53 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 53 "./format/sentinel-1.c"
                , 1, 2, 3, 4);
  foo6 (
# 54 "./format/sentinel-1.c" 3 4
       ((void *)0)
# 54 "./format/sentinel-1.c"
           , 1, 2, 3, 4, 5);
  foo6 ("a", 
# 55 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 55 "./format/sentinel-1.c"
                , 1, 2, 3, 4, 5);
  foo6 ("a", 0, 
# 56 "./format/sentinel-1.c" 3 4
               ((void *)0)
# 56 "./format/sentinel-1.c"
                   , 1, 2, 3, 4, 5);
  foo6 ("a", 0, 1, 2, 3, 4, 5);
  foo6 ("a", 
# 58 "./format/sentinel-1.c" 3 4
            ((void *)0)
# 58 "./format/sentinel-1.c"
                , 1, 2, 3, 4, 5, 6);

  foo7 ("a", 1, 2, 3, 
# 60 "./format/sentinel-1.c" 3 4
                     ((void *)0)
# 60 "./format/sentinel-1.c"
                         );

  execl ("/bin/ls", "/bin/ls", "-aFC");
  execl ("/bin/ls", "/bin/ls", "-aFC", 0);
  execl ("/bin/ls", "/bin/ls", "-aFC", 
# 64 "./format/sentinel-1.c" 3 4
                                      ((void *)0)
# 64 "./format/sentinel-1.c"
                                          );

  execlp ("ls", "ls", "-aFC");
  execlp ("ls", "ls", "-aFC", 0);
  execlp ("ls", "ls", "-aFC", 
# 68 "./format/sentinel-1.c" 3 4
                             ((void *)0)
# 68 "./format/sentinel-1.c"
                                 );

  execle ("ls", "ls", "-aFC", ".", envp);
  execle ("ls", "ls", "-aFC", ".", 0, envp);
  execle ("ls", "ls", "-aFC", ".", 
# 72 "./format/sentinel-1.c" 3 4
                                  ((void *)0)
# 72 "./format/sentinel-1.c"
                                      , envp);
}
