/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

defines.h -- Defines configuration parameters for a given version of the
             front end.

This version is for the Apple MacOS X operating system.

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
#ifndef DEFINES_MACOS_H
#define DEFINES_MACOS_H 1

#ifdef DEMO_VERSION
/* Demo versions should support multiple translation units. */
#define COMPILE_MULTIPLE_TRANSLATION_UNITS 1
#define DEBUG 0
#endif /* ifdef DEMO_VERSION */

/*
If the target architecture is unspecified, heuristically determine a target
architecture from the host compiler.
*/
#if !defined(TARG_SUPPORTS_X86_64) && !defined(TARG_SUPPORTS_ARM64) && \
    !defined(TARG_SUPPORTS_ARM32) && !defined(TARG_SUPPORTS_RISCV64) && \
    !defined(TARG_SUPPORTS_RISCV32)
/* Default to an unspecified target. */
#define TARG_SUPPORTS_X86_64 0
#define TARG_SUPPORTS_ARM64 0
#define TARG_SUPPORTS_ARM32 0
#define TARG_SUPPORTS_RISCV64 0
#define TARG_SUPPORTS_RISCV32 0
#if defined(__x86_64__)
/* 64-bit x86. */
#undef TARG_SUPPORTS_X86_64
#define TARG_SUPPORTS_X86_64 1
#elif defined(__aarch64__)
/* 64-bit ARM. */
#undef TARG_SUPPORTS_ARM64
#define TARG_SUPPORTS_ARM64 1
#else /* !defined(__x86_64__) */
#endif /* defined(__x86_64__) */
#endif /* !defined(TARG_SUPPORTS_X86_64) && !defined(TARG_SUPPORTS_ARM64)... */

/* Configuration definitions determined by dettarg.c: */
#if TARG_SUPPORTS_X86_64 || TARG_SUPPORTS_ARM64
/* 64-bit targets. */
#define TARG_SIZEOF_LONG 8
#define TARG_ALIGNOF_LONG 8
#define TARG_SIZEOF_POINTER 8
#define TARG_ALIGNOF_POINTER 8
#define TARG_ALIGNOF_DOUBLE 8
#define TARG_WCHAR_T_INT_KIND ((an_integer_kind)ik_int)
#define HOST_ALIGNMENT_REQUIRED 8
#define TARG_ALIGNOF_LONG_LONG 8
#else /* !(TARG_SUPPORTS_X86_64 || TARG_SUPPORTS_ARM64) */
/* 32-bit targets. */
#define TARG_SIZEOF_LONG 4
#define TARG_ALIGNOF_LONG 4
#define TARG_SIZEOF_POINTER 4
#define TARG_ALIGNOF_POINTER 4
#define TARG_ALIGNOF_DOUBLE 4
#define TARG_WCHAR_T_INT_KIND ((an_integer_kind)ik_long)
#define HOST_ALIGNMENT_REQUIRED 4
#define TARG_ALIGNOF_LONG_LONG 4
#endif /* TARG_SUPPORTS_X86_64 || TARG_SUPPORTS_ARM64 */

#ifdef __ppc__
/* PowerPC. */
#define TARG_LITTLE_ENDIAN 0
#define TARG_JMP_BUF_NUM_ELEMENTS 192
#define TARG_SIZEOF_LONG_DOUBLE 8
#define TARG_ALIGNOF_LONG_DOUBLE 4
#define USE_LONG_DOUBLE_FOR_HOST_FP_VALUE 0
#define FP_LONG_DOUBLE_IS_BINARY64 1
#define TARG_DUAL_ALIGNMENTS_FOR_BUILTIN_TYPES 0
#elif TARG_SUPPORTS_ARM64
/* ARM64. */
#define TARG_SIZEOF_LONG_DOUBLE 8
#define TARG_ALIGNOF_LONG_DOUBLE 8
#define TARG_JMP_BUF_NUM_ELEMENTS 48
#define USE_LONG_DOUBLE_FOR_HOST_FP_VALUE 0
#define FP_LONG_DOUBLE_IS_BINARY64 1
#elif TARG_SUPPORTS_X64_64
/* 64-bit x86. */
#define TARG_SIZEOF_LONG_DOUBLE 16
#define TARG_ALIGNOF_LONG_DOUBLE 16
#define FP_LONG_DOUBLE_IS_80BIT_EXTENDED 1
#define TARG_JMP_BUF_NUM_ELEMENTS 37
#elif TARG_SUPPORTS_ARM32
/* ARM32. Untested currently (but can be configured manually). */
 #error Support for ARM32 is untested
#else /* Non-specific target. */
/* Presume 32-bit x86. */
#define TARG_JMP_BUF_NUM_ELEMENTS 18
#endif /* ifndef __ppc__ */

#define TARG_SIZE_T_INT_KIND ((an_integer_kind)ik_unsigned_long)
#define TARG_PTRDIFF_T_INT_KIND ((an_integer_kind)ik_long)
#define TARG_CHAR_BIT 8
#define TARG_HAS_SIGNED_CHARS 1
#define TARG_CHAR_CONSTANT_FIRST_CHAR_MOST_SIGNIFICANT 1
#define TARG_SIZEOF_SHORT 2
#define TARG_ALIGNOF_SHORT 2
#define TARG_SIZEOF_INT 4
#define TARG_ALIGNOF_INT 4
#define TARG_SIZEOF_FLOAT 4
#define TARG_ALIGNOF_FLOAT 4
#define TARG_SIZEOF_DOUBLE 8
#define TARG_RIGHT_SHIFT_IS_ARITHMETIC 1
#define TARG_MINIMUM_STRUCT_ALIGNMENT 1
#define TARG_JMP_BUF_ELEMENTS_ARE_FLOAT 0
#define TARG_JMP_BUF_ELEMENT_INT_KIND ((an_integer_kind)ik_int)
#ifndef ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
#define ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS 1
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
#define GUARD_MACRO_FOR_VA_LIST "_VA_LIST"


/* Language extensions. */
#if !defined(GNU_EXTENSIONS_ALLOWED)
#define GNU_EXTENSIONS_ALLOWED 1
#endif /* !defined(GNU_EXTENSIONS_ALLOWED) */
#ifndef DEFAULT_GNU_COMPATIBILITY 
#define DEFAULT_GNU_COMPATIBILITY 0
#endif /* ifndef DEFAULT_GNU_COMPATIBILITY */
#ifndef DEFAULT_USE_PREDEFINED_MACRO_FILE
#define DEFAULT_USE_PREDEFINED_MACRO_FILE 1
#endif /* !defined(DEFAULT_USE_PREDEFINED_MACRO_FILE) */
#define C99_IL_EXTENSIONS_SUPPORTED 1
#ifndef MICROSOFT_EXTENSIONS_ALLOWED
#define MICROSOFT_EXTENSIONS_ALLOWED 0
#endif /* ifndef MICROSOFT_EXTENSIONS_ALLOWED */
#define LONG_LONG_ALLOWED 1
#define UNICODE_SOURCE_SUPPORTED 1
#define DEFAULT_UNICODE_SOURCE_KIND usk_utf8


/* ABI selection. */
#ifndef IA64_ABI
#define IA64_ABI 1
#endif /* ifndef IA64_ABI */
#ifndef DEFAULT_EMULATE_GNU_ABI_BUGS
#define DEFAULT_EMULATE_GNU_ABI_BUGS 0
#endif /* DEFAULT_EMULATE_GNU_ABI_BUGS */
#ifndef CP_GEN_BE_VERSION
#ifndef GCC_IS_GENERATED_CODE_TARGET
#define GCC_IS_GENERATED_CODE_TARGET 0
#endif /* ifndef GCC_IS_GENERATED_CODE_TARGET */
#ifndef CLANG_IS_GENERATED_CODE_TARGET
#define CLANG_IS_GENERATED_CODE_TARGET 1
#endif /* ifndef CLANG_IS_GENERATED_CODE_TARGET */
#endif /* ifndef CP_GEN_BE_VERSION */
#define TARG_EXTERNAL_NAMES_GET_UNDERSCORE_ADDED 1
#define TARG_BIT_FIELD_CONTAINER_SIZE (-1)
#define ALLOW_NON_INT_BIT_FIELD_BASE_TYPE_IN_GENERATED_C 1
#define DEFAULT_PASS_STDARG_REFERENCES_TO_GENERATED_CODE 1

#endif /* ifndef DEFINES_MACOS_H */

