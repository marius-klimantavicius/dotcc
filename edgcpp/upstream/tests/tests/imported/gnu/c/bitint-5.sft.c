//type: fp
//options: --c23
# 0 "./bitint-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./bitint-5.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./bitint-5.c" 2
# 1 "/usr/include/stdlib.h" 1 3 4
# 24 "/usr/include/stdlib.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 25 "/usr/include/stdlib.h" 2 3 4







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 33 "/usr/include/stdlib.h" 2 3 4


# 95 "/usr/include/stdlib.h" 3 4


typedef struct
  {
    int quot;
    int rem;
  } div_t;



typedef struct
  {
    long int quot;
    long int rem;
  } ldiv_t;







__extension__ typedef struct
  {
    long long int quot;
    long long int rem;
  } lldiv_t;


# 139 "/usr/include/stdlib.h" 3 4
extern size_t __ctype_get_mb_cur_max (void) __attribute__ ((__nothrow__ , __leaf__)) ;




extern double atof (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern int atoi (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern long int atol (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





__extension__ extern long long int atoll (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





extern double strtod (const char *__restrict __nptr,
        char **__restrict __endptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





extern float strtof (const char *__restrict __nptr,
       char **__restrict __endptr) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

extern long double strtold (const char *__restrict __nptr,
       char **__restrict __endptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





extern long int strtol (const char *__restrict __nptr,
   char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

extern unsigned long int strtoul (const char *__restrict __nptr,
      char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

# 206 "/usr/include/stdlib.h" 3 4


__extension__
extern long long int strtoll (const char *__restrict __nptr,
         char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtoull (const char *__restrict __nptr,
     char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

# 372 "/usr/include/stdlib.h" 3 4


extern int rand (void) __attribute__ ((__nothrow__ , __leaf__));

extern void srand (unsigned int __seed) __attribute__ ((__nothrow__ , __leaf__));

# 463 "/usr/include/stdlib.h" 3 4


extern void *malloc (size_t __size) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;

extern void *calloc (size_t __nmemb, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;










extern void *realloc (void *__ptr, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__warn_unused_result__));

extern void free (void *__ptr) __attribute__ ((__nothrow__ , __leaf__));

# 508 "/usr/include/stdlib.h" 3 4
extern void *aligned_alloc (size_t __alignment, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__, __alloc_size__ (2)));




extern void abort (void) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



extern int atexit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));







extern int at_quick_exit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



# 538 "/usr/include/stdlib.h" 3 4




extern void exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));





extern void quick_exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));







extern void _Exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));






extern char *getenv (const char *__name) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;

# 711 "/usr/include/stdlib.h" 3 4





extern int system (const char *__command) ;

# 741 "/usr/include/stdlib.h" 3 4
typedef int (*__compar_fn_t) (const void *, const void *);
# 751 "/usr/include/stdlib.h" 3 4



extern void *bsearch (const void *__key, const void *__base,
        size_t __nmemb, size_t __size, __compar_fn_t __compar)
     __attribute__ ((__nonnull__ (1, 2, 5))) ;



extern void qsort (void *__base, size_t __nmemb, size_t __size,
     __compar_fn_t __compar) __attribute__ ((__nonnull__ (1, 4)));
# 770 "/usr/include/stdlib.h" 3 4
extern int abs (int __x) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;
extern long int labs (long int __x) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;



__extension__ extern long long int llabs (long long int __x)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;







extern div_t div (int __numer, int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;
extern ldiv_t ldiv (long int __numer, long int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;




__extension__ extern lldiv_t lldiv (long long int __numer,
        long long int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;

# 856 "/usr/include/stdlib.h" 3 4



extern int mblen (const char *__s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int mbtowc (wchar_t *__restrict __pwc,
     const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int wctomb (char *__s, wchar_t __wchar) __attribute__ ((__nothrow__ , __leaf__)) ;



extern size_t mbstowcs (wchar_t *__restrict __pwcs,
   const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__));

extern size_t wcstombs (char *__restrict __s,
   const wchar_t *__restrict __pwcs, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__));

# 951 "/usr/include/stdlib.h" 3 4
# 1 "/usr/include/bits/stdlib-float.h" 1 3 4
# 952 "/usr/include/stdlib.h" 2 3 4
# 964 "/usr/include/stdlib.h" 3 4

# 7 "./bitint-5.c" 2



# 9 "./bitint-5.c"
unsigned _BitInt(128) b, c;


signed _BitInt(575) d, e;


void
foo (int a, ...)
{
  va_list ap;
  
# 19 "./bitint-5.c" 3 4
 __builtin_c23_va_start(
# 19 "./bitint-5.c"
 ap, a
# 19 "./bitint-5.c" 3 4
 )
# 19 "./bitint-5.c"
                 ;
  if (a == 1)
    {
      if (
# 22 "./bitint-5.c" 3 4
         __builtin_va_arg(
# 22 "./bitint-5.c"
         ap
# 22 "./bitint-5.c" 3 4
         ,
# 22 "./bitint-5.c"
         _BitInt(2)
# 22 "./bitint-5.c" 3 4
         ) 
# 22 "./bitint-5.c"
                                 != 1wb)
 abort ();
      if (
# 24 "./bitint-5.c" 3 4
         __builtin_va_arg(
# 24 "./bitint-5.c"
         ap
# 24 "./bitint-5.c" 3 4
         ,
# 24 "./bitint-5.c"
         _BitInt(3)
# 24 "./bitint-5.c" 3 4
         ) 
# 24 "./bitint-5.c"
                                 != 3wb)
 abort ();
      if (
# 26 "./bitint-5.c" 3 4
         __builtin_va_arg(
# 26 "./bitint-5.c"
         ap
# 26 "./bitint-5.c" 3 4
         ,
# 26 "./bitint-5.c"
         _BitInt(15)
# 26 "./bitint-5.c" 3 4
         ) 
# 26 "./bitint-5.c"
                                  != 16383wb)
 abort ();
      if (
# 28 "./bitint-5.c" 3 4
         __builtin_va_arg(
# 28 "./bitint-5.c"
         ap
# 28 "./bitint-5.c" 3 4
         ,
# 28 "./bitint-5.c"
         unsigned _BitInt(32)
# 28 "./bitint-5.c" 3 4
         ) 
# 28 "./bitint-5.c"
                                           != 4294967295uwb)
 abort ();
      if (
# 30 "./bitint-5.c" 3 4
         __builtin_va_arg(
# 30 "./bitint-5.c"
         ap
# 30 "./bitint-5.c" 3 4
         ,
# 30 "./bitint-5.c"
         _BitInt(64)
# 30 "./bitint-5.c" 3 4
         ) 
# 30 "./bitint-5.c"
                                  != 0x7fffffffffffffffwb)
 abort ();
    }

  b = 
# 34 "./bitint-5.c" 3 4
     __builtin_va_arg(
# 34 "./bitint-5.c"
     ap
# 34 "./bitint-5.c" 3 4
     ,
# 34 "./bitint-5.c"
     unsigned _BitInt(128)
# 34 "./bitint-5.c" 3 4
     )
# 34 "./bitint-5.c"
                                       ;


  d = 
# 37 "./bitint-5.c" 3 4
     __builtin_va_arg(
# 37 "./bitint-5.c"
     ap
# 37 "./bitint-5.c" 3 4
     ,
# 37 "./bitint-5.c"
     _BitInt(575)
# 37 "./bitint-5.c" 3 4
     )
# 37 "./bitint-5.c"
                              ;

  if (
# 39 "./bitint-5.c" 3 4
     __builtin_va_arg(
# 39 "./bitint-5.c"
     ap
# 39 "./bitint-5.c" 3 4
     ,
# 39 "./bitint-5.c"
     int
# 39 "./bitint-5.c" 3 4
     ) 
# 39 "./bitint-5.c"
                      != 42)
    abort ();
  
# 41 "./bitint-5.c" 3 4
 __builtin_va_end(
# 41 "./bitint-5.c"
 ap
# 41 "./bitint-5.c" 3 4
 )
# 41 "./bitint-5.c"
            ;
}

void
bar (void)
{
  foo (1, 1wb, 3wb, 16383wb, 4294967295uwb, 9223372036854775807wb,

       c,


       e,

       42);
  foo (2,

       c,


       e,

       42);
}
