//type: fp
//options: --c99 --strict_gnu
# 0 "./format/ms_c99-scanf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/ms_c99-scanf-1.c"
# 10 "./format/ms_c99-scanf-1.c"
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
# 11 "./format/ms_c99-scanf-1.c" 2

void
foo (int *ip, unsigned int *uip, short int *hp, unsigned short int *uhp,
     signed char *hhp, unsigned char *uhhp, long int *lp,
     unsigned long int *ulp, float *fp, double *dp, long double *ldp, char *s,
     void **pp, int *n, long long *llp, unsigned long long *ullp, wchar_t *ls,
     short int *hn, signed char *hhn, long int *ln, long long int *lln,
     intmax_t *jp, uintmax_t *ujp, intmax_t *jn, size_t *zp,
     signed_size_t *szp, signed_size_t *zn, ptrdiff_t *tp,
     unsigned_ptrdiff_t *utp, ptrdiff_t *tn)
{







  scanf ("%*d%*i%*o%*u%*x%*X%*e%*E%*f%*g%*G%*s%*[abc]%*c%*p");
  scanf ("%*2d%*8s%*3c");
  scanf ("%*n", n);
  scanf ("%*hd");
  scanf ("%2d%3i%4o%5u%6x%7X%10e%11E%12f%14g%15G%16s%3[abc]%4c%5p",
  ip, ip, uip, uip, uip, uip, fp, fp, fp, fp, fp,
  s, s, s, pp);
  scanf ("%0d", ip);
  scanf ("%3n", n);

  scanf ("%hd%hi%ho%hu%hx%hX%hn", hp, hp, uhp, uhp, uhp, uhp, hn);
  scanf ("%he", fp);
  scanf ("%hE", fp);
  scanf ("%hf", fp);
  scanf ("%hg", fp);
  scanf ("%hG", fp);
  scanf ("%hs", hp);
  scanf ("%h[ac]", s);
  scanf ("%hc", (short *)s);
  scanf ("%hp", pp);
  scanf ("%hhd", hhp);
  scanf ("%ld%li%lo%lu%lx%lX%ln", lp, lp, ulp, ulp, ulp, ulp, ln);
  scanf ("%le%lE%lf%lg%lG", dp, dp, dp, dp, dp);
  scanf ("%lp", pp);
  scanf ("%ls", ls);
  scanf ("%l[ac]", ls);
  scanf ("%lc", ls);
  scanf ("%jd", jp);
  scanf ("%zd", zp);
  scanf ("%td", tp);
  scanf ("%Lf", llp);

  scanf ("%d%i%o%u%x%X%e%E%f%g%G%s%[abc]%c%p%n%%", ip, ip, uip, uip, uip,
         uip, fp, fp, fp, fp, fp, s, s, s, pp, n);
}
