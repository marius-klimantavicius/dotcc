//type: fp
//options: --c99 --strict_gnu
# 0 "./format/ms_attr-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/ms_attr-2.c"






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
# 8 "./format/ms_attr-2.c" 2

extern void tformatprintf (const char *, ...) __attribute__((format(ms_printf, 1, 2)));
extern void tformat__printf__ (const char *, ...) __attribute__((format(__ms_printf__, 1, 2)));
extern void tformatscanf (const char *, ...) __attribute__((format(ms_scanf, 1, 2)));
extern void tformat__scanf__ (const char *, ...) __attribute__((format(__ms_scanf__, 1, 2)));
extern void tformatstrftime (const char *) __attribute__((format(ms_strftime, 1, 0)));
extern void tformat__strftime__ (const char *) __attribute__((format(__ms_strftime__, 1, 0)));
extern void tformatstrfmon (const char *, ...) __attribute__((format(strfmon, 1, 2)));
extern void tformat__strfmon__ (const char *, ...) __attribute__((format(__strfmon__, 1, 2)));
extern void t__format__printf (const char *, ...) __attribute__((__format__(ms_printf, 1, 2)));
extern void t__format____printf__ (const char *, ...) __attribute__((__format__(__ms_printf__, 1, 2)));
extern void t__format__scanf (const char *, ...) __attribute__((__format__(ms_scanf, 1, 2)));
extern void t__format____scanf__ (const char *, ...) __attribute__((__format__(__ms_scanf__, 1, 2)));
extern void t__format__strftime (const char *) __attribute__((__format__(ms_strftime, 1, 0)));
extern void t__format____strftime__ (const char *) __attribute__((__format__(__ms_strftime__, 1, 0)));
extern void t__format__strfmon (const char *, ...) __attribute__((__format__(strfmon, 1, 2)));
extern void t__format____strfmon__ (const char *, ...) __attribute__((__format__(__strfmon__, 1, 2)));

extern char *tformat_arg (const char *) __attribute__((format_arg(1)));
extern char *t__format_arg__ (const char *) __attribute__((__format_arg__(1)));

void
foo (int i, int *ip, double d)
{
  tformatprintf ("%d", i);
  tformatprintf ("%");
  tformat__printf__ ("%d", i);
  tformat__printf__ ("%");
  tformatscanf ("%d", ip);
  tformatscanf ("%");
  tformat__scanf__ ("%d", ip);
  tformat__scanf__ ("%");
  tformatstrftime ("%a");
  tformatstrftime ("%");
  tformat__strftime__ ("%a");
  tformat__strftime__ ("%");
  tformatstrfmon ("%n", d);
  tformatstrfmon ("%");
  tformat__strfmon__ ("%n", d);
  tformat__strfmon__ ("%");
  t__format__printf ("%d", i);
  t__format__printf ("%");
  t__format____printf__ ("%d", i);
  t__format____printf__ ("%");
  t__format__scanf ("%d", ip);
  t__format__scanf ("%");
  t__format____scanf__ ("%d", ip);
  t__format____scanf__ ("%");
  t__format__strftime ("%a");
  t__format__strftime ("%");
  t__format____strftime__ ("%a");
  t__format____strftime__ ("%");
  t__format__strfmon ("%n", d);
  t__format__strfmon ("%");
  t__format____strfmon__ ("%n", d);
  t__format____strfmon__ ("%");
  tformatprintf (tformat_arg ("%d"), i);
  tformatprintf (tformat_arg ("%"));
  tformatprintf (t__format_arg__ ("%d"), i);
  tformatprintf (t__format_arg__ ("%"));
}
