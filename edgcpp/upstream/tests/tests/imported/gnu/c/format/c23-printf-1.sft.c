//type: fp
//options: --c23
# 0 "./format/c23-printf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/c23-printf-1.c"




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
# 6 "./format/c23-printf-1.c" 2

void
foo (unsigned int u, unsigned short us, unsigned char uc, unsigned long ul,
     unsigned long long ull, uintmax_t uj, size_t z, unsigned_ptrdiff_t ut,
     int_least8_t i8, int_least16_t i16, int_least32_t i32, int_least64_t i64,
     uint_least8_t u8, uint_least16_t u16, uint_least32_t u32,
     uint_least64_t u64, int_fast8_t if8, int_fast16_t if16, int_fast32_t if32,
     int_fast64_t if64, uint_fast8_t uf8, uint_fast16_t uf16,
     uint_fast32_t uf32, uint_fast64_t uf64)
{

  printf ("%b %hb %hhb %lb %llb %jb %zb %tb\n", u, us, uc, ul, ull, uj, z, ut);
  printf ("%*.*llb\n", 1, 2, ull);
  printf ("%-b\n", u);
  printf ("%#b\n", u);
  printf ("%08b\n", u);

  printf ("%+b\n", u);
  printf ("% b\n", u);

  printf ("%-08b\n", u);
  printf ("%08.5b\n", u);

  printf ("%Lb", ull);
  printf ("%qb", ull);

  printf ("%B %hB %hhB %lB %llB %jB %zB %tB\n", u, us, uc, ul, ull, uj, z, ut);
  printf ("%*.*llB\n", 1, 2, ull);
  printf ("%-B\n", u);
  printf ("%#B\n", u);
  printf ("%08B\n", u);
  printf ("%+B\n", u);
  printf ("% B\n", u);
  printf ("%-08B\n", u);
  printf ("%08.5B\n", u);
  printf ("%LB", ull);
  printf ("%qB", ull);

  printf ("%w8d %w16d %w32d %w64d %wf8d %wf16d %wf32d %wf64d",
   i8, i16, i32, i64, if8, if16, if32, if64);
  printf ("%w8i %w16i %w32i %w64i %wf8i %wf16i %wf32i %wf64i",
   i8, i16, i32, i64, if8, if16, if32, if64);
  printf ("%w8b %w16b %w32b %w64b %wf8b %wf16b %wf32b %wf64b",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8B %w16B %w32B %w64B %wf8B %wf16B %wf32B %wf64B",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8o %w16o %w32o %w64o %wf8o %wf16o %wf32o %wf64o",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8u %w16u %w32u %w64u %wf8u %wf16u %wf32u %wf64u",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8x %w16x %w32x %w64x %wf8x %wf16x %wf32x %wf64x",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8X %w16X %w32X %w64X %wf8X %wf16X %wf32X %wf64X",
   u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  printf ("%w8n %w16n %w32n %w64n %wf8n %wf16n %wf32n %wf64n",
   &i8, &i16, &i32, &i64, &if8, &if16, &if32, &if64);

  printf ("%w8a", i8);
  printf ("%w16a", i16);
  printf ("%w32a", i32);
  printf ("%w64a", i64);
  printf ("%wf8a", if8);
  printf ("%wf16a", if16);
  printf ("%wf32a", if32);
  printf ("%wf64a", if64);
  printf ("%w8A", i8);
  printf ("%w16A", i16);
  printf ("%w32A", i32);
  printf ("%w64A", i64);
  printf ("%wf8A", if8);
  printf ("%wf16A", if16);
  printf ("%wf32A", if32);
  printf ("%wf64A", if64);
  printf ("%w8c", i8);
  printf ("%w16c", i16);
  printf ("%w32c", i32);
  printf ("%w64c", i64);
  printf ("%wf8c", if8);
  printf ("%wf16c", if16);
  printf ("%wf32c", if32);
  printf ("%wf64c", if64);
  printf ("%w8e", i8);
  printf ("%w16e", i16);
  printf ("%w32e", i32);
  printf ("%w64e", i64);
  printf ("%wf8e", if8);
  printf ("%wf16e", if16);
  printf ("%wf32e", if32);
  printf ("%wf64e", if64);
  printf ("%w8E", i8);
  printf ("%w16E", i16);
  printf ("%w32E", i32);
  printf ("%w64E", i64);
  printf ("%wf8E", if8);
  printf ("%wf16E", if16);
  printf ("%wf32E", if32);
  printf ("%wf64E", if64);
  printf ("%w8f", i8);
  printf ("%w16f", i16);
  printf ("%w32f", i32);
  printf ("%w64f", i64);
  printf ("%wf8f", if8);
  printf ("%wf16f", if16);
  printf ("%wf32f", if32);
  printf ("%wf64f", if64);
  printf ("%w8F", i8);
  printf ("%w16F", i16);
  printf ("%w32F", i32);
  printf ("%w64F", i64);
  printf ("%wf8F", if8);
  printf ("%wf16F", if16);
  printf ("%wf32F", if32);
  printf ("%wf64F", if64);
  printf ("%w8g", i8);
  printf ("%w16g", i16);
  printf ("%w32g", i32);
  printf ("%w64g", i64);
  printf ("%wf8g", if8);
  printf ("%wf16g", if16);
  printf ("%wf32g", if32);
  printf ("%wf64g", if64);
  printf ("%w8G", i8);
  printf ("%w16G", i16);
  printf ("%w32G", i32);
  printf ("%w64G", i64);
  printf ("%wf8G", if8);
  printf ("%wf16G", if16);
  printf ("%wf32G", if32);
  printf ("%wf64G", if64);
  printf ("%w8p", i8);
  printf ("%w16p", i16);
  printf ("%w32p", i32);
  printf ("%w64p", i64);
  printf ("%wf8p", if8);
  printf ("%wf16p", if16);
  printf ("%wf32p", if32);
  printf ("%wf64p", if64);
  printf ("%w8s", i8);
  printf ("%w16s", i16);
  printf ("%w32s", i32);
  printf ("%w64s", i64);
  printf ("%wf8s", if8);
  printf ("%wf16s", if16);
  printf ("%wf32s", if32);
  printf ("%wf64s", if64);
}
