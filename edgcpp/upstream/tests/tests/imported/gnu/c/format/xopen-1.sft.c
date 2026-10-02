//type: fp
//options: --c99 --strict_gnu
# 0 "./format/xopen-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/xopen-1.c"







# 1 "./format/format.h" 1
# 35 "./format/format.h"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 36 "./format/format.h" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 37 "./format/format.h" 2






# 42 "./format/format.h"
typedef unsigned int wint_t;
# 61 "./format/format.h"
typedef long signed int signed_size_t;

typedef long signed int ssize_t;


typedef unsigned long int unsigned_ptrdiff_t;


__extension__ typedef long long int llong;
__extension__ typedef unsigned long long int ullong;



typedef llong quad_t;
typedef ullong u_quad_t;

__extension__ typedef long int intmax_t;
__extension__ typedef long unsigned int uintmax_t;

__extension__ typedef signed char int_least8_t;
__extension__ typedef short int int_least16_t;
__extension__ typedef int int_least32_t;
__extension__ typedef long int int_least64_t;
__extension__ typedef unsigned char uint_least8_t;
__extension__ typedef short unsigned int uint_least16_t;
__extension__ typedef unsigned int uint_least32_t;
__extension__ typedef long unsigned int uint_least64_t;

__extension__ typedef signed char int_fast8_t;
__extension__ typedef long int int_fast16_t;
__extension__ typedef long int int_fast32_t;
__extension__ typedef long int int_fast64_t;
__extension__ typedef unsigned char uint_fast8_t;
__extension__ typedef long unsigned int uint_fast16_t;
__extension__ typedef long unsigned int uint_fast32_t;
__extension__ typedef long unsigned int uint_fast64_t;
# 105 "./format/format.h"
typedef struct _FILE FILE;
extern FILE *stdin;
extern FILE *stdout;

extern int fprintf (FILE *restrict, const char *restrict, ...);
extern int printf (const char *restrict, ...);
extern int fprintf_unlocked (FILE *restrict, const char *restrict, ...);
extern int printf_unlocked (const char *restrict, ...);
extern int sprintf (char *restrict, const char *restrict, ...);
extern int vfprintf (FILE *restrict, const char *restrict, va_list);
extern int vprintf (const char *restrict, va_list);
extern int vsprintf (char *restrict, const char *restrict, va_list);
extern int snprintf (char *restrict, size_t, const char *restrict, ...);
extern int vsnprintf (char *restrict, size_t, const char *restrict, va_list);

extern int fscanf (FILE *restrict, const char *restrict, ...);
extern int scanf (const char *restrict, ...);
extern int sscanf (const char *restrict, const char *restrict, ...);
extern int vfscanf (FILE *restrict, const char *restrict, va_list);
extern int vscanf (const char *restrict, va_list);
extern int vsscanf (const char *restrict, const char *restrict, va_list);

extern char *gettext (const char *);
extern char *dgettext (const char *, const char *);
extern char *dcgettext (const char *, const char *, int);

struct tm;

extern size_t strftime (char *restrict, size_t, const char *restrict,
   const struct tm *restrict);

extern ssize_t strfmon (char *restrict, size_t, const char *restrict, ...);
# 9 "./format/xopen-1.c" 2

void
foo (int i, unsigned int u, wint_t lc, wchar_t *ls, int *ip, double d,
     char *s, void *p, int *n, long int l, int i2, float *fp, long int *lp,
     va_list va)
{



  printf ("%C", lc);
  printf ("%3C", lc);
  printf ("%.3C", lc);
  printf ("%hC", lc);
  printf ("%hhC", lc);
  printf ("%lC", lc);
  printf ("%llC", lc);
  printf ("%jC", lc);
  printf ("%zC", lc);
  printf ("%tC", lc);
  printf ("%LC", lc);
  printf ("%-C", lc);
  printf ("%+C", lc);
  printf ("% C", lc);
  printf ("%#C", lc);
  printf ("%0C", lc);
  printf ("%'C", lc);
  printf ("%S", ls);
  printf ("%3S", ls);
  printf ("%.3S", ls);
  printf ("%hS", ls);
  printf ("%hhS", ls);
  printf ("%lS", ls);
  printf ("%llS", ls);
  printf ("%jS", ls);
  printf ("%zS", ls);
  printf ("%tS", ls);
  printf ("%LS", ls);
  printf ("%-S", ls);
  printf ("%+S", ls);
  printf ("% S", ls);
  printf ("%#S", ls);
  printf ("%0S", ls);
  printf ("%'S", ls);
  scanf ("%C", ls);
  scanf ("%S", ls);
  scanf ("%*C%*S");
  scanf ("%2C%3S", ls, ls);
  scanf ("%hC", ls);
  scanf ("%hhC", ls);
  scanf ("%lC", ls);
  scanf ("%llC", ls);
  scanf ("%jC", ls);
  scanf ("%zC", ls);
  scanf ("%tC", ls);
  scanf ("%LC", ls);
  scanf ("%hS", ls);
  scanf ("%hhS", ls);
  scanf ("%lS", ls);
  scanf ("%llS", ls);
  scanf ("%jS", ls);
  scanf ("%zS", ls);
  scanf ("%tS", ls);
  scanf ("%LS", ls);



  scanf ("%aS", fp);

  printf ("%'d%'i%'u%'f%'F%'g%'G", i, i, u, d, d, d, d);
  printf ("%'o", u);
  printf ("%'x", u);
  printf ("%'X", u);
  printf ("%'e", d);
  printf ("%'E", d);
  printf ("%'a", d);
  printf ("%'A", d);
  printf ("%'c", i);
  printf ("%'s", s);
  printf ("%'p", p);
  printf ("%'n", n);

  scanf ("%1$d", ip);
  printf ("%1$d", i);
  printf ("%1$d", l);
  printf ("%3$*2$.*1$ld", i2, i, l);
  printf ("%4$ld%7$ld%5$d%6$d%3$d%1$d%2$d", i, i, i, l, i, i, l);
  scanf ("%4$ld%7$ld%5$d%6$d%3$d%1$d%2$d", ip, ip, ip, lp, ip, ip, lp);
  printf ("%1$d%d", i, i);
  printf ("%%%1$d%%%2$d", i, i);
  printf ("%d%2$d", i);
  printf ("%1$*d", i, i);
  printf ("%*1$d", i);
  scanf ("%1$d%d", ip, ip);
  scanf ("%*f%%%1$d%%%2$d", ip, ip);
  printf ("%2$d", i);
  printf ("%0$d", i);
  printf ("%3$d%1$d", i, i, i);
  printf ("%2$d%1$d", i, i, i);
  vprintf ("%3$d%1$d", va);




  scanf ("%3$d%1$d", ip, ip, ip);


  scanf ("%3$d%1$d", ip, ip, ip, i);

  scanf ("%3$d%1$d", ip, i, ip);


  vscanf ("%3$d%1$d", va);

  scanf ("%2$*d%1$d", ip, ip);
  printf ("%1$d%1$d", i);
  scanf ("%1$d%1$d", ip);
}
