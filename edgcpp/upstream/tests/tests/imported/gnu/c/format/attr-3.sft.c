//type: fn
//options: --c99 --strict_gnu
# 0 "./format/attr-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/attr-3.c"






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
# 8 "./format/attr-3.c" 2


extern void fa0 (const char *, ...) __attribute__((format(printf, 1, 2)));
extern void fa1 (char *, ...) __attribute__((format(printf, 1, 2)));
extern char *fa2 (const char *) __attribute__((format_arg(1)));
extern char *fa3 (char *) __attribute__((format_arg(1)));


extern void fb0 (const char *, ...) __attribute__((format));
extern void fb1 (const char *, ...) __attribute__((format()));
extern void fb2 (const char *, ...) __attribute__((format(printf)));
extern void fb3 (const char *, ...) __attribute__((format(printf, 1)));
extern void fb4 (const char *, ...) __attribute__((format(printf, 1, 2, 3)));

extern void fc1 (const char *) __attribute__((format_arg));
extern void fc2 (const char *) __attribute__((format_arg()));
extern void fc3 (const char *) __attribute__((format_arg(1, 2)));




struct s0 { int i; } __attribute__((format(printf, 1, 2)));
union u0 { int i; } __attribute__((format(printf, 1, 2)));
enum e0 { E0V0 } __attribute__((format(printf, 1, 2)));

struct s1 { int i; } __attribute__((format_arg(1)));
union u1 { int i; } __attribute__((format_arg(1)));
enum e1 { E1V0 } __attribute__((format_arg(1)));


extern void fe0 (const char *, ...) __attribute__((format(12345, 1, 2)));
extern void fe1 (const char *, ...) __attribute__((format(nosuch, 1, 2)));


extern void ff0 (const char *, ...) __attribute__((format(printf, 3-2, (long long)(10/5))));
int foo;
extern void ff1 (const char *, ...) __attribute__((format(printf, foo, 10/5)));
extern void ff2 (const char *, ...) __attribute__((format(printf, 3-2, foo)));
extern char *ff3 (const char *) __attribute__((format_arg(3-2)));
extern char *ff4 (const char *) __attribute__((format_arg(foo)));




extern void fg0 () __attribute__((format(printf, 1, 2)));
extern void fg1 () __attribute__((format(printf, 1, 0)));
extern void fg2 () __attribute__((format(printf, 1, 1)));
extern void fg3 () __attribute__((format(printf, 2, 1)));



extern void fh0 (int, ...) __attribute__((format(printf, 1, 2)));
extern void fh1 (signed char *, ...) __attribute__((format(printf, 1, 2)));
extern void fh2 (unsigned char *, ...) __attribute__((format(printf, 1, 2)));
extern void fh3 (const char *, ...) __attribute__((format(printf, 1, 3)));
extern void fh4 (const char *, int, ...) __attribute__((format(printf, 1, 2)));


extern char *fi0 (int) __attribute__((format_arg(1)));
extern char *fi1 (signed char *) __attribute__((format_arg(1)));
extern char *fi2 (unsigned char *) __attribute__((format_arg(1)));
extern int fi3 (const char *) __attribute__((format_arg(1)));
extern signed char *fi4 (const char *) __attribute__((format_arg(1)));
extern unsigned char *fi5 (const char *) __attribute__((format_arg(1)));
