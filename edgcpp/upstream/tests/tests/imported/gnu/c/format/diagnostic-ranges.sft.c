//type: fp
//options: 
# 0 "./format/diagnostic-ranges.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./format/diagnostic-ranges.c"




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
# 6 "./format/diagnostic-ranges.c" 2

void test_mismatching_types (const char *msg)
{
  printf("hello %i", msg);
# 20 "./format/diagnostic-ranges.c"
  printf("hello %s", 42);
# 30 "./format/diagnostic-ranges.c"
  printf("hello %i", (long)0);







}

void test_multiple_arguments (void)
{
  printf ("arg0: %i  arg1: %s arg 2: %i",
          100, 101, 102);
# 55 "./format/diagnostic-ranges.c"
}

void test_multiple_arguments_2 (int i, int j)
{
  printf ("arg0: %i  arg1: %s arg 2: %i",
          100, i + j, 102);
# 72 "./format/diagnostic-ranges.c"
}

void multiline_format_string (void) {
  printf ("before the fmt specifier"





          "%"
          "d"
          "after the fmt specifier");
# 93 "./format/diagnostic-ranges.c"
}

void test_hex (const char *msg)
{


  printf("hello \x25\x69", msg);
# 108 "./format/diagnostic-ranges.c"
}

void test_oct (const char *msg)
{


  printf("hello \045\151", msg);
# 123 "./format/diagnostic-ranges.c"
}

void test_multiple (const char *msg)
{


  printf("prefix" "\x25" "\151" "suffix",
         msg);
# 147 "./format/diagnostic-ranges.c"
}

void test_u8 (const char *msg)
{
  printf(u8"hello %i", msg);







}

void test_param (long long_i, long long_j)
{
  printf ("foo %s bar", long_i + long_j);







}

void test_field_width_specifier (long l, int i1, int i2)
{
  printf (" %*.*d ", l, i1, i2);






}



void test_field_width_specifier_2 (char *d, long foo, long bar)
{
  __builtin_sprintf (d, " %*ld ", foo, foo);







  __builtin_sprintf (d, " %*ld ", foo + bar, foo);






}

void test_field_precision_specifier (char *d, long foo, long bar)
{
  __builtin_sprintf (d, " %.*ld ", foo, foo);







  __builtin_sprintf (d, " %.*ld ", foo + bar, foo);






}

void test_spurious_percent (void)
{
  printf("hello world %");





}

void test_empty_precision (char *s, size_t m, double d)
{
  strfmon (s, m, "%#.5n", d);





  strfmon (s, m, "%#5.n", d);




}

void test_repeated (int i)
{
  printf ("%++d", i);




}

void test_conversion_lacks_type (void)
{
  printf (" %h");




}

void test_embedded_nul (void)
{
  printf (" \0 ");




}

void test_macro (const char *msg)
{

  printf("hello " "%i" " world", msg);
# 294 "./format/diagnostic-ranges.c"
}

void test_macro_2 (const char *msg)
{

  printf("hello %" "u" " world", msg);
# 313 "./format/diagnostic-ranges.c"
}

void test_macro_3 (const char *msg)
{


  printf("hello %i world", msg);
# 337 "./format/diagnostic-ranges.c"
}

void test_macro_4 (const char *msg)
{

  printf("hello %i world" "\n", msg);
# 359 "./format/diagnostic-ranges.c"
}

void test_non_contiguous_strings (void)
{
  __builtin_printf(" %" "d ", 0.5);
# 378 "./format/diagnostic-ranges.c"
}

void test_const_arrays (void)
{


  const char a[] = " %d ";
  __builtin_printf(a, 0.5);






}
