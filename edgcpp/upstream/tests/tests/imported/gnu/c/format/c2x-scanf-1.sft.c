//type: fp
//options: --c23
# 0 "./format/c2x-scanf-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/c2x-scanf-1.c"




# 1 "./format/format.h" 1
# 35 "./format/format.h"
# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 36 "./format/format.h" 2
# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
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
# 6 "./format/c2x-scanf-1.c" 2

void
foo (unsigned int *uip, unsigned short int *uhp, unsigned char *uhhp,
     unsigned long int *ulp, unsigned long long *ullp, uintmax_t *ujp,
     size_t *zp, unsigned_ptrdiff_t *utp, int_least8_t *i8, int_least16_t *i16,
     int_least32_t *i32, int_least64_t *i64, uint_least8_t *u8,
     uint_least16_t *u16, uint_least32_t *u32, uint_least64_t *u64,
     int_fast8_t *if8, int_fast16_t *if16, int_fast32_t *if32,
     int_fast64_t *if64, uint_fast8_t *uf8, uint_fast16_t *uf16,
     uint_fast32_t *uf32, uint_fast64_t *uf64)
{
  scanf ("%*b");
  scanf ("%2b", uip);
  scanf ("%hb%hhb%lb%llb%jb%zb%tb", uhp, uhhp, ulp, ullp, ujp, zp, utp);
  scanf ("%Lb", ullp);
  scanf ("%qb", ullp);

  scanf ("%w8d %w16d %w32d %w64d %wf8d %wf16d %wf32d %wf64d",
  i8, i16, i32, i64, if8, if16, if32, if64);
  scanf ("%w8i %w16i %w32i %w64i %wf8i %wf16i %wf32i %wf64i",
  i8, i16, i32, i64, if8, if16, if32, if64);
  scanf ("%w8b %w16b %w32b %w64b %wf8b %wf16b %wf32b %wf64b",
  u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  scanf ("%w8o %w16o %w32o %w64o %wf8o %wf16o %wf32o %wf64o",
  u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  scanf ("%w8u %w16u %w32u %w64u %wf8u %wf16u %wf32u %wf64u",
  u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  scanf ("%w8x %w16x %w32x %w64x %wf8x %wf16x %wf32x %wf64x",
  u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  scanf ("%w8X %w16X %w32X %w64X %wf8X %wf16X %wf32X %wf64X",
  u8, u16, u32, u64, uf8, uf16, uf32, uf64);
  scanf ("%w8n %w16n %w32n %w64n %wf8n %wf16n %wf32n %wf64n",
  i8, i16, i32, i64, if8, if16, if32, if64);

  scanf ("%w8a", i8);
  scanf ("%w16a", i16);
  scanf ("%w32a", i32);
  scanf ("%w64a", i64);
  scanf ("%wf8a", if8);
  scanf ("%wf16a", if16);
  scanf ("%wf32a", if32);
  scanf ("%wf64a", if64);
  scanf ("%w8A", i8);
  scanf ("%w16A", i16);
  scanf ("%w32A", i32);
  scanf ("%w64A", i64);
  scanf ("%wf8A", if8);
  scanf ("%wf16A", if16);
  scanf ("%wf32A", if32);
  scanf ("%wf64A", if64);
  scanf ("%w8c", i8);
  scanf ("%w16c", i16);
  scanf ("%w32c", i32);
  scanf ("%w64c", i64);
  scanf ("%wf8c", if8);
  scanf ("%wf16c", if16);
  scanf ("%wf32c", if32);
  scanf ("%wf64c", if64);
  scanf ("%w8e", i8);
  scanf ("%w16e", i16);
  scanf ("%w32e", i32);
  scanf ("%w64e", i64);
  scanf ("%wf8e", if8);
  scanf ("%wf16e", if16);
  scanf ("%wf32e", if32);
  scanf ("%wf64e", if64);
  scanf ("%w8E", i8);
  scanf ("%w16E", i16);
  scanf ("%w32E", i32);
  scanf ("%w64E", i64);
  scanf ("%wf8E", if8);
  scanf ("%wf16E", if16);
  scanf ("%wf32E", if32);
  scanf ("%wf64E", if64);
  scanf ("%w8f", i8);
  scanf ("%w16f", i16);
  scanf ("%w32f", i32);
  scanf ("%w64f", i64);
  scanf ("%wf8f", if8);
  scanf ("%wf16f", if16);
  scanf ("%wf32f", if32);
  scanf ("%wf64f", if64);
  scanf ("%w8F", i8);
  scanf ("%w16F", i16);
  scanf ("%w32F", i32);
  scanf ("%w64F", i64);
  scanf ("%wf8F", if8);
  scanf ("%wf16F", if16);
  scanf ("%wf32F", if32);
  scanf ("%wf64F", if64);
  scanf ("%w8g", i8);
  scanf ("%w16g", i16);
  scanf ("%w32g", i32);
  scanf ("%w64g", i64);
  scanf ("%wf8g", if8);
  scanf ("%wf16g", if16);
  scanf ("%wf32g", if32);
  scanf ("%wf64g", if64);
  scanf ("%w8G", i8);
  scanf ("%w16G", i16);
  scanf ("%w32G", i32);
  scanf ("%w64G", i64);
  scanf ("%wf8G", if8);
  scanf ("%wf16G", if16);
  scanf ("%wf32G", if32);
  scanf ("%wf64G", if64);
  scanf ("%w8p", i8);
  scanf ("%w16p", i16);
  scanf ("%w32p", i32);
  scanf ("%w64p", i64);
  scanf ("%wf8p", if8);
  scanf ("%wf16p", if16);
  scanf ("%wf32p", if32);
  scanf ("%wf64p", if64);
  scanf ("%w8s", i8);
  scanf ("%w16s", i16);
  scanf ("%w32s", i32);
  scanf ("%w64s", i64);
  scanf ("%wf8s", if8);
  scanf ("%wf16s", if16);
  scanf ("%wf32s", if32);
  scanf ("%wf64s", if64);
  scanf ("%w8[abc]", i8);
  scanf ("%w16[abc]", i16);
  scanf ("%w32[abc]", i32);
  scanf ("%w64[abc]", i64);
  scanf ("%wf8[abc]", if8);
  scanf ("%wf16[abc]", if16);
  scanf ("%wf32[abc]", if32);
  scanf ("%wf64[abc]", if64);
}
