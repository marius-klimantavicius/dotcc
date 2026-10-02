//type: fp
//options: 
# 0 "./format/pr72858.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/pr72858.c"


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
# 4 "./format/pr72858.c" 2
# 14 "./format/pr72858.c"
void
test_x (char *d,
 int iexpr, unsigned int uiexpr,
 long lexpr, unsigned long ulexpr,
 long long llexpr, unsigned long long ullexpr,
 float fexpr, double dexpr, long double ldexpr,
 void *ptr)
{


  sprintf (d, " %-8x ", iexpr);
  sprintf (d, " %-8x ", uiexpr);

  sprintf (d, " %-8x ", lexpr);
# 36 "./format/pr72858.c"
  sprintf (d, " %-8x ", ulexpr);
# 46 "./format/pr72858.c"
  sprintf (d, " %-8x ", llexpr);
# 55 "./format/pr72858.c"
  sprintf (d, " %-8x ", ullexpr);
# 67 "./format/pr72858.c"
  sprintf (d, " %-8x ", fexpr);
# 76 "./format/pr72858.c"
  sprintf (d, " %-8x ", dexpr);
# 85 "./format/pr72858.c"
  sprintf (d, " %-8x ", ldexpr);
# 96 "./format/pr72858.c"
  sprintf (d, " %-8x ", ptr);
# 107 "./format/pr72858.c"
  struct s { int i; };
  struct s s;
  sprintf (d, " %-8x ", s);







}




void
test_lx (char *d,
  int iexpr, unsigned int uiexpr,
  long lexpr, unsigned long ulexpr,
  long long llexpr, unsigned long long ullexpr,
  float fexpr, double dexpr, long double ldexpr)
{


  sprintf (d, " %-8lx ", iexpr);
# 140 "./format/pr72858.c"
  sprintf (d, " %-8lx ", uiexpr);
# 150 "./format/pr72858.c"
  sprintf (d, " %-8lx ", lexpr);
  sprintf (d, " %-8lx ", ulexpr);

  sprintf (d, " %-8lx ", llexpr);
# 162 "./format/pr72858.c"
  sprintf (d, " %-8lx ", ullexpr);
# 174 "./format/pr72858.c"
  sprintf (d, " %-8lx ", fexpr);
# 183 "./format/pr72858.c"
  sprintf (d, " %-8lx ", dexpr);
# 192 "./format/pr72858.c"
  sprintf (d, " %-8lx ", ldexpr);
# 201 "./format/pr72858.c"
}




void
test_o (char *d,
 int iexpr, unsigned int uiexpr,
 long lexpr, unsigned long ulexpr,
 long long llexpr, unsigned long long ullexpr)
{


  sprintf (d, " %-8o ", iexpr);
  sprintf (d, " %-8o ", uiexpr);

  sprintf (d, " %-8o ", lexpr);
# 226 "./format/pr72858.c"
  sprintf (d, " %-8o ", ulexpr);
# 236 "./format/pr72858.c"
  sprintf (d, " %-8o ", llexpr);
# 245 "./format/pr72858.c"
  sprintf (d, " %-8o ", ullexpr);
# 254 "./format/pr72858.c"
}




void
test_lo (char *d,
 int iexpr, unsigned int uiexpr,
 long lexpr, unsigned long ulexpr,
 long long llexpr, unsigned long long ullexpr)
{


  sprintf (d, " %-8lo ", iexpr);
# 276 "./format/pr72858.c"
  sprintf (d, " %-8lo ", uiexpr);
# 286 "./format/pr72858.c"
  sprintf (d, " %-8lo ", lexpr);
  sprintf (d, " %-8lo ", ulexpr);

  sprintf (d, " %-8lo ", llexpr);
# 298 "./format/pr72858.c"
  sprintf (d, " %-8lo ", ullexpr);
# 307 "./format/pr72858.c"
}




void
test_e (char *d, int iexpr, float fexpr, double dexpr, long double ldexpr)
{


  sprintf (d, " %-8e ", iexpr);
# 329 "./format/pr72858.c"
  sprintf (d, " %-8e ", fexpr);
  sprintf (d, " %-8e ", dexpr);
  sprintf (d, " %-8e ", ldexpr);
# 340 "./format/pr72858.c"
}




void
test_Le (char *d, int iexpr, float fexpr, double dexpr, long double ldexpr)
{


  sprintf (d, " %-8Le ", iexpr);
# 362 "./format/pr72858.c"
  sprintf (d, " %-8Le ", fexpr);
# 372 "./format/pr72858.c"
  sprintf (d, " %-8Le ", dexpr);
# 382 "./format/pr72858.c"
  sprintf (d, " %-8Le ", ldexpr);
}




void
test_E (char *d, int iexpr, float fexpr, double dexpr, long double ldexpr)
{


  sprintf (d, " %-8E ", iexpr);
# 405 "./format/pr72858.c"
  sprintf (d, " %-8E ", fexpr);
  sprintf (d, " %-8E ", dexpr);
  sprintf (d, " %-8E ", ldexpr);
# 416 "./format/pr72858.c"
}




void
test_LE (char *d, int iexpr, float fexpr, double dexpr, long double ldexpr)
{


  sprintf (d, " %-8LE ", iexpr);
# 436 "./format/pr72858.c"
  sprintf (d, " %-8LE ", fexpr);
# 446 "./format/pr72858.c"
  sprintf (d, " %-8LE ", dexpr);
# 456 "./format/pr72858.c"
  sprintf (d, " %-8LE ", ldexpr);
}





void
test_everything (char *d, long lexpr)
{
  sprintf (d, "before %-+*.*lld after", lexpr, lexpr, lexpr);
# 492 "./format/pr72858.c"
}
