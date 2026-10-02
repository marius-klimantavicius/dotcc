//type: fp
//options: 
# 0 "./nonnull-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./nonnull-3.c"






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
# 8 "./nonnull-3.c" 2


# 9 "./nonnull-3.c"
void
foo (void *p, char *s)
{
  __builtin_bzero (
# 12 "./nonnull-3.c" 3 4
                  ((void *)0)
# 12 "./nonnull-3.c"
                      , 0);
  __builtin_bcopy (
# 13 "./nonnull-3.c" 3 4
                  ((void *)0)
# 13 "./nonnull-3.c"
                      , p, 0);
  __builtin_bcopy (p, 
# 14 "./nonnull-3.c" 3 4
                     ((void *)0)
# 14 "./nonnull-3.c"
                         , 0);
  __builtin_bcmp (
# 15 "./nonnull-3.c" 3 4
                 ((void *)0)
# 15 "./nonnull-3.c"
                     , p, 0);
  __builtin_bcmp (p, 
# 16 "./nonnull-3.c" 3 4
                    ((void *)0)
# 16 "./nonnull-3.c"
                        , 0);
  __builtin_index (
# 17 "./nonnull-3.c" 3 4
                  ((void *)0)
# 17 "./nonnull-3.c"
                      , 16);
  __builtin_rindex (
# 18 "./nonnull-3.c" 3 4
                   ((void *)0)
# 18 "./nonnull-3.c"
                       , 16);

  __builtin_memcpy (p, 
# 20 "./nonnull-3.c" 3 4
                      ((void *)0)
# 20 "./nonnull-3.c"
                          , 16);
  __builtin_memcpy (
# 21 "./nonnull-3.c" 3 4
                   ((void *)0)
# 21 "./nonnull-3.c"
                       , p, 16);
  __builtin_memmove (p, 
# 22 "./nonnull-3.c" 3 4
                       ((void *)0)
# 22 "./nonnull-3.c"
                           , 16);
  __builtin_memmove (
# 23 "./nonnull-3.c" 3 4
                    ((void *)0)
# 23 "./nonnull-3.c"
                        , p, 16);
  __builtin_memcmp (p, 
# 24 "./nonnull-3.c" 3 4
                      ((void *)0)
# 24 "./nonnull-3.c"
                          , 16);
  __builtin_memcmp (
# 25 "./nonnull-3.c" 3 4
                   ((void *)0)
# 25 "./nonnull-3.c"
                       , p, 16);
  __builtin_memset (
# 26 "./nonnull-3.c" 3 4
                   ((void *)0)
# 26 "./nonnull-3.c"
                       , 0, 16);
  __builtin_mempcpy (p, 
# 27 "./nonnull-3.c" 3 4
                       ((void *)0)
# 27 "./nonnull-3.c"
                           , 16);
  __builtin_mempcpy (
# 28 "./nonnull-3.c" 3 4
                    ((void *)0)
# 28 "./nonnull-3.c"
                        , p, 16);

  __builtin_strcat (
# 30 "./nonnull-3.c" 3 4
                   ((void *)0)
# 30 "./nonnull-3.c"
                       , s);
  __builtin_strcat (s, 
# 31 "./nonnull-3.c" 3 4
                      ((void *)0)
# 31 "./nonnull-3.c"
                          );
  __builtin_strncat (
# 32 "./nonnull-3.c" 3 4
                    ((void *)0)
# 32 "./nonnull-3.c"
                        , s, 16);
  __builtin_strncat (s, 
# 33 "./nonnull-3.c" 3 4
                       ((void *)0)
# 33 "./nonnull-3.c"
                           , 16);
  __builtin_stpcpy (
# 34 "./nonnull-3.c" 3 4
                   ((void *)0)
# 34 "./nonnull-3.c"
                       , s);
  __builtin_stpcpy (s, 
# 35 "./nonnull-3.c" 3 4
                      ((void *)0)
# 35 "./nonnull-3.c"
                          );
  __builtin_stpncpy (
# 36 "./nonnull-3.c" 3 4
                    ((void *)0)
# 36 "./nonnull-3.c"
                        , s, 16);
  __builtin_stpncpy (s, 
# 37 "./nonnull-3.c" 3 4
                       ((void *)0)
# 37 "./nonnull-3.c"
                           , 16);
  __builtin_strcpy (
# 38 "./nonnull-3.c" 3 4
                   ((void *)0)
# 38 "./nonnull-3.c"
                       , s);
  __builtin_strcpy (s, 
# 39 "./nonnull-3.c" 3 4
                      ((void *)0)
# 39 "./nonnull-3.c"
                          );
  __builtin_strncpy (
# 40 "./nonnull-3.c" 3 4
                    ((void *)0)
# 40 "./nonnull-3.c"
                        , s, 16);
  __builtin_strncpy (s, 
# 41 "./nonnull-3.c" 3 4
                       ((void *)0)
# 41 "./nonnull-3.c"
                           , 16);
  __builtin_strcasecmp (
# 42 "./nonnull-3.c" 3 4
                       ((void *)0)
# 42 "./nonnull-3.c"
                           , s);
  __builtin_strcasecmp (s, 
# 43 "./nonnull-3.c" 3 4
                          ((void *)0)
# 43 "./nonnull-3.c"
                              );
  __builtin_strncasecmp (
# 44 "./nonnull-3.c" 3 4
                        ((void *)0)
# 44 "./nonnull-3.c"
                            , s, 16);
  __builtin_strncasecmp (s, 
# 45 "./nonnull-3.c" 3 4
                           ((void *)0)
# 45 "./nonnull-3.c"
                               , 16);
  __builtin_strcmp (
# 46 "./nonnull-3.c" 3 4
                   ((void *)0)
# 46 "./nonnull-3.c"
                       , s);
  __builtin_strcmp (s, 
# 47 "./nonnull-3.c" 3 4
                      ((void *)0)
# 47 "./nonnull-3.c"
                          );
  __builtin_strncmp (
# 48 "./nonnull-3.c" 3 4
                    ((void *)0)
# 48 "./nonnull-3.c"
                        , s, 16);
  __builtin_strncmp (s, 
# 49 "./nonnull-3.c" 3 4
                       ((void *)0)
# 49 "./nonnull-3.c"
                           , 16);
  __builtin_strlen (
# 50 "./nonnull-3.c" 3 4
                   ((void *)0)
# 50 "./nonnull-3.c"
                       );
  __builtin_strstr (
# 51 "./nonnull-3.c" 3 4
                   ((void *)0)
# 51 "./nonnull-3.c"
                       , s);
  __builtin_strstr (s, 
# 52 "./nonnull-3.c" 3 4
                      ((void *)0)
# 52 "./nonnull-3.c"
                          );
  __builtin_strpbrk (
# 53 "./nonnull-3.c" 3 4
                    ((void *)0)
# 53 "./nonnull-3.c"
                        , s);
  __builtin_strpbrk (s, 
# 54 "./nonnull-3.c" 3 4
                       ((void *)0)
# 54 "./nonnull-3.c"
                           );
  __builtin_strspn (
# 55 "./nonnull-3.c" 3 4
                   ((void *)0)
# 55 "./nonnull-3.c"
                       , s);
  __builtin_strspn (s, 
# 56 "./nonnull-3.c" 3 4
                      ((void *)0)
# 56 "./nonnull-3.c"
                          );
  __builtin_strchr (
# 57 "./nonnull-3.c" 3 4
                   ((void *)0)
# 57 "./nonnull-3.c"
                       , 16);
  __builtin_strrchr (
# 58 "./nonnull-3.c" 3 4
                    ((void *)0)
# 58 "./nonnull-3.c"
                        , 16);
  void *p1 = __builtin_strdup (
# 59 "./nonnull-3.c" 3 4
                              ((void *)0)
# 59 "./nonnull-3.c"
                                  );
  void *p2 = __builtin_strndup (
# 60 "./nonnull-3.c" 3 4
                               ((void *)0)
# 60 "./nonnull-3.c"
                                   , 16);

  __builtin_nan (
# 62 "./nonnull-3.c" 3 4
                ((void *)0)
# 62 "./nonnull-3.c"
                    );
  __builtin_nanf (
# 63 "./nonnull-3.c" 3 4
                 ((void *)0)
# 63 "./nonnull-3.c"
                     );
  __builtin_nanl (
# 64 "./nonnull-3.c" 3 4
                 ((void *)0)
# 64 "./nonnull-3.c"
                     );
  __builtin_nans (
# 65 "./nonnull-3.c" 3 4
                 ((void *)0)
# 65 "./nonnull-3.c"
                     );
  __builtin_nansf (
# 66 "./nonnull-3.c" 3 4
                  ((void *)0)
# 66 "./nonnull-3.c"
                      );
  __builtin_nansl (
# 67 "./nonnull-3.c" 3 4
                  ((void *)0)
# 67 "./nonnull-3.c"
                      );

  __builtin_puts (
# 69 "./nonnull-3.c" 3 4
                 ((void *)0)
# 69 "./nonnull-3.c"
                     );
  __builtin_fputc (*s, 
# 70 "./nonnull-3.c" 3 4
                      ((void *)0)
# 70 "./nonnull-3.c"
                          );
  __builtin_fputs (s, 
# 71 "./nonnull-3.c" 3 4
                     ((void *)0)
# 71 "./nonnull-3.c"
                         );
  __builtin_fputs (
# 72 "./nonnull-3.c" 3 4
                  ((void *)0)
# 72 "./nonnull-3.c"
                      , p);
  __builtin_fwrite (s, 16, 16, 
# 73 "./nonnull-3.c" 3 4
                              ((void *)0)
# 73 "./nonnull-3.c"
                                  );
  __builtin_fwrite (
# 74 "./nonnull-3.c" 3 4
                   ((void *)0)
# 74 "./nonnull-3.c"
                       , 16, 16, p);
  __builtin_puts_unlocked (
# 75 "./nonnull-3.c" 3 4
                          ((void *)0)
# 75 "./nonnull-3.c"
                              );
  __builtin_fputc_unlocked (*s, 
# 76 "./nonnull-3.c" 3 4
                               ((void *)0)
# 76 "./nonnull-3.c"
                                   );
  __builtin_fputs_unlocked (s, 
# 77 "./nonnull-3.c" 3 4
                              ((void *)0)
# 77 "./nonnull-3.c"
                                  );
  __builtin_fputs_unlocked (
# 78 "./nonnull-3.c" 3 4
                           ((void *)0)
# 78 "./nonnull-3.c"
                               , p);
  __builtin_fwrite_unlocked (s, 16, 16, 
# 79 "./nonnull-3.c" 3 4
                                       ((void *)0)
# 79 "./nonnull-3.c"
                                           );
  __builtin_fwrite_unlocked (
# 80 "./nonnull-3.c" 3 4
                            ((void *)0)
# 80 "./nonnull-3.c"
                                , 16, 16, p);

}
