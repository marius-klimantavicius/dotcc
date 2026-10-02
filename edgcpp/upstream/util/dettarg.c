/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*
Determine target parameters for C/C++ Front End configuration.
Writes C #defines for the target parameters to stdout.
Those #defines can then be placed in the defines.h used when the
Front End is built.

Currently, handles only two's complement machines.

Copyright 1991, 1999-2026, Edison Design Group, Inc.
*/

/*
HAVE_LONG_DOUBLE can be set (to 0 or 1) to indicate whether the host
C compiler supports long double.  If it is not set, the default is
based on __STDC__.
*/
#ifndef HAVE_LONG_DOUBLE
#ifdef __STDC__
#define HAVE_LONG_DOUBLE 1
#else /* !defined(__STDC__) */
#define HAVE_LONG_DOUBLE 0
#endif /* ifdef __STDC__ */
#endif /* ifndef HAVE_LONG_DOUBLE */

/*
HAVE_WCHAR_T can be set (to 0 or 1) to indicate whether the host
C compiler supports wchar_t.  If it is not set, the default is
based on __STDC__.
*/
#ifndef HAVE_WCHAR_T
#ifdef __STDC__
#define HAVE_WCHAR_T 1
#else /* !defined(__STDC__) */
#define HAVE_WCHAR_T 0
#endif /* ifdef __STDC__ */
#endif /* ifndef HAVE_WCHAR_T */

/*
HAVE_SIZE_T can be set (to 0 or 1) to indicate whether the host
C compiler supports size_t.  If it is not set, the default is
based on __STDC__.
*/
#ifndef HAVE_SIZE_T
#ifdef __STDC__
#define HAVE_SIZE_T 1
#else /* !defined(__STDC__) */
#define HAVE_SIZE_T 0
#endif /* ifdef __STDC__ */
#endif /* ifndef HAVE_SIZE_T */

/*
HAVE_PTRDIFF_T can be set (to 0 or 1) to indicate whether the host
C compiler supports ptrdiff_t.  If it is not set, the default is
based on __STDC__.
*/
#ifndef HAVE_PTRDIFF_T
#ifdef __STDC__
#define HAVE_PTRDIFF_T 1
#else /* !defined(__STDC__) */
#define HAVE_PTRDIFF_T 0
#endif /* ifdef __STDC__ */
#endif /* ifndef HAVE_PTRDIFF_T */

/*
HAVE_LONG_LONG can be set (to 0 or 1) to indicate whether the host C
compiler supports long long.  If it is not set, the default is based on
C99 support.
*/
#ifndef HAVE_LONG_LONG
#ifndef __STDC_VERSION__
#define HAVE_LONG_LONG 0
#else /* defined(__STDC_VERSION__) */
#if __STDC_VERSION__ < 199901L
#define HAVE_LONG_LONG 0
#else /* !(__STDC_VERSION__ < 199901L) */
#define HAVE_LONG_LONG 1
#endif /* __STDC_VERSION__ < 199901L */
#endif /* ifndef __STDC_VERSION__ */
#endif /* ifndef HAVE_LONG_LONG */

#include <stdio.h>
#include <string.h>
#include <setjmp.h>
#if HAVE_WCHAR_T
#include <stddef.h>
#endif /* HAVE_WCHAR_T */
#if HAVE_SIZE_T
#include <stddef.h>
#endif /* HAVE_SIZE_T */
#if HAVE_PTRDIFF_T
#include <stddef.h>
#endif /* HAVE_PTRDIFF_T */

extern void exit();

unsigned long targ_sizeof_short;
unsigned long targ_sizeof_int;
unsigned long targ_sizeof_long;
unsigned long targ_sizeof_float;
unsigned long targ_sizeof_double;
unsigned long targ_sizeof_pointer;
unsigned long targ_alignof_short;
unsigned long targ_alignof_int;
unsigned long targ_alignof_long;
unsigned long targ_alignof_float;
unsigned long targ_alignof_double;
unsigned long targ_alignof_pointer;
#if HAVE_LONG_DOUBLE
unsigned long targ_sizeof_long_double;
unsigned long targ_alignof_long_double;
#endif /* HAVE_LONG_DOUBLE */
#if HAVE_WCHAR_T
unsigned long targ_sizeof_wchar_t;
unsigned long targ_alignof_wchar_t;
#endif /* HAVE_WCHAR_T */
#if HAVE_SIZE_T
unsigned long targ_sizeof_size_t;
unsigned long targ_alignof_size_t;
#endif /* HAVE_SIZE_T */
#if HAVE_PTRDIFF_T
unsigned long targ_sizeof_ptrdiff_t;
unsigned long targ_alignof_ptrdiff_t;
#endif /* HAVE_PTRDIFF_T */
#if HAVE_LONG_LONG
unsigned long targ_sizeof_long_long;
unsigned long targ_alignof_long_long;
#endif /* HAVE_LONG_LONG */

/* Convert the given preprocessor definition "X" into a string with the value X
   expands to. */
#define TO_STRING_QOUTE(X) #X
#define TO_STRING(x) TO_STRING_QOUTE(x)

static int	targ_char_bit;
			/* The number of bits in a char. */


static unsigned long bit_mask(int nbits)
/* Return a bit-mask of nbits bits. */
{
  unsigned long i, j;
  i = 1;
  i <<= nbits - 1;  /* Top bit. */
  j = i - 1;  /* All bits except top. */
  i |= j;  /* All bits. */
  return i;
}  /* bit_mask */


static unsigned long alignment(char *p2, char *p1)
/* Determine an alignment value based on the difference between p2 and p1. */
{
  unsigned long align;
  unsigned long diff = (unsigned long)(p2 - p1);

  for (align = 1; (diff & align) == 0; align *= 2) {}
  return align;
}  /* alignment */


/*
Representation of different possible integer kinds.
*/
enum int_kind {
  ik_char,
  ik_short,
  ik_int,
  ik_long,
  ik_long_long,
};


static const char *int_kind_to_type_str(int_kind kind,
                                        int      is_signed)
/*
Return a string for the integer kind that matches the given kind and is signed
or unsigned according to is_signed (is_signed can be -1 to indicate "don't
care").
*/
{
  const char *result;

  switch (kind) {
    case ik_char:
      if (is_signed == -1) {
        result = "ik_char";
      } else if (is_signed == 1) {
        result = "ik_signed_char";
      } else {
        result = "ik_unsigned_char";
      }  /* if */
      break;
    case ik_short:
      if (is_signed == 1) {
        result = "ik_short";
      } else {
        result = "ik_unsigned_short";
      }  /* if */
      break;
    case ik_int:
      if (is_signed == 1) {
        result = "ik_int";
      } else {
        result = "ik_unsigned_int";
      }  /* if */
      break;
    case ik_long:
      if (is_signed == 1) {
        result = "ik_long";
      } else {
        result = "ik_unsigned_long";
      }  /* if */
      break;
    case ik_long_long:
      if (is_signed == 1) {
        result = "ik_long_long";
      } else {
        result = "ik_unsigned_long_long";
      }  /* if */
      break;
    default:
      result = "<error>";
      break;
  }  /* switch */
  return result;
}  /* int_kind_to_type_str */


static unsigned long int_kind_to_size(int_kind kind)
/* Convert the given int kind to a byte size. */
{
  unsigned long result;

  switch (kind) {
    case ik_char:
      result = 1;
      break;
    case ik_short:
      result = targ_sizeof_short;
      break;
    case ik_int:
      result = targ_sizeof_int;
      break;
    case ik_long:
      result = targ_sizeof_long;
      break;
#if HAVE_LONG_LONG
    case ik_long_long:
      result = targ_sizeof_long_long;
      break;
#endif /* HAVE_LONG_LONG */
    default:
      result = 0;
      break;
  }  /* switch */
  return result;
}  /* int_kind_to_size */


static int_kind int_kind_for_alignment(unsigned long alignment,
                                       int           *error)
/*
Return the int kind enumerator for the smallest integer kind that matches the
given alignment.  Set *error set to TRUE if no such integer kind exists.
*/
{
  int_kind result;

  *error = 0;
  if (alignment == 1) {
    result = ik_char;
  } else if (alignment == targ_alignof_short) {
    result = ik_short;
  } else if (alignment == targ_alignof_int) {
    result = ik_int;
  } else if (alignment == targ_alignof_long) {
    result = ik_long;
#if HAVE_LONG_LONG
  } else if (alignment == targ_alignof_long_long) {
    result = ik_long_long;
#endif /* HAVE_LONG_LONG */
  } else {
    *error = 1;
    result = ik_int;
  }  /* if */
  return result;
}  /* int_kind_for_alignment */


static const char *int_kind_for_integral_type(unsigned long size,
                                              unsigned long alignment,
                                              int           is_signed,
                                              int           favor_long,
                                              int           *error)
/*
Return a string for the integer kind that matches the given size and
alignment, and is signed or unsigned according to is_signed (is_signed
can be -1 to indicate "don't care").  If favor_long is TRUE, choose long
over int if long and int have the same size.  Return *error set to TRUE
if no such integer kind exists.
*/
{
  const char *result;
  int_kind   kind;

  *error = 0;
  if (size == 1 && alignment == 1) {
    kind = ik_char;
  } else if (size == targ_sizeof_short && alignment == targ_alignof_short) {
    kind = ik_short;
  } else if (favor_long &&
             size == targ_sizeof_long && alignment == targ_alignof_long) {
    kind = ik_long;
  } else if (size == targ_sizeof_int && alignment == targ_alignof_int) {
    kind = ik_int;
  } else if (size == targ_sizeof_long && alignment == targ_alignof_long) {
    kind = ik_long;
#if HAVE_LONG_LONG
  } else if (size == targ_sizeof_long_long &&
             alignment == targ_alignof_long_long) {
    kind = ik_long_long;
#endif /* HAVE_LONG_LONG */
  } else {
    *error = 1;
  }  /* if */
  if (*error) {
    result = "";
  } else {
    result = int_kind_to_type_str(kind, is_signed);
  }  /* if */
  return result;
}  /* int_kind_for_integral_type */


static const char *type_for_integer_size(int  size,
                                         int  is_signed)
/*
Return the C data type for an integer data type of size bits.  Return the
signed version if is_signed is TRUE, the unsigned one otherwise.  Return
NULL if no suitable type is found.
*/
{
  const char *result = NULL;

  if (size == sizeof(char) * targ_char_bit ) {
    result = is_signed ? "signed char" : "unsigned char";
  } else if (size == sizeof(short) * targ_char_bit ) {
    result = is_signed ? "short" : "unsigned short";
  } else if (size == sizeof(int) * targ_char_bit ) {
    result = is_signed ? "int" : "unsigned int";
  } else if (size == sizeof(long) * targ_char_bit ) {
    result = is_signed ? "long" : "unsigned long";
  }  /* if */
  return result;
}  /* type_for_integer_size */


int main() {
  long i;
  unsigned long ui;
  unsigned char uch;
  unsigned long targ_uchar_max;
  unsigned long targ_minimum_struct_alignment;
  char ch;

  printf("/* Configuration definitions determined by dettarg.c: */\n");
  /* Determine endian-ness. */
  i = 1;
  if (*((char *)&i) == 1) {
    printf("#define TARG_LITTLE_ENDIAN TRUE\n");
  } else {
    printf("#define TARG_LITTLE_ENDIAN FALSE\n");
  }  /* if */
  /* Determine number of bits in a character. */
  for (targ_char_bit = 1;
       ;
       targ_char_bit++) {
    ui = (unsigned long)1L << (unsigned long)targ_char_bit;
    uch = (unsigned char)ui;
    if (uch != ui) break;
  }  /* for */
  printf("#define TARG_CHAR_BIT %d\n", targ_char_bit);
  targ_uchar_max = bit_mask(targ_char_bit);
  ch = (char)targ_uchar_max;
  if (ch < 0) {
    /* Signed char. */
    printf("#define TARG_HAS_SIGNED_CHARS TRUE\n");
  } else {
    /* Unsigned char. */
    printf("#define TARG_HAS_SIGNED_CHARS FALSE\n");
  }  /* if */
  /* Find out ordering in multi-character constants. */
  i = 'ab';
  if (i == (('a' << targ_char_bit) | 'b')) {
    printf("#define TARG_CHAR_CONSTANT_FIRST_CHAR_MOST_SIGNIFICANT TRUE\n");
  } else {
    printf("#define TARG_CHAR_CONSTANT_FIRST_CHAR_MOST_SIGNIFICANT FALSE\n");
  }  /* if */
  /* Determine size and alignment for integral types. */
  { struct {char c; short s;} v;
    targ_sizeof_short = sizeof(short);
    targ_alignof_short = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_SHORT %lu\n", targ_sizeof_short);
    printf("#define TARG_ALIGNOF_SHORT %lu\n", targ_alignof_short);
  }
  { struct {char c; int s;} v;
    targ_sizeof_int = sizeof(int);
    targ_alignof_int = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_INT %lu\n", targ_sizeof_int);
    printf("#define TARG_ALIGNOF_INT %lu\n", targ_alignof_int);
  }
  { struct {char c; long s;} v;
    targ_sizeof_long = sizeof(long);
    targ_alignof_long = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_LONG %lu\n", targ_sizeof_long);
    printf("#define TARG_ALIGNOF_LONG %lu\n", targ_alignof_long);
  }
#if HAVE_LONG_LONG
  { struct {char c; long long s;} v;
    targ_sizeof_long_long = sizeof(long long);
    targ_alignof_long_long = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_LONG_LONG %lu\n", targ_sizeof_long_long);
    printf("#define TARG_ALIGNOF_LONG_LONG %lu\n", targ_alignof_long_long);
  }
#endif /* HAVE_LONG_LONG */
  /* Pointer size and alignment. */
  { struct {char c; char *s;} v;
    targ_sizeof_pointer = sizeof(char *);
    targ_alignof_pointer = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_POINTER %lu\n", targ_sizeof_pointer);
    printf("#define TARG_ALIGNOF_POINTER %lu\n", targ_alignof_pointer);
  }
  /* Floating types size and alignment. */
  { struct {char c; float s;} v;
    targ_sizeof_float = sizeof(float);
    targ_alignof_float = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_FLOAT %lu\n", targ_sizeof_float);
    printf("#define TARG_ALIGNOF_FLOAT %lu\n", targ_alignof_float);
  }
  { struct {char c; double s;} v;
    targ_sizeof_double = sizeof(double);
    targ_alignof_double = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_DOUBLE %lu\n", targ_sizeof_double);
    printf("#define TARG_ALIGNOF_DOUBLE %lu\n", targ_alignof_double);
  }
#if HAVE_LONG_DOUBLE
  { struct {char c; long double s;} v;
    targ_sizeof_long_double = sizeof(long double);
    targ_alignof_long_double = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_SIZEOF_LONG_DOUBLE %lu\n", targ_sizeof_long_double);
    printf("#define TARG_ALIGNOF_LONG_DOUBLE %lu\n", targ_alignof_long_double);
  }
#else /* !HAVE_LONG_DOUBLE */
  /* No long double; use double for long double. */
  printf("#define TARG_SIZEOF_LONG_DOUBLE %lu\n", targ_sizeof_double);
  printf("#define TARG_ALIGNOF_LONG_DOUBLE %lu\n", targ_alignof_double);
  printf("#define LONG_DOUBLE_AS_DOUBLE_IN_GENERATED_C 1\n");
#endif /* HAVE_LONG_DOUBLE */
#if HAVE_WCHAR_T
  { struct {char c; wchar_t s;} v;
    const char                  *targ_wchar_t_int_kind;
    int                         is_signed, error;
    targ_sizeof_wchar_t = sizeof(wchar_t);
    targ_alignof_wchar_t = alignment((char *)&v.s, (char *)&v);
    is_signed = ((wchar_t)(-1) < 0);
    targ_wchar_t_int_kind = int_kind_for_integral_type(targ_sizeof_wchar_t,
                                                       targ_alignof_wchar_t,
                                                       is_signed,
                                                       /*favor_long=*/1,
                                                       &error);
    if (error) {
      fprintf(stderr, "Unable to determine TARG_WCHAR_T_INT_KIND.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else {
      printf("#define TARG_WCHAR_T_INT_KIND ((an_integer_kind)%s)\n",
             targ_wchar_t_int_kind);
    }  /* if */
  }
#endif /* HAVE_WCHAR_T */
#if HAVE_SIZE_T
  { struct {char c; size_t s;} v;
    const char                  *targ_size_t_int_kind;
    int                         error;
    targ_sizeof_size_t = sizeof(size_t);
    targ_alignof_size_t = alignment((char *)&v.s, (char *)&v);
    targ_size_t_int_kind = int_kind_for_integral_type(targ_sizeof_size_t,
                                                      targ_alignof_size_t,
                                                      /*is_signed=*/0,
                                                      /*favor_long=*/0,
                                                      &error);
    if (error) {
      fprintf(stderr, "Unable to determine TARG_SIZE_T_INT_KIND.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else {
      printf("#define TARG_SIZE_T_INT_KIND ((an_integer_kind)%s)\n",
             targ_size_t_int_kind);
    }  /* if */
  }
#endif /* HAVE_SIZE_T */
#if HAVE_PTRDIFF_T
  { struct {char c; ptrdiff_t s;} v;
    const char                  *targ_ptrdiff_t_int_kind;
    int                         error;
    targ_sizeof_ptrdiff_t = sizeof(ptrdiff_t);
    targ_alignof_ptrdiff_t = alignment((char *)&v.s, (char *)&v);
    targ_ptrdiff_t_int_kind = int_kind_for_integral_type(
                                                        targ_sizeof_ptrdiff_t,
                                                        targ_alignof_ptrdiff_t,
                                                        /*is_signed=*/1,
                                                        /*favor_long=*/0,
                                                        &error);
    if (error) {
      fprintf(stderr, "Unable to determine TARG_PTRDIFF_T_INT_KIND.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else {
      printf("#define TARG_PTRDIFF_T_INT_KIND ((an_integer_kind)%s)\n",
             targ_ptrdiff_t_int_kind);
    }  /* if */
  }
#endif /* HAVE_PTRDIFF_T */
  { unsigned long max_alignment = 1;
    if (max_alignment < targ_alignof_short) max_alignment = targ_alignof_short;
    if (max_alignment < targ_alignof_int)   max_alignment = targ_alignof_int;
    if (max_alignment < targ_alignof_long)  max_alignment = targ_alignof_long;
    if (max_alignment < targ_alignof_pointer) {
      max_alignment = targ_alignof_pointer;
    }  /* if */
#if HAVE_LONG_LONG
    if (max_alignment < targ_alignof_long_long) {
      max_alignment = targ_alignof_long_long;
    }  /* if */
#endif /* HAVE_LONG_LONG */
    /* The floating types are not considered because, with the default
       float_pt.c, an_internal_float_value is a string of bytes of the
       proper length, and therefore has no effect on the alignment
       requirement. */
    printf("#define HOST_ALIGNMENT_REQUIRED %lu\n", max_alignment);
  }
  /* Right shift is arithmetic? */
  if (((-1) >> 1) > 0) {
    /* No. */
    printf("#define TARG_RIGHT_SHIFT_IS_ARITHMETIC FALSE\n");
  } else {
    printf("#define TARG_RIGHT_SHIFT_IS_ARITHMETIC TRUE\n");
  }  /* if */
  /* Too-large shift count is masked to right number of bits? */
  { int i = 16;
    int j = targ_sizeof_int*targ_char_bit + 1;
    if (j == 33) {
      /* Try to fold at compile time, because compilers usually fold shifts
         differently at compile time than the way they are done at runtime. */
      i = 16 >> 33;
    } else {
      i = i >> j;
    }  /* if */
    if (i == 8) {
      /* Yes, the shift count gets truncated to the right number of bits. */
      printf("#define TARG_TOO_LARGE_SHIFT_COUNT_IS_TAKEN_MODULO_SIZE TRUE\n");
    } else {
      /* No, the shift count is treated as if we really shift that many
         places. */
      printf(
            "#define TARG_TOO_LARGE_SHIFT_COUNT_IS_TAKEN_MODULO_SIZE FALSE\n");
    }  /* if */
  }
  { struct { char c; struct { char d; } s; } v;
    targ_minimum_struct_alignment = alignment((char *)&v.s, (char *)&v);
    printf("#define TARG_MINIMUM_STRUCT_ALIGNMENT %lu\n",
           targ_minimum_struct_alignment);
  }
  { struct { char c; jmp_buf s; } v;
    unsigned long alignof_jmp_buf = alignment((char *)&v.s, (char *)&v);
    int           error;
    int_kind      buffer_type;

    buffer_type = int_kind_for_alignment(alignof_jmp_buf, &error);
    if (error) {
      fprintf(stderr, "Unable to determine TARG_JMP_BUF_NUM_ELEMENTS.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
      fprintf(stderr, "Unable to determine TARG_JMP_BUF_ELEMENT_INT_KIND.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else {
      unsigned long buffer_element_size = int_kind_to_size(buffer_type);
      unsigned long buffer_element_count =
                                         sizeof(jmp_buf) / buffer_element_size;
      int      buffer_overflow = (sizeof(jmp_buf) % buffer_element_size) != 0;

      /* If the type does not evenly divide, add an element to address
         overflow. */
      if (buffer_overflow) {
        ++buffer_element_count;
      }  /* if */
      printf("#define TARG_JMP_BUF_NUM_ELEMENTS %lu\n", buffer_element_count);
      printf("#define TARG_JMP_BUF_ELEMENTS_ARE_FLOAT 0\n");
      printf("#define TARG_JMP_BUF_ELEMENT_INT_KIND ((an_integer_kind)%s)\n",
             int_kind_to_type_str(buffer_type, /*is_signed*/-1));
    }  /* if */
  }
#ifdef setjmp
  /* setjmp is defined as a macro, attempt to expand setjmp and determine the
     underlying function being used.  setjmp is typically defined as "define
     setjmp(env) _setjmp (env)" in libstdc++ for Linux; foo is thus passed as a
     dummy argument for macro expansion.  Notably, (while no standard library
     implementation is known to do this currently) this approach is equally
     compatible with "define setjmp _setjmp", the dummy argument is simply
     ignored by the preprocessor. */
  { const char func_call_expanded[] = TO_STRING(setjmp(foo));
    char func_call_result[sizeof(func_call_expanded)];

    for (unsigned i = 0, k = 0; i < sizeof(func_call_expanded); ++i) {
      char curr_char = func_call_expanded[i];

      if (curr_char == ' ' || curr_char == '(') {
        func_call_result[k] = '\0';
        break;
      }  /* if */
      func_call_result[k++] = curr_char;
    }  /* for */
    printf("#define TARG_SETJMP_FUNC \"%s\"\n", func_call_result);
  }
#else /* !ifdef setjmp */
  /* The normal setjmp function is in use, there's no macro trick to modify
     consuming code (as done by libstdc++). */
  printf("#define TARG_SETJMP_FUNC \"setjmp\"\n");
#endif /* ifdef setjmp */
  { const char *type_string;
    type_string = type_for_integer_size(8, /*is_signed=*/1);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_INT8_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "signed char") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_INT8_T %s\n", type_string);
    }  /* if */
  }
  { const char *type_string;
    type_string = type_for_integer_size(8, /*is_signed=*/0);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_UINT8_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "unsigned char") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_UINT8_T %s\n", type_string);
    }  /* if */
  }
  { const char *type_string;
    type_string = type_for_integer_size(16, /*is_signed=*/1);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_INT16_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "short") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_INT16_T %s\n", type_string);
    }  /* if */
  }
  { const char *type_string;
    type_string = type_for_integer_size(16, /*is_signed=*/0);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_UINT16_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "unsigned short") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_UINT16_T %s\n", type_string);
    }  /* if */
  }
  { const char *type_string;
    type_string = type_for_integer_size(32, /*is_signed=*/1);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_INT32_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "int") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_INT32_T %s\n", type_string);
    }  /* if */
  }
  { const char *type_string;
    type_string = type_for_integer_size(32, /*is_signed=*/0);
    if (type_string == NULL) {
      fprintf(stderr, "Unable to determine EDG_UINT32_T.\n");
      fprintf(stderr, "(It will have to be done manually.)\n");
    } else if (strcmp(type_string, "unsigned int") == 0) {
      /* The default value can be used. */
    } else {
      printf("#define EDG_UINT32_T %s\n", type_string);
    }  /* if */
  }
  return 0;
}  /* main */
