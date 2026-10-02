/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

defines.h -- Defines configuration parameters for a given version of the
             front end.

This version is for the Sun Solaris operating system.

*/

/*
A word of caution about using TRUE/FALSE values in this header file: The
macros TRUE and FALSE are defined in basics.h, but their definition occurs
only after defines.h is included.  A side-effect of this is that any #if
test of a macro whose value is either TRUE or FALSE will result in the false
branch being taken.  Consequently, it's best to use 1 or 0 instead of TRUE and
FALSE in this header file.
*/

/* Avoid including these declarations more than once. */
#ifndef DEFINES_SOLARIS_H
#define DEFINES_SOLARIS_H 1

/* Configuration definitions determined by dettarg.c: */

#ifdef DEMO_VERSION
/* Demo versions should support multiple translation units. */
#define COMPILE_MULTIPLE_TRANSLATION_UNITS 1
#define DEBUG 0
#define IA64_ABI 1
#endif /* ifdef DEMO_VERSION */

#ifdef SUNOS
/* SPARC SunOS specific defines. */
#define TARG_LITTLE_ENDIAN 0
#define TARG_JMP_BUF_NUM_ELEMENTS 9
#define TARG_TOO_LARGE_SHIFT_COUNT_IS_TAKEN_MODULO_SIZE 1
#else /* ifndef SUNOS */
#ifdef __sparc
/* SPARC Solaris specific defines. */
#define TARG_LITTLE_ENDIAN 0
#define TARG_JMP_BUF_NUM_ELEMENTS 12
#define TARG_TOO_LARGE_SHIFT_COUNT_IS_TAKEN_MODULO_SIZE 1
#else /* ifndef __sparc */
/* Intel Solaris specific defines. */
#define TARG_LITTLE_ENDIAN 1
#define TARG_JMP_BUF_NUM_ELEMENTS 10
#define DEFAULT_STDC_ZERO_IN_SYSTEM_HEADERS 1
#endif /* ifdef __sparc */
#endif /* ifdef SUNOS */

#define TARG_CHAR_BIT 8
#define TARG_CHAR_CONSTANT_FIRST_CHAR_MOST_SIGNIFICANT 1
#define TARG_HAS_SIGNED_CHARS 1
#define TARG_SIZEOF_SHORT 2
#define TARG_ALIGNOF_SHORT 2
#define TARG_SIZEOF_INT 4
#define TARG_ALIGNOF_INT 4
#define TARG_SIZEOF_LONG 4
#define TARG_ALIGNOF_LONG 4
#define TARG_SIZEOF_POINTER 4
#define TARG_ALIGNOF_POINTER 4
#define TARG_SIZEOF_FLOAT 4
#define TARG_ALIGNOF_FLOAT 4
#define TARG_SIZEOF_DOUBLE 8
#ifdef __sparc
#define TARG_ALIGNOF_DOUBLE 8
#define TARG_ALIGNOF_LONG_DOUBLE 8
#define TARG_ALIGNOF_LONG_LONG 8
#define FP_LONG_DOUBLE_IS_BINARY128 1
#else /* ifndef __sparc */
#define TARG_ALIGNOF_DOUBLE 4
#define TARG_ALIGNOF_LONG_DOUBLE 4
#define TARG_ALIGNOF_LONG_LONG 4
#define TARG_ALIGNOF_SIGNED_LONG_ACCUM 4
#define TARG_ALIGNOF_UNSIGNED_LONG_ACCUM 4
#ifndef FP_LONG_DOUBLE_IS_BINARY128
#define FP_LONG_DOUBLE_IS_80BIT_EXTENDED 1
#endif /* defined(FP_LONG_DOUBLE_IS_BINARY128) */
#endif /* ifdef __sparc */


#ifndef TARG_SIZEOF_LONG_DOUBLE
#ifdef __sparc
#define TARG_SIZEOF_LONG_DOUBLE 16
#else /* ifndef __sparc */
#define TARG_SIZEOF_LONG_DOUBLE 12
#endif /* ifdef __sparc */
#endif /* ifndef TARG_SIZEOF_LONG_DOUBLE */

#define TARG_WCHAR_T_INT_KIND ((an_integer_kind)ik_long)
#define TARG_SIZE_T_INT_KIND ((an_integer_kind)ik_unsigned_int)
#define TARG_PTRDIFF_T_INT_KIND ((an_integer_kind)ik_int)
#define HOST_ALIGNMENT_REQUIRED 4
#define TARG_RIGHT_SHIFT_IS_ARITHMETIC 1
#define TARG_MINIMUM_STRUCT_ALIGNMENT 1
#define TARG_JMP_BUF_ELEMENT_INT_KIND ((an_integer_kind)ik_int)

/*
Definitions for Solaris:
*/
#ifndef __BSD__
/* __BSD__ may be set if building on SunOS. */
#define __ANSIC__ 1
#endif /* ifndef __BSD__ */
/* C_GEN_BE_GENERATES_ANSI_C may be set to 0 if building on SunOS. */
#ifndef C_GEN_BE_GENERATES_ANSI_C
#define C_GEN_BE_GENERATES_ANSI_C 1
#endif /* ifndef C_GEN_BE_GENERATES_ANSI_C */
#define MAKE_ALL_FUNCTIONS_UNPROTOTYPED 0
#define LONG_LONG_ALLOWED 1
#define INTEGER_VALUE_REPR_IS_A_HOST_INTEGER 0
#ifdef SUNOS
#define TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED 1
#else /* !defined(SUNOS) */
#define TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED 0
#endif /* ifdef SUNOS */
#define TARG_ZERO_WIDTH_BIT_FIELD_AFFECTS_STRUCT_ALIGNMENT 0
#define STDC_ZERO_IN_NONSTRICT_MODE 1
#define GUARD_MACRO_FOR_VA_LIST "_VA_LIST"
#define DEFAULT_PASS_STDARG_REFERENCES_TO_GENERATED_CODE 1
#define IGNORE_CARRIAGE_RETURN_IN_SOURCE 1
#ifndef GNU_EXTENSIONS_ALLOWED
#define GNU_EXTENSIONS_ALLOWED 1
#define DEFAULT_GNU_COMPATIBILITY 0
#define GNU_NAKED_ATTRIBUTE_ALLOWED 1
#define GNU_VISIBILITY_ATTRIBUTE_ALLOWED 1
#define DEFAULT_GNU_VISIBILITY_ATTRIBUTE_ENABLED 1
#endif /* ifndef GNU_EXTENSIONS_ALLOWED */
#ifndef DEFAULT_USE_PREDEFINED_MACRO_FILE
#define DEFAULT_USE_PREDEFINED_MACRO_FILE 1
#endif /* !defined(DEFAULT_USE_PREDEFINED_MACRO_FILE) */
#define DEFAULT_SUN_LINKER_SCOPE_ALLOWED 1

#ifndef USE_LONG_DOUBLE_FOR_HOST_FP_VALUE
#define USE_LONG_DOUBLE_FOR_HOST_FP_VALUE 1
#endif /* ifndef USE_LONG_DOUBLE_FOR_HOST_FP_VALUE */

/*
Determine the C compiler being used to configure initialization handling
in the C-generating back end.
*/
#ifdef SUNOS
#define USE_PATCH_INIT_STARTUP 1
#else /* ifndef SUNOS */
#if !defined(__GNUC__) && !defined(__CENTERLINE__) && \
    !defined(__SUNPRO_C) && !defined(__SUNPRO_CC)
#define USE_INIT_SECTION_IN_GENERATED_C 1
#endif /* !defined(__GNUC__) && !defined(__CENTERLINE__) && ... */
#endif /* ifdef SUNOS */

#ifdef __SUNPRO_CC
/* At least as of 12.1, Sun C++ does not define __func__. */
#define FUNC_AVAILABLE 0
#endif /* ifdef __SUNPRO_CC */

#endif /* ifndef DEFINES_SOLARIS_H */

