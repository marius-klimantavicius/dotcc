//type: fp
//options: 
# 0 "./format/dfp-scanf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/dfp-scanf-1.c"







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
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
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
# 9 "./format/dfp-scanf-1.c" 2

void
voo (_Decimal32 *x, _Decimal64 *y, _Decimal128 *z, int *i, unsigned int *j,
     double *d, char **p)
{






  scanf ("%Ha", x);
  scanf ("%HA", x);
  scanf ("%Hf", x);
  scanf ("%HF", x);
  scanf ("%He", x);
  scanf ("%HE", x);
  scanf ("%Hg", x);
  scanf ("%HG", x);

  scanf ("%Da", y);
  scanf ("%DA", y);
  scanf ("%Df", y);
  scanf ("%DF", y);
  scanf ("%De", y);
  scanf ("%DE", y);
  scanf ("%Dg", y);
  scanf ("%DG", y);

  scanf ("%DDa", z);
  scanf ("%DDA", z);
  scanf ("%DDf", z);
  scanf ("%DDF", z);
  scanf ("%DDe", z);
  scanf ("%DDE", z);
  scanf ("%DDg", z);
  scanf ("%DDG", z);

  scanf ("%DG%DDE%HF%DDe%He%HE%DF%DDF%De%DDG%HG%Df%Hg%DE%DDf%Dg%DDg%Hf\n",
           y, z, x, z, x, x, y, z, y, z, x, y, x, y, z, y, z, x);



  scanf ("%Ha", y);
  scanf ("%HA", y);
  scanf ("%Hf", y);
  scanf ("%HF", y);
  scanf ("%He", y);
  scanf ("%HE", y);
  scanf ("%Hg", y);
  scanf ("%HG", y);
  scanf ("%Ha", z);
  scanf ("%HA", z);
  scanf ("%Hf", z);
  scanf ("%HF", z);
  scanf ("%He", z);
  scanf ("%HE", z);
  scanf ("%Hg", z);
  scanf ("%HG", z);

  scanf ("%Da", x);
  scanf ("%DA", x);
  scanf ("%Df", x);
  scanf ("%DF", x);
  scanf ("%De", x);
  scanf ("%DE", x);
  scanf ("%Dg", x);
  scanf ("%DG", x);
  scanf ("%Da", z);
  scanf ("%DA", z);
  scanf ("%Df", z);
  scanf ("%DF", z);
  scanf ("%De", z);
  scanf ("%DE", z);
  scanf ("%Dg", z);
  scanf ("%DG", z);

  scanf ("%DDa", x);
  scanf ("%DDA", x);
  scanf ("%DDf", x);
  scanf ("%DDF", x);
  scanf ("%DDe", x);
  scanf ("%DDE", x);
  scanf ("%DDg", x);
  scanf ("%DDG", x);
  scanf ("%DDa", y);
  scanf ("%DDA", y);
  scanf ("%DDf", y);
  scanf ("%DDF", y);
  scanf ("%DDe", y);
  scanf ("%DDE", y);
  scanf ("%DDg", y);
  scanf ("%DDG", y);



  scanf ("%Hd\n", i);
  scanf ("%Hi\n", i);
  scanf ("%Ho\n", j);
  scanf ("%Hu\n", j);
  scanf ("%Hx\n", j);
  scanf ("%HX\n", j);
  scanf ("%Ha\n", d);
  scanf ("%HA\n", d);
  scanf ("%Hc\n", i);
  scanf ("%Hs\n", p);
  scanf ("%Hp\n", p);
  scanf ("%Hn\n", p);
}
