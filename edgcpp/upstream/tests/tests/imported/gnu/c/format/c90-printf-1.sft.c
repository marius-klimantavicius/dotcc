//type: fp
//options: --c99 --strict_gnu
# 0 "./format/c90-printf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/c90-printf-1.c"
# 9 "./format/c90-printf-1.c"
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
# 10 "./format/c90-printf-1.c" 2

void
foo (int i, int i1, int i2, unsigned int u, double d, char *s, void *p,
     int *n, short int *hn, long int l, unsigned long int ul,
     long int *ln, long double ld, wint_t lc, wchar_t *ls, llong ll,
     ullong ull, unsigned int *un, const int *cn, signed char *ss,
     unsigned char *us, const signed char *css, unsigned int u1,
     unsigned int u2)
{


  printf ("%d\n", i);
  printf ("%+d\n", i);
  printf ("%3d\n", i);
  printf ("%-3d\n", i);
  printf ("%.7d\n", i);
  printf ("%+9.4d\n", i);
  printf ("%.3ld\n", l);
  printf ("%*d\n", i1, i);
  printf ("%.*d\n", i2, i);
  printf ("%*.*ld\n", i1, i2, l);
  printf ("%d %lu\n", i, ul);



  printf ("%.e\n", d);

  printf ("%5n\n", n);






  printf ("%.-5d\n", i);

  printf ("%.-*d\n", i);

  printf ("%.3c\n", i);
  printf ("%.3p\n", p);
  printf ("%.3n\n", n);




  printf ("%%");
  printf ("%.3%");

  printf ("%-%");

  printf ("%-%\n");

  printf ("%5%\n");

  printf ("%h%\n");


  printf ("%hd", i);
  printf ("%hi", i);






  printf ("%ho%hu%hx%hX", u, u, u, u);
  printf ("%hn", hn);
  printf (" %hf", d);
  printf (" %he", d);
  printf (" %hE", d);
  printf (" %hg", d);
  printf (" %hG", d);
  printf (" %hc", i);
  printf (" %hs", s);
  printf (" %hp", p);
  printf (" %h");
  printf (" %h.");
  printf (" %ld%li%lo%lu%lx%lX", l, l, ul, ul, ul, ul);
  printf (" %ln", ln);
  printf (" %lf", d);
  printf (" %le", d);
  printf (" %lE", d);
  printf (" %lg", d);
  printf (" %lG", d);
  printf (" %lp", p);




  printf ("%lc", lc);
  printf ("%ls", ls);



  printf ("%Le%LE%Lf%Lg%LG", ld, ld, ld, ld, ld);



  printf ("%Ld", ll);
  printf ("%Li", ll);
  printf ("%Lo", ull);
  printf ("%Lu", ull);
  printf ("%Lx", ull);
  printf ("%LX", ull);
  printf ("%Lc", i);
  printf ("%Ls", s);
  printf ("%Lp", p);
  printf ("%Ln", n);

  printf ("%d%i%o%u%x%X%f%e%E%g%G%c%s%p%n%%", i, i, u, u, u, u, d, d, d, d, d,
   i, s, p, n);

  printf ("%-d%-i%-o%-u%-x%-X%-f%-e%-E%-g%-G%-c%-s%-p", i, i, u, u, u, u,
   d, d, d, d, d, i, s, p);
  printf ("%-n", n);

  printf ("%+d%+i%+f%+e%+E%+g%+G\n", i, i, d, d, d, d, d);
  printf ("%+o", u);
  printf ("%+u", u);
  printf ("%+x", u);
  printf ("%+X", u);
  printf ("%+c", i);
  printf ("%+s", s);
  printf ("%+p", p);
  printf ("%+n", n);



  printf ("% +d", i);
  printf ("%+ d", i);
  printf ("% d% i% f% e% E% g% G\n", i, i, d, d, d, d, d);
  printf ("% o", u);
  printf ("% u", u);
  printf ("% x", u);
  printf ("% X", u);
  printf ("% c", i);
  printf ("% s", s);
  printf ("% p", p);
  printf ("% n", n);

  printf ("%#o%#x%#X%#e%#E%#f%#g%#G", u, u, u, d, d, d, d, d);
  printf ("%#d", i);
  printf ("%#i", i);
  printf ("%#u", u);
  printf ("%#c", i);
  printf ("%#s", s);
  printf ("%#p", p);
  printf ("%#n", n);

  printf ("%08d%08i%08o%08u%08x%08X%08e%08E%08f%08g%08G", i, i, u, u, u, u,
   d, d, d, d, d);
  printf ("%0c", i);
  printf ("%0s", s);
  printf ("%0p", p);
  printf ("%0n", n);

  printf ("%08.5d", i);
  printf ("%08.5i", i);
  printf ("%08.5o", u);
  printf ("%08.5u", u);
  printf ("%08.5x", u);
  printf ("%08.5X", u);
  printf ("%08.5f%08.5e%08.5E%08.5g%08.5G", d, d, d, d, d);

  printf ("%-08d", i);
  printf ("%-08i", i);
  printf ("%-08o", u);
  printf ("%-08u", u);
  printf ("%-08x", u);
  printf ("%-08X", u);
  printf ("%-08e", d);
  printf ("%-08E", d);
  printf ("%-08f", d);
  printf ("%-08g", d);
  printf ("%-08G", d);

  printf ("%d", l);
  printf ("%*.*d", l, i2, i);
  printf ("%*.*d", i1, l, i);
  printf ("%ld", i);
  printf ("%s", n);
  printf ("%p", i);
  printf ("%n", p);
# 206 "./format/c90-printf-1.c"
  printf ("%p", foo);
  printf ("%n", un);
  printf ("%p", n);

  printf ("%p%p%p%p", s, ss, us, css);

  printf ("%s%s%s%s", s, ss, us, css);




  printf ("%s", p);





  printf ("%d", u);




  printf ("%*.*d", u1, u2, i);

  printf ("%d%d", i);
  printf ("%d", i, i);

  printf ("");
  printf ("\0");
  printf ("%d\0", i);
  printf ("%d\0%d", i, i);
  printf (
# 237 "./format/c90-printf-1.c" 3 4
         ((void *)0)
# 237 "./format/c90-printf-1.c"
             );
  printf ("%");
  printf ("%++d", i);
  printf ("%n", cn);
  printf ((const char *)L"foo");
  printf ("%n", (int *)0);
  printf ("%s", (char *)0);


  printf ("\\\a\n \"\t%5n\n", n);
  printf ("\\a\\n%5n\n", n);
}
