//type: fp
//options: --c99 --strict_gnu
# 0 "./format/c99-printf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/c99-printf-1.c"
# 9 "./format/c99-printf-1.c"
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
# 10 "./format/c99-printf-1.c" 2

void
foo (int i, unsigned int u, double d, char *s, void *p, int *n,
     long double ld, wint_t lc, wchar_t *ls, long long int ll,
     unsigned long long int ull, signed char *ss, unsigned char *us,
     long long int *lln, intmax_t j, uintmax_t uj, intmax_t *jn,
     size_t z, signed_size_t sz, signed_size_t *zn,
     ptrdiff_t t, ptrdiff_t *tn)
{





  printf ("%hf", d);
  printf ("%hF", d);
  printf ("%he", d);
  printf ("%hE", d);
  printf ("%hg", d);
  printf ("%hG", d);
  printf ("%ha", d);
  printf ("%hA", d);
  printf ("%hc", i);
  printf ("%hs", s);
  printf ("%hp", p);
  printf ("%hhd%hhi%hho%hhu%hhx%hhX", i, i, u, u, u, u);
  printf ("%hhn", ss);
  printf ("%hhf", d);
  printf ("%hhF", d);
  printf ("%hhe", d);
  printf ("%hhE", d);
  printf ("%hhg", d);
  printf ("%hhG", d);
  printf ("%hha", d);
  printf ("%hhA", d);
  printf ("%hhc", i);
  printf ("%hhs", s);
  printf ("%hhp", p);
  printf ("%lc", lc);
  printf ("%ls", ls);
  printf ("%lf%lF%le%lE%lg%lG%la%lA", d, d, d, d, d, d, d, d);
  printf ("%lp", p);
  printf ("%lld%lli%llo%llu%llx%llX", ll, ll, ull, ull, ull, ull);
  printf ("%lln", lln);
  printf ("%llf", d);
  printf ("%llF", d);
  printf ("%lle", d);
  printf ("%llE", d);
  printf ("%llg", d);
  printf ("%llG", d);
  printf ("%lla", d);
  printf ("%llA", d);
  printf ("%llc", i);
  printf ("%lls", s);
  printf ("%llp", p);
  printf ("%jd%ji%jo%ju%jx%jX", j, j, uj, uj, uj, uj);
  printf ("%jn", jn);
  printf ("%jf", d);
  printf ("%jF", d);
  printf ("%je", d);
  printf ("%jE", d);
  printf ("%jg", d);
  printf ("%jG", d);
  printf ("%ja", d);
  printf ("%jA", d);
  printf ("%jc", i);
  printf ("%js", s);
  printf ("%jp", p);
  printf ("%zd%zi%zo%zu%zx%zX", sz, sz, z, z, z, z);
  printf ("%zn", zn);
  printf ("%zf", d);
  printf ("%zF", d);
  printf ("%ze", d);
  printf ("%zE", d);
  printf ("%zg", d);
  printf ("%zG", d);
  printf ("%za", d);
  printf ("%zA", d);
  printf ("%zc", i);
  printf ("%zs", s);
  printf ("%zp", p);
  printf ("%td%ti%to%tu%tx%tX", t, t, t, t, t, t);
  printf ("%tn", tn);
  printf ("%tf", d);
  printf ("%tF", d);
  printf ("%te", d);
  printf ("%tE", d);
  printf ("%tg", d);
  printf ("%tG", d);
  printf ("%ta", d);
  printf ("%tA", d);
  printf ("%tc", i);
  printf ("%ts", s);
  printf ("%tp", p);
  printf ("%Le%LE%Lf%LF%Lg%LG%La%LA", ld, ld, ld, ld, ld, ld, ld, ld);



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

  printf ("%d%i%o%u%x%X%f%F%e%E%g%G%a%A%c%s%p%n%%", i, i, u, u, u, u,
   d, d, d, d, d, d, d, d, i, s, p, n);

  printf ("%-d%-i%-o%-u%-x%-X%-f%-F%-e%-E%-g%-G%-a%-A%-c%-s%-p", i, i,
   u, u, u, u, d, d, d, d, d, d, d, d, i, s, p);
  printf ("%-n", n);

  printf ("%+d%+i%+f%+F%+e%+E%+g%+G%+a%+A\n", i, i, d, d, d, d, d, d, d, d);
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
  printf ("% d% i% f% F% e% E% g% G% a% A\n", i, i, d, d, d, d, d, d, d, d);
  printf ("% o", u);
  printf ("% u", u);
  printf ("% x", u);
  printf ("% X", u);
  printf ("% c", i);
  printf ("% s", s);
  printf ("% p", p);
  printf ("% n", n);

  printf ("%#o%#x%#X%#e%#E%#f%#F%#g%#G%#a%#A", u, u, u, d, d, d, d,
   d, d, d, d);
  printf ("%#d", i);
  printf ("%#i", i);
  printf ("%#u", u);
  printf ("%#c", i);
  printf ("%#s", s);
  printf ("%#p", p);
  printf ("%#n", n);

  printf ("%08d%08i%08o%08u%08x%08X%08e%08E%08f%08F%08g%08G%08a%08A", i, i,
   u, u, u, u, d, d, d, d, d, d, d, d);
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
  printf ("%08.5f%08.5F%08.5e%08.5E%08.5g%08.5G%08.5a%08.5A",
   d, d, d, d, d, d, d, d);

  printf ("%-08d", i);
  printf ("%-08i", i);
  printf ("%-08o", u);
  printf ("%-08u", u);
  printf ("%-08x", u);
  printf ("%-08X", u);
  printf ("%-08e", d);
  printf ("%-08E", d);
  printf ("%-08f", d);
  printf ("%-08F", d);
  printf ("%-08g", d);
  printf ("%-08G", d);
  printf ("%-08a", d);
  printf ("%-08A", d);





  printf ("%hhn", s);
  printf ("%hhn", us);
}
